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
        [SerializeField] private TMP_InputField[] alphaFields;
        [SerializeField] private TMP_InputField[] numericFields;
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI statusText;

        private Main _app;
        private GameObject _oneTimeSetup;
        
        private bool _activateInputFieldListeners;

        void Start()
        {
            BettrVisualsController.SwitchOrientationToLandscape();
            
            _app = gameObject.scene.GetRootGameObjects().First(o => o.name == "Bettr").GetComponent<Main>();
            _oneTimeSetup = gameObject.scene.GetRootGameObjects().First(o => o.name == "OneTimeSetup");
            
            var lastInput = PlayerPrefs.GetString(ConfigData.TaskCodeKey, ConfigData.DefaultTaskCode);
            Debug.Log($"PlayerPrefs {ConfigData.TaskCodeKey}=" + lastInput);
            if (lastInput != null && lastInput.Length != 6)
            {
                lastInput = null;
            }

            if (lastInput != null && lastInput.Length == 6)
            {
                SplitAndStoreCode(lastInput);
                ValidateInput();
            }
            else
            {
                DisableContinueButton();
                FocusOnInputField();
                ValidateInput();
            }
            
            _activateInputFieldListeners = true;
            
        }
        
        private void SplitAndStoreCode(string code)
        {
            for (int i = 0; i < alphaFields.Length; i++)
            {
                alphaFields[i].text = code[i].ToString();
            }
            for (int i = 0; i < numericFields.Length; i++)
            {
                numericFields[i].text = code[i+alphaFields.Length].ToString();
            }
        }
        
        private void FocusOnInputField()
        {
            alphaFields[0].ActivateInputField();
            alphaFields[0].Select();
        }

        public void HandleAlphaInputValueChanged(int index)
        {
            if (!_activateInputFieldListeners) return;
            
            var alphaField = alphaFields[index];
            var text = alphaField.text;
            
            if (!string.IsNullOrEmpty(text) && text.Length > 1)
            {
                alphaField.text = text.Substring(0, 1);
            }

            if (!ValidateAlphaInputField(index))
            {
                ValidateInput();
                return;
            }
            
            text = alphaField.text;
            if (text.Length == 1)
            {
                if (index < alphaFields.Length - 1)
                {
                    alphaFields[index + 1].OnPointerClick(new PointerEventData(EventSystem.current));
                }
                else
                {
                    numericFields[0].OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
            
            ValidateInput();
        }
        
        public void HandleAlphaInputEndEdit(int index)
        {
            if (!_activateInputFieldListeners) return;
            
            ValidateInput();
        }
        
        public void HandleNumericInputValueChanged(int index)
        {
            if (!_activateInputFieldListeners) return;
            
            var numericField = numericFields[index];
            var text = numericField.text;
            
            if (!string.IsNullOrEmpty(text) && text.Length > 1)
            {
                numericField.text = text.Substring(0, 1);
            }
            
            if (!ValidateNumericInputField(index))
            {
                ValidateInput();
                return;
            }
            
            text = numericField.text;
            if (text.Length == 1 && index < numericFields.Length - 1)
            {
                // Automatically move to the next field if the current one is filled
                numericFields[index + 1].OnPointerClick(new PointerEventData(EventSystem.current));
            }
            
            ValidateInput();
        }
        
        public void HandleNumericInputEndEdit(int index)
        {
            if (!_activateInputFieldListeners) return;
            
            ValidateInput();
        }

        void SaveTaskCode()
        {
            PlayerPrefs.SetString(ConfigData.TaskCodeKey, GetCombinedCode());
            PlayerPrefs.Save();
        }
        
        public void OnContinue()
        {
            SaveTaskCode();
            StartApp();
        }
        
        private bool ValidateAlphaInputField(int index)
        {
            var field = alphaFields[index];

            return (!string.IsNullOrEmpty(field.text)
                && field.text.Length == 1
                && char.IsLetter(field.text, 0));
        }
        
        private bool ValidateNumericInputField(int index)
        {
            var field = numericFields[index];

            return (!string.IsNullOrEmpty(field.text)
                    && field.text.Length == 1
                    && char.IsDigit(field.text, 0));
        }

        private void ValidateInput()
        {
            DisableContinueButton();
            for (int i = 0; i < alphaFields.Length; i++)
            {
                alphaFields[i].text = alphaFields[i].text.ToLower();
            }
            if (alphaFields.Any(field => string.IsNullOrEmpty(field.text) 
                                         || field.text.Length != 1 
                                         || !char.IsLetter(field.text, 0)))
            {
                return;
            }
            if (numericFields.Any(field => string.IsNullOrEmpty(field.text) 
                                           || field.text.Length != 1 
                                           || !char.IsDigit(field.text, 0)))
            {
                return;
            }
            EnableContinueButton();
        }

        private string GetCombinedCode()
        {
            string combinedCode = "";

            // Combine alpha characters
            foreach (var field in alphaFields)
            {
                combinedCode += field.text;
            }

            // Combine numeric characters
            foreach (var field in numericFields)
            {
                combinedCode += field.text;
            }

            return combinedCode;
        }
        
        private void EnableContinueButton()
        {
            continueButton.interactable = true;
        }

        private void DisableContinueButton()
        {
            continueButton.interactable = false;
        }
        
        void ShowStatus(string message)
        {
            // Set the status text active and change its message
            statusText.text = message;
        }
        
        private void StartApp()
        {
            HideModalWindow();
            DestroyModalWindow();
            _app.StartApp();
        }
        
        private void HideModalWindow()
        {
            Debug.Log("HideModalWindow called");
            _oneTimeSetup.SetActive(false);
        }

        private void DestroyModalWindow()
        {
            Debug.Log("DestroyModalWindow called");
            Destroy(_oneTimeSetup);
        }
    }
    
}