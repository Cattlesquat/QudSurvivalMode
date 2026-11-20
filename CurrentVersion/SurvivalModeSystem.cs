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
    [Serializable]
    public class MODSurvivalModeSystem : IGameSystem
    {
        public bool FirstBootup = true;
        public override void Register(XRLGame Game, IEventRegistrar Registrar)
        {
            Registrar.Register(ScaleXPEvent.ID);
            Registrar.Register(GetLibrarianXPEvent.ID);
            Registrar.Register(GetIntrinsicWeightEvent.ID);
            Registrar.Register(TinkeringVendorCostEvent.ID);
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

        public override bool HandleEvent(TinkeringVendorCostEvent E)
        {
            if ((E.Action == "Recharge") && (E.Rechargeable != null))
            {
                E.Cost = Math.Max(E.Rechargeable.GetRechargeAmount() / 100, 1); // Presently x5 the base game cost
            } else if (E.Action == "Identify")
            {
                // Base game formula for reference
                //int complexity = GO.GetComplexity();
                //int difficulty = GO.GetExamineDifficulty();
                //float x = (complexity + difficulty);
                //int Cost = (int)Math.Max(2, -0.0667 + 1.24 * x + 0.0967 * Math.Pow(x,2) + 0.0979 * Math.Pow(x,3));
                
                E.Cost = E.Cost * 50; 
            } else if (E.Action == "Repair")
            {
                //int cost = Math.Max(5 + (int) (GetValue(GO, false) / 25), 5) * GO.Count;  // base game formula for reference

                // Repair has a gaping bug in base game where value is being heavily discounted *for being broken* before checking repair costs
                Commerce Commerce = E.Object.GetPart<Commerce>();
                double ValueEach = (Commerce != null) ? Commerce.Value : 0.01;
                if (E.Object.WantEvent(GetIntrinsicValueEvent.ID, GetIntrinsicValueEvent.CascadeLevel))
                {
                    GetIntrinsicValueEvent IV = GetIntrinsicValueEvent.FromPool(E.Object, ValueEach);
                    HandleEvent(IV);
                    ValueEach = IV.Value;
                }
                
                E.Cost = Math.Max((int) (ValueEach / 3), 10) * E.Object.Count; 
            }
            E.Cost = 0;
            return base.HandleEvent(E);
        }
        
        public void Startup()
        {
            if (FirstBootup)
            {
                FirstBootup = false;

                The.Game.PlayerReputation.Modify("YdFreehold", -600, Silent: true);
                The.Game.PlayerReputation.Modify("Ezra", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Chavvah", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Kyakukya", -175, Silent: true);

                The.Game.PlayerReputation.Modify("Newly Sentient Beings", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Dromad", -125, Silent: true);
                
                The.Game.PlayerReputation.Modify("Entropic", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Worms", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Oozes", -550, Silent: true);
                The.Game.PlayerReputation.Modify("Robots", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Insects", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Winged Mammals", -300, Silent: true);
                The.Game.PlayerReputation.Modify("Apes", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Birds", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Fish", -200, Silent: true);
                The.Game.PlayerReputation.Modify("Goatfolk", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Hermits", -125, Silent: true);
                The.Game.PlayerReputation.Modify("Frogs", -200, Silent: true);
                The.Game.PlayerReputation.Modify("Svardym", -475, Silent: true);
                The.Game.PlayerReputation.Modify("Naphtaali", -50, Silent: true);
                The.Game.PlayerReputation.Modify("Strangers", -150, Silent: true);
                The.Game.PlayerReputation.Modify("Pariahs", -100, Silent: true);
                The.Game.PlayerReputation.Modify("Mopango", -100, Silent: true);
                The.Game.PlayerReputation.Modify("Barathrumites", -150, Silent: true);
                The.Game.PlayerReputation.Modify("Consortium", -110, Silent: true);
                The.Game.PlayerReputation.Modify("Templar", -100, Silent: true);
                The.Game.PlayerReputation.Modify("Mechanimists", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Gyre Wights", -250, Silent: true);
                The.Game.PlayerReputation.Modify("Seekers", -250, Silent: true);
                The.Game.PlayerReputation.Modify("Flowers", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Roots", -150, Silent: true);
                The.Game.PlayerReputation.Modify("Fungi", -250, Silent: true);
                The.Game.PlayerReputation.Modify("Vines", -250, Silent: true);
                The.Game.PlayerReputation.Modify("Urchins", -250, Silent: true);
                The.Game.PlayerReputation.Modify("Succulents", -175, Silent: true);
                The.Game.PlayerReputation.Modify("Trees", -300, Silent: true);
                The.Game.PlayerReputation.Modify("Merchants", -150, Silent: true);
                The.Game.PlayerReputation.Modify("Farmers", -300, Silent: true);
                The.Game.PlayerReputation.Modify("Daughters", -150, Silent: true);
                The.Game.PlayerReputation.Modify("SultanCult1", -200, Silent: true);
                The.Game.PlayerReputation.Modify("SultanCult2", -200, Silent: true);
                The.Game.PlayerReputation.Modify("SultanCult3", -200, Silent: true);
                The.Game.PlayerReputation.Modify("SultanCult4", -200, Silent: true);
                The.Game.PlayerReputation.Modify("SultanCult5", -200, Silent: true);
                
                Popup.ShowBlock(
                    "Welcome to {{psychalflesh|Survival Mode}}! Caves of Qud just got ... harder!\n" +
                    "* Everything has weight! No more weight freebies...\n" +
                    "* Much lower starting faction levels, including with certain key factions\n" +
                    "* Water rituals: companions cost more, secrets give you less\n" +
                    "* Fewer XP for turning in books\n" +
                    "* Fewer XP from defeating lower leveled enemies\n" + 
                    "* Cloned merchants don't get restocking inventories (Sorry, clone-of-Tilli!)\n" +
                    "* Permanent body-swapping increases Psychic Glimmer\n" + 
                    "* Identifying artifacts is much trickier!\n",
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

