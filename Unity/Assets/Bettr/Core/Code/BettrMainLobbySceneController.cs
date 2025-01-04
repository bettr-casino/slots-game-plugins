using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using CrayonScript.Interpreter.Execution.VM;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrMainLobbySceneControllerState
    {
        public BettrMainLobbySceneControllerState()
        {
            TileController.RegisterType<BettrMainLobbySceneControllerState>("BettrMainLobbySceneControllerState");
        }
    }
    
    public class BettrMainLobbySceneController
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public BettrMainLobbySceneControllerState State = new BettrMainLobbySceneControllerState();
        
        public BettrExperimentController BettrExperimentController { get; private set; }
        
        public bool IsMainLobbyLoaded { get; private set; }

        public int CurrentPageNumber = 1;

        public BettrLobbyCardConfig CurrentLobbyCard { get; private set; }
        
        public string webAssetBaseURL;
        
        public Dictionary<string, Material> LobbyCardMaterialMap { get; private set; }

        public BettrMainLobbyManifests Manifests { get; private set; }
        
        [NonSerialized] private int LoadedLobbyCardCount = 0;
        
        [NonSerialized] private int TotalLobbyCardCount = 0;
        
        [NonSerialized] private int LobbyCardStartIndex = 0;
        
        [NonSerialized] private int LobbyCardEndIndex = 0;
        
        [NonSerialized] private string TopPanelLobbyCardPropertyId;

        [NonSerialized] private Tile GameTile;
        
        [NonSerialized] private Tile BaseGameMachineTile;
        
        public BettrMainLobbySceneController(BettrExperimentController bettrExperimentController)
        {
            TileController.RegisterType<BettrMainLobbySceneController>("BettrMainLobbySceneController");
            TileController.AddToGlobals("BettrMainLobbySceneController", this);
            
            BettrExperimentController = bettrExperimentController;

            IsMainLobbyLoaded = false;
            
            LobbyCardMaterialMap = new Dictionary<string, Material>();
        }

        public IEnumerator LoadManifests()
        {
            Manifests = new BettrMainLobbyManifests();
            string webAssetName = "lobbycardv0_1_0.merged.control.manifest.gz";
            string assetURL = $"{webAssetBaseURL}/{webAssetName}";

            using UnityWebRequest webRequest = UnityWebRequest.Get(assetURL);
            // Add header to accept gzip encoding
            webRequest.SetRequestHeader("Accept-Encoding", "gzip");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error loading manifest error={webRequest.error} webAssetName={webAssetName}");
            }
            else
            {
                try
                {
                    // Directly use the text from the downloadHandler since Unity might have already decompressed it.
                    string jsonResponse = webRequest.downloadHandler.text;
                    Manifests = JsonConvert.DeserializeObject<BettrMainLobbyManifests>(jsonResponse);
                    Debug.Log($"Manifest loaded successfully webAssetName={webAssetName}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing manifest error={e.Message} webAssetName={webAssetName}");
                }
            }
        }

        public void SetTopPanelSelector(Table mainLobbyTable, GameObject gameObject)
        {
            var bettrCardSelectorProperty = (PropertyGameObject) mainLobbyTable["LobbyCardSelector"];
            if (gameObject != null)
            {
                bettrCardSelectorProperty.GameObject.transform.position = gameObject.transform.position;
            }
            bettrCardSelectorProperty.SetActive(true);
        }
        
        public IEnumerator UnloadTopPanelGameAsync(string machineBundleName, string machineBundleVariant)
        {
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineBundleName, machineBundleVariant);
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

        public GameObject GetTopPanelGameObject(Table self, string key)
        {
            var group = "TopPanel";
            var groupProperty = (TilePropertyGameObjectGroup) self[group];
            var property = groupProperty[key];
            var gameObject = property.GameObject;
            return gameObject;
        }
        
        public BettrLobbyCardConfig GetTopPanelLobbyCard(Table self, string key)
        {
            var gameObject = GetTopPanelGameObject(self, key);
            var lobbyCardKey = gameObject.name;
            // find from the LobbyCards
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCard = bettrUser.LobbyCards.Find(lc => lc.Card == lobbyCardKey);
            return lobbyCard;
        }

        public bool IsTopPanelVideoCardActive(Table self)
        {
            var gamePanelProperty = (PropertyGameObject) self["GamePanel"];
            var gamePanel = gamePanelProperty.GameObject;
            var childCount = gamePanel.transform.childCount;
            return childCount == 0;
        }

        private IEnumerator OnTopPanelPrevClick(Table self)
        {
            if (CurrentPageNumber <= 1) 
            {
                yield break;
            }
            CurrentPageNumber--;
            yield return LoadLobbyPage(self, CurrentPageNumber);
        }

        private IEnumerator OnTopPanelNextClick(Table self)
        {
            var maxPageNumber = GetMaxPageNumber();
            if (CurrentPageNumber >= maxPageNumber)
            {
                yield break;
            }
            CurrentPageNumber++;
            yield return LoadLobbyPage(self, CurrentPageNumber);
        }

        public int GetMaxPageNumber()
        {
            var lobbyCardsPerPage = 8;
            var pageNumber = 1;
            var numLobbyCardsToLoad = lobbyCardsPerPage;
            var endIndex = numLobbyCardsToLoad;
            while (endIndex < TotalLobbyCardCount)
            {
                pageNumber += 1;
                numLobbyCardsToLoad = lobbyCardsPerPage;
                var startIndex = (pageNumber - 1) * lobbyCardsPerPage;
                endIndex = startIndex + numLobbyCardsToLoad;
            }
            return pageNumber;
        }

        private void UpdateVolumeControls(Table self)
        {
            ((PropertyGameObject) self["VolumeButton"]).SetActive(BettrAudioController.Instance.IsVolumeOn());
            ((PropertyGameObject) self["VolumeOffButton"]).SetActive(!BettrAudioController.Instance.IsVolumeOn());
        }

        public IEnumerator OnSettingsClick(Table self, string settingsPropertyKey)
        {
            var group = "Settings";
            var settingsPropertyId = settingsPropertyKey.Replace($"{group}__", "");

            switch (settingsPropertyId)
            {
                case "VolumeOn":
                case "VolumeOff":    
                    ToggleVolume(self);
                    UpdateVolumeControls(self);
                    break;
                case "Info":
                    var isGamePanelActive = !IsTopPanelVideoCardActive(self);
                    if (!isGamePanelActive)
                    {
                        BettrVideoPlayerController.Instance.Replay();
                    }
                    break;
                case "Spin":
                    if (BaseGameMachineTile != null)
                    {
                        BaseGameMachineTile.Call("OnPointerClick");
                    }
                    break;
                default:
                    break;
            }
            
            yield break;
        }

        public IEnumerator OnTopPanelClick(Table self, string topPanelPropertyKey)
        {
            Debug.Log($"ScriptRunner.PoolSize={ScriptRunner.PoolSize}");

            var currentTopPanelPropertyId = TopPanelLobbyCardPropertyId;
            var wasGamePanelActive = !IsTopPanelVideoCardActive(self);
            if (wasGamePanelActive)
            {
                var currentTopPanelLobbyCard = GetTopPanelLobbyCard(self, currentTopPanelPropertyId);
                if (currentTopPanelLobbyCard != null)
                {
                    var machineName = currentTopPanelLobbyCard.MachineName;
                    var globals = (Table) self.OwnerScript.Globals;
                    var baseGameState = (Table) globals[$"{machineName}BaseGameState"];
                    var spinState = (Table) baseGameState["SpinState"];
                    var firstState = (Table) spinState["First"];
                    var state = (string) firstState["State"];
                    if (state != "Waiting")
                    {
                        Debug.Log($"OnTopPanelClick skip since Panel={currentTopPanelPropertyId} is in ({state}) state");
                        yield break;
                    }

                }
            }
            
            var group = "TopPanel";
            var topPanelPropertyId = topPanelPropertyKey.Replace($"{group}__", "");
            TopPanelLobbyCardPropertyId = topPanelPropertyId;
            
            var isGameCardClicked = TopPanelLobbyCardPropertyId != "LobbyCard001";
            
            if (topPanelPropertyId == "Prev")
            {
                yield return OnTopPanelPrevClick(self);
                yield break;
            }
            if (topPanelPropertyId == "Next")
            {
                yield return OnTopPanelNextClick(self);
                yield break;
            }
            
            // update the MachineControls
            var machineControlsProperty = (PropertyGameObject) self["MachineControls"];
            machineControlsProperty.SetActive(false);
            
            var gamePanelProperty = (PropertyGameObject) self["GamePanel"];
            var gamePanel = gamePanelProperty.GameObject;

            if (wasGamePanelActive)
            {
                // remove the children of the gamePanelProperty.GameObject
                foreach (Transform child in gamePanel.transform)
                {
                    Object.Destroy(child.gameObject);
                }
                    
                var currentTopPanelLobbyCard = GetTopPanelLobbyCard(self, currentTopPanelPropertyId);
                if (currentTopPanelLobbyCard != null)
                {
                    var currentMachineBundleName = currentTopPanelLobbyCard.MachineBundleName;
                    var currentMachineBundleVariant = currentTopPanelLobbyCard.MachineBundleVariant;
                    // unload any cached version
                    yield return BettrAssetController.Instance.UnloadCachedAssetBundle(currentMachineBundleName, currentMachineBundleVariant);
                }
            }
            
            // this is on a card
            var groupProperty = (TilePropertyGameObjectGroup) self[group];
            var property = groupProperty[topPanelPropertyId];
            var gameObject = property.GameObject;

            SetTopPanelSelector(self, gameObject);

            if (!isGameCardClicked)
            {
                yield break;
            }
            
            // this is click on a game card
            // first locate the card using the game object name
            var card = property.GameObject.name;
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCards = bettrUser.LobbyCards;
            // find the LobbyCard by card
            var lobbyCard = lobbyCards.Find(lc => lc.Card == card);
            if (lobbyCard != null)
            {
                CurrentLobbyCard = lobbyCard;
                yield return LoadLobbySideBar(self, card);
            }

        }

        private IEnumerator EnableMachine(Table self, BettrLobbyCardConfig lobbyCard)
        {
            // get the machineName and machineVariant
            var machineBundleName = lobbyCard.MachineBundleName;
            var machineBundleVariant = lobbyCard.MachineBundleVariant;
            var machineName = lobbyCard.MachineName;
            var machineVariant = lobbyCard.GetMachineVariant();
            var machineSceneName = lobbyCard.MachineSceneName;
            
            var gamePanelProperty = (PropertyGameObject) self["GamePanel"];
            var gamePanel = gamePanelProperty.GameObject;
            
            // update the MachineControls
            var machineControlsProperty = (PropertyGameObject) self["MachineControls"];
            machineControlsProperty.SetActive(false);

            yield return LoadGamePrefabAsync(machineBundleName, machineBundleVariant, machineName, machineVariant, gamePanel);

            var properties = new string[] { "CreditsText", "BetText", "WinText" };
            foreach (var p in properties)
            {
                var propValue = self[p];
                BaseGameMachineTile?.SetProperty(p, propValue);
            }
            
            // yield return null to wait for start to be called
            yield return null;
                
            BaseGameMachineTile?.Call("ConfigureSettings");
            
            yield return BaseGameMachineTile?.CallAction("ShowSettings", self);
        }

        public IEnumerator HideLobbySideBar(Table self)
        {
            var sideBar = (TilePropertyGameObjectGroup) self["SideBar"];
            if (sideBar == null)
            {
                Debug.LogError($"LoadLobbySideBar sideBar is null");
                yield break;
            }

            var gameDetails = (PropertyGameObject) sideBar["GameDetails"];
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            if (bettrUser == null)
            {
                Debug.Log($"LoadLobbyCardMachine invalid BettrUserConfig");
                yield break;
            }
            
            gameDetails.SetActive(false);
        }

        public IEnumerator LoadLobbySideBar(Table self, string lobbyCardName)
        {
            var sideBar = (TilePropertyGameObjectGroup) self["SideBar"];
            if (sideBar == null)
            {
                Debug.LogError($"LoadLobbySideBar sideBar is null lobbyCardName={lobbyCardName}");
                yield break;
            }

            var gameDetails = (PropertyGameObject) sideBar["GameDetails"];
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            if (bettrUser == null)
            {
                Debug.Log($"LoadLobbyCardMachine invalid BettrUserConfig");
                yield break;
            }
            var lobbyCardIndex = FindLobbyCardIndex(lobbyCardName);
            if (lobbyCardIndex == -1)
            {
                Debug.Log($"LoadLobbyCardMachine invalid lobbyCardIndex={lobbyCardIndex} lobbyCardName={lobbyCardName}");
                yield break;
            }
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            
            var cachedAssetBundle = AssetBundle.GetAllLoadedAssetBundles()
                .FirstOrDefault(bundle => bundle.name == lobbyCard.LobbyCardBundleId);
            if (cachedAssetBundle == null)
            {
                Debug.Log($"Lobby cachedAssetBundle is null assetBundleName={lobbyCard.BundleName} assetBundleVersion={lobbyCard.BundleVersion} isScene=false");
                yield break;
            }
            var textureName = lobbyCard.MaterialName;
            var texture = cachedAssetBundle.LoadAsset<Texture2D>(textureName);
            if (texture == null)
            {
                Debug.LogError($"LoadLobbySideBar texture is null textureName={textureName} lobbyCardName={lobbyCardName}");
                yield break;
            }
            var imageGameObject = FindChildRecursive(gameDetails.GameObject, "Image");
            if (imageGameObject == null)
            {
                Debug.LogError($"LoadLobbySideBar imageGameObject is null lobbyCardName={lobbyCardName}");
                yield break;
            }
            var imageComponent = imageGameObject.GetComponent<Image>();
            if (imageComponent == null)
            {
                Debug.LogError($"LoadLobbySideBar imageComponent is null lobbyCardName={lobbyCardName}");
                yield break;
            }
            imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageComponent.color = new Color(2f, 2f, 2f, 1f);
            
            // load the preview text asset
            // split the textureName which is of the form Game<NNN>__<Variant>__LobbyCard into Game<NNN><Variant>
            var textAssetName = textureName.Replace("__LobbyCard", "").Replace("__", "");
            var textAsset = cachedAssetBundle.LoadAsset<TextAsset>(textAssetName);
            var textMechanicsAssetName = $"{textAssetName}__Mechanics";
            var textMechanicsAsset = cachedAssetBundle.LoadAsset<TextAsset>(textMechanicsAssetName);
            // get the Details GameObject which has the TextMeshPro input field which is read only
            var detailsGameObject = FindChildRecursive(gameDetails.GameObject, "Details");
            if (detailsGameObject == null)
            {
                Debug.LogError($"LoadLobbySideBar detailsGameObject is null textAssetName={textAssetName} lobbyCardName={lobbyCardName}");
                yield break;
            }
            // set the text of the TextMeshPro input field
            var tmpInputField = detailsGameObject.GetComponent<TMPro.TMP_InputField>();
            if (tmpInputField == null)
            {
                Debug.LogError("LoadLobbySideBar detailsGameObject textMeshPro is null lobbyCardName={lobbyCardName}");
                yield break;
            }
            // set the rich text to true
            tmpInputField.richText = true;
            // set the text to the textAsset text
            tmpInputField.text = textAsset.text;
            tmpInputField.text += "\n\n";
            tmpInputField.text += textMechanicsAsset.text;
            
            var (machineBundleName, machineBundleVariant) = GetMachineBundleDetails(lobbyCardName);

            yield return StartGamePreviewMusic(machineBundleName, machineBundleVariant);
            
            gameDetails.SetActive(true);
        }

        public Tuple<string, string> GetMachineBundleDetails(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCardIndex = FindLobbyCardIndex(lobbyCardName);
            if (lobbyCardIndex == -1)
            {
                Debug.Log($"GetMachineDetails invalid lobbyCardIndex={lobbyCardIndex} lobbyCardName={lobbyCardName}");
                return null;
            }
            BettrUserController.Instance.DisableUserPreviewMode();
            bettrUser.LobbyCardIndex = lobbyCardIndex;
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            // TODO: FIXME fix hacky way to get the machine name and the machine variant
            var materialName = lobbyCard.MaterialName;
            // Material Name is of the form Game<NNN>__<Variant>__LobbyCard
            var machineBundleName = materialName.Split("__")[0];
            var machineBundleVariant = materialName.Split("__")[1];
            return new Tuple<string, string>(machineBundleName, machineBundleVariant);
        }
        
        private IEnumerator StartGamePreviewMusic(string machineBundleName, string machineBundleVariant)
        {
            yield return BettrAudioController.Instance.LoadBackgroundAudio($"{machineBundleName}{machineBundleVariant}");
            BettrAudioController.Instance.PlayGamePreviewAudioLoop(machineBundleName, machineBundleVariant,
                $"{machineBundleName}{machineBundleVariant}BackgroundMusic");
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public int FindLobbyCardIndex(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCardIndex = bettrUser.FindLobbyCardIndexById(lobbyCardName);
            return lobbyCardIndex;
        }
        
        private IEnumerator LoadGamePrefabAsync(string machineBundleName, string machineBundleVariant, string machineName, string machineVariant, GameObject parent)
        {
            // Load the prefab
            var prefabName = $"{machineName}{machineVariant}Prefab";
            yield return BettrAssetController.Instance.LoadPrefab(context:null, machineBundleName, machineBundleVariant, prefabName, parent);
            
            // get the loaded prefab
            var pivotGameObject = parent.transform.GetChild(0).gameObject;
            
            // turn off Machines
            var machines = FindChildRecursive(pivotGameObject, "Machines");
            if (machines != null)
            {
                machines.SetActive(false);
            }
            // Find the Background Camera Camera_Background
            var backgroundCamera = FindChildRecursive(pivotGameObject, "Camera_Background");
            // turn it off
            if (backgroundCamera != null)
            {
                backgroundCamera.SetActive(false);
            }
            
            // Find the UI Camera (To fix the > 1 active AudioListeners issue)
            var uiCamera = FindChildRecursive(pivotGameObject, "UI Camera");
            // disable it
            if (uiCamera != null)
            {
                uiCamera.SetActive(false);
            }
            
            // Find the BackgroundFBX from the pivotGameObject
            var backgroundFBX = FindChildRecursive(pivotGameObject, "BackgroundFBX");
            var backgroundGameObject = backgroundFBX.transform.parent.parent.transform.gameObject;
            
            // Find the Game GameObject
            var gameGameObject = FindChildRecursive(pivotGameObject, "Game");
            // Get the Game Tile component
            var gameTile = gameGameObject?.GetComponentInChildren<Tile>();
            GameTile = gameTile;
            
            // Get the Background Tile component
            var backgroundTile = backgroundGameObject?.GetComponentInChildren<Tile>();
            var backgroundTable = backgroundTile?.Type;
            
            // wait a few frames to ensure Awake and Start are called on the Tile components
            // ReSharper disable once PossibleNullReferenceException
            while (!gameTile.IsInitialized)
            {
                yield return null;
            }
            
            // wait a few frames to ensure Awake and Start are called on the Tile components
            // ReSharper disable once PossibleNullReferenceException
            while (!backgroundTile.IsInitialized)
            {
                yield return null;
            }
            
            var baseGameMachineGameObject = FindChildRecursive(pivotGameObject, $"{machineName}BaseGameMachine");
            // Get the Game Tile component
            var baseGameMachineTile = baseGameMachineGameObject?.GetComponentInChildren<Tile>();
            
            BaseGameMachineTile = baseGameMachineTile;
            
            var machineExperiment = machineBundleVariant; // this is the experiment variant
            
            yield return BettrVideoController.Instance.LoadBackgroundVideo(backgroundTable, machineName, machineVariant, machineExperiment);

            yield return BettrAudioController.Instance.LoadBackgroundAudio($"{machineName}{machineVariant}");
            
            // Turn on UI Camera (To fix the > 1 active AudioListeners issue)
            if (uiCamera != null)
            {
                uiCamera.SetActive(true);
            }            
            
            // turn on Background Camera
            if (backgroundCamera != null)
            {
                backgroundCamera.SetActive(true);
            }
            
            // check BettrVideoController.Instance.HasBackgroundVideo
            if (BettrVideoController.Instance.HasBackgroundVideo)
            {
                // Play Audio
                BettrAudioController.Instance.PlayGameAudioLoop(machineName, machineVariant, $"{machineName}{machineVariant}BackgroundMusic");
                
                yield return BettrVideoController.Instance.PlayBackgroundVideo(backgroundTable, machineName, machineVariant, machineExperiment);
                
                // wait until video preparation is complete
                yield return new WaitUntil(() => BettrVideoController.Instance.VideoPreparationComplete);
                
                if (!BettrVideoController.Instance.VideoPreparationError)
                {
                    // wait until VideoStartedPlaying is true
                    yield return new WaitUntil(() => BettrVideoController.Instance.VideoStartedPlaying);
                }
            }
            
            // Commented out since VideoLoopPointReached is controlled from PlayBackgroundVideo
            // if (!BettrVideoController.Instance.VideoPreparationError)
            // {
            //     // wait until VideoLoopPointReached is true
            //     yield return new WaitUntil(() => BettrVideoController.Instance.VideoLoopPointReached);
            // }
            
            // turn on Machines
            if (machines != null)
            {
                machines.SetActive(true);
            }

            // set the base game to active
            if (gameTile != null)
            {
                gameTile?.Call("SetBaseGameActive", true);
            }
        }

        private IEnumerator LoadGameSceneAsync(string machineSceneName, string machineBundleName, string machineBundleVariant, string machineName, string machineVariant)
        {
            // TODO: move this into a separate method
            
            // get the current scene
            Scene currentScene = SceneManager.GetActiveScene();
            string currentSceneName = currentScene.name;
            
            // this will be loaded in Additive mode
            yield return BettrAssetController.Instance.LoadScene(machineBundleName, machineBundleVariant, machineSceneName, loadSingle:false);
            
            Scene newScene = SceneManager.GetSceneByName(machineSceneName);
            // get root game objects in newScene
            GameObject[] rootGameObjects = newScene.GetRootGameObjects();
            // Find Pivot from rootGameObjects
            GameObject pivotGameObject = rootGameObjects.FirstOrDefault(go => go.name == "Pivot");
            // turn off Machines
            var machines = FindChildRecursive(pivotGameObject, "Machines");
            if (machines != null)
            {
                machines.SetActive(false);
            }
            // Find the Background Camera Camera_Background
            var backgroundCamera = FindChildRecursive(pivotGameObject, "Camera_Background");
            // turn it off
            if (backgroundCamera != null)
            {
                backgroundCamera.SetActive(false);
            }
            
            // Find the UI Camera (To fix the > 1 active AudioListeners issue)
            var uiCamera = FindChildRecursive(pivotGameObject, "UI Camera");
            // disable it
            if (uiCamera != null)
            {
                uiCamera.SetActive(false);
            }
            
            // Find the BackgroundFBX from the pivotGameObject
            var backgroundFBX = FindChildRecursive(pivotGameObject, "BackgroundFBX");
            var backgroundGameObject = backgroundFBX.transform.parent.parent.transform.gameObject;
            
            // Find the Game GameObject
            var gameGameObject = FindChildRecursive(pivotGameObject, "Game");
            // Get the Game Tile component
            var gameTile = gameGameObject?.GetComponentInChildren<Tile>();
            
            // Get the Background Tile component
            var backgroundTile = backgroundGameObject?.GetComponentInChildren<Tile>();
            var backgroundTable = backgroundTile?.Type;
            
            // now activate the scene
            SceneManager.SetActiveScene(newScene);
            
            // wait a few frames to ensure Awake and Start are called on the Tile components
            // ReSharper disable once PossibleNullReferenceException
            while (!gameTile.IsInitialized)
            {
                yield return null;
            }
            
            // wait a few frames to ensure Awake and Start are called on the Tile components
            // ReSharper disable once PossibleNullReferenceException
            while (!backgroundTile.IsInitialized)
            {
                yield return null;
            }
            
            var machineExperiment = machineBundleVariant; // this is the experiment variant
            
            yield return BettrVideoController.Instance.LoadBackgroundVideo(backgroundTable, machineName, machineVariant, machineExperiment);
            
            // unload the current scene
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentScene);
            yield return new WaitUntil(() => asyncUnload.isDone);
            
            // Turn on UI Camera (To fix the > 1 active AudioListeners issue)
            if (uiCamera != null)
            {
                uiCamera.SetActive(true);
            }            
            
            // turn on Background Camera
            if (backgroundCamera != null)
            {
                backgroundCamera.SetActive(true);
            }
            
            // check BettrVideoController.Instance.HasBackgroundVideo
            if (BettrVideoController.Instance.HasBackgroundVideo)
            {
                yield return BettrVideoController.Instance.PlayBackgroundVideo(backgroundTable, machineName, machineVariant, machineExperiment);
                
                // wait until video preparation is complete
                yield return new WaitUntil(() => BettrVideoController.Instance.VideoPreparationComplete);
                
                if (!BettrVideoController.Instance.VideoPreparationError)
                {
                    // wait until VideoStartedPlaying is true
                    yield return new WaitUntil(() => BettrVideoController.Instance.VideoStartedPlaying);
                }
            }
            
            // check if VideoPreparationError is true
            if (!BettrVideoController.Instance.VideoPreparationError)
            {
                // wait until VideoLoopPointReached is true
                yield return new WaitUntil(() => BettrVideoController.Instance.VideoLoopPointReached);
            }
            
            // turn on Machines
            if (machines != null)
            {
                machines.SetActive(true);
            }
            
            // set the base game to active
            gameTile.Call("SetBaseGameActive", true);
        }
        
        public IEnumerator LoadLobbyCardMachinePreview(Table self)
        {
            BettrUserController.Instance.EnableUserPreviewMode();
            BettrVisualsController.Instance.Reset();
            yield return HideLobbySideBar(self);
            yield return EnableMachine(self, CurrentLobbyCard);
        }
        
        public IEnumerator LoadLobbyCardMachine(Table self)
        {
            BettrUserController.Instance.DisableUserPreviewMode();
            BettrVisualsController.Instance.Reset();
            yield return HideLobbySideBar(self);
            yield return EnableMachine(self, CurrentLobbyCard);
        }

        public IEnumerator LoadMachine()
        {
            // TODO: this should be from the user preferences
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            bettrUser.LobbyCardIndex = 0;
            var lobbyCard = bettrUser.LobbyCards[0];
            bettrUser.LobbyCardName = lobbyCard.Card;
            var (machineBundleName, machineBundleVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineBundleName, machineBundleVariant);
            
            // get the machineName and machineVariant
            var machineName = lobbyCard.MachineName;
            var machineVariant = lobbyCard.GetMachineVariant();
            var machineSceneName = lobbyCard.MachineSceneName;

            yield return LoadGameSceneAsync(machineSceneName, machineBundleName, machineBundleVariant, machineName, machineVariant);
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
            var (machineBundleName, machineBundleVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineBundleName, machineBundleVariant);
            
            // get the machineName and machineVariant
            var machineName = lobbyCard.MachineName;
            var machineVariant = lobbyCard.GetMachineVariant();
            var machineSceneName = lobbyCard.MachineSceneName;

            yield return LoadGameSceneAsync(machineSceneName, machineBundleName, machineBundleVariant, machineName, machineVariant);
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
            var (machineBundleName, machineBundleVariant) = GetLobbyCardExperiment(lobbyCard);
            // unload any cached version
            yield return BettrAssetController.Instance.UnloadCachedAssetBundle(machineBundleName, machineBundleVariant);
            
            // get the machineName and machineVariant
            var machineName = lobbyCard.MachineName;
            var machineVariant = lobbyCard.GetMachineVariant();
            var machineSceneName = lobbyCard.MachineSceneName;

            yield return LoadGameSceneAsync(machineSceneName, machineBundleName, machineBundleVariant, machineName, machineVariant);
        }

        public void ToggleVolume(Table mainLobbyTable)
        {
            BettrAudioController.Instance.ToggleVolume();
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

        public IEnumerator LoadLobbyPage(Table self, int pageNumber)
        {
            var lobbyCardsPerPage = 8;
            var numLobbyCardsToLoad = lobbyCardsPerPage;
            var lobbyCardOffset = 2;
            var bettrUser = BettrUserController.Instance.BettrUserConfig;

            var totalLobbyCardCount = bettrUser.LobbyCards.Count;
            TotalLobbyCardCount = totalLobbyCardCount;
            
            // calculate the start index for the pageNumber
            // 1st page number has only 8 cards, rest have 9
            // pageNumber startIndex
            // 1 0
            // 2 8
            // 3 16
            var startIndex = (pageNumber - 1) * lobbyCardsPerPage;
            LobbyCardStartIndex = startIndex;
            
            var endIndex = startIndex + numLobbyCardsToLoad;
            if (endIndex >= TotalLobbyCardCount)
            {
                // readjust the startIndex
                endIndex = TotalLobbyCardCount;
                startIndex = endIndex - numLobbyCardsToLoad;
            }
            LobbyCardEndIndex = endIndex;
            
            var manifests = new List<BettrAssetMultiByteRange>();
            
            //get lobby cards from startIndex to endIndex
            var lobbyCardConfigs = bettrUser.LobbyCards.GetRange(startIndex, numLobbyCardsToLoad);
            foreach (var lobbyCardConfig in lobbyCardConfigs)
            {
                var machineBundleName = lobbyCardConfig.MachineBundleName;
                var machineBundleVariant = lobbyCardConfig.MachineBundleVariant;
                // find the manifest
                var manifest = Manifests.FindManifest(machineBundleName, machineBundleVariant);
                if (manifest == null)
                {
                    Debug.LogError($"LoadLobbyPage manifest is null machineBundleName={machineBundleName} machineBundleVariant={machineBundleVariant} pageNumber={pageNumber}");
                    yield break;
                }
                manifests.Add(manifest);
            }

            var groupName = "TopPanel";
            var groupProperty = (TilePropertyGameObjectGroup) self[groupName];

            var binaryFile = "lobbycardv0_1_0.merged.control.bin";

            BettrAssetController.Instance.LoadMultiByteRangeAssets(binaryFile, manifests,
                (manifest, machineBundleName, machineBundleVariant, assetBundle, assetBundleManifest, success, loaded, error) =>
                {
                    // gets called once per asset bundle
                    if (!success)
                    {
                        Debug.LogError($"MultiByteRange asset={machineBundleName} version={machineBundleVariant} success={false} loaded={loaded} error={error}");
                        return;
                    }
                    
                    Debug.Log($"MultiByteRange asset={machineBundleName} version={machineBundleVariant} success={true} loaded={loaded}");

                    var index = manifests.FindIndex(m => m == manifest);
                    var lobbyCard = lobbyCardConfigs[index];
                    var propertyId = $"LobbyCard{index + lobbyCardOffset:D3}";
                    var machineCardProperty = groupProperty[propertyId];
                    if (machineCardProperty == null)
                    {
                        Debug.LogWarning($"LoadApp machineCardProperty is nil group={groupName} lobbyCard={propertyId} lobbyCard.MachineName={lobbyCard.MachineName} lobbyCard.MaterialName={lobbyCard.MaterialName} card={lobbyCard.Card}");
                        return;
                    }
                    
                    var lobbyCardGameObject = machineCardProperty.GameObject;
                    
                    // this will be the key used to locate the lobby card later
                    // lobbyCard.Card ids are now unique
                    var lobbyCardKey = lobbyCard.Card;
                            
                    // Debug.Log($"LoadApp lobbyCardGameObject={lobbyCardGameObject.name} renaming to lobbyCardKey={lobbyCardKey}");
                    lobbyCardGameObject.name = lobbyCardKey;

                    GameObject quadGameObject = lobbyCardGameObject.transform.GetChild(0).GetChild(0).gameObject;

                    // Get the MeshRenderer component
                    var renderer = quadGameObject.GetComponent<MeshRenderer>();
                    if (renderer.material != null)
                    {
                        Object.Destroy(renderer.material);
                        renderer.material = null;
                    }
                    
                    if (LobbyCardMaterialMap.TryGetValue(lobbyCardKey, out var newMaterial))
                    {
                        renderer.material = newMaterial;
                    }
                    else
                    {
                        var materialName = lobbyCard.MaterialName;
                    
                        var material = assetBundle.LoadAsset<Material>(materialName);
                        if (material == null)
                        {
                            Debug.LogError(
                                $"Failed to load material={materialName} from asset bundle={machineBundleName} version={machineBundleVariant}");
                            return;
                        }
                        
                        renderer.material = material;
                        
                        // cache the material
                        LobbyCardMaterialMap[lobbyCardKey] = material;
                    }
                    
                    LoadedLobbyCardCount++;

                });
        }
        
        // Convert the method from Lua to C#
        public IEnumerator LoadTopPanelLobbyCards(Table self)
        {
            // turn off the loading screen
            // TODO: remove loading screen
            var loadingProperty = (PropertyGameObject) self["Loading"];
            if (loadingProperty != null)
            {
                loadingProperty.SetActive(false);
            }
            
            // Update Volume Controls
            UpdateVolumeControls(self);
            
            yield return LoadLobbyPage(self, CurrentPageNumber);
            
            TopPanelLobbyCardPropertyId = "LobbyCard001";
            
            IsMainLobbyLoaded = true;
        }
        
        // Convert the method from Lua to C#
        // TODO: deprecated
        [Obsolete]
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
            
            // Get the card count
            int cardCount = bettrUser.LobbyCards.Count;  // Assuming LobbyCards is a list or similar collection
            Console.WriteLine($"LoadApp lobbyCard cardCount={cardCount}");

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