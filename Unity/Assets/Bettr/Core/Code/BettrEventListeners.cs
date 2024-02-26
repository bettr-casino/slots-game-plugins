using System.Collections.Generic;
using CrayonScript.Code;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrEventListeners : MonoBehaviour
    {
        public List<PropertyAnimator> onShowAnimators;
        public List<PropertyAnimator> onHideAnimators;

        public void PlayShowAnimations()
        {
            foreach (var animator in onShowAnimators)
            {
                animator.animator.Play(animator.animationStateName);
            }
        }

        public void PlayHideAnimations()
        {
            foreach (var animator in onHideAnimators)
            {
                animator.animator.Play(animator.animationStateName);
            }
        }
    }    
}
