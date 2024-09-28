using System;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class MegawaysConfig : BettrMechanicConfig
    {
        public MegawaysConfig(): base()
        {
            this.MechanicName = "Megaways";
            
            TileController.RegisterType<MegawaysConfig>("MegawaysConfig");
        }
    }
    
    public class Megaways
    {
        
    }    
}

