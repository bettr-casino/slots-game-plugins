using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrUnityEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent onLongPress;
        public float longPressThreshold = 1.0f; // Duration in seconds to consider as a long press

        // ReSharper disable once InconsistentNaming
        private bool isPointerDown = false;
        // ReSharper disable once InconsistentNaming
        private float pointerDownTimer = 0.0f;

        void Update()
        {
            if (isPointerDown)
            {
                pointerDownTimer += Time.deltaTime;
                if (pointerDownTimer >= longPressThreshold)
                {
                    if (onLongPress != null)
                    {
                        onLongPress.Invoke();
                    }
                    Reset();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            pointerDownTimer = 0.0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Reset();
        }

        private void Reset()
        {
            isPointerDown = false;
            pointerDownTimer = 0.0f;
        }
    }    
}
