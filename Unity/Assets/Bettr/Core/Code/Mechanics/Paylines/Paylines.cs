using System;
using System.Collections.Generic;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class PaylinesConfig : BettrMechanicConfig
    {
        public PaylinesConfig(): base()
        {
            this.MechanicName = "Paylines";
            
            TileController.RegisterType<PaylinesConfig>("PaylinesConfig");
        }
    }
    
    public class Paylines
    {
        
    }    
}

