using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 主职：把上一 tick 攒下的"回原位"请求在正常 tick 阶段发放出去；兼职：每局开始时清零跨局静态状态。
///
/// 为什么需要它：toil 的 finish action 里直接 TryTakeOrderedJob 会在 job 收尾栈里同步起新 job，
/// 新 job 又反过来结束正在收尾的旧 job ⇒ 同 tick 无限递归（实测日志同文刷到折叠上限、进程直接消失）。
/// 排到下一个 tick、由这里发放，栈早就退干净了。1.6 没有 LateTickManager 之类的现成延迟队列，
/// 所以这个组件不是轮子，是最小的必要落点。选点原语在 KissDirector，面板在 Dialog_KissDirector。
/// 每局生命周期：Game.FillComponents 在建局与读档时都会重新实例化组件，构造器因此是
/// "每局必跑"的钩子。冷却表、回位队列、导演台代号/开关、面板静态引用都是 static，而 TicksGame 与
/// thingIDNumber 每局重新从 0 分配，不在这里清零就会让新局继承上一局的残留。
/// </summary>
public class KissTicker : GameComponent
{
    public KissTicker(Game game) : base()
    {
        KissCooldown.Reset();
        KissReturnQueue.Reset();
        KissDirector.Active = false;
        Dialog_KissDirector.ResetForNewGame();
    }

    public override void GameComponentTick()
    {
        KissReturnQueue.Drain();
        // 设置里关掉导演台时：正在点选的收掉，开着的面板也关掉（入口都看不见了）。
        if (!MwahMod.Settings.DirectorEnabled)
        {
            if (KissDirector.Active)
            {
                KissDirector.Stop();
            }
            Dialog_KissDirector.Current?.Close();
        }
    }

    /// <summary>
    /// 翻译自证：FinalizeInit 在所有 Def 与语言注入都加载完之后运行，这里读回引擎
    /// "实际解析到"的 label，直接区分两种失败：打出中文 ⇒ 注入生效（屏幕英文另有原因）；
    /// 打出英文 ⇒ 注入没套上（安装目录缺 DefInjected 子树，或 apply 时找不到 def）。
    /// 全构建都在（2026-09-30 起诊断改运行时开关）：输出走 MwahLog.Dev，关着就一行不发。
    /// </summary>
    public override void FinalizeInit()
    {
        base.FinalizeInit();
        MainButtonDef mb = DefDatabase<MainButtonDef>.GetNamedSilentFail("MWAH_KissDirector");
        ThoughtDef th = DefDatabase<ThoughtDef>.GetNamedSilentFail("MWAH_Kissed");
        string thoughtLabel = th?.stages != null && th.stages.Count > 0 ? th.stages[0].label.ToString() : "<none>";
        MwahLog.Note("i18n check: button='" + (mb?.LabelCap ?? "<none>") + "' thought='" + thoughtLabel + "'");
    }
}
