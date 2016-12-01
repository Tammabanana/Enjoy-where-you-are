using RimWorld;
using Verse;
using Verse.AI;

namespace EnjoyWhereYouAre
{
    public class JoyGiver_WatchBuildingUsingRelevantSkill : JoyGiver_WatchBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			IntVec3 vec;
			Building t2;
			if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out vec, out t2))
			{
				return null;
			}

			//Add the pawn skill relevancy check
			if (pawn.skills.GetSkill(this.def.jobDef.joySkill).TotallyDisabled)
			{
				//If the skill is disabled, pawn won't do the activity
				return null;
			}

			return new Job(this.def.jobDef, t, vec, t2);
		}
	}
}