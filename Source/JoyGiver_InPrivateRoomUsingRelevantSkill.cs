using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EnjoyWhereYouAre
{
    public class JoyGiver_InPrivateRoomUsingRelevantSkill : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.ownership == null)
			{
				return null;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return null;
			}

			//Add the pawn skill relevancy check
			if (pawn.skills.GetSkill(this.def.jobDef.joySkill).TotallyDisabled)
			{
				//If the skill is disabled, pawn won't do the activity
				return null;
			}

			IntVec3 vec;
			if (!(from c in ownedRoom.Cells
				  where c.Standable() && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1)
				  select c).TryRandomElement(out vec))
			{
				return null;
			}
			return new Job(this.def.jobDef, vec);
		}
	}
}