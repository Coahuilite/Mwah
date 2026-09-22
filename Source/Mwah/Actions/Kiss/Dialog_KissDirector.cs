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
[StaticConstructorOnStartup] // 静态 Texture2D 字段会被原版启动扫描器点名（ReportProbablyMissingAttributes 只认 attribute，不认惰性加载）
public class Dialog_KissDirector : Window
{
    /// <summary>布局基准逻辑高（1080 参考系）。k 上下一夹：小屏不至于不可读，大屏不至于过大。</summary>
    private const float RefHeight = 1080f;
    private const float MinScale = 0.55f;
    private const float MaxScale = 1.35f;

    private enum PickSlot : byte { None, Left, Right }

    private Pawn? pawnA;
    private Thing? targetB; // 右槽：pawn 或墙（导演台也能点墙）
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
        // 基类默认 true：只要窗口在 WindowStack 里，CameraDriver 的
        // AnythingPreventsCameraMotion 就整体掐掉方向键平移与滚轮缩放。
        // 这是块常驻式非模态面板，相机必须照常能动。
        preventCameraMotion = false;
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
    // 文案字号不跟着 k 缩（Tiny 恒为 16 逻辑像素），所以 caption 框宽要有"一行中文"的硬下限；
    // 否则小 k（1024×768 实测）下文字折行溢出，戳出窗沿、撞上快速发配按钮。
    private static float CaptionWidth => Mathf.Max(SlotSize + Gap, 122f);
    private static float CaptionHeight => 46f; // 两行 Tiny：长名字换行也接得住
    private static float ButtonHeight => 30f * Scale;
    // 基类 Window.OnGUI：optionalTitle 非空时内容矩形再让出 Margin + 25f（反编译 rect3.yMin +=
    // Margin + 25f 实证）。高度公式漏掉这一行，底部按钮整体上移压进 caption 第二行 —— 图证的真正根因。
    private float TitleRow => Margin + 25f; // Window.Margin 是实例字段，不能进静态成员

