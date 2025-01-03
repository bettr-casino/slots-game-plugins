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

        public Table GetBaseGameMechanicDataSummary(string machineID, string mechanicName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return GetBaseGameMechanicDataSummary(machineID, mechanicName, null);
        }
        
        public Table GetBaseGameMechanicDataSummary(string machineID, string mechanicName, string pk)
        {
            var tableName = $"BaseGame{mechanicName}DataSummary";
            pk = pk ?? mechanicName;
            var table = GetTableFirst(tableName, machineID, pk);
            return table;
        }

        public Table GetBaseGameMechanicData(string machineID, string mechanicName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return GetBaseGameMechanicData(machineID, mechanicName, null);
        }
        
        public Table GetBaseGameMechanicData(string machineID, string mechanicName, string pk)
        {
            var tableName = $"BaseGame{mechanicName}Data";
            pk = pk ?? mechanicName;
            var table = GetTableArray(tableName, machineID, pk);
            return table;
        }

        public Table GetBaseGameMechanicDataRow(string machineID, string mechanicName, params string[] kvPairs)
        {
            return GetBaseGameMechanicDataRow(machineID, mechanicName, null, kvPairs);
        }
        

        public Table GetBaseGameMechanicDataRow(string machineID, string mechanicName, string pk, params string[] kvPairs)
        {
            // get the table
            var table = GetBaseGameMechanicData(machineID, mechanicName, pk);
            // find the row where the kvPairs match
            for (int i = 0; i < table.Length; i++)
            {
                var row = (Table) table[i + 1];
                for (int j = 0; j < kvPairs.Length; j = j + 2)
                {
                    var key = kvPairs[j];
                    var value = kvPairs[j + 1];
                    if (row[key].ToString() != value)
                    {
                        break;
                    }
                    if (j == kvPairs.Length - 2)
                    {
                        return row;
                    }
                }
            }
            return null;
        }

        public Table GetBaseGameMechanicDataMatrix(string machineID, string mechanicName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return GetBaseGameMechanicDataMatrix(machineID, mechanicName, null);
        }
        
        public Table GetBaseGameMechanicDataMatrix(string machineID, string mechanicName, string pk)
        {
            var tableName = $"BaseGame{mechanicName}DataMatrix";
            pk = pk ?? mechanicName;
            var table = GetTableArray(tableName, machineID, pk);
            return table;
        }

        public Table GetBaseGameMechanicSummary(string machineID, string mechanicName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return GetBaseGameMechanicSummary(machineID, mechanicName, null);
        }

        public Table GetBaseGameMechanicSummary(string machineID, string mechanicName, string pk)
        {
            var tableName = $"BaseGame{mechanicName}Summary";
            pk = pk ?? mechanicName;
            var table = GetTableFirst(tableName, machineID, pk);
            return table;
        }

        public Table GetBaseGameMechanic(string machineID, string mechanicName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return GetBaseGameMechanic(machineID, mechanicName, null);
        }
        
        public Table GetBaseGameMechanic(string machineID, string mechanicName, string pk)
        {
            var tableName = $"BaseGame{mechanicName}";
            pk = pk ?? mechanicName;
            var table = GetTableArray(tableName, machineID, pk);
            return table;
        }

        public Table GetBaseGameMechanicRow(string machineID, string mechanicName, params string[] kvPairs)
        {
            return GetBaseGameMechanicRow(machineID, mechanicName, null, kvPairs);
        }
        
        public Table GetBaseGameMechanicRow(string machineID, string mechanicName, string pk, params string[] kvPairs)
        {
            // get the table
            var table = GetBaseGameMechanic(machineID, mechanicName, pk);
            // find the row where the kvPairs match
            for (int i = 0; i < table.Length; i++)
            {
                var row = (Table) table[i + 1];
                for (int j = 0; j < kvPairs.Length; j = j + 2)
                {
                    var key = kvPairs[j];
                    var value = kvPairs[j + 1];
                    if (row[key].ToString() != value)
                    {
                        break;
                    }
                    if (j == kvPairs.Length - 2)
                    {
                        return row;
                    }
                }
            }
            return null;
        }

        public Table GetBaseGameMechanicMatrix(string machineID, string mechanicName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return GetBaseGameMechanicMatrix(machineID, mechanicName, null);
        }
        
        public Table GetBaseGameMechanicMatrix(string machineID, string mechanicName, string pk)
        {
            var tableName = $"BaseGame{mechanicName}Matrix";
            pk = pk ?? mechanicName;
            var table = GetTableArray(tableName, machineID, pk);
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

        public Table GetTableFirst(string tableName, string machineID, string pk)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[pk];
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