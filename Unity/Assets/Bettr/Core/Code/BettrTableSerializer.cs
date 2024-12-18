using System.Text;
using CrayonScript.Code;
using CrayonScript.Interpreter;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrLuaTableToStringSerializer
    {
        private static BettrLuaTableToStringSerializer Instance { get; set; }
        
        public BettrLuaTableToStringSerializer()
        {
            TileController.RegisterType<BettrLuaTableToStringSerializer>("BettrLuaTableToStringSerializer");
            TileController.AddToGlobals("BettrLuaTableToStringSerializer", this);
            
            Instance = this;
        }
        
        public string ConvertTableToLuaCode(Table table, int indent = 0)
        {
            var builder = new StringBuilder();
            string indentStr = new string(' ', indent * 2);
            builder.AppendLine(indentStr + "{");

            foreach (var pair in table.Pairs)
            {
                string key = FormatKey(pair.Key);
                string value = FormatValue(pair.Value, indent + 1);
                builder.AppendLine($"{indentStr}  {key} = {value},");
            }

            builder.AppendLine(indentStr + "}");
            return builder.ToString();
        }

        private static string FormatKey(DynValue key)
        {
            if (key.Type == DataType.String)
                return $"[{EscapeString(key.String)}]";
            if (key.Type == DataType.Number)
                return $"[{key.Number}]";

            // Handle other key types (e.g., user-defined objects)
            return $"[\"{key.ToString()}\"]";
        }

        private string FormatValue(DynValue value, int indent)
        {
            switch (value.Type)
            {
                case DataType.String:
                    return EscapeString(value.String);
                case DataType.Number:
                    return value.Number.ToString();
                case DataType.Boolean:
                    return value.Boolean ? "true" : "false";
                case DataType.Table:
                    return ConvertTableToLuaCode(value.Table, indent);
                case DataType.Nil:
                    return "nil";
                default:
                    return $"\"{value.ToString()}\""; // Fallback for unsupported types
            }
        }

        private static string EscapeString(string str)
        {
            return $"\"{str.Replace("\"", "\\\"")}\"";
        }
    }
}