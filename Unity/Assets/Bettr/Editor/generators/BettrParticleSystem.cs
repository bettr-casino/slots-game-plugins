using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrParticleSystem
    {
        public static ParticleSystem AddOrGetParticleSystem(GameObject go)
        {
            var particleSystem = go.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = go.AddComponent<ParticleSystem>();
            }
            
            var customData = particleSystem.customData;
            customData.enabled = true;
            customData.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Vector);
            
            return particleSystem;
        }

        public static int GetParticleSystemId(ParticleSystem particleSystem)
        {
            var customData = particleSystem.customData;
            if (customData.enabled && customData.GetMode(ParticleSystemCustomData.Custom1) == ParticleSystemCustomDataMode.Vector)
            {
                ParticleSystem.MinMaxCurve curve = customData.GetVector(ParticleSystemCustomData.Custom1, 0);
                return Mathf.RoundToInt(curve.constant);
            }
            return -1; // Return an invalid ID or handle it accordingly
        }
    }
}