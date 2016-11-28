using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace EnjoyWhereYouAre
{
	public class JoyGiver_SocialRelaxUsingRelevantSkill : JoyGiver
	{
		private const float GatherRadius = 3.9f;

		private static List<CompGatherSpot> workingSpots = new List<CompGatherSpot>();

		private static readonly int NumRadiusCells = GenRadial.NumCellsInRadius(3.9f);

		private static readonly List<IntVec3> RadialPatternMiddleOutward = (from c in GenRadial.RadialPattern.Take(JoyGiver_SocialRelaxUsingRelevantSkill.NumRadiusCells)
		orderby Mathf.Abs((c - IntVec3.Zero).LengthHorizontal - 1.95f)
		select c).ToList<IntVec3>();

		private static List<ThingDef> nurseableDrugs = new List<ThingDef>();

		public override Job TryGiveJob(Pawn pawn)
		{
			return this.TryGiveJobInt(pawn, null);
		}

		public override Job TryGiveJobInPartyArea(Pawn pawn, IntVec3 partySpot)
		{
			return this.TryGiveJobInt(pawn, (CompGatherSpot x) => PartyUtility.InPartyArea(x.parent.Position, partySpot));
		}

		private Job TryGiveJobInt(Pawn pawn, Predicate<CompGatherSpot> gatherSpotValidator)
		{
            //Add the pawn skill relevancy check, if any of the conditions match the job is null
            if (pawn.story.DisabledWorkTypes.Any(type => type.relevantSkills.Any(skill => skill == this.def.jobDef.joySkill)) || pawn.story.DisabledWorkTags.Any(tag => tag == this.def.jobDef.joySkill.disablingWorkTags))
            {
                return null;
            }

            if (GatherSpotLister.activeSpots.Count == 0)
			{
				return null;
			}
			JoyGiver_SocialRelaxUsingRelevantSkill.workingSpots.Clear();
			for (int i = 0; i < GatherSpotLister.activeSpots.Count; i++)
			{
				JoyGiver_SocialRelaxUsingRelevantSkill.workingSpots.Add(GatherSpotLister.activeSpots[i]);
			}
			CompGatherSpot compGatherSpot;
			while (JoyGiver_SocialRelaxUsingRelevantSkill.workingSpots.TryRandomElement(out compGatherSpot))
			{
				JoyGiver_SocialRelaxUsingRelevantSkill.workingSpots.Remove(compGatherSpot);
				if (!compGatherSpot.parent.IsForbidden(pawn) && pawn.CanReach(compGatherSpot.parent, PathEndMode.Touch, Danger.None, false, TraverseMode.ByPawn) && compGatherSpot.parent.IsSociallyProper(pawn) && (gatherSpotValidator == null || gatherSpotValidator(compGatherSpot)))
				{
					Job job;
					Thing t2;
					if (compGatherSpot.parent.def.surfaceType == SurfaceType.Eat)
					{
						Thing t;
						if (!JoyGiver_SocialRelaxUsingRelevantSkill.TryFindChairBesideTable(compGatherSpot.parent, pawn, out t))
						{
							return null;
						}
						job = new Job(JobDefOf.SocialRelax, compGatherSpot.parent, t);
					}
					else if (JoyGiver_SocialRelaxUsingRelevantSkill.TryFindChairNear(compGatherSpot.parent.Position, pawn, out t2))
					{
						job = new Job(JobDefOf.SocialRelax, compGatherSpot.parent, t2);
					}
					else
					{
						IntVec3 vec;
						if (!JoyGiver_SocialRelaxUsingRelevantSkill.TryFindSitSpotOnGroundNear(compGatherSpot.parent.Position, pawn, out vec))
						{
							return null;
						}
						job = new Job(JobDefOf.SocialRelax, compGatherSpot.parent, vec);
					}
					Thing thing;
					if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && JoyGiver_SocialRelaxUsingRelevantSkill.TryFindIngestibleToNurse(compGatherSpot.parent.Position, pawn, out thing))
					{
						job.targetC = thing;
						job.maxNumToCarry = Mathf.Min(thing.stackCount, thing.def.ingestible.maxNumToIngestAtOnce);
					}
					return job;
				}
			}
			return null;
		}

		private static bool TryFindIngestibleToNurse(IntVec3 center, Pawn ingester, out Thing ingestible)
		{
			if (ingester.story != null && ingester.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0)
			{
				ingestible = null;
				return false;
			}
			if (ingester.drugs == null)
			{
				ingestible = null;
				return false;
			}
			JoyGiver_SocialRelaxUsingRelevantSkill.nurseableDrugs.Clear();
			DrugPolicy currentPolicy = ingester.drugs.CurrentPolicy;
			for (int i = 0; i < currentPolicy.Count; i++)
			{
				if (currentPolicy[i].allowedForJoy && currentPolicy[i].drug.ingestible.nurseable)
				{
					JoyGiver_SocialRelaxUsingRelevantSkill.nurseableDrugs.Add(currentPolicy[i].drug);
				}
			}
			JoyGiver_SocialRelaxUsingRelevantSkill.nurseableDrugs.Shuffle<ThingDef>();
			for (int j = 0; j < JoyGiver_SocialRelaxUsingRelevantSkill.nurseableDrugs.Count; j++)
			{
				List<Thing> list = Find.ListerThings.ThingsOfDef(JoyGiver_SocialRelaxUsingRelevantSkill.nurseableDrugs[j]);
				if (list.Count > 0)
				{
					Predicate<Thing> validator = (Thing t) => ingester.CanReserve(t, 1) && !t.IsForbidden(ingester);
					ingestible = GenClosest.ClosestThing_Global_Reachable(center, list, PathEndMode.OnCell, TraverseParms.For(ingester, Danger.Deadly, TraverseMode.ByPawn, false), 40f, validator, null);
					if (ingestible != null)
					{
						return true;
					}
				}
			}
			ingestible = null;
			return false;
		}

		private static bool TryFindChairBesideTable(Thing table, Pawn sitter, out Thing chair)
		{
			for (int i = 0; i < 30; i++)
			{
				IntVec3 c = table.RandomAdjacentCellCardinal();
				Building edifice = c.GetEdifice();
				if (edifice != null && edifice.def.building.isSittable && sitter.CanReserve(edifice, 1))
				{
					chair = edifice;
					return true;
				}
			}
			chair = null;
			return false;
		}

		private static bool TryFindChairNear(IntVec3 center, Pawn sitter, out Thing chair)
		{
			for (int i = 0; i < JoyGiver_SocialRelaxUsingRelevantSkill.RadialPatternMiddleOutward.Count; i++)
			{
				IntVec3 c = center + JoyGiver_SocialRelaxUsingRelevantSkill.RadialPatternMiddleOutward[i];
				Building edifice = c.GetEdifice();
				if (edifice != null && edifice.def.building.isSittable && sitter.CanReserve(edifice, 1) && !edifice.IsForbidden(sitter) && GenSight.LineOfSight(center, edifice.Position, true))
				{
					chair = edifice;
					return true;
				}
			}
			chair = null;
			return false;
		}

		private static bool TryFindSitSpotOnGroundNear(IntVec3 center, Pawn sitter, out IntVec3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[Rand.Range(1, JoyGiver_SocialRelaxUsingRelevantSkill.NumRadiusCells)];
				if (sitter.CanReserveAndReach(intVec, PathEndMode.OnCell, Danger.None, 1) && intVec.GetEdifice() == null && GenSight.LineOfSight(center, intVec, true))
				{
					result = intVec;
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}
	}
}
