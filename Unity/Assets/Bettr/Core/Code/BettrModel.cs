using System;
using System.Collections.Generic;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public static class BettrModel
    {
        public static void Init()
        {
            TileController.RegisterType<BettrSceneConfig>("BettrSceneConfig");
            TileController.RegisterType<BettrLobbyCardConfig>("BettrLobbyCardConfig");
            TileController.RegisterType<BettrUserConfig>("BettrUserConfig");
        }
    }
    
    [Serializable]
    public class BettrSceneConfig
    {
        public string BundleName { get; set; }
        public string BundleVersion { get; set; }
    }
    
    [Serializable]
    public class BettrLobbyCardConfig
    {
        public string Group { get; set; }
        public string MachineName { get; set; }
        public string BundleName { get; set; }
        public string BundleVersion { get; set; }
        public string PrefabName { get; set; }
        public string Format { get; set; }
    }
    
    [Serializable]
    public class BettrUserConfig
    {
        public long Coins { get; set; }
        // ReSharper disable once InconsistentNaming
        public long XP { get; set; }
        public long Level { get; set; }
        
        public BettrSceneConfig LobbyScene { get; set; }
        
        public List<BettrLobbyCardConfig> LobbyCards { get; set; }
    }
}