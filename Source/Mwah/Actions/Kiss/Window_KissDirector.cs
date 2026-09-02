using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻导演台：底栏按钮打开，从当前地图里**任选两个 pawn** 指定它们亲吻。
///
/// 为什么必须有它：右键菜单这条路对"玩家管不着的 pawn"永远不通 —— 原版
/// FloatMenuContext 构造时先按 CanTakeOrder 过滤，敌人连被选中都做不到。
/// 而本模组最宽档位的字面意思就是 every pawn kiss each other：玩家要能指定任意两个。
/// 零 Harmony 下唯一干净的入口就是自建窗口 —— 不碰下令链，直接对目标起 job。
///
/// 判定不另搞一套：仍走 KissUtility.Propose（门禁、参与层、主动层、冷却全部复用），
/// 只是"发起方"从"你选中的那位"变成"你点名的这两位"。
/// </summary>
public class Window_KissDirector : MainTabWindow
{
    private const float RowHeight = 28f;
    private const float NameWidth = 200f;
    private const float KindWidth = 140f;
    private const float FactionWidth = 110f;
    private const float ButtonWidth = 72f;
    private const float GoWidth = 168f;

    private Pawn? doer;
    private Pawn? receiver;
    private string filter = "";
    private Vector2 scroll;

    public override Vector2 RequestedTabSize => new Vector2(720f, 620f);

    public override void DoWindowContents(Rect inRect)
    {
        DropInvalidSelection();
        Map? map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }

