using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻导演台面板：左头像框=谁发起，右头像框=亲谁，中间心形=派发，下面「快速发配」=不碰头像框、
/// 直接进地图两段点选并自动派发（选完仍回显到头像框）。
///
/// 三段式布局与坐标全部从 UI.screenWidth/screenHeight 推导，没有一个写死的绝对像素、
/// 也不读 Screen.width —— 1.6 的 UI 缩放是 Verse.UI.ApplyUIScale 把整个 GUI 矩阵乘上
/// Prefs.UIScale，逻辑画布 = 物理分辨率 ÷ UIScale，窗口坐标都在这个逻辑空间里。
/// 读物理值或用固定像素，在 2560×1440@100% 与 1920×1080@200% 下就不是同一个面板。
/// 缩放/分辨率变化时 WindowStack 会调 Notify_ResolutionChanged：没拖动过就按锚点重摆，
/// 拖动过就按旧逻辑画布等比映射再夹回屏内。
///
/// 非模态三件套：forcePause=false、absorbInputAroundWindow=false、layer=GameUI —— 面板开着
/// 地图照常右键框选缩放。点选依旧走原版 Find.Targeter（在窗口之后处理：点面板=操作面板，
/// 点地图=取点），这里只是状态显示与入口，和手搓输入的老版本是两回事。
/// Esc 刻意不关面板（closeOnCancel=false）：点选中 Esc 的含义是取消选人，归 Targeter。
///
/// 面板里的 pawn 引用是实例字段、随窗口死；静态只剩一个 Current 打开标记，
/// 由 KissTicker 的 (Game) 构造器按局清零（static 寿命是进程，不是局 —— 见 MEMORY）。
/// </summary>
public class Dialog_KissDirector : Window
{
    /// <summary>布局基准逻辑高（1080 参考系）。k 上下一夹：小屏不至于不可读，大屏不至于过大。</summary>
    private const float RefHeight = 1080f;
    private const float MinScale = 0.55f;
    private const float MaxScale = 1.35f;

    private enum PickSlot : byte { None, Left, Right }

    private Pawn? pawnA;
    private Pawn? pawnB;
    private PickSlot picking;
    private bool userMoved;
    private Rect lastAutoRect;
    private Vector2 lastCanvas;

    private static Texture2D? heartTex;

    /// <summary>当前打开的面板；null=没开。底栏按钮的开/关判据。</summary>
    public static Dialog_KissDirector? Current;

    private static float Scale => Mathf.Clamp((float)UI.screenHeight / RefHeight, MinScale, MaxScale);
    public Dialog_KissDirector()
    {
        Current = this;
        doCloseX = true;
        draggable = true;
        resizeable = false;
        forcePause = false;
        absorbInputAroundWindow = false;
        closeOnCancel = false;
        layer = WindowLayer.GameUI;
        optionalTitle = "MWAH.Director.Title".Translate();
        lastCanvas = new Vector2(UI.screenWidth, UI.screenHeight);
    }

    /// <summary>底栏按钮入口：没开就开，开着就关。</summary>
    public static void OpenOrClose()
    {
        if (Current != null)
        {
            Current.Close();
        }
        else
        {
            Find.WindowStack.Add(new Dialog_KissDirector());
        }
    }

    public override void PostClose()
    {
        // 关窗时还挂在点选里就把 Targeter 收掉，别留下看不见入口的拾取模式。
        if (picking != PickSlot.None)
        {
            KissDirector.Stop();
        }
        Current = null;
        base.PostClose();
    }

    /// <summary>每局清零（KissTicker 构造器调）：窗口本身随 WindowStack 与旧局同灭，这里只断静态引用。</summary>
    public static void ResetForNewGame()
    {
        Current = null;
    }

    // ---- 尺寸与位置：全部派生自逻辑画布 ------------------------------------

    private static float SlotSize => 88f * Scale;
    private static float HeartSize => 62f * Scale;
    private static float Gap => 18f * Scale;
    private static float CaptionHeight => 40f * Scale;
    private static float ButtonHeight => 30f * Scale;

    public override Vector2 InitialSize
    {
        get
        {
            float w = SlotSize * 2f + Gap * 2f + HeartSize + Margin * 2f;
            float h = SlotSize + CaptionHeight + ButtonHeight + Gap * 2f + Margin * 2f;
            // 兜底：极小逻辑画布（高分屏高 UIScale）下不超过约束屏宽的 62%。
            float maxW = UI.screenWidth * 0.62f;
            if (w > maxW)
            {
                h *= maxW / w;
                w = maxW;
            }
            return new Vector2(w, h);
        }
    }

