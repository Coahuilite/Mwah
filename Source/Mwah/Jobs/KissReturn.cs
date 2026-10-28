using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>Shared, bounded departure inside the originating kiss job.</summary>
public sealed class KissReturn
{
    private const int PhaseTicksCap = 1200;
    private int elapsedTicks;
    private readonly WalkTally tally = new();

    public void ExposeData()
    {
        Scribe_Values.Look(ref elapsedTicks, "returnTicks", 0);
    }

    public Toil MakeToil(string traceTag, Pawn pawn, Func<IntVec3> homeCell, Func<bool> passive, Action arrive)
    {
        var toil = ToilMaker.MakeToil("ToilReturnHome");
        toil.initAction = delegate
        {
            KissTrace.Set(passive(), traceTag);
            Advance(pawn, homeCell(), arrive);
        };
        toil.tickIntervalAction = delegate (int delta)
        {
            elapsedTicks += delta;
            if (elapsedTicks >= PhaseTicksCap)
            {
                arrive();
                return;
            }
            // Loading a current toil rebuilds delegates without rerunning initAction.
            // The saved home and elapsed time, not closure-local flags, own the departure.
            Advance(pawn, homeCell(), arrive);
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private void Advance(Pawn pawn, IntVec3 home, Action arrive)
    {
        if (!MwahMod.Settings.ReturnsHome || !home.IsValid || pawn.Position == home
            || !KissBoundary.CanMoveNow(pawn))
        {
            pawn.pather?.StopDead();
            arrive();
            return;
        }
        if (pawn.pather.Moving)
        {
            return;
        }
        if (!tally.KeepTrying(home) || !pawn.CanReach(home, PathEndMode.OnCell, Danger.Deadly))
        {
            arrive();
            return;
        }
        pawn.pather.StartPath(home, PathEndMode.OnCell);
    }

    /// <summary>
    /// Return a Job, never start it in cleanup. Vanilla schedules it after the old job releases
    /// ownership. Replacements and violence must not acquire a return prefix.
    /// </summary>
    public static Job? Finalizer(Pawn pawn, IntVec3 home, JobCondition condition, bool departureFinished)
    {
        if (condition != JobCondition.Succeeded || !departureFinished || !MwahMod.Settings.ReturnsHome
            || pawn.Dead || !pawn.Spawned || !home.IsValid || pawn.Position == home
            || !home.InBounds(pawn.Map) || !KissBoundary.CanMoveNow(pawn)
            || !pawn.CanReach(home, PathEndMode.OnCell, Danger.Deadly))
        {
            return null;
        }
        return JobMaker.MakeJob(JobDefOf.Goto, home);
    }
}
