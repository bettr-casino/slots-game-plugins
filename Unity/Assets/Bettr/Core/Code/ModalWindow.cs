using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class ModalWindowScript : MonoBehaviour
    {
        private Main _app;
        
        private bool _activateInputFieldListeners;

        void Start()
        {
            BettrVisualsController.SwitchOrientationToLandscape();
            
            _app = gameObject.scene.GetRootGameObjects().First(o => o.name == "Bettr").GetComponent<Main>();
            StartApp();
        }
        
        private void StartApp()
        {
            _app.StartApp();
        }
        
    }
    
}