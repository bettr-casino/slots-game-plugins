using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class ToastMessage : MonoBehaviour
    {
        public Text toastText;
        public float displayTime = 2.0f;

        public void ShowToast(string message)
        {
            StartCoroutine(ToastRoutine(message));
        }

        private IEnumerator ToastRoutine(string message)
        {
            toastText.text = message;
            toastText.enabled = true;
            yield return new WaitForSeconds(displayTime);
            toastText.enabled = false;
        }
    }
}