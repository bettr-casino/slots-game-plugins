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

        public IEnumerator LoadMachine()
        {
            // TODO: this should be from the user preferences
            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            var lobbyCard = bettrUser.LobbyCards[0];
            bettrUser.LobbyCardIndex = 0;
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

            // Iterate over LobbyCards
            for (int i = 0; i < cardCount; i++)
            {
                lobbyCard = bettrUser.LobbyCards[i];
                string group = lobbyCard.Group;
                string card = lobbyCard.Card;
                Console.WriteLine($"LoadApp lobbyCard={lobbyCard} lobbyCard.MachineName={lobbyCard.MachineName} lobbyCard.MaterialName={lobbyCard.MaterialName}");

                var machineGroupProperty = (TilePropertyGameObjectGroup) self[group];
                var machineCardProperty = machineGroupProperty[card];

                if (machineCardProperty == null)
                {
                    Console.WriteLine($"LoadApp machineCardProperty is nil lobbyCard={lobbyCard} lobbyCard.MachineName={lobbyCard.MachineName} lobbyCard.MaterialName={lobbyCard.MaterialName} group={group} card={card}");
                }
                else
                {
                    var lobbyCardGameObject = machineCardProperty.GameObject;
                    Console.WriteLine($"LoadApp lobbyCardGameObject={lobbyCardGameObject.name}");

                    string lobbyCardKey = group + "__" + card;
                    lobbyCardGameObject.name = lobbyCardKey;

                    var quadGameObject = lobbyCardGameObject.transform.GetChild(0).GetChild(0).gameObject;
                    yield return BettrAssetController.Instance.LoadMaterial(lobbyCard.BundleName, lobbyCard.BundleVersion, lobbyCard.MaterialName, quadGameObject);

                    State.LobbyCardMap[lobbyCardKey] = lobbyCard;
                }
            }
        }
    }
}