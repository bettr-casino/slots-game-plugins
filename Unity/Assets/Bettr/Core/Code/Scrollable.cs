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
        [NonSerialized] private bool _isScrollingEnabled = false;
        [NonSerialized] private bool _isPointerOverScrollable = false;
        [NonSerialized] private bool? _isVerticalScroll = null;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverGameObject())
                {
                    _isPointerOverScrollable = true;
                    _previousMousePosition = Input.mousePosition;
                    if (!_isScrolling)
                    {
                        onScrollBegin.Invoke();
                        _isScrollingEnabled = true;
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (_isPointerOverScrollable)
                {
                    Vector3 delta = Input.mousePosition - _previousMousePosition;
                    if (_isScrollingEnabled && !_isVerticalScroll.HasValue)
                    {
                        // Determine the primary direction of the scroll
                        if (delta.magnitude > 0)
                        {
                            _isVerticalScroll = Mathf.Abs(delta.y) > Mathf.Abs(delta.x);
                            _isScrolling = true;
                        }
                    }
                    
                    if (_isVerticalScroll == vertical)
                    {
                        // Translate the scrollable GameObject
                        var localPosition = scrollable.transform.localPosition;
                        if (vertical)
                        {
                            localPosition.y = Mathf.Clamp(localPosition.y + delta.y * scrollSpeed, minY, maxY);
                        }
                        else
                        {
                            localPosition.x = Mathf.Clamp(localPosition.x + delta.x * scrollSpeed, minX, maxX);
                        }
                        scrollable.transform.localPosition = localPosition;
                    }

                    if (_isScrolling)
                    {
                        _previousMousePosition = Input.mousePosition;
                    }
                }
            }
            else if (_isScrolling)
            {
                onScrollEnd.Invoke();
                _isScrolling = false;
                _isScrollingEnabled = false;
                _isPointerOverScrollable = false;
                _isVerticalScroll = null;
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
