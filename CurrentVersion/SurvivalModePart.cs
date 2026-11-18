using System;
using System.Collections.Generic;
using HistoryKit;
using Qud.API;
using XRL.World.Parts;
using XRL.World.Skills;
using XRL.UI;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;
using XRL.World.Parts.Mutation;
using XRL.World.Encounters;
using static XRL.Language.Strings;

namespace XRL.World.Parts
{
	/// <summary>
	/// Have to sell more secrets to get same reputation
	/// Gaining a companion x2 cost
	/// Examining artifacts much harder
	/// </summary>
	[Serializable]
	public class SurvivalModePart  : IPart
	{
		public override bool WantEvent(int ID, int cascade)
		{
			return
				base.WantEvent(ID, cascade)
				|| ID == GetWaterRitualSellSecretBehaviorEvent.ID
				|| ID == GetWaterRitualCostEvent.ID
				|| ID == TinkeringIdentifyEvent.ID;
		}
		
		public override bool HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
		{
			E.ReputationProvided = (E.ReputationProvided * 2 + 2) / 3;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetWaterRitualCostEvent E)
		{
			if (E.Type == "Join")
			{
				E.Cost *= 2;
			}
			return base.HandleEvent(E);
		}

		/// <summary>
		/// Replaces existing identify system with a harder one. The old modifiers all still count, it's just that
		/// the die rolls themselves now need the modifiers a lot more in order to succeed (and not break things)
		/// </summary>
		public override bool HandleEvent(TinkeringIdentifyEvent E)
		{
			//E.Actor = Actor;
			//E.Item = Item;
			//E.Result = Result;
			//E.Difficulty = Difficulty;
			//E.Understanding = Understanding;
			//E.Complexity = Complexity;
			//E.InspectRating = InspectRating;
			//E.InspectBonus = InspectBonus;

			int Roll = XRL.Rules.Stat.Rnd.Next(1, 100);
			if (Roll <= 5)
			{
				E.Result = 0; // Critical failure
			} else if (Roll > 95)
			{
				E.Result = 10; // Critical success
			}
			else
			{
				int BreakThreshold = ((E.Complexity - E.Understanding) * 5) + 40 - E.InspectRating - The.Player.Level;
				if (Roll <= BreakThreshold)
				{
					E.Result = 0; // Fail, break
				}
				else
				{
					int SucceedThreshold = 40 - (E.Complexity * 5) + E.InspectRating + The.Player.Level;
					if (Roll >= 101 - SucceedThreshold)
					{
						int AmountOfSuccessPossible = Math.Max(1, E.Actor.StatMod("Intelligence") + ((E.InspectBonus + E.Actor.Level) / 10));
						E.Result = E.Understanding + XRL.Rules.Stat.Rnd.Next(1, AmountOfSuccessPossible);
					}
					else
					{
						E.Result = 1; // Puzzled (failed, didn't break, except with no-comprehension-yet items counts as success)
					}
					
					if ((E.Understanding == 0) && (E.Complexity > 1) && (E.Understanding + E.Result < E.Complexity))
					{
						// If it's our very first try and we didn't break it, give a little clue about how hard this is going to be. (A chance to reassess) 
						if (E.Complexity <= 3)
						{
							XRL.UI.Popup.Show("Well it's not the most complicated thing you've ever run across, let's see how this goes.");
						} else if (E.Complexity <= 5)
						{
							XRL.UI.Popup.Show("Hmm, this seems a bit complicated.");
						} else if (E.Complexity <= 7)
						{
							XRL.UI.Popup.Show("It seems like quite a complicated device.");
						}
						else
						{
							XRL.UI.Popup.Show("This artifact seems insanely complicated.");
						}							
					} 
				}
			}

			return base.HandleEvent(E);
		}
	}


	[PlayerMutator]
	public class SurvivalModePart_PlayerMutator : IPlayerMutator
	{
		// Adds our part for new games
		public void mutate(GameObject Player)
		{ 
			Player.RequirePart<SurvivalModePart>();
		}
	}
	
	[HasCallAfterGameLoaded]
	public class SurvivalModePart_LoadGameHandler
	{
		[CallAfterGameLoaded]
		public static void LoadGameCallback()
		{
			// Called whenever loading a save game -- retrofit our system into play
			The.Player.RequirePart<SurvivalModePart>();
		}
	}
}