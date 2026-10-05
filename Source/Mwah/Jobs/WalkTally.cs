using Verse;

namespace Mwah;

/// <summary>
/// 走位止损的记账本："同一段路连发多次 = 走不动"（门被关、被人堵住）。
/// 双人 job 的定台走位、亲物 job 的走位、回程段三处共用这一把尺 ——
/// 旧写法各写一份、预算常数靠"同值同义"的注释互认，注释一掉三处行为就悄悄分家
/// （2026-10-05 代码审查定罪，三合一）。
/// 计数器不进存档：读档后允许多试一次，比永久卡死便宜（三处原取舍，合并后不变）。
/// </summary>
internal sealed class WalkTally
{
    /// <summary>同一段路重复发到第 3 次仍走不动，下一次起判负。</summary>
    public const int Budget = 3;

    private IntVec3 lastCell = IntVec3.Invalid;

    /// <summary>当前目标格已发出的次数；只给日志用。</summary>
    public int Attempts { get; private set; }

    /// <summary>记一次发路；同一格超出预算即 false（调用方就地结束或转兜底）。</summary>
    public bool KeepTrying(IntVec3 cell)
    {
        if (cell == lastCell)
        {
            Attempts++;
        }
        else
        {
            lastCell = cell;
            Attempts = 1;
        }
        return Attempts <= Budget;
    }
}
