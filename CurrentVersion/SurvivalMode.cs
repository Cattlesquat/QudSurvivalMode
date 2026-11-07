using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.UI;
using Qud.API;
using XRL;
using XRL.Core;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Capabilities;
using XRL.World.Parts;
using UnityEngine;


namespace XRL.World
{
    public class MODSurvivalModeSystem : IGameSystem
    {
        private bool FirstBootup = true;
        public override void Register(XRLGame Game, IEventRegistrar Registrar)
        {
            Registrar.Register(ScaleXPEvent.ID);
            Registrar.Register(GetLibrarianXPEvent.ID);
            Registrar.Register(GetIntrinsicWeightEvent.ID);
        }

        /// <summary>
        /// Minimum weights for most things, even food and ammo 
        /// </summary>
        public override bool HandleEvent(GetIntrinsicWeightEvent E)
        {
            string Category = E.Object.GetInventoryCategory();
            
            Double MinWeight = 1.0;
            if (Category == "Ammo")
            {
                MinWeight = 0.01;
            } else if (Category == "Food")
            {
                if (E.Object.HasPart<PreservableItem>())
                {
                    MinWeight = 0.2;
                }
                else
                {
                    MinWeight = 0.05;
                }
            } else if (Category == "Meds")
            {
                MinWeight = 0.1;
            } else if (Category == "Tonics")
            {
                MinWeight = 0.05;
            }

            if (E.Weight < MinWeight)
            {
                E.Weight = MinWeight;
            }
            return base.HandleEvent(E);
        }

        /// <summary>
        /// Far fewer XP from defeating lower-leveled opponents 
        /// </summary>
        public override bool HandleEvent(ScaleXPEvent E)
        {
            if (E.Actor.IsPlayer() && (E.Target != null))
            {
                // Matching mostly to how the target cons to the player, instead of base-game's opaquely-slightly-different-from-that system.
                // If we ever brought some of this into the game, I'd actually work code some of the code for this into DifficultyEvaluation itself to avoid code duplication.
                int PlayerLevel = E.Actor.Stat("Level");
                int TargetLevel = E.Target.Stat("Level");
                string TargetRole = E.Target.IsPlayer() ? "Player" : E.Target.GetPropertyOrTag("Role");
                int TargetEffectiveLevel = TargetLevel + DifficultyEvaluation.GetRoleLevelOffset(TargetRole);
                int Difference = TargetEffectiveLevel - PlayerLevel;

                if (Difference < DifficultyEvaluation.EASY)
                {
                    E.XPValue = 0;   // Trivial
                } else 
                if (Difference < DifficultyEvaluation.AVERAGE)
                {
                    E.XPValue /= 10; // Easy
                } else 
                if (Difference < DifficultyEvaluation.TOUGH)
                {
                    E.XPValue /= 2; // Average
                }
                return false; // don't do the default tierscaling
            }

            return base.HandleEvent(E);
        }
        
        /// <summary>
        /// Fewer XP from turning in books at the Stilt
        /// </summary>
        public override bool HandleEvent(GetLibrarianXPEvent E)
        {
            E.XPValue /= 2;
            return base.HandleEvent(E);
        } 
        
        public void Startup()
        {
            if (FirstBootup)
            {
                FirstBootup = false;
                Popup.ShowBlock(
                    "Welcome to {{psychalflesh|Survival Mode}}! Caves of Qud just got ... harder!\n" +
                    "* Fewer XP for turning in books\n" +
                    "* Fewer XP from defeating lower leveled enemies" + 
                    "* Everything has weight! No more weight freebies...",
                    Title: "{{psychalflesh|Survival Mode 1.0}}"
                );
            }
        }
    }


    /// <summary>
    /// This badboy just bootstraps the system into the game
    /// </summary>
    [HasCallStartOfGameSession]
    public class MODSurvivalSystemBootstrapper
    {
        [CallStartOfGameSession]
        public static void Bootstrap()
        {
            var Game = XRLCore.Core?.Game;
            if (Game != null)
            {
                Game.RequireSystem<MODSurvivalModeSystem>().Startup();
            }
        }
    }
}

