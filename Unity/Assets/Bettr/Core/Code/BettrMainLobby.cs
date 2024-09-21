using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrMainLobby : MonoBehaviour
    {
        public Vector2 referenceResolution = new Vector2(960, 600);
    
        private Transform lobbyTransform;
        private int lastScreenWidth;
        private int lastScreenHeight;

        void Start()
        {
            // Get the Transform of the Lobby (for 3D objects)
            lobbyTransform = GetComponent<Transform>();

            // Dynamically adjust the scale at the start
            AdjustLobbyScale3D();

            // Store the initial screen resolution
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        void AdjustLobbyScale3D()
        {
            // Get current screen dimensions
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Calculate the scaling factor based on the reference resolution
            float widthScaleFactor = screenWidth / referenceResolution.x;
            float heightScaleFactor = screenHeight / referenceResolution.y;
            
            float scaleFactor = Mathf.Min(widthScaleFactor, heightScaleFactor);

            // Adjust the scale of the 3D lobby
            lobbyTransform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // Optionally adjust position if needed to center the 3D object
            lobbyTransform.position = new Vector3(0, 0, lobbyTransform.position.z);
        }

        void Update()
        {
            // Check if the screen has been resized
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                // Only adjust the scale if the screen resolution has changed
                AdjustLobbyScale3D();

                // Update the stored screen dimensions
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
            }
        }
    }
}