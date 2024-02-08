using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class LongPressHandler : MonoBehaviour
    {
        private bool _isInteracting = false;
        private float _interactionDuration = 0f;
        private readonly float _requiredHoldTime = 1f;
        
        void Update()
        {
            // Check for touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        StartInteraction();
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        EndInteraction();
                        break;
                }
            }

            // Check for mouse input
            else if (Input.GetMouseButtonDown(0)) // Mouse button pressed
            {
                StartInteraction();
            }
            else if (Input.GetMouseButtonUp(0)) // Mouse button released
            {
                EndInteraction();
            }

            // Update interaction duration
            if (_isInteracting)
            {
                _interactionDuration += Time.deltaTime;

                if (_interactionDuration >= _requiredHoldTime)
                {
                    // Long press detected
                    OnLongPress();
                    _isInteracting = false; // Reset interaction
                }
            }
        }
        
        private void StartInteraction()
        {
            _isInteracting = true;
            _interactionDuration = 0f;
        }

        private void EndInteraction()
        {
            _isInteracting = false;
        }

        private void OnLongPress()
        {
            Debug.Log("Long press detected. Capturing Scene.");
            var devTools = gameObject.GetComponent<DevTools>();
            StartCoroutine(devTools.CaptureSceneState());
        }
        
    }
}