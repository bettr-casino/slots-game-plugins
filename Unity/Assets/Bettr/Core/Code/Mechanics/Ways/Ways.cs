using System;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class WaysConfig : BettrMechanicConfig
    {
        public WaysConfig(): base()
        {
            this.MechanicName = "WaysConfig";
            
            TileController.RegisterType<WaysConfig>("WaysConfig");
        }
    }
    
    public class Ways
    {
        
    }    
}

