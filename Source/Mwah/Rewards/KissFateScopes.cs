using System;
using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 无心情参与者的天意表有序注册表：**首中即停，没有兜底 else**。
/// 旧形是一条三元链，尾巴"其余一律 → 机械族表"——任何未来的非血肉、非人形、
/// 非 mutant 的存在都会被塞进机械族的冷笑话里说话。错归类比沉默更糟：
/// 认不出的存在就不发言，并留一行 dev 日志让维护者在测试期立刻看见缺口。
/// 新增一类 = 在合适的优先级位置插一行（与 <see cref="KissThingAddons.All"/>
/// 同一注册表惯例：一行注册 = 全部入口接通）。判别只用原版语义
/// （RaceProps / MutantBaseUtility），不引入新的判断数据。
/// </summary>
public static class KissFateScopes
{
    /// <summary>类人双人吻的旁白表作用域（MWAH_FateDef.scope）：只出句子、不掷心情。
    /// 与下面三张"无心情消息通道"表不同族，但同受"grep 一词贯穿"纪律管。</summary>
    public const string PawnNarrationScope = "KissPawn";

    private static readonly (string Scope, Func<Pawn, bool> Accepts)[] Table =
    {
        // 实体：mutant 空壳（原版三族被 disableNeeds 摘掉需求）或"有人形没心情"的 mod 种族。
        // 排最前——非人形的 mod mutant 不该掉进下面的动物表。
        ("KissPawnMutant", p => p.RaceProps.Humanlike || p.IsMutant),
        // 血肉且非人形：动物的嗅觉叙事。
        ("KissPawnBeast", p => p.RaceProps.IsFlesh),
        // 非血肉：机械族与无人机的冷笑话（"机器样"的显式判据，不是其余一切）。
        ("KissPawnMechanoid", p => !p.RaceProps.IsFlesh),
    };

    /// <summary>首中即停地查表；null = 无人认领（调用方保持静默并记 dev 日志）。</summary>
    public static string? ScopeFor(Pawn pawn)
    {
        for (int i = 0; i < Table.Length; i++)
        {
            if (Table[i].Accepts(pawn))
            {
                return Table[i].Scope;
            }
        }
        return null;
    }
}
