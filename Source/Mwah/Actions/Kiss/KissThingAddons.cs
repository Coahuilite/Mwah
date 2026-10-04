using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// "pawn 亲非 pawn 目标"的提案结果。与双人 KissProposal 的差别只有一条、但它是本层的宪法：
/// <see cref="Visible"/>=false（addon 开关关着、目标已塌）时**一切入口静默失效** —— 不给灰项、
/// 不派 job、不弹消息；只有 Visible 且 Allowed=false 才配一句真实原因。
/// </summary>
public readonly struct ThingKissProposal
{
    public readonly bool Visible;
    public readonly bool Allowed;
    public readonly Pawn? Doer;
    public readonly string? BlockedReason;

    private ThingKissProposal(bool visible, bool allowed, Pawn? doer, string? blockedReason)
    {
        Visible = visible;
        Allowed = allowed;
        Doer = doer;
        BlockedReason = blockedReason;
    }

    public static ThingKissProposal Hidden() => new(false, false, null, null);

    public static ThingKissProposal Allow(Pawn doer) => new(true, true, doer, null);

    public static ThingKissProposal Blocked(string reason) => new(true, false, null, reason);
}

/// <summary>
/// addon 层的宪法：core 只有"pawn 亲 pawn"；"pawn 亲非 pawn 目标"是一族附加功能 ——
/// 本次是墙，下次可以是树。可动的永远是 pawn，所以每个 addon 只接"由 pawn 发起"的单向亲吻。
///
/// 管线（提案判定、落脚点、job、右键菜单、导演台取点与派发、静默失效语义）全部在基类与
/// 注册表里现成完成；一个 addon 只提供四件事：自己的开关（<see cref="Active"/>）、
/// 目标谓词（<see cref="Accepts"/>）、天意表作用域（<see cref="Scope"/>）、
/// 右键标签口径（<see cref="OptionLabel"/>）。新加一种可亲的东西 = 一个子类 +
/// 注册表一行 + 自己的 defs/语言键，零管线代码。
///
/// 与双人吻共用冷却表与"正在亲"判定；**不进门禁滑条** —— 七档管的是"谁能亲谁"
/// （两个 pawn 的关系），Thing 不是 pawn，各归各的开关。
/// </summary>
public abstract class KissThingAddon
{
    /// <summary>天意表作用域（MWAH_FateDef.scope）。</summary>
    public abstract string Scope { get; }

    /// <summary>
    /// addon 的稳定 Id：设置字典的 key、存档里的名字。**改名即迁移**，定死不动。
    /// </summary>
    public abstract string Id { get; }

    /// <summary>本 addon 心情记忆的出厂时长（tick）；玩家覆盖走设置字典。</summary>
    public abstract int DefaultThoughtDurationTicks { get; }

    /// <summary>当前生效时长：设置字典按 Id 查，缺槽落出厂值（读侧已 clamp）。</summary>
    public int ThoughtDurationTicks => MwahMod.Settings.AddonThoughtDuration(Id, DefaultThoughtDurationTicks);

    /// <summary>总开关 ∧ 本 addon 自己的开关。</summary>
    public abstract bool Active { get; }

    /// <summary>什么算本 addon 的目标（不含开关与迷雾 —— 那是 Active/Pickable 的事）。</summary>
    public abstract bool Accepts(Thing thing);

    /// <summary>右键菜单文案（墙在这里做人形迟疑版）。</summary>
    public abstract string OptionLabel(Pawn doer, Thing target);

    /// <summary>表为空（用户删干净了）时的兜底分布；默认什么都不发。</summary>
    public virtual void SettleWithoutTable(Pawn doer, Thing target)
    {
    }

    /// <summary>地图点选（导演台右槽、快速发配第二段）能不能选中它：开着、算数、且不在迷雾里 —— 未知的部分不能提前选中。</summary>
    public bool Pickable(Thing thing)
    {
        return Active && Accepts(thing) && !thing.Position.Fogged(thing.Map);
    }

    /// <summary>
    /// 提案模板：开关 → 参与层 → 发起层（墙/树当不了发起方，没有角色互换的余地）→
    /// busy → 单人/成对冷却 → 落脚点。所有 addon 共享这一条判定顺序，
    /// 差别只在谓词与文案键。
    /// </summary>
    public ThingKissProposal Propose(Pawn doer, Thing target)
    {
        MwahSettings settings = MwahMod.Settings;
        if (!Active || !Accepts(target))
        {
            return ThingKissProposal.Hidden();
        }

        AcceptanceReport participation = KissBoundary.CanParticipate(doer);
        if (!participation.Accepted)
        {
            return ThingKissProposal.Blocked(participation.Reason);
        }
        AcceptanceReport initiator = KissBoundary.CanInitiate(doer, out _);
        if (!initiator.Accepted)
        {
            return ThingKissProposal.Blocked(initiator.Reason);
        }

        if (KissUtility.IsKissing(doer) || IsKissingThing(doer))
        {
            return ThingKissProposal.Blocked("MWAH.Fail.Busy".Translate());
        }
        if (!settings.CooldownsIgnored)
        {
            int pawnRemaining = KissCooldown.PawnRemaining(doer);
            if (pawnRemaining > 0)
            {
                return ThingKissProposal.Blocked("MWAH.Fail.CooldownPawn".Translate(
                    doer.Named("PAWN"), MwahTime.FormatTicks(pawnRemaining).Named("TIME")));
            }
            int pairRemaining = KissCooldown.PairRemaining(doer, target);
            if (pairRemaining > 0)
            {
                return ThingKissProposal.Blocked("MWAH.Fail.CooldownPair".Translate(
                    doer.Named("PAWN"), target.Named("OTHER"), MwahTime.FormatTicks(pairRemaining).Named("TIME")));
            }
        }

        if (FindStandCell(doer, target) == IntVec3.Invalid)
        {
            return ThingKissProposal.Blocked("MWAH.KissThing.Fail.NoStandCell".Translate(
                doer.Named("PAWN"), target.Named("WALL")));
        }
        return ThingKissProposal.Allow(doer);
    }

