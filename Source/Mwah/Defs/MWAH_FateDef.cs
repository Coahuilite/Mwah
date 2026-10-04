using Verse;

// 命名空间是生存问题，不是风格问题：def 的 XML 根元素按**短名**解析
// （DirectXmlLoader.DefFromNode → GenTypes.GetTypeInAnyAssembly），只有落在
// GenTypes.IgnoredNamespaceNames 白名单（RimWorld/Verse/LudeonTK/…/System）或无命名空间
// 的类才按短名可达。放 Mwah 命名空间 = 整张天意表被静默丢弃（2026-09-30 实机首爆的教训，
// verify-local 有门钉住这条）。类名带 MWAH_ 前缀保证在共享命名空间里也不撞原版。
namespace RimWorld;

/// <summary>
/// 天意表的一行（纯数据，XML 驱动，见 1.6/Defs/Kiss/MWAH_FateDefs.xml）。
/// 分工是刻意的：**结构在 Defs，文字在翻译文件** —— 本行只说"发哪条心情、概率多重、
/// 用哪个键当旁白/短讯"，句子本身永远是 Keyed 键（双语齐备由 verify-local 把关）。
/// 概率 = weight ÷ 同 scope 全部有效行之和；weight 是相对值，不必凑 100。
/// thought 留空 = 这一行什么都不发（允许"无事发生"占概率位）；
/// narrationKey 写进 thought 实例（<see cref="Thought_MemoryFated"/>），
/// 于是每个 pawn 的每段记忆各自显示各自抽中的那句 —— 零补丁、零全局改动。
/// </summary>
public class MWAH_FateDef : Def
{
    /// <summary>作用域（MWAH_FateDef.scope）：KissWall、将来的 KissTree…… 每种亲吻各查各的表。XML 必填。</summary>
    public string? scope;

    /// <summary>发哪条心情记忆；空 = 不发。</summary>
    public ThoughtDef? thought;

    /// <summary>相对权重；≤0 的行视为坏数据，掷骰时跳过。</summary>
    public int weight = 1;

    /// <summary>心情条目描述位的旁白键（Keyed）。空 = 用 thought 自己的静态描述。</summary>
    public string? narrationKey;

    /// <summary>结算短讯键（Keyed）。空 = 不弹短讯（旁白仍进心情条目）。</summary>
    public string? messageKey;

    /// <summary>短讯的消息类型 def；空 = NeutralEvent。</summary>
    public MessageTypeDef? messageType;
}
