using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrMainLobbySceneControllerState
    {
        public Dictionary<string, BettrLobbyCardConfig> LobbyCardMap;

        public BettrMainLobbySceneControllerState()
        {
            TileController.RegisterType<BettrMainLobbySceneControllerState>("BettrMainLobbySceneControllerState");
            LobbyCardMap = new Dictionary<string, BettrLobbyCardConfig>();
        }
    }
    
    public class BettrMainLobbySceneController
    {
        public BettrMainLobbySceneControllerState State = new BettrMainLobbySceneControllerState();
        
        public BettrExperimentController BettrExperimentController { get; private set; }
        
        public BettrMainLobbySceneController(BettrExperimentController bettrExperimentController)
        {
            TileController.RegisterType<BettrMainLobbySceneController>("BettrMainLobbySceneController");
            TileController.AddToGlobals("BettrMainLobbySceneController", this);
            
            BettrExperimentController = bettrExperimentController;
        }
        
        public IEnumerator LoadLobbyCardMachine(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            // lobbyCardName example Game001__LobbyCard001
            var lobbyCardIndex = bettrUser.FindLobbyCardIndexById(lobbyCardName.Split("__")[1]);
            bettrUser.LobbyCardIndex = lobbyCardIndex;
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            var (machineName, machineVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineName, machineVariant);
            yield return BettrAssetController.Instance.LoadScene(machineName, machineVariant, lobbyCard.MachineSceneName);
        }

        public IEnumerator LoadMachine()
        {
            // TODO: this should be from the user preferences
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            bettrUser.LobbyCardIndex = 0;
            var lobbyCard = bettrUser.LobbyCards[0];
            var (machineName, machineVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineName, machineVariant);
            yield return BettrAssetController.Instance.LoadScene(machineName,
                machineVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadPreviousMachine()
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var bettrUserLobbyCardIndex = bettrUser.LobbyCardIndex;
            // Wrap around
            var previousIndex = (bettrUserLobbyCardIndex - 1 + bettrUser.LobbyCards.Count) % bettrUser.LobbyCards.Count;
            bettrUser.LobbyCardIndex = previousIndex;
            var lobbyCard = bettrUser.LobbyCards[previousIndex];
            var (machineName, machineVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineName, machineVariant);
            yield return BettrAssetController.Instance.LoadScene(machineName, machineVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadNextMachine()
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var bettrUserLobbyCardIndex = bettrUser.LobbyCardIndex;
            // Wrap around
            var nextIndex = (bettrUserLobbyCardIndex + 1) % bettrUser.LobbyCards.Count;
            bettrUser.LobbyCardIndex = nextIndex;
            var lobbyCard = bettrUser.LobbyCards[nextIndex];
            var (machineName, machineVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineName, machineVariant);
            yield return BettrAssetController.Instance.LoadScene(machineName, machineVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadMainLobby()
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyScene = bettrUser.LobbyScene;
            var (bundleName, bundleVersion) = GetLobbyExperiment(lobbyScene);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(bundleName, bundleVersion);
            yield return BettrAssetController.Instance.LoadScene(bundleName, bundleVersion, "MainLobbyScene");
        }
        
        // Convert the method from Lua to C#
        public IEnumerator LoadLobbyCards(Table self)
        {
            Console.WriteLine("LoadLobbyCards invoked");
            
            var bettrUser = BettrUserController.Instance.BettrUserConfig;

            // Get the group count
            int groupCount = bettrUser.LobbyCardGroups.Count;  // Assuming LobbyCardGroups is a list or similar collection

            // Iterate over LobbyCardGroups
            for (int i = 0; i < groupCount; i++)
            {
                var lobbyCardGroup = bettrUser.LobbyCardGroups[i];
                var groupLabelProperty = (PropertyTextMeshPro) self[lobbyCardGroup.Group + "Label"];
                groupLabelProperty.SetText(lobbyCardGroup.Text);
            }

            // Get the card count
            int cardCount = bettrUser.LobbyCards.Count;  // Assuming LobbyCards is a list or similar collection
            Console.WriteLine($"LoadApp lobbyCard cardCount={cardCount}");

            State.LobbyCardMap.Clear();
            
            var lobbyCard = bettrUser.LobbyCards[0];
            
            yield return BettrAssetController.Instance.LoadPackage(lobbyCard.BundleName, lobbyCard.BundleVersion, false);
            
            // 9 groups, 12 per group
            for (var groupIndex = 1; groupIndex <= 9; groupIndex++)
            {
                var group = $"Group{groupIndex}";
                var machineGroupProperty = (TilePropertyGameObjectGroup) self[group];
                for (var cardIndex = 1; cardIndex <= 12; cardIndex++)
                {
                    GameObject quadGameObject = null;
                    var lobbyCardIndex = (groupIndex - 1) * 12 + cardIndex - 1;
                    var lobbyCardId = $"LobbyCard{lobbyCardIndex+1:D3}";
                    
                    Debug.Log($"LoadLobbyCards group={group} lobbyCardIndex={lobbyCardIndex} lobbyCardId={lobbyCardId} groupIndex={groupIndex} cardIndex={cardIndex}");
                    
                    try
                    {
                        lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
                        var machineCardProperty = machineGroupProperty[lobbyCardId];

                        if (machineCardProperty == null)
                        {
                            Debug.LogWarning($"LoadApp machineCardProperty is nil lobbyCard={lobbyCard} lobbyCard.MachineName={lobbyCard.MachineName} lobbyCard.MaterialName={lobbyCard.MaterialName} card={lobbyCard.Card}");
                        }
                        else
                        {
                            var lobbyCardGameObject = machineCardProperty.GameObject;
                            if (lobbyCardGameObject == null) continue;
                            string lobbyCardKey = group + "__" + lobbyCard.Card;
                            
                            Debug.Log($"LoadApp lobbyCardGameObject={lobbyCardGameObject.name} renaming to lobbyCardKey={lobbyCardKey}");

                            lobbyCardGameObject.name = lobbyCardKey;

                            quadGameObject = lobbyCardGameObject.transform.GetChild(0).GetChild(0).gameObject;

                            State.LobbyCardMap[lobbyCardKey] = lobbyCard;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        throw;
                    }
                    
                    yield return BettrAssetController.Instance.LoadMaterial(lobbyCard.BundleName, lobbyCard.BundleVersion, lobbyCard.MaterialName, quadGameObject);
                }
            }
        }
        
        public Tuple<string, string> GetLobbyCardExperiment(BettrLobbyCardConfig lobbyCard)
        {
            var experimentVariant = BettrExperimentController.GetMachineExperimentVariant(lobbyCard.MachineBundleName, lobbyCard.MachineBundleVariant);
            return new Tuple<string, string>(lobbyCard.MachineBundleName, experimentVariant);
        } 
        
        public Tuple<string, string> GetLobbyExperiment(BettrBundleConfig lobby)
        {
            var experimentVariant = BettrExperimentController.GetMachineExperimentVariant(lobby.BundleName, lobby.BundleVersion);
            return new Tuple<string, string>(lobby.BundleName, experimentVariant);
        } 
    }
}