using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// "每实例旁白"的载体契约：心情记忆自己记住抽中了哪句。类人版有两个 thoughtClass
/// （纯心情 <see cref="Thought_MemoryFated"/> 与带意见的 <see cref="Thought_MemorySocialFated"/>），
/// 结算侧（KissMoodReward / KissFate）只认这个接口，不关心变体。
/// </summary>
public interface IFatedNarration
{
    public string? NarrationKey { get; set; }

    public string? NarrationSubject { get; set; }
}

/// <summary>
/// 承载"每实例旁白"的心情记忆：原版心情列表（NeedsCardUtility.DrawThoughtGroup）读的是
/// thought **实例**的虚属性 Description —— Thought_FoodEaten 等原版子类就这么干 ——
/// 所以把抽中的旁白键存在实例字段里，即可让每个 pawn 的每段记忆各说各话，
/// 不打任何补丁、不碰共享的 ThoughtDef。
/// 读档后旁白原样回来（键随实例存盘）；键缺失（语言包被改坏）时 Translate 会显式露出
/// 键名 —— 那是可诊断的坏，比静默换文案好；字段整体为空则回退静态描述，即兜底失效形式。
/// 字段是唯一存储、属性只做转发（Thought 没有 PostExposeData 钩子，Scribe 必须直接 Look 字段）。
/// </summary>
public class Thought_MemoryFated : Thought_Memory, IFatedNarration
{
    private string? narrationKey;

    private string? narrationSubject;

    public string? NarrationKey { get => narrationKey; set => narrationKey = value; }

    public string? NarrationSubject { get => narrationSubject; set => narrationSubject = value; }

    public override string Description
    {
        get
        {
            if (narrationKey.NullOrEmpty() || narrationSubject.NullOrEmpty() || pawn == null)
            {
                return base.Description;
            }
            return narrationKey.Translate(pawn.Named("PAWN"), narrationSubject.Named("WALL"));
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref narrationKey, "MWAH_narrationKey");
        Scribe_Values.Look(ref narrationSubject, "MWAH_narrationSubject");
    }
}

/// <summary>
/// 带意见变体（"亲吻留下痕迹"开着时使用）：同一套旁白字段与 Description 覆写，
/// 挂在 Thought_MemorySocial 上。两份三行样板是 C# 单继承的税，逻辑口径仍只有一处。
/// </summary>
public class Thought_MemorySocialFated : Thought_MemorySocial, IFatedNarration
{
    private string? narrationKey;

    private string? narrationSubject;

    public string? NarrationKey { get => narrationKey; set => narrationKey = value; }

    public string? NarrationSubject { get => narrationSubject; set => narrationSubject = value; }

    public override string Description
    {
        get
        {
            if (narrationKey.NullOrEmpty() || narrationSubject.NullOrEmpty() || pawn == null)
            {
                return base.Description;
            }
            return narrationKey.Translate(pawn.Named("PAWN"), narrationSubject.Named("WALL"));
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref narrationKey, "MWAH_narrationKey");
        Scribe_Values.Look(ref narrationSubject, "MWAH_narrationSubject");
    }
}
