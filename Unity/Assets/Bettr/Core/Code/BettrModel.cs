using System;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public static class BettrModel
    {
        public static void Init()
        {
            TileController.RegisterType<BettrBundleConfig>("BettrSceneConfig");
            TileController.RegisterType<BettrLobbyCardConfig>("BettrLobbyCardConfig");
            TileController.RegisterType<BettrLobbyCardGroupConfig>("BettrLobbyCardGroupConfig");
            TileController.RegisterType<BettrUserConfig>("BettrUserConfig");
        }
    }
    
    [Serializable]
    public class BettrBundleConfig
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
        
        public string MachineBundleId => $"{MachineBundleName}.{MachineBundleVariant}";
        public string LobbyCardBundleId => $"lobbycard{MachineBundleId}";
        
        public string GetMachineVariant()
        {
            var partial = MachineSceneName.Substring(MachineName.Length);
            // remove the "Scene" suffix
            return partial.Substring(0, partial.Length - 5);
        }

    }
    
    [Serializable]
    public class BettrUserConfig
    {
        public string UserId { get; set; }
        private long _coins = 0;

        public long Coins
        {
            get
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                return _coins;
            }
            set
            {
                _coins = value;
                if (_coins < 0)
                {
                    _coins = 0;
                }
            }
        }

        private long _spinCoins = 0;
        public long SpinCoins
        {
            get
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                return _spinCoins;
            }
            set
            {
                _spinCoins = value;
                if (_spinCoins < 0)
                {
                    _spinCoins = 0;
                }
            }
        }

        public void InitSpinCoins()
        {
            SpinCoins = Coins;
        }
        
        public void ApplySpinCoins()
        {
            Coins = SpinCoins;
        }
        
        // ReSharper disable once InconsistentNaming
        public long XP { get; set; }
        public long Level { get; set; }
        public BettrBundleConfig Main { get; set; }
        public BettrBundleConfig LobbyScene { get; set; }
        public List<BettrLobbyCardGroupConfig> LobbyCardGroups { get; set; }
        public List<BettrLobbyCardConfig> LobbyCards { get; set; }
        public int LobbyCardIndex { get; set; } = -1;
        public string LobbyCardName { get; set; }

        public int FindLobbyCardIndexById(string lobbyCardId)
        {
            for (var index = 0; index < LobbyCards.Count; index++)
            {
                var t = LobbyCards[index];
                if (t.Card == lobbyCardId)
                {
                    return index;
                }
            }
            return -1;
        }
    }
    
    [Serializable]
    public class BettrMechanicConfig
    {
        public string MechanicName { get; set; }
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
    
    [Serializable]
    public class BettrUserExperiment
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("experiment_name")]
        public string ExperimentName { get; set; }
        [JsonProperty("excluded_for")]
        public string ExcludedFor { get; set; }
        [JsonProperty("audience_evaluated")]
        public bool AudienceEvaluated { get; set; }
        [JsonProperty("in_audience")]
        public bool InAudience { get; set; }
        [JsonProperty("in_ramp")]
        public bool InRamp { get; set; }
        [JsonProperty("ramp_roll")]
        public int RampRoll { get; set; }
        [JsonProperty("assigned_variant")]
        public string AssignedVariant { get; set; }
        [JsonProperty("treatment")]
        public string Treatment { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("is_override")]
        public bool IsOverride { get; set; }
    }
}