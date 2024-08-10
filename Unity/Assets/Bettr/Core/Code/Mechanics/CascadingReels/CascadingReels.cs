using System;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class CascadingReelsConfig : BettrMechanicConfig
    {
        public CascadingReelsConfig(): base()
        {
            this.MechanicName = "CascadingReels";
            
            TileController.RegisterType<CascadingReelsConfig>("CascadingReelsConfig");
        }
    }
    
    public class CascadingReels
    {
        
    }    
}

