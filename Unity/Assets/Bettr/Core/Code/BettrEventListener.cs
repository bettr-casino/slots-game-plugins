using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrEventListener : MonoBehaviour
    {
        public void OnPointerClick()
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerClick(string param)
        {
            // param has the structure StaticClass__paramvalue
            var parts = param.Split(new[] { "__" }, System.StringSplitOptions.None);
            var className = parts[0];
            var paramValue = parts[1];
            if (className == "BettrDialogController")
            {
                BettrDialogController.Instance.OnPointerClick(paramValue);
            }
        }
    }    
}