    /// <summary>默认锚点：水平居中，坐在底栏上方。基类 SetInitialSizeAndPosition 居中于屏，这里换成贴底。</summary>
    protected override void SetInitialSizeAndPosition()
    {
        Vector2 size = InitialSize;
        float x = ((float)UI.screenWidth - size.x) / 2f;
        float y = (float)UI.screenHeight - size.y - MainButtonDef.ButtonHeight - 6f * Scale;
        windowRect = new Rect(x, y, size.x, size.y).Rounded();
        lastAutoRect = windowRect;
    }

    /// <summary>分辨率/UIScale 变了（WindowStack 会逐个喊 Notify_ResolutionChanged，面板每帧也比对兜底）：
    /// 没拖过 → 按锚点重摆；拖过 → 按旧画布的相对位置等比映射，再夹回屏内。</summary>
    public override void Notify_ResolutionChanged()
    {
        Vector2 canvas = new Vector2(UI.screenWidth, UI.screenHeight);
        Vector2 size = InitialSize;
        if (!userMoved)
        {
            SetInitialSizeAndPosition();
        }
        else
        {
            float fx = (float)windowRect.x / Mathf.Max(1f, lastCanvas.x);
            float fy = (float)windowRect.y / Mathf.Max(1f, lastCanvas.y);
            float x = Mathf.Clamp(fx * canvas.x, 0f, canvas.x - size.x);
            float y = Mathf.Clamp(fy * canvas.y, 0f, canvas.y - size.y);
            windowRect = new Rect(x, y, size.x, size.y).Rounded();
            lastAutoRect = windowRect;
        }
        lastCanvas = canvas;
    }

    // ---- 绘制与交互 ----------------------------------------------------------

    public override void DoWindowContents(Rect inRect)
    {
        Vector2 canvas = new Vector2(UI.screenWidth, UI.screenHeight);
        if (canvas != lastCanvas)
        {
            Notify_ResolutionChanged();
        }
        // 基类对 draggable 的窗口整面挂 GUI.DragWindow() —— 和上次自动位置一比就知道被没被拖走。
        if (!userMoved && (windowRect.position - lastAutoRect.position).sqrMagnitude > 1f)
        {
            userMoved = true;
        }
        if (picking != PickSlot.None && !KissDirector.Active)
        {
            picking = PickSlot.None; // 点选被右键/Esc 取消，或被外部收掉
        }
        if (!IsLive(pawnA))
        {
            pawnA = null;
        }
        if (!IsLive(pawnB))
        {
            pawnB = null;
        }

        float slot = SlotSize;
        float top = inRect.yMin;
        Rect left = new Rect(inRect.xMin, top, slot, slot);
        Rect right = new Rect(inRect.xMax - slot, top, slot, slot);
        Rect heart = new Rect(inRect.center.x - HeartSize / 2f, top + (slot - HeartSize) / 2f, HeartSize, HeartSize);
        Rect leftCaption = new Rect(left.x - Gap / 2f, left.yMax + 4f * Scale, left.width + Gap, CaptionHeight);
        Rect rightCaption = new Rect(right.x - Gap / 2f, right.yMax + 4f * Scale, right.width + Gap, CaptionHeight);
        Rect quick = new Rect(inRect.center.x - inRect.width * 0.28f, top + inRect.height - ButtonHeight,
            inRect.width * 0.56f, ButtonHeight);

        DrawSlot(left, pawnA, active: picking == PickSlot.Left, isLeft: true);
        DrawSlot(right, pawnB, active: picking == PickSlot.Right, isLeft: false);
        DrawHeart(heart);
        DrawCaption(leftCaption, pawnA == null
            ? "MWAH.Director.PickFirst".Translate()
            : "MWAH.Director.LeftPicked".Translate(pawnA.Named("PAWN")));
        DrawCaption(rightCaption, pawnA == null
            ? "MWAH.Director.RightWait".Translate()
            : pawnB == null
                ? "MWAH.Director.PickSecond".Translate(pawnA.Named("PAWN"))
                : "MWAH.Director.PairDone".Translate(pawnA.Named("PAWN"), pawnB.Named("OTHER")));
        if (Widgets.ButtonText(quick, "MWAH.Director.QuickSend".Translate()))
        {
            QuickSend();
        }
        if (Mouse.IsOver(quick))
        {
            GUI.tooltip = "MWAH.Director.QuickSendDesc".Translate();
        }
    }

