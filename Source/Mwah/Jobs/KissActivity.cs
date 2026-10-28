using Verse;

namespace Mwah;

/// <summary>Job ownership, including approach and departure, excludes another kiss.</summary>
public static class KissActivity
{
    public static bool IsOccupied(Pawn pawn)
    {
        return pawn.jobs?.curDriver is JobDriver_Kiss or JobDriver_KissThing;
    }
}
