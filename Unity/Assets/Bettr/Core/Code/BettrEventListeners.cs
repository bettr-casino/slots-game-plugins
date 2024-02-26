using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class BettrEventListeners : MonoBehaviour
{

    // Lists of GameObjects to show or hide, configurable via the Unity Editor
    public List<GameObject> gameObjectsToShow;
    public List<GameObject> gameObjectsToHide;

    public void ShowGameObjects()
    {
        foreach (var go in gameObjectsToShow)
        {
            go.SetActive(true);
        }
    }

    public void HideGameObjects()
    {
        foreach (var go in gameObjectsToHide)
        {
            go.SetActive(false);
        }
    }
}