using System.Collections;
using CrayonScript.Code;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrRoutineRunner : MonoBehaviour
    {
        public static BettrRoutineRunner Instance {
            get
            {
                if (_instance != null) return _instance;

                var gameObject = GameObject.Find("Bettr");
                if (gameObject == null)
                {
                    gameObject = new GameObject("Bettr");
                    DontDestroyOnLoad(gameObject);
                }
                
                _instance = gameObject.AddComponent<BettrRoutineRunner>();
                    
                TileController.RegisterType<BettrRoutineRunner>("BettrRoutineRunner");
                TileController.AddToGlobals("BettrRoutineRunner", _instance);
                
                return _instance;
            }
        }
        
        private static BettrRoutineRunner _instance;
        
        public IEnumerator RunRoutine(IEnumerator enumerator)
        {
            yield return StartCoroutine(enumerator);
        }
    }
}