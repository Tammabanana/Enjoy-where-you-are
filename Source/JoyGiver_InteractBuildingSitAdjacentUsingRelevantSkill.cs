using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EnjoyWhereYouAre
{
	public class JoyGiver_InteractBuildingSitAdjacentUsingRelevantSkill : JoyGiver_InteractBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			Thing thing = null;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
				if (!c.IsForbidden(pawn))
				{
					Building edifice = c.GetEdifice();
					if (edifice != null && edifice.def.building.isSittable && pawn.CanReserve(edifice, 1))
					{
						thing = edifice;
						break;
					}
				}
			}
			if (thing == null)
			{
				return null;
			}

			//Add the pawn skill relevancy check, if any of the conditions match the job is null
			if (pawn.story.DisabledWorkTypes.Any(type => type.relevantSkills.Any(skill => skill == this.def.jobDef.joySkill)) || pawn.story.DisabledWorkTags.Any(tag => tag == this.def.jobDef.joySkill.disablingWorkTags))
			{
				return null;
			}

			return new Job(this.def.jobDef, t, thing);
		}
	}
}
