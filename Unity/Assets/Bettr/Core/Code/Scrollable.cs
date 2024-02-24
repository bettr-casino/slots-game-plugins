using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class Scrollable : MonoBehaviour
    {
        public GameObject scrollable;
        public float scrollSpeed = 0.5f;
        public float minY = 0f;
        public float maxY = 0f;
        public float minX = 0f;
        public float maxX = 0f;
        public bool vertical = true;
        public UnityEvent onScrollBegin;
        public UnityEvent onScrollEnd;
        [NonSerialized] private Vector3 _previousMousePosition;
        [NonSerialized] private bool _isScrolling = false;
        [NonSerialized] private bool _isPointerOverScrollable = false;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverGameObject())
                {
                    _isPointerOverScrollable = true;
                    // Record the position of the mouse when the button is first pressed
                    _previousMousePosition = Input.mousePosition;
                    // Invoke the onScrollBegin event
                    if (!_isScrolling)
                    {
                        onScrollBegin.Invoke();
                        _isScrolling = true;
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (_isPointerOverScrollable)
                {
                    // Calculate the distance the mouse has moved since the last frame
                    Vector3 delta = Input.mousePosition - _previousMousePosition;
                    _previousMousePosition = Input.mousePosition;

                    // Translate the scrollable GameObject
                    var localPosition = scrollable.transform.localPosition;
                    if (vertical)
                    {
                        localPosition.y = Mathf.Clamp(localPosition.y + delta.y * scrollSpeed, minY, maxY);
                    }
                    else // If not vertical, assume horizontal scrolling.
                    {
                        localPosition.x = Mathf.Clamp(localPosition.x + delta.x * scrollSpeed, minX, maxX);
                    }
                    scrollable.transform.localPosition = localPosition;
                }
            }
            else if (_isScrolling)
            {
                // Mouse button is not down, if we were scrolling, invoke the onScrollEnd event
                onScrollEnd.Invoke();
                _isScrolling = false;
                _isPointerOverScrollable = false;
            }
        }

        private bool IsPointerOverGameObject()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            LayerMask layerMask = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                return hit.collider.gameObject == scrollable || hit.collider.transform.IsChildOf(scrollable.transform);
            }
            return false;
        }
    }
}
