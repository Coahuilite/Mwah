using Verse;

namespace Mwah;

/// <summary>
/// 只干一件事：把上一 tick 攒下的"回原位"请求在正常 tick 阶段发放出去。
///
/// 为什么需要它：toil 的 finish action 里直接 TryTakeOrderedJob 会在 job 收尾栈里同步起新 job，
/// 新 job 又反过来结束正在收尾的旧 job ⇒ 同 tick 无限递归（实测日志同文刷到折叠上限、进程直接消失）。
/// 排到下一个 tick、由这里发放，栈早就退干净了。1.6 没有 LateTickManager 之类的现成延迟队列，
/// 所以这个组件不是轮子，是最小的必要落点。选点本身在 KissDirector，走原版 Find.Targeter。
/// </summary>
public class KissTicker : GameComponent
{
    public KissTicker(Game game) : base()
    {
    }

    public override void GameComponentTick()
    {
        KissUtility.DrainReturns();
        // 设置里关掉导演台时，正在进行的选点也一起结束（与旧行为一致）。
        if (KissDirector.Active && MwahMod.Settings != null && !MwahMod.Settings.DirectorEnabled)
        {
            KissDirector.Stop();
        }
    }
}
