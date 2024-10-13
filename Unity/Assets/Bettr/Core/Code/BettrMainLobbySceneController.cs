using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.UI;
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
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public BettrMainLobbySceneControllerState State = new BettrMainLobbySceneControllerState();
        
        public BettrExperimentController BettrExperimentController { get; private set; }
        
        public bool IsMainLobbyLoaded { get; private set; }
        
        public Dictionary<string, Material> LobbyCardMaterialMap { get; private set; }

        [NonSerialized] private int LoadedLobbyCardCount = 0;
        
        [NonSerialized] private int TotalLobbyCardCount = 0;
        
        public BettrMainLobbySceneController(BettrExperimentController bettrExperimentController)
        {
            TileController.RegisterType<BettrMainLobbySceneController>("BettrMainLobbySceneController");
            TileController.AddToGlobals("BettrMainLobbySceneController", this);
            
            BettrExperimentController = bettrExperimentController;

            IsMainLobbyLoaded = false;
            
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
        
        GameObject FindChildRecursive(GameObject parentGO, string childName)
        {
            var parent = parentGO.transform;
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child.gameObject;
                }
                // Recursively search through each child's children
                GameObject foundChild = FindChildRecursive(child.gameObject, childName);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }
            return null; // Return null if the child is not found
        }

        public void LoadLobbySideBar(Table self, string lobbyCardName)
        {
            var sideBar = (TilePropertyGameObjectGroup) self["SideBar"];
            if (sideBar == null)
            {
                Debug.LogError($"LoadLobbySideBar sideBar is null lobbyCardName={lobbyCardName}");
                return;
            }

            var gameDetails = (PropertyGameObject) sideBar["GameDetails"];
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            if (bettrUser == null)
            {
                Debug.Log($"LoadLobbyCardMachine invalid BettrUserConfig");
                return;
            }
            var lobbyCardIndex = FindLobbyCardIndex(lobbyCardName);
            if (lobbyCardIndex == -1)
            {
                Debug.Log($"LoadLobbyCardMachine invalid lobbyCardIndex={lobbyCardIndex} lobbyCardName={lobbyCardName}");
                return;
            }
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            var cachedAssetBundle = BettrAssetController.Instance.GetCachedAssetBundle(lobbyCard.BundleName, lobbyCard.BundleVersion, false);
            if (cachedAssetBundle == null)
            {
                Debug.Log($"Lobby cachedAssetBundle is null assetBundleName={lobbyCard.BundleName} assetBundleVersion={lobbyCard.BundleVersion} isScene=false");
                return;
            }
            var textureName = lobbyCard.MaterialName;
            var texture = cachedAssetBundle.LoadAsset<Texture2D>(textureName);
            if (texture == null)
            {
                Debug.LogError($"LoadLobbySideBar texture is null textureName={textureName} lobbyCardName={lobbyCardName}");
                return;
            }
            var imageGameObject = FindChildRecursive(gameDetails.GameObject, "Image");
            if (imageGameObject == null)
            {
                Debug.LogError($"LoadLobbySideBar imageGameObject is null lobbyCardName={lobbyCardName}");
                return;
            }
            var imageComponent = imageGameObject.GetComponent<Image>();
            if (imageComponent == null)
            {
                Debug.LogError($"LoadLobbySideBar imageComponent is null lobbyCardName={lobbyCardName}");
                return;
            }
            imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageComponent.color = new Color(2f, 2f, 2f, 1f);
            
            // load the preview text asset
            // split the textureName which is of the form Game<NNN>__<Variant>__LobbyCard into Game<NNN><Variant>
            var textAssetName = textureName.Replace("__LobbyCard", "").Replace("__", "");
            var textAsset = cachedAssetBundle.LoadAsset<TextAsset>(textAssetName);
            // get the Details GameObject which has the TextMeshPro input field which is read only
            var detailsGameObject = FindChildRecursive(gameDetails.GameObject, "Details");
            if (detailsGameObject == null)
            {
                Debug.LogError($"LoadLobbySideBar detailsGameObject is null textAssetName={textAssetName} lobbyCardName={lobbyCardName}");
                return;
            }
            // set the text of the TextMeshPro input field
            var textMeshPro = detailsGameObject.GetComponent<TMPro.TMP_InputField>();
            if (textMeshPro == null)
            {
                Debug.LogError("LoadLobbySideBar detailsGameObject textMeshPro is null lobbyCardName={lobbyCardName}");
                return;
            }
            // set the rich text to true
            textMeshPro.richText = true;
            // set the text to the textAsset text
            textMeshPro.text = textAsset.text;
            
            gameDetails.SetActive(true);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public int FindLobbyCardIndex(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            // lobbyCardName example Game001__LobbyCard001
            if (!lobbyCardName.Contains("__"))
            {
                Debug.Log($"FindLobbyCard invalid lobbyCardName={lobbyCardName}");
                return -1;
            }
            var lobbyCardIndex = bettrUser.FindLobbyCardIndexById(lobbyCardName.Split("__")[1]);
            return lobbyCardIndex;
        }

        public IEnumerator LoadLobbyCardMachine(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCardIndex = FindLobbyCardIndex(lobbyCardName);
            if (lobbyCardIndex == -1)
            {
                Debug.Log($"LoadLobbyCardMachine invalid lobbyCardIndex={lobbyCardIndex} lobbyCardName={lobbyCardName}");
                yield break;
            }
            BettrUserController.Instance.DisableUserPreviewMode();
            bettrUser.LobbyCardIndex = lobbyCardIndex;
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            bettrUser.LobbyCardName = lobbyCard.Card;
            var (machineName, machineVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineName, machineVariant);
            yield return BettrAssetController.Instance.LoadScene(machineName, machineVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadLobbyCardMachinePreview(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCardIndex = FindLobbyCardIndex(lobbyCardName);
            if (lobbyCardIndex == -1)
            {
                Debug.Log($"LoadLobbyCardMachinePreview invalid lobbyCardIndex={lobbyCardIndex} lobbyCardName={lobbyCardName}");
                yield break;
            }
            BettrUserController.Instance.EnableUserPreviewMode();
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

        public void ToggleVolume(Table mainLobbyTable)
        {
            BettrAudioController.Instance.ToggleVolume();
            // get the game object
            var volumeButton = (PropertyGameObject) mainLobbyTable["VolumeButton"];
            // get the Image Component
            var imageComponent = volumeButton.GameObject.GetComponent<Image>();
            // if volume is on set the color to 54,233,12,255 else set it to 233,54,12,255
            imageComponent.color = BettrAudioController.Instance.IsVolumeOn() ? new Color(54f / 255f, 233f / 255f, 12f / 255f, 1f) : new Color(233f / 255f, 54f / 255f, 12f / 255f, 1f);
        }
        
        public IEnumerator WaitUntilMainLobbyLoaded()
        {
            while (!IsMainLobbyLoaded)
            {
                yield return null;
            }
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

        public IEnumerator UpdateLoadingText(Table self)
        {
            while (!IsMainLobbyLoaded)
            {
                var loadingTextProperty = (PropertyTextMeshPro) self["LoadingText"];
                if (loadingTextProperty != null)
                {
                    if (LoadedLobbyCardCount > 0)
                    {
                        loadingTextProperty.SetText($"Adding Game {LoadedLobbyCardCount} of {TotalLobbyCardCount} ...");
                    }
                }
                yield return null;
            }
        }
        
        // Convert the method from Lua to C#
        public IEnumerator LoadLobbyCards(Table self)
        {
            Console.WriteLine("LoadLobbyCards invoked");

            LoadedLobbyCardCount = 0;
            
            var bettrUser = BettrUserController.Instance.BettrUserConfig;

            var totalLobbyCardCount = bettrUser.LobbyCards.Count;
            TotalLobbyCardCount = totalLobbyCardCount;

            if (IsMainLobbyLoaded)
            {
                // turn off the loading screen
                var loadingProperty = (PropertyGameObject) self["Loading"];
                if (loadingProperty != null)
                {
                    loadingProperty.SetActive(false);
                }
            }
            
            BettrRoutineRunner.Instance.StartCoroutine(UpdateLoadingText(self));
            
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

                    if (IsMainLobbyLoaded)
                    {
                        LoadedLobbyCardCount++;
                        
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
                        
                        LoadedLobbyCardCount++;
                    }
                }
            }
            
            IsMainLobbyLoaded = true;
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