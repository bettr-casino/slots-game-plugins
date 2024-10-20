using System;
using System.Collections;
using System.Reflection;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using CrayonScript.Interpreter.Execution.VM;
using PathCreation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void RollupText(long value);
    
    public class BettrVisualsController
    {
        public BettrVisualsController()
        {
            TileController.RegisterType<BettrVisualsController>("BettrVisualsController");
            TileController.AddToGlobals("BettrVisualsController", this);
            
            TileController.RegisterType<PathCreator>("PathCreator");
            TileController.RegisterType<EndOfPathInstruction>("EndOfPathInstruction");
            
            TileController.RegisterType<PropertyPathCreator>("PropertyPathCreator");
            TileController.RegisterType<TilePropertyPathCreator>("TilePropertyPathCreator");
            TileController.RegisterType<TilePropertyPathCreatorGroup>("TilePropertyPathCreatorGroup");
            TileController.RegisterType<TilePropertyPathCreators>("TilePropertyPathCreators");
            
            TileController.RegisterType<PropertyTween>("PropertyTween");
            TileController.RegisterType<TilePropertyTween>("TilePropertyTween");
            TileController.RegisterType<TilePropertyTweenGroup>("TilePropertyTweenGroup");
            TileController.RegisterType<TilePropertyTweens>("TilePropertyTweens");

            TileController.RegisterType<iTween>("iTween");
            TileController.RegisterType<iTween.EaseType>("iTween.EaseType");
        }
        
        public void RollUpCounter(PropertyTextMeshPro counterTextProperty, long start, long end, float duration)
        {
            if (counterTextProperty == null)
            {
                throw new ScriptRuntimeException(new NullReferenceException("null text mesh pro property"));
            }
            
            BettrRoutineRunner.Instance.StartCoroutine(RollUpCoroutine(
                value => counterTextProperty.SetText(value.ToString()), 
                (long) start, (long) end, duration));
        }
        
        public void RollUpFormatCounter(PropertyTextMeshPro counterTextProperty, long start, long end, float duration)
        {
            if (counterTextProperty == null)
            {
                throw new ScriptRuntimeException(new NullReferenceException("null text mesh pro property"));
            }
            
            BettrRoutineRunner.Instance.StartCoroutine(RollUpCoroutine(
                value => counterTextProperty.Format(value.ToString()), 
                (long) start, (long) end, duration));
        }

        private IEnumerator RollUpCoroutine(RollupText rollupText, long start, long end, float duration)
        {
            var difference = end - start;
            var isRollingUp = difference > 0;

            // Calculate the constant speed
            var speed = difference / duration;

            var elapsed = 0.0f;
            float currentValue = start;

            // While the elapsed time is less than the intended duration
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Update the current value
                currentValue += (isRollingUp ? 1 : -1) * speed * Time.deltaTime;

                // Update the text and wait for the next frame
                rollupText(Mathf.RoundToInt(currentValue));

                yield return null;
            }

            // Ensure the counter ends exactly at the end value
            rollupText(end);
        }

        public IEnumerator FollowPath(CrayonScriptContext context, GameObject gameObject, PropertyPathCreator pathCreatorProperty)
        {
            if (pathCreatorProperty == null)
            {
                context.SetError(new ScriptRuntimeException(new NullReferenceException("null path creator property")));
                yield break;
            }
            
            var pathCreator = pathCreatorProperty.pathCreator;
            var speed = 3.0f;
            var endOfPathInstruction = EndOfPathInstruction.Stop;
            var distanceTravelled = 0.0f;
            while (true)
            {
                distanceTravelled += speed * Time.deltaTime;
                var newPosition = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                if (newPosition == gameObject.transform.position)
                {
                    break;
                }
                gameObject.transform.position = newPosition;
                yield return null;
            }
        }
        
        public void TweenMultipleMoveBetween(TilePropertyGameObjectGroup objectPropertyGroupToMove, PropertyTween propertyTween)
        {
            if (propertyTween == null)
            {
                throw new ScriptRuntimeException("null animator property");
            }
            
            if (!propertyTween.useMoveBetween)
            {
                throw new ScriptRuntimeException($"useMoveBetween is false. Skipping tween for TilePropertyGameObjectGroup={objectPropertyGroupToMove.groupKey}. Check tween properties");
            }
            
            foreach (var objectPropertyToMove in objectPropertyGroupToMove.gameObjectProperties)
            {
                TweenMoveBetween(objectPropertyToMove.value.gameObject, propertyTween);
            }
        }
        
        public void TweenMoveBetween(GameObject objectToMove, PropertyTween propertyTween)
        {
            if (propertyTween == null)
            {
                throw new ScriptRuntimeException("null animator property");
            }

            if (!propertyTween.useMoveBetween)
            {
                throw new ScriptRuntimeException($"useMoveBetween is false. Skipping tween for objectToMove={objectToMove.name}. Check tween properties");
            }
            
            var startObj = propertyTween.startGameObject;
            var useStartObj = propertyTween.useStartGameObject;
            var endObj = propertyTween.endGameObject;
            var useEndObj = propertyTween.useEndGameObject;
            var tweenDuration = propertyTween.tweenDuration;
            var tweenDelay = propertyTween.tweenDelay;
            var easeType = propertyTween.easeType;

            if (useStartObj)
            {
                objectToMove.transform.position = startObj.transform.position;
            }
            
            if (!useEndObj)
            {
                endObj = objectToMove;
            }
            
            // Tween the objectToMove to the position of endObj
            iTween.MoveTo(objectToMove, iTween.Hash(
                "position", endObj.transform.position,
                "delay", tweenDelay,
                "time", tweenDuration,
                "easetype", easeType
            ));
        }

        public IEnumerator TweenMoveBetweenAndWait(GameObject objectToMove, PropertyTween propertyTween)
        {
            TweenMoveBetween(objectToMove, propertyTween);
            yield return new WaitForSeconds(propertyTween.tweenDuration);
        }
        
        public void TweenMultipleScaleTo(TilePropertyGameObjectGroup objectPropertyGroupToScale, PropertyTween propertyTween)
        {
            if (propertyTween == null)
            {
                throw new ScriptRuntimeException("null animator property");
            }
            
            if (!propertyTween.useScale)
            {
                throw new ScriptRuntimeException($"useScale is false. Skipping tween for TilePropertyGameObjectGroup={objectPropertyGroupToScale.groupKey}. Check tween properties");
            }
            
            foreach (var objectPropertyToScale in objectPropertyGroupToScale.gameObjectProperties)
            {
                TweenScaleTo(objectPropertyToScale.value.gameObject, propertyTween);
            }
        }
        
        public void TweenScaleTo(GameObject objectToScale, PropertyTween propertyTween)
        {
            if (propertyTween == null)
            {
                throw new ScriptRuntimeException("null animator property");
            }
            
            if (!propertyTween.useScale)
            {
                throw new ScriptRuntimeException($"useScale is false. Skipping tween for objectToScale={objectToScale.name}. Check tween properties");
            }
            
            var scaleTo = propertyTween.scaleTo;
            var tweenDuration = propertyTween.tweenDuration;
            var tweenDelay = propertyTween.tweenDelay;
            var easeType = propertyTween.easeType;
            
            iTween.ScaleTo(objectToScale, iTween.Hash(
                "scale", scaleTo,
                "delay", tweenDelay,
                "time", tweenDuration,
                "easetype", easeType
            ));
        }
        
        public void TweenFadeMultipleTo(TilePropertyGameObjectGroup objectPropertyGroupToFade, PropertyTween propertyTween)
        {
            if (propertyTween == null)
            {
                throw new ScriptRuntimeException("null animator property");
            }
            
            if (!propertyTween.useFade)
            {
                throw new ScriptRuntimeException($"useFade is false. Skipping tween for TilePropertyGameObjectGroup={objectPropertyGroupToFade.groupKey}. Check tween properties");
            }
            
            foreach (var objectPropertyToScale in objectPropertyGroupToFade.gameObjectProperties)
            {
                TweenFadeTo(objectPropertyToScale.value.gameObject, propertyTween);
            }
        }
        
        public void TweenFadeTo(GameObject objectToFade, PropertyTween propertyTween)
        {
            if (propertyTween == null)
            {
                throw new ScriptRuntimeException("null animator property");
            }
            
            if (!propertyTween.useFade)
            {
                Debug.LogWarning($"useFade is false. Skipping tween for objectToFade={objectToFade.name}. propertyTween={propertyTween}");
                return;
            }
            
            var fadeTo = propertyTween.fadeTo;
            var tweenDelay = propertyTween.tweenDelay;
            var tweenDuration = propertyTween.tweenDuration;
            
            iTween.FadeTo(objectToFade, iTween.Hash(
                "alpha", fadeTo, 
                "delay", tweenDelay, 
                "time", tweenDuration
            ));
        }
        
        public void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public void StopAnimatorProperty(CrayonScriptContext context, PropertyAnimator animatorProperty)
        {
            
        }

        public IEnumerator PlayAnimatorProperty(CrayonScriptContext context, PropertyAnimator animatorProperty)
        {
            if (animatorProperty == null)
            {
                context.SetError(new ScriptRuntimeException(new NullReferenceException("null animator property")));
                yield break;
            }

            if (animatorProperty.animator == null)
            {
                context.SetError(new ScriptRuntimeException(new NullReferenceException($"null animator {animatorProperty.animationStateName}")));
                yield break;
            }
            
            var delayBeforeAnimationStart = animatorProperty.delayBeforeAnimationStart;
            if (delayBeforeAnimationStart > 0)
            {
                yield return new WaitForSeconds(delayBeforeAnimationStart);
            }
            var animator = animatorProperty.animator;
            var normalizedTime = 0.0f;
            var isAnimationDurationOverridden = animatorProperty.overrideAnimationDuration;
            var animationStateName = animatorProperty.animationStateName;            
            //Debug.Log($"animator animationStateName={animatorProperty.animationStateName} gameObject={animatorProperty.GameObject.name} path={GetGameObjectFullPath(animatorProperty.GameObject)}");
            if (!animatorProperty.GameObject.activeSelf)
            {
                Debug.LogWarning($"!activeSelf animator animationStateName={animatorProperty.animationStateName} gameObject={animatorProperty.GameObject.name} path={GetGameObjectFullPath(animatorProperty.GameObject)}");
            }
            if (!animatorProperty.GameObject.activeInHierarchy)
            {
                Debug.LogWarning($"!activeSelf animator animationStateName={animatorProperty.animationStateName} gameObject={animatorProperty.GameObject.name} path={GetGameObjectFullPath(animatorProperty.GameObject)}");
            }
            // verify animator state exists
            if (!animator.HasState(0, Animator.StringToHash(animationStateName)))
            {
                context.SetError(new ScriptRuntimeException($"animator state \"{animationStateName}\" not found, gameObject={animatorProperty.GameObject.name} path={GetGameObjectFullPath(animatorProperty.GameObject)}"));
                yield break;
            }
            animator.Play(animationStateName, -1, normalizedTime);
            yield return null;
            yield return null;
            var animationDuration = isAnimationDurationOverridden ? animatorProperty.animationDuration : animator.GetCurrentAnimatorStateInfo(0).length;
            var waitForAnimationComplete = animatorProperty.waitForAnimationComplete;
            if (waitForAnimationComplete)
            {
                yield return new WaitForSeconds(animationDuration);
            }
            context.FloatResult = animationDuration;
        }

        public IEnumerator PlayParticleSystemPropertyGroup(CrayonScriptContext context,
            TilePropertyParticleSystemGroup propertyParticleSystemGroup)
        {
            if (propertyParticleSystemGroup == null)
            {
                context.SetError(new ScriptRuntimeException(new NullReferenceException("null particle system group property")));
                yield break;
            }
            
            var waitForComplete = false;
            var maxWaitDuration = 0.0f;
            
            foreach (var particleSystemProperty in propertyParticleSystemGroup.particleSystemProperties)
            {
                if (particleSystemProperty.value.waitForParticleSystemComplete)
                {
                    waitForComplete = true;
                    maxWaitDuration = Math.Max(maxWaitDuration, particleSystemProperty.value.particleSystemDuration + particleSystemProperty.value.delayBeforeParticleSystemStart);
                }
            }
            
            foreach (var particleSystemProperty in propertyParticleSystemGroup.particleSystemProperties)
            {
                BettrRoutineRunner.Instance.StartCoroutine(PlayParticleSystemProperty(context,
                    particleSystemProperty.value));
            }

            if (waitForComplete && maxWaitDuration > 0) yield return new WaitForSeconds(maxWaitDuration);

        }
        
        public IEnumerator PlayParticleSystemProperty(CrayonScriptContext context, PropertyParticleSystem particleSystemProperty)
        {
            if (particleSystemProperty == null)
            {
                context.SetError(new ScriptRuntimeException(new NullReferenceException("null particle system property")));
                yield break;
            }
            
            var delay = particleSystemProperty.delayBeforeParticleSystemStart;
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            var particleSystem = particleSystemProperty.particleSystem;
            particleSystem.Play();
            var waitForComplete = particleSystemProperty.waitForParticleSystemComplete;
            var duration = particleSystemProperty.particleSystemDuration;
            if (waitForComplete && duration > 0)
            {
                yield return new WaitForSeconds(duration);
            }
            context.FloatResult = duration;
        }

        public void StopParticleSystemProperty(PropertyParticleSystem particleSystemProperty)
        {
            var particleSystem = particleSystemProperty.particleSystem;
            particleSystem.Stop();
        }

        public void OverlayFirstOverSecond(GameObject firstGameObject, GameObject secondGameObject)
        {
            firstGameObject.transform.position = secondGameObject.transform.position;
        }
        
        public void DestroyGameObject(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }

        public static void SwitchOrientationToPortrait()
        {
#if !UNITY_WEBGL
            Screen.orientation = ScreenOrientation.Portrait;
#endif
#if UNITY_EDITOR
            UpdateEditorGameViewSize(720, 1280, "1280x720 Portrait");
#endif
        }
        
        public static void SwitchOrientationToLandscape()
        {
            // Disable Screen.Orientation for WebGL
#if !UNITY_WEBGL
            Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
#if UNITY_EDITOR
            UpdateEditorGameViewSize(1280, 720, "1280x720 Landscape");
#endif
        }

        public static void UpdateEditorGameViewSize(int width, int height, string baseName)
        {
#if UNITY_EDITOR
            var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            var gameView = EditorWindow.GetWindow(gameViewType);
            var setCustomResolutionMethod = gameViewType?.GetMethod("SetCustomResolution", BindingFlags.Instance | BindingFlags.NonPublic);
            if (setCustomResolutionMethod != null)
            {
                setCustomResolutionMethod.Invoke(gameView, new object[] { new Vector2(width, height), baseName });
            }
#endif
        }

        public static string GetGameObjectFullPath(GameObject obj)
        {
            var scene = SceneManager.GetActiveScene();
            var sceneName = scene.name;
            string path = $"{sceneName}/{obj.name}";
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = $"{path}/{obj.name}";
            }
            return path;
        }
        
    }
}