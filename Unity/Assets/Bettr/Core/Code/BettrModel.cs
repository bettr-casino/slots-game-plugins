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
            TileController.RegisterType<BettrLobbyCardGroupConfig>("BettrLobbyCardGroupConfig");
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
    public class BettrLobbyCardGroupConfig
    {
        public string Group { get; set; }
        public string Text { get; set; }
    }
    
    [Serializable]
    public class BettrLobbyCardConfig
    {
        public string Group { get; set; }
        public string Card { get; set; }
        public string MachineName { get; set; }
        public string MachineBundleName { get; set; }
        public string MachineBundleVariant { get; set; }
        public string MachineSceneName { get; set; }
        public string BundleName { get; set; }
        public string BundleVersion { get; set; }
        public string MaterialName { get; set; }
        public string Format { get; set; }
    }
    
    [Serializable]
    public class BettrUserConfig
    {
        public string UserId { get; set; }
        public long Coins { get; set; }
        // ReSharper disable once InconsistentNaming
        public long XP { get; set; }
        public long Level { get; set; }
        public BettrSceneConfig LobbyScene { get; set; }
        public List<BettrLobbyCardGroupConfig> LobbyCardGroups { get; set; }
        public List<BettrLobbyCardConfig> LobbyCards { get; set; }
    }

    [Serializable]
    public class BettrUserEvents
    {
        public List<BettrUserEvent> Events { get; set; }
    }

    [Serializable]
    public class BettrUserEvent
    {
        public string EventId { get; set; }
        public bool Persistent { get; set; }
        public bool Acked { get; set; }
        public string Value { get; set; }
    }
}