using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 底栏按钮的行为：点一下进入两步点选，再点一下取消。
///
/// 不覆写 Visible/Disabled：基类已经处理了"没有地图时置灰"（validWithoutMap=false）
/// 与 buttonVisible 两件事，多覆写一层只会让将来的门禁条件少一个明确的落点。
/// </summary>
public class MainButtonWorker_KissDirector : MainButtonWorker
{
    public override void Activate()
    {
        if (KissPickMode.Active)
        {
            KissPickMode.End();
            return;
        }
        KissPickMode.Begin();
    }
}
