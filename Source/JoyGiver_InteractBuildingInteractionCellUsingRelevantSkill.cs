using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace EnjoyWhereYouAre
{
	public class JoyGiver_InteractBuildingInteractionCellUsingRelevantSkill : JoyGiver_InteractBuilding
	{
		protected override Job TryGivePlayJob(Pawn pawn, Thing t)
		{
			//InteractionCell base check - checks if the building is suitable
			if (t.InteractionCell.Standable() && !t.IsForbidden(pawn) && !t.InteractionCell.IsForbidden(pawn) && !Find.PawnDestinationManager.DestinationIsReserved(t.InteractionCell))
			{
				//Check if the pawn's story contains work types or work tags that should disable this activity
				//e.g. If pawn is "incapable of intellectual" - pawn can't do Research, pawn can't enjoy research-boosting joy activities
				if (pawn.story.DisabledWorkTypes.Any(type => type.relevantSkills.Any(skill => skill == this.def.jobDef.joySkill)) || pawn.story.DisabledWorkTags.Any(tag => tag == this.def.jobDef.joySkill.disablingWorkTags))
				{
					//If pawn is incapable, activity is null
					return null;
				}
				//If pawn is capable, execute jobDef from the JoyGiverDef
				return new Job(this.def.jobDef, t, t.InteractionCell);
			}
			return null;
		}
	}
}