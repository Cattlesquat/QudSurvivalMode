using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace OldVersions
{
    public static readonly string MOD_NAME = "{{psychalflesh|Survival Mode}}";

    [HasCallAfterGameLoaded]
    public class OldVersions : IPlayerPart
    {
        [CallAfterGameLoaded]
        public static void LoadGameCallback()
        {
            XRL.UI.Popup.ShowFail("The " + MOD_NAME + " mod presently only works on the Diaspora testing branch.");
        }
	}

    [PlayerMutator]
    public class MyPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            XRL.UI.Popup.ShowFail("The " + MOD_NAME + " mod presently only works on the Diaspora testing branch.");
        }
    }
}
