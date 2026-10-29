using Verse;

namespace Mwah;

/// <summary>Approach and performance occupy a kiss; the optional departure does not.</summary>
public static class KissActivity
{
    public static bool IsOccupied(Pawn pawn)
    {
        return pawn.jobs?.curDriver switch
        {
            JobDriver_Kiss pair => pair.IsKissing,
            JobDriver_KissThing thing => thing.IsKissing,
            _ => false,
        };
    }
}
