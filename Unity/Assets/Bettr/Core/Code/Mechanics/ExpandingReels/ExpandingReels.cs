using System;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class ExpandingReelsConfig : BettrMechanicConfig
    {
        public ExpandingReelsConfig(): base()
        {
            this.MechanicName = "ExpandingReels";
            
            TileController.RegisterType<ExpandingReelsConfig>("ExpandingReelsConfig");
        }
    }
    
    public class ExpandingReels
    {
        
    }    
}

