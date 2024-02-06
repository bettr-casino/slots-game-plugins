using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using TMPro;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class DisplayCounterConfiguration
    {
        public TextMeshPro counterTextMeshPro;
        public BigInteger counterStartValue;
        public BigInteger counterEndValue;
        public BigInteger currentCounterValue;
        public int counterFixedDigits;
        public float counterIncrementRatePerFrame;
        public bool paused;
    }
    
    [Serializable]
    public class BettrDisplayCounters : MonoBehaviour
    {
        private Dictionary<string, DisplayCounterConfiguration> _displayCounterConfigurations =
            new Dictionary<string, DisplayCounterConfiguration>();
        
        public void AddCounter(string counterName, TextMeshPro counterTextMeshPro, BigInteger counterStartValue, BigInteger counterEndValue, int counterFixedDigits, float counterIncrementRatePerFrame)
        {
            _displayCounterConfigurations[counterName] = new DisplayCounterConfiguration() {
                counterTextMeshPro = counterTextMeshPro,
                counterStartValue = counterStartValue,
                counterEndValue = counterEndValue,
                counterFixedDigits = counterFixedDigits,
                counterIncrementRatePerFrame = counterIncrementRatePerFrame,
                currentCounterValue = counterStartValue,
            };
        }

        public void PauseCounter(string counterName)
        {
            _displayCounterConfigurations[counterName].paused = true;
        }
        
        public void ResumeCounter(string counterName)
        {
            _displayCounterConfigurations[counterName].paused = false;
        }

        void Update()
        {
            foreach (var kvPair in _displayCounterConfigurations)
            {
                var configuration = kvPair.Value;
                if (configuration.paused)
                {
                    continue;
                }

                var textMeshPro = configuration.counterTextMeshPro;
                var fixedDigits = configuration.counterFixedDigits;
                
                configuration.currentCounterValue += (BigInteger) (configuration.counterIncrementRatePerFrame);
                
                if (configuration.counterIncrementRatePerFrame > 0)
                {
                    if (configuration.currentCounterValue > configuration.counterEndValue)
                    {
                        configuration.currentCounterValue = configuration.counterEndValue;
                        continue;
                    }
                }
                else if (configuration.counterIncrementRatePerFrame < 0)
                {
                    if (configuration.currentCounterValue < configuration.counterEndValue)
                    {
                        configuration.currentCounterValue = configuration.counterEndValue;
                        continue;
                    }
                }

                textMeshPro.text = configuration.currentCounterValue.ToString("D" + fixedDigits);
            }
        }
    }
}