    public override Vector2 InitialSize
    {
        get
        {
            float w = CaptionWidth * 2f + HeartSize + Gap * 2f + Margin * 2f;
            float h = TitleRow + SlotSize + 4f * Scale + CaptionHeight + Gap + ButtonHeight + Margin * 2f;
            // 不再按"屏宽 62%"回缩：caption 宽度已是不随 k 的下限，整块面板最宽约 350 逻辑像素，
            // 连 UIScale 2 的 512 逻辑宽画布都放得下；旧的比例回缩反而破坏垂直节奏（标题行/定高
            // caption 都不跟着缩），是上一版遮挡的帮凶。
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
        if (!IsAlive(targetB))
        {
            targetB = null;
        }

        float slot = SlotSize;
        float top = inRect.yMin;
        Rect left = new Rect(inRect.xMin, top, slot, slot);
        Rect right = new Rect(inRect.xMax - slot, top, slot, slot);
        Rect heart = new Rect(inRect.center.x - HeartSize / 2f, top + (slot - HeartSize) / 2f, HeartSize, HeartSize);
        Rect leftCaption = new Rect(inRect.xMin, left.yMax + 4f * Scale, CaptionWidth, CaptionHeight);
        Rect rightCaption = new Rect(inRect.xMax - CaptionWidth, right.yMax + 4f * Scale, CaptionWidth, CaptionHeight);
        Rect quick = new Rect(inRect.center.x - inRect.width * 0.28f, top + inRect.height - ButtonHeight,
            inRect.width * 0.56f, ButtonHeight);

        DrawSlot(left, pawnA, active: picking == PickSlot.Left, isLeft: true);
        DrawSlot(right, targetB, active: picking == PickSlot.Right, isLeft: false);
        DrawHeart(heart);
        DrawCaption(leftCaption, pawnA == null
            ? "MWAH.Director.PickFirst".Translate()
            : "MWAH.Director.LeftPicked".Translate(pawnA.Named("PAWN")));
        DrawCaption(rightCaption, pawnA == null
            ? "MWAH.Director.RightWait".Translate()
            : targetB == null
                ? "MWAH.Director.PickSecond".Translate(pawnA.Named("PAWN"))
                : "MWAH.Director.PairDone".Translate(pawnA.Named("PAWN"), targetB.Named("OTHER")));
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

    private static bool IsAlive(Thing? thing) => thing switch
    {
        null => false,
        Pawn pawn => IsLive(pawn),
        _ => thing.Spawned && !thing.Destroyed,
    };

    private void DrawSlot(Rect rect, Thing? thing, bool active, bool isLeft)
    {
        if (!active)
        {
            Widgets.DrawHighlightIfMouseover(rect);
        }
        Rect inner = rect.ContractedBy(3f * Scale);
        if (thing is Pawn pawn)
        {
            GUI.DrawTexture(inner, PortraitsCache.Get(pawn, inner.size * 1.25f, Rot4.North));
        }
        else if (thing != null)
        {
            // 墙：uiIcon 在 ResolveReferences 里保证填充（无 iconPath 时退到材质主纹理），
            // 按原版惯例乘 uiIconColor 再 ScaleToFit。
            GUI.color = thing.def.uiIconColor;
            GUI.DrawTexture(inner, thing.def.uiIcon ?? Texture2D.whiteTexture, ScaleMode.ScaleToFit);
            GUI.color = Color.white;
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
            else if (Event.current.button == 1 && thing != null && (isLeft || pawnA != null))
            {
                if (isLeft)
                {
                    pawnA = null;
                    targetB = null; // 换/清发起方后原来的对象不再有指代，整个右半归零
                }
                else
                {
                    targetB = null;
                }
            }
            Event.current.Use();
        }
    }

    private void DrawHeart(Rect rect)
    {
        bool leftReady = pawnA != null;
        bool rightReady = targetB != null;
        bool ready = leftReady && rightReady;
        heartTex ??= ContentFinder<Texture2D>.Get("Things/Mote/Heart", reportFailure: false);
        Texture2D tex = heartTex ?? Texture2D.whiteTexture;
        // 半心填充（用户设计）：底图整颗灰；选了一边就把那一半盖回原色，两边都选上即整颗红
        // = 可派发的高亮态。
        GUI.color = new Color(0.42f, 0.42f, 0.42f, 0.9f);
        GUI.DrawTexture(rect, tex);
        float half = rect.width / 2f;
        GUI.color = Color.white;
        if (leftReady)
        {
            // 该 Unity 版本没有带 sourceRect 的 DrawTexture 重载：整颗画出来、用裁剪只露左半。
            GUI.BeginClip(new Rect(rect.x, rect.y, half, rect.height));
            GUI.DrawTexture(new Rect(0f, 0f, rect.width, rect.height), tex);
            GUI.EndClip();
        }
        if (rightReady)
        {
            GUI.BeginClip(new Rect(rect.x + half, rect.y, half, rect.height));
            GUI.DrawTexture(new Rect(-half, 0f, rect.width, rect.height), tex);
            GUI.EndClip();
        }
        if (Mouse.IsOver(rect))
        {
            GUI.tooltip = ready
                ? "MWAH.Director.HeartReady".Translate(pawnA!.Named("PAWN"), targetB!.Named("OTHER"))
                : "MWAH.Director.HeartWaiting".Translate();
        }
        if (ready && Event.current.type == EventType.MouseDown && Event.current.button == 0
            && rect.Contains(Event.current.mousePosition))
        {
            Event.current.Use();
            KissDirector.Dispatch(pawnA!, targetB!);
            // 决定：派发后不清槽。成对冷却自动把下一次点击变成带原因的弹信，槽位保留即"还是这俩"。
        }
    }

    private static void DrawCaption(Rect rect, string text)
    {
        GameFont fontBefore = Text.Font;
        Text.Anchor = TextAnchor.UpperCenter;
        Text.Font = GameFont.Tiny;
        // WordWrap 的原版契约是"帧末必须为 true"（Verse.Text 帧末哨兵会 ErrorOnce）。
        // 这里本来就想要折行，画完把 Tiny/锚点还原即可，别碰 WordWrap。
        GUI.Label(rect, text);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = fontBefore;
    }

    /// <summary>点框 = 进该槽的单段点选；换左发起方会清空右框（文案的指代前提变了）。
    /// 右槽收 pawn 也收墙（墙要开着亲墙开关、不在迷雾里 —— 过滤在 KissDirector 的取点参数里）。</summary>
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
        if (right)
        {
            KissDirector.BeginPickThing(
                thing =>
                {
                    targetB = thing;
                    picking = PickSlot.None;
                },
                t => t != first,
                prompt);
            return;
        }
        KissDirector.BeginPick(
            pawn =>
            {
                pawnA = pawn;
                targetB = null;
                picking = PickSlot.None;
            },
            null,
            prompt);
    }

    /// <summary>快速发配：链式两段地图点选、第二段完成即派发（现导演台行为），结果同步回头像槽。</summary>
    private void QuickSend()
    {
        KissDirector.QuickChain((a, b) =>
        {
            pawnA = a;
            targetB = b;
        });
    }
}
