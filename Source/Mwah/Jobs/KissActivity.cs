using Verse;

namespace Mwah;

/// <summary>
/// 只读逻辑阶段：从原生执行进度（<c>CurToilIndex</c>，以及原版 job 自己的寿命标记 <c>ended</c>）
/// 加上模组的完成事实 <c>completed</c> **推导**出来的视图。
/// 它不是第二套状态机 —— 不进存档、没人写它、也不决定任何转移；权威永远是上面那三项本身。
/// 存在的理由只是给"现在算不算正在亲"一个能读出来的名字（KISS-09）。
/// </summary>
internal enum KissPhase
{
    /// <summary>不在 Mwah 亲吻工作里，或那场吻已经结束。</summary>
    None,

    /// <summary>已入场并正在接近（begin 与走位两个 toil；begin 是瞬时的，只占启动那一帧）。</summary>
    Approaching,

    /// <summary>表演中：发起方的倒计时可能正在走，也可能在等搭档到位。</summary>
    Performing,

    /// <summary>表演已经过场（倒计时走完或中途散场），可选返程仍在同一个 job 内 —— 不再占用亲吻资格。</summary>
    Departing,
}

/// <summary>
/// 亲吻状态的唯一口径：接近与表演算占用，完成之后的返程不算（KISS-09）。
/// 两个 driver 的阶段与工作报告、以及所有准入入口都从这同一条推导读，
/// 免得"正在亲"的几种问法各自解释 Toil 索引。角色（发起／被动）与结算仍归各自的 job。
/// </summary>
public static class KissActivity
{
    public static bool IsOccupied(Pawn pawn)
    {
        return pawn.jobs?.curDriver switch
        {
            JobDriver_Kiss pair => pair.IsKissing,
            JobDriver_KissThing thing => thing.IsKissing,
            _ => false,
        };
    }

    /// <summary>
    /// 两个 driver 共用的阶段推导，纯函数：不写任何字段，也不改 Toil 顺序。
    /// 完成事实优先于索引 —— 旧档可能把 <c>completed</c> 与索引分别存在"刚跨过表演 toil"的两侧，
    /// 此时按完成处理，与 <see cref="JobDriver_Kiss.IsKissing"/> 一直保住的 <c>!completed</c> 闸同判据。
    /// </summary>
    internal static KissPhase Phase(int curToilIndex, bool ended, bool completed, int performanceToilIndex)
    {
        if (ended)
        {
            return KissPhase.None;
        }
        if (completed)
        {
            return KissPhase.Departing;
        }
        if (curToilIndex < performanceToilIndex)
        {
            return KissPhase.Approaching;
        }
        return curToilIndex == performanceToilIndex ? KissPhase.Performing : KissPhase.Departing;
    }

    /// <summary>
    /// finish-action 的幂等闸（集中一处，两个 driver 共用）：第一次调用放行，之后一律静默。
    /// 置真只说明"收尾入口已经跑过"，**不说明奖励发出去了** —— 中途取消的吻同样会走到这里，
    /// 发奖条件仍是完成事实。保存键保持原样（<c>finished</c>），语义不变。
    /// </summary>
    internal static bool TryEnterFinishAction(ref bool finished)
    {
        if (finished)
        {
            return false;
        }
        finished = true;
        return true;
    }
}
