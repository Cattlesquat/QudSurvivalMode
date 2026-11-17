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
	/// Undergoing metempsychosis increases glimmer 
	/// </summary>
	[Serializable]
	public class DominationGlimmers : IPart
	{
		public int LastMetempsychosisCount = 0;
		
		public override bool WantEvent(int ID, int cascade)
		{
			return
				base.WantEvent(ID, cascade)
				|| ID == GetPsychicGlimmerEvent.ID;
		}
		
		public override bool HandleEvent(GetPsychicGlimmerEvent E)
		{
			if (The.Game.HasIntGameState(Domination.METEMPSYCHOSIS_COUNT_KEY))
            {
                int count = The.Game.GetIntGameState(Domination.METEMPSYCHOSIS_COUNT_KEY);
                E.Level += count * DimensionManager.GLIMMER_FLOOR;

                if (LastMetempsychosisCount < count)
                {
	                LastMetempsychosisCount = count;
	                Popup.Show("{{K|You get the disconcerting feeling that someone or something took note of your mind coming to rest in a different body.}}");
                }
            }

			return base.HandleEvent(E);
		}
	}


	[PlayerMutator]
	public class DominationGlimmers_PlayerMutator : IPlayerMutator
	{
		// Adds our part for new games
		public void mutate(GameObject Player)
		{ 
			Player.RequirePart<DominationGlimmers>();
		}
	}
	
	[HasCallAfterGameLoaded]
	public class DominationGlimmers_LoadGameHandler
	{
		[CallAfterGameLoaded]
		public static void LoadGameCallback()
		{
			// Called whenever loading a save game -- retrofit our system into play
			The.Player.RequirePart<DominationGlimmers>();
		}
	}
}