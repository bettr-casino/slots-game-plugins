using System;
using UnityEngine;

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
        public bool vertical = true;
        [NonSerialized] private Vector3 _previousMousePosition;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Record the position of the mouse when the button is first pressed
                _previousMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                // Calculate the distance the mouse has moved since the last frame
                Vector3 delta = Input.mousePosition - _previousMousePosition;
                _previousMousePosition = Input.mousePosition;

                // Translate the Background plane along its local Y axis
                var localPosition = scrollable.transform.localPosition;
                localPosition = new Vector3(localPosition.x, Mathf.Clamp(localPosition.y + delta.y * scrollSpeed, minY, maxY), localPosition.z);
                scrollable.transform.localPosition = localPosition;

                Debug.Log($"scrollable.transform.localPosition={localPosition}");
            }
        }
    }
}