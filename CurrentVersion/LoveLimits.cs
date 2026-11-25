using System;
using XRL.World;
using XRL.World.Parts;
using XRL.World.AI;
using UnityEngine;

namespace XRL.World.Effects
{
    [Serializable]
    public class LoveLimits : Effect
    {
        public override int GetEffectType()
        {
            return (int)EffectType.Mental;
        }

        public override string GetDetails()
        {
            return "Pines for you but won't give away the store. So to speak.";
        }

        public override bool Apply(GameObject Object)
        {
            DisplayName = "{{lovesickness|tough love}}";

            Lovesick Lovesick = Object.GetEffect<Lovesick>();
            if (Lovesick != null)
            {
                Duration = Lovesick.Duration;
                if ((Object.Brain != null) && !Object.IsPlayer() && Object.IsPlayerLed())
                {
                    // Base game will have set player as party leader - undo that
                    Object.Brain.SetPartyLeader(Lovesick.PreviousLeader);

                    // Blarg... sort of poked around at the brain until it stopped attacking me. 
                    Object.Brain.Goals.Clear();
                    Object.Brain.Target = null;
                    Object.StopFighting(The.Player);
                    Object.Brain.Hostile = false;
                    Object.Brain.Opinions.Clear();
                    
                    Object.Brain.AddOpinion<OpinionBeguile>(The.Player);
                    
                    The.Player.StopFighting(Object);
                    foreach (var Companion in The.Player.GetCompanions())
                    {
                        Companion.StopFighting(Object);
                    }
                }
            }
            return true;
        }
        
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                   || ID == GetFeelingEvent.ID
                   || ID == TookDamageEvent.ID
                   || ID == EffectRemovedEvent.ID;
        }

        // WantEvent didn't seem to be getting the job done for whatever reason?
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(GetFeelingEvent.ID);
            Registrar.Register(TookDamageEvent.ID);
            Registrar.Register(EffectRemovedEvent.ID);
            base.Register(Object, Registrar);
        }
        
        public override void Remove(GameObject Object)
        {
            if (!Object.IsPlayer() && (Object.Brain != null))
            {
                // Clean out some of the lobotomy we did earlier
                
                Object.Brain.Goals.Clear();
                Object.Brain.RemoveOpinion<OpinionBeguile>(The.Player);
            }
            base.Remove(Object);
        }
        
        public override bool HandleEvent(GetFeelingEvent E)
        {
            if (E.Target == The.Player)
            {
                E.Feeling = 100;
                return false;
            }
            return base.HandleEvent(E);
        }
        
        public void Reconsider()
        {
            if (Object.IsVisible())
            {
                DidX("reconsider", Object.its + " feelings for you", "!", UsePopup: true);
            }
            Object.RemoveEffect(this);
        }

        public override bool HandleEvent(TookDamageEvent E)
        {
            if (E.Actor != null && ((E.Actor == The.Player) || E.Actor.IsLedBy(The.Player)))
            {
                Reconsider();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EffectRemovedEvent E)
        {
            if (E.Effect is Lovesick Lovesick)
            {
                Object.RemoveEffect(this);
            }
            return base.HandleEvent(E);
        }
    }
}