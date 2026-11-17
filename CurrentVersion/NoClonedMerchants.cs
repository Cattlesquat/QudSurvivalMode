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


namespace XRL.World.Parts
{
    public class NoClonedMerchants : IPart
    {
        public override bool WantEvent(int ID, int cascade)
        {
            return
                base.WantEvent(ID, cascade)
                || ID == ReplicaCreatedEvent.ID
                ;
        }
        
        public override bool HandleEvent(ReplicaCreatedEvent E)
        {
            if (E.Object == ParentObject)
            {
                var restocker = ParentObject.GetPart<GenericInventoryRestocker>();
                if (restocker != null)
                {
                    E.WantToRemove(restocker);
                }
            }
            return base.HandleEvent(E);
        }
    }
}