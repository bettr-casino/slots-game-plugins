using CrayonScript.Code;
using CrayonScript.Interpreter;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrMathController
    {
        public static BettrMathController Instance { get; private set; }

        public BettrMathController()
        {
            TileController.RegisterType<BettrMathController>("BettrMathController");
            TileController.AddToGlobals("BettrMathController", this);
            
            Instance = this;
        }
        
        public Table GetBaseGameMechanicDataSummaryTable(string machineID, string mechanicName)
        {
            var tableName = $"BaseGame{mechanicName}DataSummary";
            var table = GetTableFirst(tableName, machineID, mechanicName);
            return table;
        }
        
        public Table GetBaseGameMechanicDataTable(string machineID, string mechanicName)
        {
            var tableName = $"BaseGame{mechanicName}Data";
            var table = GetTableArray(tableName, machineID, mechanicName);
            return table;
        }
        
        public Table GetBaseGameMechanicDataMatrix(string machineID, string mechanicName)
        {
            var tableName = $"BaseGame{mechanicName}DataMatrix";
            var table = GetTableArray(tableName, machineID, mechanicName);
            return table;
        }
        
        public Table GetBaseGameMechanicSummaryTable(string machineID, string mechanicName)
        {
            var tableName = $"BaseGame{mechanicName}Summary";
            var table = GetTableFirst(tableName, machineID, mechanicName);
            return table;
        }
        
        public Table GetBaseGameMechanicTable(string machineID, string mechanicName)
        {
            var tableName = $"BaseGame{mechanicName}";
            var table = GetTableArray(tableName, machineID, mechanicName);
            return table;
        }
        
        public Table GetBaseGameMechanicMatrix(string machineID, string mechanicName)
        {
            var tableName = $"BaseGame{mechanicName}Matrix";
            var table = GetTableArray(tableName, machineID, mechanicName);
            return table;
        }

        public int GetBaseGameWager(string machineID)
        {
            var table = GetTableFirst("BaseGameProperties", machineID, "BaseWager");
            var baseWager = (int) (double) table["Value"];
            return baseWager;
        }

        public int GetBaseGameMultiplier(string machineID)
        {
            var table = GetTableFirst("BetMultiplierState", machineID, "Current");
            var betMultiplier = (int) (double) table["BetMultiplier"];
            return betMultiplier;
        }
        
        public float GetBaseGameBet(string machineID)
        {
            var baseWager = GetBaseGameWager(machineID);
            var betMultiplier = GetBaseGameMultiplier(machineID);
            var bet = baseWager * betMultiplier;
            return bet;
        }
        
        public Table GetGlobalTable(string tableName)
        {
            var globalTable = (Table) TileController.LuaScript.Globals[tableName];
            return globalTable;
        }

        public Table GetTableFirst(string tableName, string machineID, string reelID)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[reelID];
            var reelStateTable = (Table) reelTable["First"];
            return reelStateTable;
        }

        public Table GetTableArray(string tableName, string machineID, string reelID)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[reelID];
            var reelStateTable = (Table) reelTable["Array"];
            return reelStateTable;
        }
        
        public int GetTableCount(string tableName, string machineID, string reelID)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[reelID];
            var reelTableCount = (int) (double) reelTable["Count"];
            return reelTableCount;
        }
    }
}