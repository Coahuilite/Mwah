#if MWAH_DEV
using System;
using System.IO;
using System.Threading;
using Verse;

namespace Mwah;

/// <summary>
/// 旁路阶段采样器（仅 dev 构建编译进来）。
///
/// 为什么需要它：dnSpy 一类 .NET 调试器 attach 走 ICorDebug，只认 CLR，
/// 对 RimWorld 的 MonoBleedingEdge 拿不到托管栈；Mono 软调试器又要求启动时带
/// debugger-agent 参数。而"卡住一会儿然后崩"这种故障，崩溃转储里只有原生帧，
/// 托管侧发生了什么一行都不剩。
///
/// 做法是最土也最稳的：一个后台线程每秒把主线程写好的阶段戳记抄进独立文件。
/// 主线程卡死 ⇒ 文件停在最后一行，行与行的时间戳间隔就是卡死时刻；
/// 主线程崩溃 ⇒ 文件尾巴就是崩溃前最后到达的阶段。两条都无需任何调试器。
/// 采样线程只读整数字段与 TickManager 引用，不调任何 Unity 主线程 API。
/// </summary>
public static class KissTrace
{
    private const int SampleMs = 1000;

    private const long MaxBytes = 1 << 20;

    private const int TailLines = 800;

    private static readonly object Gate = new object();

    private static string? path;

    private static Timer? timer;

    /// <summary>发起方当前阶段；由主线程在 toil 切换处写，采样线程只读。</summary>
    public static string PhaseA = "-";

    /// <summary>被动方当前阶段。</summary>
    public static string PhaseB = "-";

    /// <summary>发起方剩余 tick；每 tick 写一次，用来区分"在亲"与"卡在某段"。</summary>
    public static int TicksA = -1;

    public static int TicksB = -1;

    public static void Start()
    {
        lock (Gate)
        {
            if (timer != null)
            {
                return;
            }
            path = Path.Combine(GenFilePaths.SaveDataFolderPath, "Mwah-trace.log");
            timer = new Timer(_ => Sample(), null, SampleMs, SampleMs);
        }
    }

    public static void Set(bool passive, string phase)
    {
        if (passive)
        {
            PhaseB = phase;
        }
        else
        {
            PhaseA = phase;
        }
    }

    public static void Clear()
    {
        PhaseA = "-";
        PhaseB = "-";
        TicksA = -1;
        TicksB = -1;
    }

    private static void Sample()
    {
        try
        {
            int tick = Find.TickManager?.TicksGame ?? -1;
            string line = DateTime.Now.ToString("HH:mm:ss") + " t=" + tick
                + " A=" + PhaseA + ":" + TicksA + " B=" + PhaseB + ":" + TicksB;
            lock (Gate)
            {
                if (path == null)
                {
                    return;
                }
                TrimIfNeeded();
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
        catch
        {
            // 采样器自己绝不能成为故障源：写不进去就这一秒跳过。
        }
    }

    /// <summary>限大小但保住崩溃前最近那段：超 1MB 就只留最后 800 行。</summary>
    private static void TrimIfNeeded()
    {
        if (!File.Exists(path) || new FileInfo(path).Length <= MaxBytes)
        {
            return;
        }
        string[] all = File.ReadAllLines(path);
        if (all.Length <= TailLines)
        {
            return;
        }
        string[] tail = new string[TailLines];
        Array.Copy(all, all.Length - TailLines, tail, 0, TailLines);
        File.WriteAllLines(path, tail);
    }
}
#endif
