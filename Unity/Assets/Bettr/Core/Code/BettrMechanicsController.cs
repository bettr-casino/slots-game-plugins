using CrayonScript.Code;
using CrayonScript.Interpreter;

namespace Bettr.Core
{
    public class BettrMechanicsController
    {
        // Singleton instance
        public static BettrMechanicsController Instance { get; private set; }

        // Constructor
        public BettrMechanicsController()
        {
            // Register the BettrMechanicsController type
            TileController.RegisterType<BettrMechanicsController>("BettrMechanicsController");
            TileController.AddToGlobals("BettrMechanicsController", this);

            // Set the singleton instance
            Instance = this;
        }
        
        public Table LoadUserMechanicsTable(string mechanicName, object machine, object variant)
        {
            var tableName = $"{machine.ToString().ToLower()}__{variant.ToString().ToLower()}__{mechanicName.ToLower()}";
            var table = (Table) TileController.LuaScript.Globals[tableName];
            return table;
        }
    }
}