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

			//Add the pawn skill relevancy check
			if (pawn.skills.GetSkill(this.def.jobDef.joySkill).TotallyDisabled)
			{
				//If the skill is disabled, pawn won't do the activity
				return null;
			}

			return new Job(this.def.jobDef, t, thing);
		}
	}
}