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
				|| ID == TinkeringIdentifyEvent.ID
				|| ID == ModifyBitCostEvent.ID;
		}
		
		/// <summary>
		/// Have to sell more secrets to get same amount of faction
		/// </summary>
		public override bool HandleEvent(GetWaterRitualSellSecretBehaviorEvent E)
		{
			E.ReputationProvided = (E.ReputationProvided * 2 + 2) / 3;
			return base.HandleEvent(E);
		}

		/// <summary>
		/// Companions cost more faction points
		/// </summary>
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
						// If it's our very first try and we didn't break it, give a little clue about 
                        // how hard this is going to be. (A chance to reassess, since otherwise the player
                        // really has nothing to go on about whether continuing to try is a good idea) 
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

		/// <summary>
		/// Make tinkering costs higher
		/// </summary>
		public override bool HandleEvent(ModifyBitCostEvent E)
		{
			// Alas this event doesn't touch "Ingredient" field. (And really I'd love to be able to require liquids in recipes) 

			if ((E.Context == "Build") || (E.Context == "DataDisk") || (E.Context == "Mod") || (E.Context == "Repair"))
			{
				bool AnyBigBoys = false;
				for (char bit = '1'; bit <= '8'; bit++)
				{
					BitType type = BitType.FetchBitByCode(bit); // The actual dictionary index is not the display character, it's ... the old color code character (!!!) 
					char key = type.BitID; 
					if (E.Bits.ContainsKey(key))
					{
						if ((bit >= '3') && E.Bits[key] > 0) AnyBigBoys = true;
						E.Bits[key] = E.Bits[key] * 2;
					}
				}

				for (char bit = 'A'; bit <= 'D'; bit++)
				{
					BitType type = BitType.FetchBitByCode(bit); // The actual dictionary index is not the display character, it's ... the old color code character(!!!) 
					char key = type.BitID; 
					if (E.Bits.ContainsKey(key)) E.Bits[key] = E.Bits[key] * (AnyBigBoys ? 3 : 2);
				}

            	if ((E.Context == "Mod") || (E.Context == "Repair")) {
					int high = 0;
					foreach (char bit in E.Bits.Keys)
					{
						int tier = BitType.GetBitTier(bit);
						if (tier > high)
						{
							high = tier;
						}
					}

					if (high > 0)
					{
						char highkey = BitType.FetchBitByCode((char)('0' + high)).BitID;
	                    char nextkey = BitType.FetchBitByCode((char)('0' + high - 1)).BitID; 
	                    char next2key = BitType.FetchBitByCode((char)('0' + high - 2)).BitID;

                        if (E.Context == "Mod") {
							E.Bits[highkey]--;

							if (high > 4)
							{
								E.Bits[nextkey]++;
								E.Bits[next2key] += 2;
							} else if (high >= 2)
							{
								E.Bits[nextkey]++; 
							}
						} else if (E.Context == "Repair") {
							E.Bits[highkey]--;

							if (high > 2)
							{
								E.Bits[next2key]++;
							}
							else if (high == 2)
							{
								E.Bits[nextkey]++;
							}
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