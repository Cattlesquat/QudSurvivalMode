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
	/// </summary>
	[Serializable]
	public class WaterRitualHarder : IPart
	{
		public int LastMetempsychosisCount = 0;
		
		public override bool WantEvent(int ID, int cascade)
		{
			return
				base.WantEvent(ID, cascade)
				|| ID == GetWaterRitualSellSecretBehaviorEvent.ID
				|| ID == GetWaterRitualCostEvent.ID;
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
	}


	[PlayerMutator]
	public class WaterRitualHarder_PlayerMutator : IPlayerMutator
	{
		// Adds our part for new games
		public void mutate(GameObject Player)
		{ 
			Player.RequirePart<WaterRitualHarder>();
		}
	}
	
	[HasCallAfterGameLoaded]
	public class WaterRitualHarder_LoadGameHandler
	{
		[CallAfterGameLoaded]
		public static void LoadGameCallback()
		{
			// Called whenever loading a save game -- retrofit our system into play
			The.Player.RequirePart<WaterRitualHarder>();
		}
	}
}