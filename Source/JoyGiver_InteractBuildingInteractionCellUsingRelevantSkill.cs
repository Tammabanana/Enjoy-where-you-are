using RimWorld;
using Verse;
using Verse.AI;

namespace EnjoyWhereYouAre
{
    public class JoyGiver_InteractBuildingInteractionCellUsingRelevantSkill : JoyGiver_InteractBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			//InteractionCell base check - checks if the building is suitable
			if (t.InteractionCell.Standable() && !t.IsForbidden(pawn) && !t.InteractionCell.IsForbidden(pawn) && !Find.PawnDestinationManager.DestinationIsReserved(t.InteractionCell))
			{
				//Check if the pawn can't benefit from skill XP because the skill is disabled
				if (pawn.skills.GetSkill(this.def.jobDef.joySkill).TotallyDisabled)
				{
					//If the skill is disabled, pawn won't do the activity
					return null;
				}

				//If pawn is capable, execute jobDef from the JoyGiverDef
				return new Job(this.def.jobDef, t, t.InteractionCell);
			}
			return null;
		}
	}
}