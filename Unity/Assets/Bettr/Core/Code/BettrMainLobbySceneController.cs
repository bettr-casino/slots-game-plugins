using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using Object = UnityEngine.Object;

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
        
        public bool IsMainLobbyCardsAlreadyLoaded { get; private set; }
        
        public Dictionary<string, Material> LobbyCardMaterialMap { get; private set; }
        
        public BettrMainLobbySceneController(BettrExperimentController bettrExperimentController)
        {
            TileController.RegisterType<BettrMainLobbySceneController>("BettrMainLobbySceneController");
            TileController.AddToGlobals("BettrMainLobbySceneController", this);
            
            BettrExperimentController = bettrExperimentController;

            IsMainLobbyCardsAlreadyLoaded = false;
            
            LobbyCardMaterialMap = new Dictionary<string, Material>();
        }

        public void SetSelector(Table mainLobbyTable, GameObject gameObject)
        {
            if (gameObject == null)
            {
                // get the LobbyCardName from the BettrUserConfig. Null check the BettrUserConfig
                var bettrUser = BettrUserController.Instance.BettrUserConfig;
                if (bettrUser != null)
                {
                    var lobbyCardName = bettrUser.LobbyCardName;
                    // null check
                    if (!string.IsNullOrEmpty(lobbyCardName))
                    {
                        var groupIndex = 0;
                        while (groupIndex < 9)
                        {
                            groupIndex += 1;
                            var group = $"Group{groupIndex}";
                            var machineGroupProperty = (TilePropertyGameObjectGroup) mainLobbyTable[group];
                            if (machineGroupProperty == null) continue;
                            var machineCardProperty = machineGroupProperty[lobbyCardName];
                            if (machineCardProperty == null) continue;
                            gameObject = machineCardProperty.GameObject;
                            break;
                        }
                    }
                }
            }
            var bettrCardSelectorProperty = (PropertyGameObject) mainLobbyTable["LobbyCardSelector"];
            if (gameObject != null)
            {
                bettrCardSelectorProperty.GameObject.transform.position = gameObject.transform.position;
            }
            bettrCardSelectorProperty.SetActive(true);
        }

        public IEnumerator LoadLobbyCardMachine(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            // lobbyCardName example Game001__LobbyCard001
            if (!lobbyCardName.Contains("__"))
            {
                Debug.Log($"LoadLobbyCardMachine invalid lobbyCardName={lobbyCardName}");
                yield break;
            }
            var lobbyCardIndex = bettrUser.FindLobbyCardIndexById(lobbyCardName.Split("__")[1]);
            if (lobbyCardIndex == -1)
            {
                Debug.Log($"LoadLobbyCardMachine invalid lobbyCardIndex={lobbyCardIndex} lobbyCardName={lobbyCardName}");
                yield break;
            }
            bettrUser.LobbyCardIndex = lobbyCardIndex;
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            bettrUser.LobbyCardName = lobbyCard.Card;
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
            bettrUser.LobbyCardName = lobbyCard.Card;
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
            bettrUser.LobbyCardName = lobbyCard.Card;
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
            bettrUser.LobbyCardName = lobbyCard.Card;
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

            if (IsMainLobbyCardsAlreadyLoaded)
            {
                // turn off the loading screen
                var loadingProperty = (PropertyGameObject) self["Loading"];
                if (loadingProperty != null)
                {
                    loadingProperty.SetActive(false);
                }
            }
            
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
                    
                    // Debug.Log($"LoadLobbyCards group={group} lobbyCardIndex={lobbyCardIndex} lobbyCardId={lobbyCardId} groupIndex={groupIndex} cardIndex={cardIndex}");
                    
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
                            
                            // Debug.Log($"LoadApp lobbyCardGameObject={lobbyCardGameObject.name} renaming to lobbyCardKey={lobbyCardKey}");

                            lobbyCardGameObject.name = lobbyCardKey;

                            quadGameObject = lobbyCardGameObject.transform.GetChild(0).GetChild(0).gameObject;

                            State.LobbyCardMap[lobbyCardKey] = lobbyCard;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"error cardIndex={cardIndex} lobbyCardIndex={lobbyCardIndex} groupIndex={groupIndex} lobbyCardId={lobbyCardId}");
                        Debug.LogError(e);
                        // throw; // continue with invalid cards
                    }

                    if (IsMainLobbyCardsAlreadyLoaded)
                    {
                        if (quadGameObject != null)
                        {
                            // Get the MeshRenderer component
                            var renderer = quadGameObject.GetComponent<MeshRenderer>();
    
                            // Destroy the old material if it exists
                            if (renderer.material != null)
                            {
                                Object.Destroy(renderer.material);
                            }
    
                            // Load the material from the cache and assign it
                            if (LobbyCardMaterialMap.TryGetValue(lobbyCardId, out var newMaterial))
                            {
                                renderer.material = newMaterial;
                            }
                        }
                    }
                    else
                    {
                        yield return BettrAssetController.Instance.LoadMaterial(lobbyCard.BundleName, lobbyCard.BundleVersion, lobbyCard.MaterialName, quadGameObject);
                    
                        // save the material to BettrMainLobbySceneController for later
                        LobbyCardMaterialMap[lobbyCardId] = quadGameObject?.GetComponent<MeshRenderer>().material;
                    }
                }
            }
            
            IsMainLobbyCardsAlreadyLoaded = true;
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