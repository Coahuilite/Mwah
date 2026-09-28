using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 承载"每实例旁白"的心情记忆：原版心情列表（NeedsCardUtility.DrawThoughtGroup）读的是
/// thought **实例**的虚属性 Description —— Thought_FoodEaten 等原版子类就这么干 ——
/// 所以把抽中的旁白键存在实例字段里，即可让每个 pawn 的每段记忆各说各话，
/// 不打任何补丁、不碰共享的 ThoughtDef。
/// 读档后旁白原样回来（键随实例存盘）；键缺失（语言包被改坏）时 Translate 会显式露出
/// 键名 —— 那是可诊断的坏，比静默换文案好；字段整体为空则回退静态描述，即兜底失效形式。
/// </summary>
public class Thought_MemoryFated : Thought_Memory
{
    /// <summary>抽中的旁白 Keyed 键；空 = 走基类静态描述。</summary>
    public string? narrationKey;

    /// <summary>被亲目标的标签快照（墙不会跟着记忆走，存字符串最稳）。</summary>
    public string? narrationSubject;

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
