using System;
using System.Collections;
using CrayonScript.Code;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class TilePropertyTweensInjected : TilePropertyTweensBase
    {
        private IEnumerator Start()
        {
            yield return Tile.InjectProperty(this, gameObject.transform);
        }
    }
}