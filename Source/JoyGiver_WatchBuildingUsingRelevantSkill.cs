using System;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

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

			//Add the pawn skill relevancy check, if any of the conditions match the job is null
			if (pawn.story.DisabledWorkTypes.Any(type => type.relevantSkills.Any(skill => skill == this.def.jobDef.joySkill)) || pawn.story.DisabledWorkTags.Any(tag => tag == this.def.jobDef.joySkill.disablingWorkTags))
			{
				return null;
			}

			return new Job(this.def.jobDef, t, vec, t2);
		}
	}
}