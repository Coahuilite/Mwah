using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 底栏按钮：点一下进入两步点选（走原版 Find.Targeter），再点一下取消。
///
/// 显隐走 <see cref="Visible"/> 覆写而不是 Def 里的 buttonVisible —— 后者是静态 XML 值，
/// 每帧求值的 Visible 才能跟着玩家的设置开关走（原版让"研究"按钮随进度出现也是这个路子）。
/// Disabled 仍交给基类：没有地图时它本来就该是灰的。
/// </summary>
public class MainButtonWorker_KissDirector : MainButtonWorker
{
    public override bool Visible => base.Visible && (MwahMod.Settings?.DirectorEnabled ?? true) && (MwahMod.Settings?.Enabled ?? true);

    public override void Activate()
    {
        KissDirector.Toggle();
    }
}
