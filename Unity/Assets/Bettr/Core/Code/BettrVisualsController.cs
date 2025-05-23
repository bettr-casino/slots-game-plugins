using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using CrayonScript.Interpreter.Execution.VM;
using PathCreation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void RollupText(long value);

    public class LayerToCameraMap
    {
        private static Dictionary<string, string> _layerIDToCameraNamesMap = new Dictionary<string, string>()
        {
            {"UI", "Camera_UI"},
            {"SLOT_BACKGROUND", "Camera_Background"},
            {"SLOT_REELS", "Camera_Reels"},
            {"SLOT_FRAME", "Camera_Frame"},
            {"SLOT_OVERLAY", "Camera_Overlay"},
            {"SLOT_REELS_OVERLAY", "Camera_Reels_Overlay"},
            {"SLOT_TRANSITION", "Camera_Transition"},
            {"Default", "Main Camera"},
        };
        
        Dictionary<string, Camera> _layerToCamera = new Dictionary<string, Camera>();

        public void Reset()
        {
            _layerToCamera.Clear();
        }
        
        public Camera GetCameraForLayer(string layerName)
        {
            if (_layerToCamera.TryGetValue(layerName, out var layer))
            {
                return layer;
            }
            // find the camera for the layer and if not exists throw an exception
            if (_layerIDToCameraNamesMap.TryGetValue(layerName, out var cameraName))
            {
                var camera = GameObject.Find(cameraName)?.GetComponent<Camera>();
                if (camera == null)
                {
                    throw new ScriptRuntimeException($"Camera not found for layer {layerName}");
                }
                _layerToCamera[layerName] = camera;
                return camera;
            }
            throw new ScriptRuntimeException($"Camera not found for layer {layerName}");
        }
    }

    [Serializable]
    internal class FireballObject
    {
        private GameObject _fireball;
        private ParticleSystem _particleSystem;
        
        private LayerToCameraMap _layerToCameraMap = new LayerToCameraMap();

        public GameObject Fireball
        {
            get => _fireball;
            set
            {
                _fireball = value;

                if (_fireball == null)
                {
                    _particleSystem = null;
                    return;
                }

                var ball = _fireball.transform.Find("ball")?.gameObject;
                if (ball == null)
                {
                    Debug.LogWarning("Fireball does not contain a 'ball' child object.");
                    return;
                }

                _particleSystem = ball.GetComponent<ParticleSystem>();
                if (_particleSystem != null)
                {
                    var main = _particleSystem.main;
                    main.startSpeed = 0;
                    main.duration = 0;
                    main.loop = true;
                    main.playOnAwake = false;
                    main.simulationSpeed = 1;
                }

                _fireball.SetActive(false);
            }
        }
        
        public ParticleSystem ParticleSystem => _particleSystem;

        public FireballObject(GameObject seed)
        {
            // clone seed
            Fireball = Object.Instantiate(seed);
        }

        public void SetActive(bool active)
        {
            this._fireball.SetActive(active);
        }
    }

    [Serializable]
    internal class FireballObjectsCache
    {
        private ParticleSystem _particleSystem;
        
        // list of fireballs created
        private readonly List<FireballObject> _freeList;

        private readonly GameObject _seed;

        private readonly List<FireballObjectRunningKey> _runningKeys;
        
        public FireballObjectsCache(GameObject seed)
        {
            _seed = seed;
            _freeList = new List<FireballObject>();
            _runningKeys = new List<FireballObjectRunningKey>();
        }

        public FireballObject Acquire()
        {
            if (_freeList.Count == 0)
            {
                return new FireballObject(this._seed);
            }
            var fireball = _freeList[0];
            _freeList.RemoveAt(0);
            return fireball;
        }

        public void Release(FireballObject fireball)
        {
            _freeList.Add(fireball);
        }

        public void HoldRunningKey(FireballObjectRunningKey runningKey)
        {
            this._runningKeys.Add(runningKey);
        }
        
        public void StopRunningKey(string runningKeyID)
        {
            var runningKey = this._runningKeys.Find(key => key.ID == runningKeyID);
            if (runningKey != null)
            {
                runningKey.Stop();
                this._runningKeys.Remove(runningKey);   
                // release the fireball object
                this.Release(runningKey.FireballObject);
            }
        }

        public void Reset()
        {
            // release all fireballs back into the _freeList
            foreach (var fireball in _freeList)
            {
                Object.Destroy(fireball.Fireball);
                fireball.Fireball = null;
            }
            _freeList.Clear();
            if (_runningKeys != null)
            {
                foreach (var runningKey in _runningKeys)
                {
                    runningKey.Stop();
                }
                _runningKeys.Clear();
            }
        }
    }
    
    [Serializable]
    public class FireballObjectRunningKey
    {
        internal FireballObject FireballObject { get; private set; }
        internal ParticleSystem ParticleSystem { get; private set; }
        
        internal string ID { get; private set; }
        
        internal FireballObjectRunningKey(FireballObject fireballObject, ParticleSystem particleSystem)
        {
            this.ParticleSystem = particleSystem;
            this.FireballObject = fireballObject;
            this.ID = Guid.NewGuid().ToString();
        }
        
        public void Stop()
        {
            this.ParticleSystem.Stop();
            this.FireballObject.SetActive(false);
            
            // null all
            this.ParticleSystem = null;
            this.FireballObject = null;
        }
    }
    
    [Serializable]
    internal class FireTornadoObject
    {
        private GameObject _fireTornado;
        private ParticleSystem _particleSystem;
        
        private LayerToCameraMap _layerToCameraMap = new LayerToCameraMap();

        public GameObject FireTornado
        {
            get => _fireTornado;
            set
            {
                _fireTornado = value;

                if (_fireTornado == null)
                {
                    _particleSystem = null;
                    return;
                }

                var particleSystemGameObject = _fireTornado.transform.Find("base")?.gameObject;
                if (particleSystemGameObject == null)
                {
                    Debug.LogWarning("FireTornado does not contain a 'base' child object.");
                    return;
                }

                _particleSystem = particleSystemGameObject.GetComponent<ParticleSystem>();
                if (_particleSystem != null)
                {
                    var main = _particleSystem.main;
                    main.startSpeed = 0;
                    main.duration = 0;
                    main.loop = true;
                    main.playOnAwake = false;
                    main.simulationSpeed = 1;
                }

                _fireTornado.SetActive(false);
            }
        }
        
        public ParticleSystem ParticleSystem => _particleSystem;

        public FireTornadoObject(GameObject seed)
        {
            // clone seed
            FireTornado = Object.Instantiate(seed);
        }
    }

    [Serializable]
    public class FireTornadoObjectRunningKey
    {
        
    }

    [Serializable]
    internal class FireTornadoObjectsCache
    {
        private ParticleSystem _particleSystem;
        
        // list of fireballs created
        private readonly List<FireTornadoObject> _freeList;

        private readonly GameObject _seed;
        
        public FireTornadoObjectsCache(GameObject seed)
        {
            _seed = seed;
            _freeList = new List<FireTornadoObject>();
        }

        public FireTornadoObject Acquire()
        {
            if (_freeList.Count == 0)
            {
                return new FireTornadoObject(this._seed);
            }
            var fireball = _freeList[0];
            _freeList.RemoveAt(0);
            return fireball;
        }

        public void Release(FireTornadoObject fireball)
        {
            _freeList.Add(fireball);
        }

        public void Reset()
        {
            // release all fireballs back into the _freeList
            foreach (var fireTornado in _freeList)
            {
                Object.Destroy(fireTornado.FireTornado);
                fireTornado.FireTornado = null;
            }
            _freeList.Clear();
        }
    }
    
    public class BettrVisualsController
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        private LayerToCameraMap _layerToCameraMap = new LayerToCameraMap();

        private FireballObjectsCache _fireballObjectsCache;
        
        private FireTornadoObjectsCache _fireTornadoObjectsCache;

        public void InitFireballs(GameObject fireball)
        {
            _fireballObjectsCache = new FireballObjectsCache(fireball);
        }
        
        public void InitFireTornados(GameObject fireTornado)
        {
            _fireTornadoObjectsCache = new FireTornadoObjectsCache(fireTornado);
        }

        public BettrUserController BettrUserController { get; private set; }
        
        public static BettrVisualsController Instance { get; private set; }
        
        public BettrVisualsController(BettrUserController bettrUserController)
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

            TileController.RegisterType<FireballObjectRunningKey>("FireballObjectRunningKey");
            TileController.RegisterType<FireTornadoObjectRunningKey>("FireballObjectRunningKey");
            
            _layerToCameraMap.Reset();
            
            BettrUserController = bettrUserController;
            
            Instance = this;
        }

        public void Reset()
        {
            if (_layerToCameraMap != null)
            {
                _layerToCameraMap.Reset();
            }
        }
        
        private Vector3? CalculatePosition(GameObject obj, Camera referenceCamera, float offsetY = 0)
        {
            if (obj == null) return null;

            // Retrieve the camera associated with the object's layer
            string layerName = LayerMask.LayerToName(obj.layer);
            Camera objCamera = _layerToCameraMap.GetCameraForLayer(layerName);

            if (objCamera == null)
            {
                Debug.LogError($"No camera found for layer {layerName}");
                return null;
            }

            // If objCamera and referenceCamera are the same, no need to convert
            if (objCamera == referenceCamera)
            {
                Vector3 adjustedPosition = obj.transform.position;
                adjustedPosition.y += offsetY; // Apply the offset
                return adjustedPosition;
            }

            // Convert the object's position from objCamera to referenceCamera
            Vector3 screenPos = objCamera.WorldToScreenPoint(obj.transform.position);

            return referenceCamera.ScreenToWorldPoint(new Vector3(
                screenPos.x,
                screenPos.y + offsetY, // Apply the offset
                screenPos.z // Preserve the object's depth
            ));
        }


        private bool _tweenComplete = false;

        public IEnumerator TweenRotateGameObject(CrayonScriptContext context, GameObject tweenThisGameObject, int numberOfRotations = 1, float duration = 1.0f)
        {
            if (tweenThisGameObject == null)
            {
                throw new ArgumentNullException(nameof(tweenThisGameObject));
            }

            string layerName = LayerMask.LayerToName(tweenThisGameObject.layer);
            Camera tweenCamera = _layerToCameraMap.GetCameraForLayer(layerName);
    
            if (tweenCamera == null)
            {
                throw new ScriptRuntimeException($"No camera found for layer '{layerName}'");
            }

            iTween.Stop(tweenThisGameObject);
            _tweenComplete = false;
            
            var rotationAmount = new Vector3(0, numberOfRotations, 0);

            iTween.RotateBy(tweenThisGameObject, iTween.Hash(
                "amount", rotationAmount,      // Changed "position" to "amount" for rotation
                "time", duration,
                "easetype", iTween.EaseType.linear,
                "oncomplete", "OnTweenComplete",
                "oncompletetarget", tweenThisGameObject // Changed to reference the component's gameObject
            ));

            float elapsedTime = 0f;
            while (!_tweenComplete && elapsedTime < duration + 0.1f)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        public IEnumerator TweenGameObject(
            CrayonScriptContext context, GameObject tweenThisGameObject, GameObject tweenFromThisGameObject,
            GameObject tweenToThisGameObject, float duration = 1.0f, bool tween = false, bool preserveLocalZ = false)

        {
            yield return TweenGameObject(context, tweenThisGameObject, tweenFromThisGameObject, tweenToThisGameObject, duration, tween, preserveLocalZ, 0.0f);
        }
        
        public IEnumerator TweenGameObject(
            CrayonScriptContext context, GameObject tweenThisGameObject, GameObject tweenFromThisGameObject, GameObject tweenToThisGameObject, float duration = 1.0f, bool tween = false, bool preserveLocalZ = false, float offsetZ = 0.0f)
        {
            var originalLocalPosition = tweenThisGameObject.transform.localPosition;
            
            string layerName = LayerMask.LayerToName(tweenThisGameObject.layer);
            Camera tweenCamera = _layerToCameraMap.GetCameraForLayer(layerName);
            if (tweenCamera == null)
            {
                throw new ScriptRuntimeException($"No camera found for layer '{layerName}'");
            }
            iTween.Stop(tweenThisGameObject);
            Vector3 startWorldPosition = CalculatePosition(tweenFromThisGameObject, tweenCamera) ?? tweenThisGameObject.transform.position;
            Vector3 targetWorldPosition = CalculatePosition(tweenToThisGameObject, tweenCamera) ?? tweenThisGameObject.transform.position;
            tweenThisGameObject.transform.position = startWorldPosition;
            
            // add the offsetZ to the targetWorldPosition
            targetWorldPosition.z += offsetZ;
            
            if (tween)
            {
                _tweenComplete = false;
                iTween.MoveTo(tweenThisGameObject, iTween.Hash(
                    "position", targetWorldPosition,
                    "time", duration,
                    "easetype", iTween.EaseType.linear,
                    "oncomplete", "OnTweenComplete",
                    "oncompletetarget", tweenThisGameObject
                ));
                var elapsedTime = 0f;
                while (!_tweenComplete && elapsedTime < duration + 0.1f)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                tweenThisGameObject.transform.position = targetWorldPosition;
                yield return new WaitForSeconds(duration);
            }

            if (preserveLocalZ)
            {
                var localPosition = tweenThisGameObject.transform.localPosition;
                localPosition.z = originalLocalPosition.z;
                tweenThisGameObject.transform.localPosition = localPosition;
            }
        }

        public IEnumerator CloneAndTweenGameObject(
            CrayonScriptContext context, GameObject tweenFromThisGameObject, GameObject tweenToThisGameObject,
            float duration = 1.0f, bool tween = false)
        {
            yield return CloneAndTweenGameObject(context, tweenFromThisGameObject, tweenToThisGameObject, duration, tween, 0.0f, false);
        }
        
        public IEnumerator CloneAndTweenGameObject(
            CrayonScriptContext context, GameObject tweenFromThisGameObject, GameObject tweenToThisGameObject, float duration = 1.0f, bool tween = false, float offsetZ = 0.0f, bool hideOriginal = false)
        {
            bool destroyAfter = true;
            // clone this tweenThisGameObject
            var tweenThisGameObject = Object.Instantiate(tweenFromThisGameObject, tweenFromThisGameObject.transform.parent);
            // ensure this is an overlay over the original object
            OverlayFirstOverSecond(tweenThisGameObject, tweenFromThisGameObject);
            if (hideOriginal)
            {
                tweenFromThisGameObject.SetActive(false);
            }
            
            string layerName = LayerMask.LayerToName(tweenThisGameObject.layer);
            Camera tweenCamera = _layerToCameraMap.GetCameraForLayer(layerName);
            if (tweenCamera == null)
            {
                throw new ScriptRuntimeException($"No camera found for layer '{layerName}'");
            }
            iTween.Stop(tweenThisGameObject);
            Vector3 startWorldPosition = CalculatePosition(tweenFromThisGameObject, tweenCamera) ?? tweenFromThisGameObject.transform.position;
            Vector3 targetWorldPosition = CalculatePosition(tweenToThisGameObject, tweenCamera) ?? tweenToThisGameObject.transform.position;
            tweenThisGameObject.transform.position = startWorldPosition;
            
            // add the offsetZ to the targetWorldPosition
            targetWorldPosition.z += offsetZ;
            
            if (tween)
            {
                _tweenComplete = false;
                iTween.MoveTo(tweenThisGameObject, iTween.Hash(
                    "position", targetWorldPosition,
                    "time", duration,
                    "easetype", iTween.EaseType.linear,
                    "oncomplete", "OnTweenComplete",
                    "oncompletetarget", tweenThisGameObject
                ));
                var elapsedTime = 0f;
                while (!_tweenComplete && elapsedTime < duration + 0.1f)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                if (destroyAfter)
                {
                    Object.Destroy(tweenThisGameObject);
                }
            }
            else
            {
                tweenThisGameObject.transform.position = targetWorldPosition;
                yield return new WaitForSeconds(duration);
                if (destroyAfter)
                {
                    Object.Destroy(tweenThisGameObject);
                }
            }

            if (hideOriginal)
            {
                tweenFromThisGameObject.SetActive(true);
            }
        }
        
        public Rect GetQuadBounds(GameObject quad)
        {
            // Get layer for quad
            string layerName = LayerMask.LayerToName(quad.layer);
            // Retrieve the camera
            Camera camera = _layerToCameraMap.GetCameraForLayer(layerName);
            if (camera == null)
            {
                throw new ScriptRuntimeException($"No camera found for {layerName}");
            }

            // Get the renderer of the quad
            Renderer quadRenderer = quad.GetComponent<Renderer>();
            
            // If no renderer found on the main object, check children
            if (quadRenderer == null)
            {
                // Get all child renderers
                Renderer[] childRenderers = quad.GetComponentsInChildren<Renderer>();
                
                if (childRenderers.Length == 0)
                {
                    Debug.LogError("Neither the quad object nor its children have a Renderer component.");
                    return new Rect();
                }

                // Calculate combined bounds of all child renderers
                Bounds bounds = childRenderers[0].bounds;
                for (int i = 1; i < childRenderers.Length; i++)
                {
                    bounds.Encapsulate(childRenderers[i].bounds);
                }

                // Convert the bounds to screen space
                Vector3 bottomWorld = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
                Vector3 topWorld = new Vector3(bounds.max.x, bounds.max.y, bounds.center.z);

                Vector3 bottomScreen = camera.WorldToScreenPoint(bottomWorld);
                Vector3 topScreen = camera.WorldToScreenPoint(topWorld);

                return new Rect(bottomScreen.x, bottomScreen.y, 
                               Mathf.Abs(topScreen.x - bottomScreen.x), 
                               Mathf.Abs(topScreen.y - bottomScreen.y));
            }
            else
            {
                // Original code for when the main object has a renderer
                Bounds bounds = quadRenderer.bounds;

                Vector3 bottomWorld = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
                Vector3 topWorld = new Vector3(bounds.max.x, bounds.max.y, bounds.center.z);

                Vector3 bottomScreen = camera.WorldToScreenPoint(bottomWorld);
                Vector3 topScreen = camera.WorldToScreenPoint(topWorld);

                return new Rect(bottomScreen.x, bottomScreen.y, 
                               Mathf.Abs(topScreen.x - bottomScreen.x), 
                               Mathf.Abs(topScreen.y - bottomScreen.y));
            }
        }

        public IEnumerator FireballTornadoAt(CrayonScriptContext context, GameObject at, float offsetY, float duration)
        {
            Camera fireTornadoCamera = _layerToCameraMap.GetCameraForLayer("SLOT_TRANSITION");
            if (fireTornadoCamera == null)
            {
                throw new ScriptRuntimeException("No camera found for SLOT_TRANSITION");
            }
            
            // acquire a fireTornado object
            var fireTornadoObject = _fireTornadoObjectsCache.Acquire();
            
            var fireTornado = fireTornadoObject.FireTornado;
            var particleSystem = fireTornadoObject.ParticleSystem;

            particleSystem.Stop();
            fireTornado.SetActive(false);

            // Calculate positions
            Vector3 startWorldPosition = fireTornado.transform.position;
            Vector3 targetWorldPosition = CalculatePosition(at, fireTornadoCamera, offsetY) ?? fireTornado.transform.position;

            // Debug log positions
            Debug.Log($"Target Position: {targetWorldPosition}");

            // Set initial position and activate
            fireTornado.transform.position = startWorldPosition;
            fireTornado.SetActive(true);
            particleSystem.Play();

            fireTornado.transform.position = targetWorldPosition;
            yield return new WaitForSeconds(duration);

            particleSystem.Stop();
            fireTornado.SetActive(false);
            
            // release the fireball object
            _fireTornadoObjectsCache.Release(fireTornadoObject);
        }
        
        public void FireballStop(string runningKeyID)
        {
            _fireballObjectsCache.StopRunningKey(runningKeyID);
        }

        public IEnumerator FireballMoveTo(CrayonScriptContext context, GameObject from, GameObject to,
            float offsetY = 10, float duration = 1.0f, bool tween = false)
        {
            yield return FireballMoveTo(context, from, to, offsetY, duration, tween, false);
        }
        
        public IEnumerator FireballMoveTo(CrayonScriptContext context, GameObject from, GameObject to, float offsetY = 10, float duration = 1.0f, bool tween = false, bool wait = false)
        {
            Camera fireballCamera = _layerToCameraMap.GetCameraForLayer("SLOT_TRANSITION");
            if (fireballCamera == null)
            {
                throw new ScriptRuntimeException("No camera found for SLOT_TRANSITION");
            }
            
            // acquire a fireball object
            var fireballObject = _fireballObjectsCache.Acquire();
            
            var fireball = fireballObject.Fireball;
            var particleSystem = fireballObject.ParticleSystem;

            particleSystem.Stop();
            fireball.SetActive(false);

            // Make sure any existing tweens are stopped
            iTween.Stop(fireball);

            // Calculate positions
            Vector3 startWorldPosition = CalculatePosition(from, fireballCamera) ?? fireball.transform.position;
            Vector3 targetWorldPosition = CalculatePosition(to, fireballCamera, offsetY) ?? fireball.transform.position;

            // Debug log positions
            Debug.Log($"Start Position: {startWorldPosition}");
            Debug.Log($"Target Position: {targetWorldPosition}");

            // Set initial position and activate
            fireball.transform.position = startWorldPosition;
            fireball.SetActive(true);
            particleSystem.Play();

            if (tween)
            {
                _tweenComplete = false;
                iTween.MoveTo(fireball, iTween.Hash(
                    "position", targetWorldPosition,
                    "time", duration,
                    "easetype", iTween.EaseType.linear,
                    "oncomplete", "OnFireballTweenComplete",
                    "oncompletetarget", fireball
                ));

                var elapsedTime = 0f;
                while (!_tweenComplete && elapsedTime < duration + 0.1f)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                fireball.transform.position = targetWorldPosition;
                yield return new WaitForSeconds(duration);
            }

            if (wait)
            {
                var fireballRunningKey = new FireballObjectRunningKey(fireballObject, particleSystem);
                _fireballObjectsCache.HoldRunningKey(fireballRunningKey);
                
                context.StringResult = fireballRunningKey.ID;
            }
            else
            {
                particleSystem.Stop();
                fireball.SetActive(false);
            
                // release the fireball object
                _fireballObjectsCache.Release(fireballObject);
            }
        }

        private void OnFireballTweenComplete()
        {
            _tweenComplete = true;
        }
        
        private void OnTweenComplete()
        {
            _tweenComplete = true;
        }

        public IEnumerator RollUpCounterAndWait(CrayonScriptContext context, PropertyTextMeshPro counterTextProperty, long start, long end, float duration)
        {
            RollUpCounter(counterTextProperty, start, end, duration);
            yield return new WaitForSeconds(duration);
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
                value => counterTextProperty.Format(value), 
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
                var isSlamStopped = BettrUserController.UserInSlamStopMode;
                if (isSlamStopped)
                {
                    break;
                }
                
                elapsed += Time.deltaTime;

                // Update the current value
                currentValue += speed * Time.deltaTime;
                
                // if rollingUp and currentValue drops above end, set it to end
                if (isRollingUp && currentValue > end)
                {
                    currentValue = end;
                }
                // if rollingDown and currentValue goes below end, set it to end
                else if (!isRollingUp && currentValue < end)
                {
                    currentValue = end;
                }

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
        
        public IEnumerator TweenScaleGameObject(GameObject objectToScale, float scaleX, float scaleY, float duration = 1.0f)
        {
            var propertyTween = new PropertyTween
            {
                useScale = true,
                scaleTo = new Vector3(scaleX, scaleY),
                tweenDuration = duration,
                tweenDelay = 0,
                easeType = iTween.EaseType.linear
            };
            
            TweenScaleTo(objectToScale, propertyTween);

            if (duration > 0)
            {
                yield return new WaitForSeconds(duration);
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
            
            if (animatorProperty.GameObject == null)
            {
                context.SetError(new ScriptRuntimeException(new NullReferenceException($"null game object {animatorProperty.animationStateName}")));
                yield break;
            }
            
            var isSlamStopped = BettrUserController.UserInSlamStopMode;
            if (isSlamStopped)
            {
                yield break;
            }
            
            var delayBeforeAnimationStart = animatorProperty.delayBeforeAnimationStart;
            if (delayBeforeAnimationStart > 0)
            {
                // use yield null so that slam stop can be detected
                var timeElapsedInMilliseconds = 0.0f;
                while (timeElapsedInMilliseconds < delayBeforeAnimationStart)
                {
                    timeElapsedInMilliseconds += Time.deltaTime;
                    isSlamStopped = BettrUserController.UserInSlamStopMode;
                    if (isSlamStopped)
                    {
                        yield break;
                    }
                    yield return null;
                }
            }
            if (animatorProperty.GameObject == null)
            {
                // this is possible if the GameObject has been destroyed
                yield break;
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
                var timeElapsedInMilliseconds = 0.0f;
                while (timeElapsedInMilliseconds < animationDuration)
                {
                    timeElapsedInMilliseconds += Time.deltaTime;
                    isSlamStopped = BettrUserController.UserInSlamStopMode;
                    if (isSlamStopped)
                    {
                        animator.Play(animationStateName, -1, 0f);
                        yield break;
                    }
                    yield return null;
                }
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
        
        public void SetZeroZPosition(GameObject gameObject)
        {
            var position = gameObject.transform.position;
            position.z = 0;
            gameObject.transform.position = position;
        }

        public TilePropertyGameObjectGroup CloneGameObjectGroup(TilePropertyGameObjectGroup group)
        {
            var groupClone = new TilePropertyGameObjectGroup
            {
                groupKey = group.groupKey,
                gameObjectProperties = new List<TilePropertyGameObject>()
            };
            foreach (var property in group.gameObjectProperties)
            {
                var value = property.value;
                value = Clone(value);
                
                var propertyClone = new TilePropertyGameObject
                {
                    key = property.key,
                    value = value
                };
                groupClone.gameObjectProperties.Add(propertyClone);
            }
            
            return groupClone;
        }

        public PropertyGameObject Clone(PropertyGameObject gameObjectProperty)
        {
            var clonedGameObject = Object.Instantiate(gameObjectProperty.GameObject, gameObjectProperty.GameObject.transform.parent);
            clonedGameObject.name = gameObjectProperty.GameObject.name;
            var clonedGameObjectProperty = new PropertyGameObject()
            {
                gameObject = clonedGameObject,
            };
            return clonedGameObjectProperty;
        }
        
        public GameObject CloneAndOverlayGameObject(GameObject gameObject)
        {
            var clonedGameObject = Object.Instantiate(gameObject, gameObject.transform.parent);
            clonedGameObject.name = gameObject.name;
            // ensure this is an overlay over the original object
            OverlayFirstOverSecond(clonedGameObject, gameObject);
            return clonedGameObject;
        }
        
        public PropertyGameObject CloneAndOverlay(PropertyGameObject gameObjectProperty)
        {
            var clonedGameObject = Object.Instantiate(gameObjectProperty.GameObject, gameObjectProperty.GameObject.transform.parent);
            clonedGameObject.name = gameObjectProperty.GameObject.name;
            // ensure this is an overlay over the original object
            OverlayFirstOverSecond(clonedGameObject, gameObjectProperty.GameObject);
            var clonedGameObjectProperty = new PropertyGameObject()
            {
                gameObject = clonedGameObject,
            };
            return clonedGameObjectProperty;
        }
        
        public PropertyTextMeshPro CloneAndOverlayText(PropertyTextMeshPro textProperty)
        {
            var text = textProperty.textMeshPro;
            var gameObject = text.gameObject;
            var clonedGameObject = Object.Instantiate(gameObject, gameObject.transform.parent);
            // ensure this is an overlay over the original object
            OverlayFirstOverSecond(clonedGameObject, gameObject);
            var clonedText = clonedGameObject.GetComponent<TMP_Text>();
            var tmPro = new PropertyTextMeshPro()
            {
                textMeshPro = clonedText,
            };
            return tmPro;
        }
        
        public void OverlayFirstOverSecond(PropertyGameObject firstGameObjectProperty, PropertyGameObject secondGameObjectProperty)
        {
            OverlayFirstOverSecond(firstGameObjectProperty.gameObject, secondGameObjectProperty.gameObject);
        }

        public void OverlayFirstOverSecond(GameObject firstGameObject, GameObject secondGameObject)
        {
            // Get the layer names
            string firstLayerName = LayerMask.LayerToName(firstGameObject.layer);
            string secondLayerName = LayerMask.LayerToName(secondGameObject.layer);

            // Retrieve the cameras associated with the layers
            Camera firstCamera = _layerToCameraMap.GetCameraForLayer(firstLayerName);
            Camera secondCamera = _layerToCameraMap.GetCameraForLayer(secondLayerName);

            if (firstCamera == null || secondCamera == null)
            {
                Debug.LogWarning($"Camera not found for layers {firstLayerName} or {secondLayerName}");
                return;
            }

            // Convert second object's world position to the first object's camera view
            Vector3 secondObjectScreenPosition = secondCamera.WorldToScreenPoint(secondGameObject.transform.position);
            Vector3 worldPositionForFirstObject = firstCamera.ScreenToWorldPoint(
                new Vector3(secondObjectScreenPosition.x, secondObjectScreenPosition.y, firstCamera.nearClipPlane)
            );

            // Align the first object to the second object's position based on cameras
            firstGameObject.transform.position = worldPositionForFirstObject;
        }
        
        public void ScaleFirstToSecond(GameObject firstGameObject, GameObject secondGameObject)
        {
            try
            {
                // Get the bounds in screen space using GetQuadBounds
                Rect firstBounds = GetQuadBounds(firstGameObject);
                Rect secondBounds = GetQuadBounds(secondGameObject);

                // Calculate scale factors based on the screen space bounds
                Vector3 scaleFactors = new Vector3(
                    secondBounds.width / firstBounds.width,
                    secondBounds.height / firstBounds.height,
                    1.0f
                );

                // Apply the scale to the first object
                firstGameObject.transform.localScale = Vector3.Scale(
                    firstGameObject.transform.localScale,
                    scaleFactors
                );
            }
            catch (ScriptRuntimeException e)
            {
                Debug.LogError($"Error scaling objects: {e.Message}");
            }
        }
        
        public void DestroyGameObject(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
        
        public void DestroyGameObject(PropertyGameObject gameObjectProperty)
        {
            Object.Destroy(gameObjectProperty.GameObject);
        }
        
        public void DestroyGameObject(TilePropertyGameObjectGroup gameObjectGroupProperty)
        {
            foreach (var gameObjectProperty in gameObjectGroupProperty.gameObjectProperties)
            {
                DestroyGameObject(gameObjectProperty.value);
            }
        }

        public void SetMaterialAlpha(GameObject go, float alpha)
        {
            var meshRenderer = go.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogWarning($"MeshRenderer not found for {go.name}");
                return;
            }
            var material = meshRenderer.material;
            // Verify material has color property
            if (!material.HasProperty(Color1))
            {
                Debug.LogWarning($"Material {material.name} does not have _Color property");
                return;
            }
            var color = material.color;
            color.a = alpha;
            material.color = color;
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