    private static bool IsLive(Pawn? pawn) => pawn is { Dead: false, Spawned: true };

    private void DrawSlot(Rect rect, Pawn? pawn, bool active, bool isLeft)
    {
        if (!active)
        {
            Widgets.DrawHighlightIfMouseover(rect);
        }
        Rect inner = rect.ContractedBy(3f * Scale);
        if (pawn != null)
        {
            GUI.DrawTexture(inner, PortraitsCache.Get(pawn, inner.size * 1.25f, Rot4.North));
        }
        else
        {
            GUI.DrawTexture(inner, Texture2D.blackTexture);
            GameFont fontBefore = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            GUI.Label(rect, "?");
            Text.Font = fontBefore;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        Widgets.DrawBoxSolidWithOutline(rect, active ? new Color(0.2f, 0.35f, 0.55f, 0.45f) : new Color(0f, 0f, 0f, 0.55f),
            active ? new Color(0.5f, 0.75f, 1f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f));

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.button == 0)
            {
                StartPick(right: !isLeft);
            }
            else if (Event.current.button == 1 && pawn != null && (isLeft || pawnA != null))
            {
                if (isLeft)
                {
                    pawnA = null;
                    pawnB = null; // 换/清发起方后原来的对象不再有指代，整个右半归零
                }
                else
                {
                    pawnB = null;
                }
            }
            Event.current.Use();
        }
    }

    private void DrawHeart(Rect rect)
    {
        bool ready = pawnA != null && pawnB != null;
        heartTex ??= ContentFinder<Texture2D>.Get("Things/Mote/Heart", reportFailure: false);
        GUI.color = ready ? Color.white : new Color(0.42f, 0.42f, 0.42f, 0.9f);
        GUI.DrawTexture(rect, heartTex ?? Texture2D.whiteTexture);
        GUI.color = Color.white;
        if (Mouse.IsOver(rect))
        {
            GUI.tooltip = ready
                ? "MWAH.Director.HeartReady".Translate(pawnA!.Named("PAWN"), pawnB!.Named("OTHER"))
                : "MWAH.Director.HeartWaiting".Translate();
        }
        if (ready && Event.current.type == EventType.MouseDown && Event.current.button == 0
            && rect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            KissDirector.Dispatch(pawnA!, pawnB!);
            // 决定：派发后不清槽。成对冷却自动把下一次点击变成带原因的弹信，槽位保留即"还是这俩"。
        }
    }

    private static void DrawCaption(Rect rect, string text)
    {
        GameFont fontBefore = Text.Font;
        Text.Anchor = TextAnchor.UpperCenter;
        Text.Font = GameFont.Tiny;
        Text.WordWrap = true; // 名字再长也只是多占一行，不裁字 —— 槽宽本来就只有逻辑宽的零头
        GUI.Label(rect, text);
        Text.WordWrap = false;
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = fontBefore;
    }

    /// <summary>点头像框 = 进该槽的单段点选；换左发起方会清空右框（文案的指代前提变了）。</summary>
    private void StartPick(bool right)
    {
        if (right && pawnA == null)
        {
            return; // 占位文案已经说明先选左，点右不做声
        }
        Pawn? first = pawnA;
        picking = right ? PickSlot.Right : PickSlot.Left;
        string prompt = right
            ? "MWAH.Director.PickSecond".Translate(first!.Named("PAWN"))
            : "MWAH.Director.PickFirst".Translate();
        KissDirector.BeginPick(
            pawn =>
            {
                if (right)
                {
                    pawnB = pawn;
                }
                else
                {
                    pawnA = pawn;
                    pawnB = null;
                }
                picking = PickSlot.None;
            },
            right ? (Func<Pawn, bool>)(p => p != first) : null,
            prompt);
    }

    /// <summary>快速发配：链式两段地图点选、第二段完成即派发（现导演台行为），结果同步回头像槽。</summary>
    private void QuickSend()
    {
        KissDirector.QuickChain((a, b) =>
        {
            pawnA = a;
            pawnB = b;
        });
    }
}
