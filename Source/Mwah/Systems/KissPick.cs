using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 两步点选式导播的状态：底栏按钮进入，地图上点两个人，立刻派发。
/// 状态放静态类是因为 MainButtonWorker 与 GameComponent 是两个互不相干的实例，
/// 而"现在是不是在点选模式"全局只有一份。
/// </summary>
public static class KissPickMode
{
    public static bool Active;
    public static Pawn? First;

    public static void Begin()
    {
        Active = true;
        First = null;
    }

    public static void End()
    {
        Active = false;
        First = null;
    }
}

/// <summary>
/// 点选模式的绘制与取点。
///
/// 为什么挂在 GameComponentOnGUI 而不是自建窗口：窗口要覆盖全屏才收得到地图点击，
/// 而全屏窗口会连带吞掉底栏与殖民者栏的输入；GameComponentOnGUI 本来就在游戏 UI 层
/// 每事件跑一次，既能读鼠标，也能在有对话框压上来时让位。
/// 构造器签名必须是 (Game)：Game.FillComponents 用 Activator.CreateInstance(type, this)。
/// </summary>
public class KissPick : GameComponent
{
    public KissPick(Game game) : base()
    {
    }

    /// <summary>底栏那一条的高度：点在它上面属于正常 UI 操作，不抢。</summary>
    private const float BottomReserved = 40f;

    public override void GameComponentTick()
    {
        // 回原位请求在这里发放：上一个 tick 的 job 收尾栈已经退干净了。
        KissUtility.DrainReturns();
    }

    public override void GameComponentOnGUI()
    {
        if (!KissPickMode.Active)
        {
            return;
        }
        if (MwahMod.Settings != null && !MwahMod.Settings.DirectorEnabled)
        {
            // 设置页里把按钮关掉时，正在进行的点选也要一起结束。
            KissPickMode.End();
            return;
        }
        // 有对话框压上来时让位，但**不退出模式**：对话框一关立刻恢复点选。
        // 旧写法在这里直接 End()，于是任何常驻的 Dialog 层窗口（不少 mod 都留着一个）
        // 会让导演台"点了没反应"——刚 Begin 就被下一个事件关掉，永远走不到绘制。
        if (Find.WindowStack.NonImmediateDialogWindowOpen)
        {
            return;
        }

        Event e = Event.current;
        if (e.type == EventType.Repaint)
        {
            DrawHint();
            return;
        }
        if (e.type != EventType.MouseDown)
        {
            return;
        }
        if (e.button == 1)
        {
            KissPickMode.End();
            e.Use();
            return;
        }
        if (e.button != 0)
        {
            return;
        }
        // 光标压在底栏或任何窗口（含浮出菜单这类 ImmediateWindow）上时，那一下归原主。
        if (e.mousePosition.y > UI.screenHeight - BottomReserved
            || Find.WindowStack.GetWindowAt(e.mousePosition) != null)
        {
            return;
        }

        Pawn? picked = PickUnder();
        if (picked == null)
        {
            MwahLog.Dev("pick miss at " + UI.MouseMapPosition());
            return;
        }
        e.Use();

        if (KissPickMode.First == null)
        {
            KissPickMode.First = picked;
            MwahLog.Dev("pick 1: " + picked.LabelShort);
            return;
        }
        MwahLog.Dev("pick 2: " + picked.LabelShort);
        Dispatch(KissPickMode.First, picked);
        KissPickMode.End();
    }

    /// <summary>
    /// 取鼠标下的 pawn：与原版右键菜单同一支 GenUI.ThingsUnderMouse，
    /// 所以"狗站在人身上"这种叠格由原版决定优先级，不自造一套。
    /// </summary>
    private static Pawn? PickUnder()
    {
        // ThingsUnderMouse 拿点击位置去和 pawn.DrawPos 比距离，要的是**世界坐标**；
        // 传屏幕像素进去的话 IntVec3.FromVector3 会落到地图外，列表永远为空 —— 上一版就是这个错。
        List<Thing> under = GenUI.ThingsUnderMouse(UI.MouseMapPosition(), 0.8f, TargetingParameters.ForPawns());
        for (int i = 0; i < under.Count; i++)
        {
            if (under[i] is Pawn p && p.Spawned && !p.Dead)
            {
                return p;
            }
        }
        return null;
    }

    private static void Dispatch(Pawn a, Pawn b)
    {
        if (KissUtility.BeginDirected(a, b))
        {
            Messages.Message("MWAH.Director.Started".Translate(a.Named("PAWN"), b.Named("OTHER")),
                new LookTargets(a, b), MessageTypeDefOf.PositiveEvent, historical: false);
            return;
        }
        // 点选模式没有"灰按钮"这种状态，可行性只能在点完之后告知。
        Messages.Message(KissUtility.DirectPreview(a, b) ?? "MWAH.Fail.Busy".Translate(),
            new LookTargets(a, b), MessageTypeDefOf.RejectInput, historical: false);
    }

    private static void DrawHint()
    {
        string text = KissPickMode.First == null
            ? "MWAH.Director.PickFirst".Translate()
            : "MWAH.Director.PickSecond".Translate(KissPickMode.First.Named("PAWN"));
        Text.Font = GameFont.Small;
        Vector2 size = Text.CalcSize(text) + new Vector2(26f, 14f);
        GUI.Box(new Rect((UI.screenWidth - size.x) * 0.5f, 66f, size.x, size.y), text);
    }
}