        Rect inner = inRect.ContractedBy(12f);

        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inner.x, inner.y, inner.width, 28f), "MWAH.Director.Title".Translate());
        Text.Font = GameFont.Small;

        float y = inner.y + 30f;
        DrawSlots(new Rect(inner.x, y, inner.width, 62f));
        y += 70f;

        DrawFilter(new Rect(inner.x, y, inner.width, 28f), map);
        y += 34f;

        DrawList(new Rect(inner.x, y, inner.width, inner.yMax - y), map);
    }

    /// <summary>两个槽位 + 执行按钮。按钮旁就是判定结果：不行就写清为什么。</summary>
    private void DrawSlots(Rect r)
    {
        float half = (r.width - GoWidth - 12f) * 0.5f;
        DrawSlot(new Rect(r.x, r.y, half, r.height), "MWAH.Director.Doer".Translate(), doer, () => doer = null);
        DrawSlot(new Rect(r.x + half + 8f, r.y, half, r.height), "MWAH.Director.Receiver".Translate(), receiver, () => receiver = null);

        bool ready = doer != null && receiver != null;
        string? reason = ready ? KissUtility.DirectPreview(doer!, receiver!) : "MWAH.Director.NeedBoth".Translate();

        Rect go = new Rect(r.xMax - GoWidth, r.y + 4f, GoWidth, 28f);
        bool enabled = GUI.enabled;
        GUI.enabled = ready && reason == null;
        if (Widgets.ButtonText(go, "MWAH.Director.Go".Translate()) && ready)
        {
            if (KissUtility.BeginDirected(doer!, receiver!))
            {
                Messages.Message("MWAH.Director.Started".Translate(doer!.Named("PAWN"), receiver!.Named("OTHER")),
                    new LookTargets(doer, receiver), MessageTypeDefOf.PositiveEvent, historical: false);
            }
        }
        GUI.enabled = enabled;

        Rect statusRect = new Rect(go.x - 260f, go.yMax + 2f, 256f, 44f);
        Color before = GUI.contentColor;
        if (reason != null)
        {
            GUI.contentColor = Color.gray;
        }
        Widgets.Label(statusRect, reason ?? "MWAH.Director.Ready".Translate());
        GUI.contentColor = before;
    }

    private void DrawSlot(Rect r, string label, Pawn? pawn, System.Action clear)
    {
        GUI.Box(r, GUIContent.none);
        Rect pad = r.ContractedBy(6f);
        Widgets.Label(new Rect(pad.x, pad.y, pad.width, 18f), label);
        if (pawn == null)
        {
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(pad.x, pad.y + 20f, pad.width, 18f), "MWAH.Director.Empty".Translate());
            Text.Font = GameFont.Small;
            return;
        }
        Widgets.Label(new Rect(pad.x, pad.y + 19f, pad.width - 26f, 18f), Describe(pawn));
        if (Widgets.ButtonText(new Rect(pad.xMax - 22f, pad.y + 19f, 22f, 18f), "X"))
        {
            clear();
        }
    }

    private void DrawFilter(Rect r, Map map)
    {
        Widgets.Label(new Rect(r.x, r.y, 52f, r.height), "MWAH.Director.Filter".Translate());
        filter = Widgets.TextField(new Rect(r.x + 56f, r.y, r.width - 200f, r.height), filter);
        string count = "MWAH.Director.Count".Translate(MatchesFilterCount(map), map.mapPawns.AllPawnsSpawned.Count);
        Widgets.Label(new Rect(r.xMax - 138f, r.y, 138f, r.height), count);
    }

    private void DrawList(Rect r, Map map)
    {
        IReadOnlyList<Pawn> all = map.mapPawns.AllPawnsSpawned;
        float rowWidth = r.width - 16f;
        float contentHeight = MatchesFilterCount(map) * RowHeight + 4f;
        var viewRect = new Rect(0f, 0f, rowWidth, Mathf.Max(contentHeight, r.height));
        Widgets.BeginScrollView(r, ref scroll, viewRect);

        float y = 0f;
        for (int i = 0; i < all.Count; i++)
        {
            Pawn p = all[i];
            if (Matches(p))
            {
                Rect row = new Rect(0f, y, rowWidth, RowHeight);
                // 视口外的行不画也不排版：模组多的档地图上几百个 pawn，每帧全画是白烧。
                if (row.yMax > scroll.y - RowHeight && row.y < scroll.y + r.height)
                {
                    DrawRow(row, p);
                }
            }
            y += RowHeight;
        }
        Widgets.EndScrollView();
    }

    private void DrawRow(Rect r, Pawn p)
    {
        if (p == doer || p == receiver)
        {
            Color before = GUI.color;
            GUI.color = new Color(1f, 1f, 0.6f, 0.25f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = before;
        }

        Widgets.Label(new Rect(r.x + 4f, r.y, NameWidth, r.height), p.LabelShortCap);
        Widgets.Label(new Rect(r.x + NameWidth + 8f, r.y, KindWidth, r.height), p.KindLabel);
        string faction = p.Faction == null ? "—" : p.Faction.Name;
        Widgets.Label(new Rect(r.x + NameWidth + KindWidth + 12f, r.y, FactionWidth, r.height), faction);

        // ♥ = 有心情系统，亲了会有收益；— = 只有动作。这是运行时事实，与门禁档位无关。
        bool hasMood = KissBoundary.HasMood(p);
        Text.Anchor = TextAnchor.MiddleCenter;
        Color moodColor = GUI.contentColor;
        GUI.contentColor = hasMood ? moodColor : Color.gray;
        Widgets.Label(new Rect(r.x + NameWidth + KindWidth + FactionWidth + 16f, r.y, 24f, r.height), hasMood ? "♥" : "—");
        GUI.contentColor = moodColor;
        Text.Anchor = TextAnchor.UpperLeft;

        float bx = r.xMax - ButtonWidth * 2f - 8f;
        if (Widgets.ButtonText(new Rect(bx, r.y + 2f, ButtonWidth, r.height - 4f), "MWAH.Director.SetDoer".Translate()))
        {
            doer = p;
        }
        if (Widgets.ButtonText(new Rect(bx + ButtonWidth + 8f, r.y + 2f, ButtonWidth, r.height - 4f), "MWAH.Director.SetReceiver".Translate()))
        {
            receiver = p;
        }
    }

    private static string Describe(Pawn p) =>
        p.LabelShortCap + "  " + p.KindLabel + "  " + (KissBoundary.HasMood(p) ? "♥" : "—");

    private bool Matches(Pawn p)
    {
        if (filter.NullOrEmpty())
        {
            return true;
        }
        string needle = filter.ToLowerInvariant();
        return (p.LabelShortCap != null && p.LabelShortCap.ToLowerInvariant().Contains(needle))
            || (p.KindLabel != null && p.KindLabel.ToLowerInvariant().Contains(needle))
            || p.def.defName.ToLowerInvariant().Contains(needle)
            || (p.Faction != null && p.Faction.Name != null && p.Faction.Name.ToLowerInvariant().Contains(needle));
    }

    private int MatchesFilterCount(Map map)
    {
        if (filter.NullOrEmpty())
        {
            return map.mapPawns.AllPawnsSpawned.Count;
        }
        int n = 0;
        IReadOnlyList<Pawn> all = map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < all.Count; i++)
        {
            if (Matches(all[i]))
            {
                n++;
            }
        }
        return n;
    }

    /// <summary>选中的 pawn 可能中途死掉或被移除，每帧先清掉失效引用。</summary>
    private void DropInvalidSelection()
    {
        if (doer != null && (doer.Destroyed || !doer.Spawned))
        {
            doer = null;
        }
        if (receiver != null && (receiver.Destroyed || !receiver.Spawned))
        {
            receiver = null;
        }
    }
}