    /// <summary>右键点击落地：菜单可能开着好几秒，重新判一次；关掉了就静默，不打扰。</summary>
    public void BeginKiss(Pawn doer, Thing target)
    {
        ThingKissProposal proposal = Propose(doer, target);
        if (!proposal.Visible)
        {
            return; // 静默失效：功能已关，玩家不该看到任何"为什么不行"
        }
        if (!proposal.Allowed)
        {
            Messages.Message(proposal.BlockedReason ?? "MWAH.Fail.Stale".Translate(),
                new LookTargets(target), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        StartJob(doer, target, forced: false);
    }

    /// <summary>导演台/快速发配的代发：Allowed 时 job 已起（playerForced），结果原样交回调用方弹信。</summary>
    public ThingKissProposal BeginDirected(Pawn doer, Thing target)
    {
        ThingKissProposal proposal = Propose(doer, target);
        if (proposal.Allowed && proposal.Doer != null)
        {
            StartJob(doer, target, forced: true);
        }
        return proposal;
    }

    /// <summary>结算：先查天意表；表空落回 addon 自带的兜底分布。心情记忆吃本 addon 的时长设置。</summary>
    public void Settle(Pawn doer, Thing target)
    {
        if (!KissFate.Grant(doer, target, Scope, ThoughtDurationTicks))
        {
            SettleWithoutTable(doer, target);
        }
    }

    /// <summary>
    /// 与双人代发同理：playerForced 否则 pawn 的 think tree 会在下一个 override 检查点把它拽回去。
    /// </summary>
    private static void StartJob(Pawn doer, Thing target, bool forced)
    {
        var job = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_KissThing, target);
        if (forced)
        {
            job.playerForced = true;
        }
        doer.jobs.StartJob(job, JobCondition.InterruptForced);
    }

    /// <summary>busy 一条管住全族：亲着墙的人没空亲树，反之亦然。</summary>
    public static bool IsKissingThing(Pawn pawn) => pawn.CurJobDef == MWAH_JobDefOf.MWAH_KissThing;

    /// <summary>
    /// 挑一格"站得下、看得见目标、够得着"的落脚点：目标占格 8 周一圈里，离人最近者优先。
    /// A* 最多烧 <see cref="ReachProbeBudget"/> 次（按距离升序试，多半第一格就中）；
    /// 全不可达即返回 Invalid —— 被完全封死的目标（密室中心）亲不了，这是物理，不是门禁。
    /// 人已经站在可站格上时该格留在候选里：CanReach 自己脚下恒真，
    /// job 的"到位即推进"靠的就是把这一格原样还回来。
    /// </summary>
    private const int ReachProbeBudget = 3;

    public static IntVec3 FindStandCell(Pawn pawn, Thing target)
    {
        Map? map = pawn.Map;
        if (map == null || target.Map != map)
        {
            return IntVec3.Invalid;
        }
        List<IntVec3> candidates = new();
        foreach (IntVec3 c in GenAdj.CellsAdjacent8Way(target))
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

/// <summary>
/// addon 注册表：导演台与派发只认这里，不点名任何具体 addon。
/// 新加一个 addon（比如树）= 一个 KissThingAddon 子类 + 这里登记一行 + 自己的 defs/语言键。
/// </summary>
public static class KissThingAddons
{
    public static readonly KissThingAddon[] All =
    {
        KissWallAddon.Instance,
    };

    /// <summary>这个 thing 此刻能不能被地图点选（任一开着的 addon 认领即可）。</summary>
    public static bool AnyPickable(Thing thing)
    {
        for (int i = 0; i < All.Length; i++)
        {
            if (All[i].Pickable(thing))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>这个非 pawn 目标归不归任何（哪怕关着的）addon 管 —— 归谁、开没开，调用方自己判。</summary>
    public static KissThingAddon? For(Thing thing)
    {
        for (int i = 0; i < All.Length; i++)
        {
            if (All[i].Accepts(thing))
            {
                return All[i];
            }
        }
        return null;
    }

    /// <summary>槽位回收与右键入口预筛：目标仍被某个**开着的** addon 认领吗。</summary>
    public static bool ActiveFor(Thing thing) => For(thing) is { Active: true };
}
