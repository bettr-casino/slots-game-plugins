// ReSharper disable All

using CrayonScript.Code;
using TMPro;
using UnityEngine;

namespace Bettr.Core
{
    public class BettrTextMeshProController
    {
        public BettrTextMeshProController()
        {
            TileController.RegisterType<BettrTextMeshProController>("BettrTextMeshProController");
            TileController.AddToGlobals("BettrTextMeshProController", this);
        }
        
        public void SetText(GameObject gameObject, string text)
        {
            var textMeshPro = gameObject.GetComponent<TextMeshPro>();
            textMeshPro.text = text;
        }
        
        public void SetText(GameObject gameObject, int number)
        {
            var textMeshPro = gameObject.GetComponent<TextMeshPro>();
            textMeshPro.text = number.ToString();
        }
    }
}