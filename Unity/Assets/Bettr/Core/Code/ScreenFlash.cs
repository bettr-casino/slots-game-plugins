using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class ScreenFlash : MonoBehaviour
    {
        public Image flashImage;
        public float flashTime = 0.5f;

        public void FlashScreen()
        {
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            flashImage.enabled = true;
            yield return new WaitForSeconds(flashTime);
            flashImage.enabled = false;
        }
    }
}