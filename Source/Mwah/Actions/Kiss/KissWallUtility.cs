using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// "亲一堵墙"的判定与落地。与双人亲吻共用冷却表与参与层，但**不进门禁滑条**：
/// 七档范围管的是"谁能亲谁"（两个 pawn 的关系），墙不是 pawn，它归自己的开关管。
/// 墙的口径（rimsage 对 Core XML 实证）：def.IsWall（人造墙，含一切继承 Wall 或自写
/// isWall 的 mod 墙）∨ building.isNaturalRock（天然墙、岩壁、坍塌岩 —— 压埋矿物 Mineable*
/// 全部 ParentName="RockBase"，一并落进这个谓词）。
/// 墙没有心情、没有嘴、不会回应：所以发起方必须独自满足全部参与层，且结算走
/// <see cref="KissWallReward"/> 的天意五档，不乘 SocialImpact（那是"对方如何接住你"的量，
/// 墙的社交反馈恒为零 —— 乘它等于把喜剧乘没）。
/// </summary>
public static class KissWallUtility
{
    /// <summary>提案结果：Visible=false 连灰项都不给（开关关着、墙已塌）；Allowed=false 给灰项带原因。</summary>
    public readonly struct WallKissProposal
    {
        public readonly bool Visible;
        public readonly bool Allowed;
        public readonly Pawn? Doer;
        public readonly string? BlockedReason;

        private WallKissProposal(bool visible, bool allowed, Pawn? doer, string? blockedReason)
        {
            Visible = visible;
            Allowed = allowed;
            Doer = doer;
            BlockedReason = blockedReason;
        }

        public static WallKissProposal Hidden() => new(false, false, null, null);

        public static WallKissProposal Allow(Pawn doer) => new(true, true, doer, null);

        public static WallKissProposal Blocked(string reason) => new(true, false, null, reason);
    }

    public static bool IsWallLike(Thing? thing)
    {
        if (thing is not { Spawned: true } || thing.Destroyed || thing.def.category != ThingCategory.Building)
        {
            return false;
        }
        return thing.def.IsWall || (thing.def.building?.isNaturalRock ?? false);
    }

    public static WallKissProposal Propose(Pawn selected, Thing wall)
    {
        MwahSettings settings = MwahMod.Settings;
        if (!settings.Enabled || !settings.WallKissingEnabled || !IsWallLike(wall))
        {
            return WallKissProposal.Hidden();
        }

        AcceptanceReport participation = KissBoundary.CanParticipate(selected);
        if (!participation.Accepted)
        {
            return WallKissProposal.Blocked(participation.Reason);
        }
        // 墙永远当不了发起方，这里没有角色互换的余地：选中的这位动不了，这桩亲就不成立。
        AcceptanceReport initiator = KissBoundary.CanInitiate(selected, out _);
        if (!initiator.Accepted)
        {
            return WallKissProposal.Blocked(initiator.Reason);
        }

        if (KissUtility.IsKissing(selected) || IsKissingWall(selected))
        {
            return WallKissProposal.Blocked("MWAH.Fail.Busy".Translate());
        }
        if (!settings.CooldownsIgnored)
        {
            int pawnRemaining = KissCooldown.PawnRemaining(selected);
            if (pawnRemaining > 0)
            {
                return WallKissProposal.Blocked("MWAH.Fail.CooldownPawn".Translate(
                    selected.Named("PAWN"), MwahTime.FormatTicks(pawnRemaining).Named("TIME")));
            }
            int pairRemaining = KissCooldown.PairRemaining(selected, wall);
            if (pairRemaining > 0)
            {
                return WallKissProposal.Blocked("MWAH.Fail.CooldownPair".Translate(
                    selected.Named("PAWN"), wall.Named("OTHER"), MwahTime.FormatTicks(pairRemaining).Named("TIME")));
            }
        }

        if (FindStandCell(selected, wall) == IntVec3.Invalid)
        {
            return WallKissProposal.Blocked("MWAH.KissWall.Fail.NoStandCell".Translate(
                selected.Named("PAWN"), wall.Named("WALL")));
        }
        return WallKissProposal.Allow(selected);
    }

    /// <summary>点击菜单项后重新判一次再落地：菜单可能开着好几秒，墙可能被拆、人可能被征召。</summary>
    public static void BeginKiss(Pawn selected, Thing wall)
    {
        WallKissProposal proposal = Propose(selected, wall);
        if (!proposal.Allowed || proposal.Doer == null)
        {
            Messages.Message(proposal.BlockedReason ?? "MWAH.Fail.Stale".Translate(),
                new LookTargets(wall), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        var job = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_KissWall, wall);
        selected.jobs.StartJob(job, JobCondition.InterruptForced);
    }

    public static bool IsKissingWall(Pawn pawn) => pawn.CurJobDef == MWAH_JobDefOf.MWAH_KissWall;

    /// <summary>
    /// 挑一格"站得下、看得见墙、够得着"的落脚点：墙占格 8 周一圈里，离人最近者优先。
    /// A* 最多烧 <see cref="ReachProbeBudget"/> 次（按距离升序试，多半第一格就中）；
    /// 全不可达即返回 Invalid —— 被完全封死的墙（密室中心）亲不了，这是物理，不是门禁。
    /// 人已经站在可站的贴墙格上时该格留在候选里：CanReach 自己脚下恒真，
    /// TryWalk 的"到位即推进"靠的就是把这一格原样还回来。
    /// </summary>
    private const int ReachProbeBudget = 3;

    public static IntVec3 FindStandCell(Pawn pawn, Thing wall)
    {
        Map? map = pawn.Map;
        if (map == null || wall.Map != map)
        {
            return IntVec3.Invalid;
        }
        List<IntVec3> candidates = new();
        foreach (IntVec3 c in GenAdj.CellsAdjacent8Way(wall))
        {
            if (c.InBounds(map) && c.Standable(map) && !c.Fogged(map))
            {
                candidates.Add(c);
            }
        }
        if (candidates.Count == 0)
        {
            return IntVec3.Invalid;
        }
        candidates.Sort((a, b) => a.DistanceToSquared(pawn.Position).CompareTo(b.DistanceToSquared(pawn.Position)));
        int budget = ReachProbeBudget;
        for (int i = 0; i < candidates.Count && budget > 0; i++)
        {
            budget--;
            if (pawn.CanReach(candidates[i], PathEndMode.OnCell, Danger.Deadly))
            {
                return candidates[i];
            }
        }
        return IntVec3.Invalid;
    }
}
