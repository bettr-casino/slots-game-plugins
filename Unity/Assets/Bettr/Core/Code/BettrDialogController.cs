using System.Collections;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrDialogController : MonoBehaviour
    {
        public static BettrDialogController Instance { get; private set; }
        
        public void Awake()
        {
            Instance = this;
        }

        public IEnumerator ShowModalDialog(GameObject dialog)
        {
            yield return new WaitForSeconds(100.0f);
        }
    }
}