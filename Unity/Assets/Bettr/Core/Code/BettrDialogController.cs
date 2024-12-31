using System;
using System.Collections;
using CrayonScript.Code;
using CrayonScript.Interpreter.Execution.VM;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrDialogController
    {
        public static BettrDialogController Instance { get; private set; }
        
        private string _param;
        private string _result;
        
        private bool _waitingForClick;
        private bool _dialogLocked;
        
        public BettrDialogController()
        {
            TileController.RegisterType<BettrDialogController>("BettrDialogController");
            TileController.AddToGlobals("BettrDialogController", this);
            
            Instance = this;
        }
        
        public IEnumerator WaitForDialogAction(CrayonScriptContext context, GameObject dialog)
        {
            _dialogLocked = true;
            _result = null;
            _waitingForClick = true;
            while (_waitingForClick)
            {
                yield return null;
            }
            _dialogLocked = false;
            
            context.StringResult = _result;
        }

        public void OnPointerClick(string param)
        {
            _param = param;
            Debug.Log("BettrDialogController OnPointerClick: " + _param);
            // handle the click
            HandlePointerClick(param);
        }
        
        private void HandlePointerClick(string param)
        {
            Debug.Log("BettrDialogController HandlePointerClick: " + param);
            _result = param;
            _waitingForClick = false;
        }
    }
}