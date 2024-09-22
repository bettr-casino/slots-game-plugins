using System;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrAudioController
    {
        public bool IsVolumeMuted { get; set; }

        public BettrAudioController()
        {
            TileController.RegisterType<BettrAudioController>("BettrAudioController");
            TileController.AddToGlobals("BettrAudioController", this);
        }
    }
}