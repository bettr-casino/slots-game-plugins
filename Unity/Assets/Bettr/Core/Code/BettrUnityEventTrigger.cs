using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrUnityEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent onLongPress;
        public float longPressThreshold = 0.2f; // Duration in seconds to consider as a long press
        public float movementThreshold = 0.01f; // Distance in units the GameObject can move before the long press is canceled

        private bool _isPointerDown = false;
        private float _pointerDownTimer = 0.0f;
        private Vector3 _initialPosition;

        void Update()
        {
            if (_isPointerDown)
            {
                _pointerDownTimer += Time.deltaTime;
                if (_pointerDownTimer >= longPressThreshold)
                {
                    if (onLongPress != null)
                    {
                        onLongPress.Invoke();
                    }
                    Reset();
                }
                // Check if the GameObject has moved more than the allowed threshold
                if (Vector3.Distance(transform.position, _initialPosition) > movementThreshold)
                {
                    Reset();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _pointerDownTimer = 0.0f;
            _initialPosition = transform.position; // Store the initial position when the pointer goes down
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Reset();
        }

        private void Reset()
        {
            _isPointerDown = false;
            _pointerDownTimer = 0.0f;
        }
    }    
}
