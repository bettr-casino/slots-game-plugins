
using System;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public enum SelectedSideEnum
    {
        None = 0,
        Good = 1,
        Evil = 2,
    }
        
    
    [Serializable]
    public class ChooseASideMechanicConfig : BettrMechanicConfig
    {
        public int SelectedSide { get; set; }
        public int MeterValue { get; set; }
        
        public ChooseASideMechanicConfig(): base()
        {
            this.MechanicName = "ChooseASide";
            this.SelectedSide = (int) SelectedSideEnum.None;
            
            TileController.RegisterType<ChooseASideMechanicConfig>("ChooseASideMechanicConfig");
        }
    }
    
    public class ChooseASide
    {
        public ChooseASide()
        {
            
        }
        
        public void Advance()
        {
            
        }
        
        public void Reverse()
        {
            
        }
        
        public void Reset()
        {
            
        }
    }
}