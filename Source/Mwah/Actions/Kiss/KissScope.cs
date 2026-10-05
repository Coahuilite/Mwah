using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻范围的七档门禁。出厂就是最右档 <see cref="Everything"/>：不限阵营、不限种族，
/// 这是模组的立场，不是待修的缺陷。往左拖是替玩家收窄，一格到位地回到"只撮合殖民地"。
///
/// 每档 = 前一档 ∨ 本档新增的那一类人群；配对成立要求双方都在档内，
/// 所以单调性由构造保证，不需要额外的白名单表。
/// 谓词全部取自原版（<c>Pawn.IsColonist/IsPrisoner/IsColony*</c>、
/// <c>RaceProperties.Humanlike/IsFlesh</c>），不引入任何新的判断数据。
/// </summary>
public enum KissScope
{
    /// <summary>0 只有自由殖民者。</summary>
    FreeColonists = 0,

    /// <summary>1 加奴隶与囚犯。</summary>
    ColonyPopulation = 1,

    /// <summary>2 加自家动物、机甲与可征召 subhuman。此档起出现没有心情的参与者。</summary>
    ColonyOwned = 2,

    /// <summary>3 加非敌对的人形（访客、商人、临时盟友、CreepJoiner）。</summary>
    NonHostileHumanlike = 3,

    /// <summary>4 加所有人形，包括敌对者。</summary>
    AllHumanlike = 4,

    /// <summary>5 加血肉生物：野生动物、昆虫、血肉兽。</summary>
    AllFlesh = 5,

    /// <summary>6 万物互亲：再加机械族、无人机、虚空实体与被摘掉需求的 mutant。默认档。</summary>
    Everything = 6,
}

/// <summary>门禁判定与档位文案键。</summary>
public static class KissScopeUtility
{
    /// <summary>本档相对前一档新增的人群。单独看没有意义，只用于累积。</summary>
    private static bool AddedBy(Pawn pawn, Pawn other, KissScope scope)
    {
        switch (scope)
        {
            case KissScope.FreeColonists:
                return pawn.IsFreeNonSlaveColonist;
            case KissScope.ColonyPopulation:
                // 囚犯能被亲但不能主动去亲：原版 IsColonistPlayerControlled 不放行，那是下令链的门槛，不是这里的。
                return pawn.IsColonist || pawn.IsPrisoner;
            case KissScope.ColonyOwned:
                return pawn.IsColonyAnimal || pawn.IsColonyMech || pawn.IsColonySubhuman;
            case KissScope.NonHostileHumanlike:
                return pawn.RaceProps.Humanlike && !pawn.HostileTo(other);
            case KissScope.AllHumanlike:
                return pawn.RaceProps.Humanlike;
            case KissScope.AllFlesh:
                return pawn.RaceProps.IsFlesh;
            default:
                return true;
        }
    }

    /// <summary>累积到 <paramref name="scope"/> 档时，这个 pawn 算不算在范围内。</summary>
    public static bool InScope(Pawn pawn, Pawn other, KissScope scope)
    {
        for (int s = (int)KissScope.FreeColonists; s <= (int)scope; s++)
        {
            if (AddedBy(pawn, other, (KissScope)s))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>把存档里可能越界的原始值收进合法档。</summary>
    public static KissScope Clamp(int raw) =>
        (KissScope)Mathf.Clamp(raw, (int)KissScope.FreeColonists, (int)KissScope.Everything);

    /// <summary>
    /// 档位名键。名字由枚举拼出来，所以语言文件里的键不在 C# 字面量扫描范围内 ——
    /// 由 scripts/verify-local.ps1 的"档位 ↔ 双语键"门反向核对，加档漏文案会直接红。
    /// </summary>
    public static string LabelKey(KissScope scope) => "MWAH.Settings.Scope." + scope;

    public static string Label(KissScope scope) => LabelKey(scope).Translate();
}
