using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;

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
        
        public BettrMainLobbySceneController()
        {
            TileController.RegisterType<BettrMainLobbySceneController>("BettrMainLobbySceneController");
            TileController.AddToGlobals("BettrMainLobbySceneController", this);
        }
        
        public IEnumerator LoadLobbyCardMachine(string lobbyCardName)
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            // lobbyCardName example Game001__LobbyCard001
            var lobbyCardIndex = bettrUser.FindLobbyCardIndexById(lobbyCardName.Split("__")[1]);
            var lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
            bettrUser.LobbyCardIndex = 1;
            yield return BettrAssetController.Instance.LoadScene(lobbyCard.MachineBundleName,
                lobbyCard.MachineBundleVariant, lobbyCard.MachineSceneName);
        }

        public IEnumerator LoadMachine()
        {
            // TODO: this should be from the user preferences
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCard = bettrUser.LobbyCards[1];
            bettrUser.LobbyCardIndex = 1;
            yield return BettrAssetController.Instance.LoadScene(lobbyCard.MachineBundleName,
                lobbyCard.MachineBundleVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadPreviousMachine()
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var bettrUserLobbyCardIndex = bettrUser.LobbyCardIndex;
            // Wrap around
            var previousIndex = (bettrUserLobbyCardIndex - 1 + bettrUser.LobbyCards.Count) % bettrUser.LobbyCards.Count;
            var lobbyCard = bettrUser.LobbyCards[previousIndex];
            bettrUser.LobbyCardIndex = previousIndex;
            yield return BettrAssetController.Instance.LoadScene(lobbyCard.MachineBundleName,
                lobbyCard.MachineBundleVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadNextMachine()
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var bettrUserLobbyCardIndex = bettrUser.LobbyCardIndex;
            // Wrap around
            var nextIndex = (bettrUserLobbyCardIndex + 1) % bettrUser.LobbyCards.Count;
            var lobbyCard = bettrUser.LobbyCards[nextIndex];
            bettrUser.LobbyCardIndex = nextIndex;
            yield return BettrAssetController.Instance.LoadScene(lobbyCard.MachineBundleName,
                lobbyCard.MachineBundleVariant, lobbyCard.MachineSceneName);
        }
        
        public IEnumerator LoadMainLobby()
        {
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            yield return BettrAssetController.Instance.LoadScene(bettrUser.LobbyScene.BundleName,
                bettrUser.LobbyScene.BundleVersion, "MainLobbyScene");
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
                    var lobbyCardIndex = 1 + (groupIndex - 1) * 12 + cardIndex - 1;
                    var lobbyCardId = $"LobbyCard{lobbyCardIndex:D3}";
                    lobbyCard = bettrUser.LobbyCards[lobbyCardIndex];
                    var machineCardProperty = machineGroupProperty[lobbyCardId];

                    if (machineCardProperty == null)
                    {
                        Console.WriteLine($"LoadApp machineCardProperty is nil lobbyCard={lobbyCard} lobbyCard.MachineName={lobbyCard.MachineName} lobbyCard.MaterialName={lobbyCard.MaterialName} card={lobbyCard.Card}");
                    }
                    else
                    {
                        var lobbyCardGameObject = machineCardProperty.GameObject;
                        if (lobbyCardGameObject == null) continue;
                        Console.WriteLine($"LoadApp lobbyCardGameObject={lobbyCardGameObject.name}");

                        string lobbyCardKey = group + "__" + lobbyCard.Card;
                        lobbyCardGameObject.name = lobbyCardKey;

                        var quadGameObject = lobbyCardGameObject.transform.GetChild(0).GetChild(0).gameObject;
                        yield return BettrAssetController.Instance.LoadMaterial(lobbyCard.BundleName, lobbyCard.BundleVersion, lobbyCard.MaterialName, quadGameObject);

                        State.LobbyCardMap[lobbyCardKey] = lobbyCard;
                    }
                }
            }
        }
    }
}