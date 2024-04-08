using System;
using System.Collections.Generic;
using System.IO;
using CrayonScript.Code;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Bettr.Editor
{
    public static class Utils
    {
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        
        public static Material CreateOrLoadMaterial(string materialName, string shaderName, string textureName, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var materialFilename = $"{materialName}.mat";
            var materialFilepath = $"{runtimeAssetPath}/Materials/{materialFilename}";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);
            if (material == null)
            {
                Debug.Log($"Creating material for {materialName} at {materialFilepath}");
                try
                {
                    material = new Material(Shader.Find(shaderName));
                    AssetDatabase.CreateAsset(material, materialFilepath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            AssetDatabase.Refresh();
            
            material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);
            string sourcePath = Path.Combine("Assets", "Bettr", "Editor", "textures", textureName);
            var destPath = $"{runtimeAssetPath}/Textures/{textureName}";
            string extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension))
            {
                extension = File.Exists($"{sourcePath}.jpg") ? ".jpg" : ".png";
                sourcePath += extension;
                destPath += extension;
            }
            ImportTexture2D( sourcePath, destPath);
            AssetDatabase.Refresh();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{InstanceComponent.RuntimeAssetPath}/Textures/{textureName}.jpg");
            if (texture == null)
            {
                throw new Exception($"{textureName} texture not found.");
            }
            material.SetTexture(MainTex, texture);

            AssetDatabase.Refresh();

            return material;
        }
        
        public static void ImportTexture2D(string sourcePath, string destPath, TextureImporterType textureImporterType = TextureImporterType.Sprite)
        {
            File.Copy(sourcePath, destPath, overwrite: true);
            // Import the copied image file as a Texture2D asset
            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
            TextureImporter textureImporter = AssetImporter.GetAtPath(destPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = textureImporterType;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    public interface IGameObject
    {
        public GameObject GameObject { get; }
        public void SetParent(GameObject parentGo);
        public void SetParent(IGameObject parentGo);
    }

    [Serializable]
    public class InstanceGameObject : IGameObject
    {
        public static Dictionary<string, InstanceGameObject> IdGameObjects = new Dictionary<string, InstanceGameObject>();
        
        private GameObject _go;
        public string Name { get; set; }
        
        private string _Id;

        public string Id
        {
            get => _Id;
            set { _Id = value;
                IdGameObjects[_Id] = this;
            }
        }
        
        public string PrefabName { get; set; }
        
        public bool IsPrefab { get; set; }
        
        public string PrimitiveMaterial { get; set; }
        
        public string PrimitiveShader { get; set; }
        
        public string PrimitiveTexture { get; set; }
        
        public int Primitive { get; set; }
        
        public bool IsPrimitive { get; set; }
        
        public bool Active { get; set; }
        
        public string Layer { get; set; }
        
        public Vector3? Position { get; set; }
        
        public Vector3? Rotation { get; set; }
        
        public Vector3? Scale { get; set; }
        
        private List<InstanceComponent> _components;
        
        public List<InstanceComponent> Components {
            get => _components;
            set
            {
                _components = value;
                EnsureGameObject();
                foreach (var component in _components)
                {
                    component.AddComponent(_go);
                }
            }
        }

        private InstanceGameObject _child;
        
        public InstanceGameObject Child {
            get => _child;
            set
            {
                _child = value;
                EnsureGameObject();
                _child.SetParent(_go);
            }
        }
        
        private List<InstanceGameObject> _children;

        public List<InstanceGameObject> Children
        {
            get => _children;
            set
            {
                _children = value;
                EnsureGameObject();
                foreach (var child in _children)
                {
                    child.SetParent(_go);
                }
            }
        }

        public GameObject GameObject => _go;

        public InstanceGameObject()
        {
            Active = true;
            Layer = "Default";
        }
        
        public InstanceGameObject(GameObject go)
        {
            _go = go;
            Name = go.name;
        }

        public InstanceGameObject(string name)
        {
            Name = name;
            EnsureGameObject();
        }
        
        public void SetParent(GameObject parentGo)
        {
            EnsureGameObject();
            _go.transform.SetParent(parentGo.transform);
            _go.transform.position = new Vector3(0, 0, 0);
            if (Position != null)
            {
                _go.transform.position = (Vector3) Position;
            }
            _go.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (Rotation != null)
            {
                _go.transform.rotation = Quaternion.Euler((Vector3) Rotation);
            }
            _go.transform.localScale = new Vector3(0, 0, 0);
            if (Scale != null)
            {
                _go.transform.localScale = (Vector3) Scale;
            }
        }

        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }

        private void EnsureGameObject()
        {
            if (_go == null)
            {
                if (IsPrefab)
                {
                    Debug.Log($"loading prefab from path: {InstanceComponent.RuntimeAssetPath}/Prefabs/{PrefabName}.prefab");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{InstanceComponent.RuntimeAssetPath}/Prefabs/{PrefabName}.prefab");
                    var prefabGameObject = new PrefabGameObject(prefab, Name);
                    _go = prefabGameObject.GameObject;
                }
                else if (IsPrimitive)
                {
                    var primitiveGameObject = GameObject.CreatePrimitive(Enum.GetValues(typeof(PrimitiveType)).GetValue(Primitive) as PrimitiveType? ?? PrimitiveType.Quad);
                    var primitiveMaterial = Utils.CreateOrLoadMaterial(PrimitiveMaterial, PrimitiveShader, PrimitiveTexture, InstanceComponent.RuntimeAssetPath);
                    
                    var primitiveMeshRenderer = primitiveGameObject.GetComponent<MeshRenderer>();
                    primitiveMeshRenderer.material = primitiveMaterial;
                    
                    _go = primitiveGameObject;
                }
                else
                {
                    _go = new GameObject(Name);
                }
                
                _go.SetActive(Active);
                _go.layer = LayerMask.NameToLayer(Layer);
            }
        }
    }

    public class PrefabGameObject : IGameObject
    {
        private readonly GameObject _prefab;
        private readonly GameObject _go;
        private string _name;
        
        public GameObject GameObject => _go;
        
        public PrefabGameObject(GameObject prefab, string name)
        {
            _prefab = prefab;
            _name = name;
            _go = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
            _go.name = _name;
        }
        
        public void SetParent(GameObject parentGo)
        {
            // Instantiate the child prefab and set it as a child of the new prefab
            _go.transform.SetParent(parentGo.transform);
        }
        
        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }
    }
    
    public class PrimitiveGameObject : IGameObject
    {
        private PrimitiveType _primitiveType;
        private GameObject _go;
        
        public GameObject GameObject => _go;
        
        public PrimitiveGameObject(PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
            _go = GameObject.CreatePrimitive(_primitiveType);
        }
        
        public void SetParent(GameObject parentGo)
        {
            _go.transform.SetParent(parentGo.transform);   
        }
        
        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }
    }
    
    public interface IComponent
    {
        public void AddComponent(GameObject gameObject);
    }

    [Serializable]
    public class InstanceComponent : IComponent
    {
        public static string RuntimeAssetPath;
        
        public string ComponentType { get; set; }
        
        public string Filename { get; set; }
        
        public string Color { get; set; }
        
        public string Text { get; set; }
        
        public int FontSize { get; set; }
        
        public Rect? Rect { get; set; }
        
        public string ReferenceId { get; set; }
        
        public InstanceComponent()
        {
        }
        
        public void AddComponent(GameObject gameObject)
        {
            switch (ComponentType)
            {
                case "AnimatorController":
                    var animatorComponent = new AnimatorComponent(Filename, RuntimeAssetPath);
                    animatorComponent.AddComponent(gameObject);
                    break;
                case "TextMeshPro":
                    var textMeshProComponent = new TextMeshProComponent(Text, FontSize, Color, Rect);
                    textMeshProComponent.AddComponent(gameObject);
                    break;
                case "TextMeshProUI":
                    var textMeshProUIComponent = new TextMeshProUIComponent(Text, FontSize, Color, Rect);
                    textMeshProUIComponent.AddComponent(gameObject);
                    break;
                case "Image":
                    var imageComponent = new ImageComponent(RuntimeAssetPath, Filename, Color, Rect);
                    imageComponent.AddComponent(gameObject);
                    break;
                case "RectTransform":
                    var rectTransformComponent = new RectTransformComponent(RuntimeAssetPath, Filename, Color, Rect);
                    rectTransformComponent.AddComponent(gameObject);
                    break;
                case "UICamera":
                    var uiCameraComponent = new UICameraComponent();
                    uiCameraComponent.AddComponent(gameObject);
                    break;
                case "Canvas":
                    InstanceGameObject.IdGameObjects.TryGetValue(ReferenceId, out var referenceGameObject);
                    var renderCamera = referenceGameObject?.GameObject.GetComponent<Camera>();
                    var canvasComponent = new CanvasComponent(renderCamera);
                    canvasComponent.AddComponent(gameObject);
                    break;
            }
        }
    }
    
    [Serializable]
    public class TileComponent : IComponent
    {
        private readonly TextAsset _scriptAsset;
        private readonly string _globalTileId;

        public TileComponent(string globalTileId, TextAsset scriptAsset)
        {
            _globalTileId = globalTileId;
            _scriptAsset = scriptAsset;
        }

        public void AddComponent(GameObject gameObject)
        {
            var tile = gameObject.AddComponent<Tile>();
            tile.scriptAsset = _scriptAsset;
            tile.globalTileId = _globalTileId;
        }
    }
    
    [Serializable]
    public class AnimatorComponent : IComponent
    {
        private AnimatorController _animatorController;

        private string _runtimeAssetPath;
        
        private string _fileName;
        
        public AnimatorComponent(string fileName, string runtimeAssetPath)
        {
            _fileName = fileName;
            _runtimeAssetPath = runtimeAssetPath;
        }

        public AnimatorComponent(AnimatorController animatorController)
        {
            _animatorController = animatorController;
        }

        public void AddComponent(GameObject gameObject)
        {
            if (_animatorController == null)
            {
                BuildAnimatorController(gameObject);
            }
            
            var animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = _animatorController;
        }

        private void BuildAnimatorController(GameObject gameObject)
        {
            var animatorControllerName = $"{_fileName}_anims.controller";
            var animatorControllerPath = $"{_runtimeAssetPath}/Animators/{animatorControllerName}";
            AnimatorController.CreateAnimatorControllerAtPath(animatorControllerPath);
            AssetDatabase.Refresh();
            var runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);
            _animatorController = runtimeAnimatorController;
        }
    }

    [Serializable]
    public class UICameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("UI");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = 10;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
        }
    }
    
    [Serializable]
    public class TextMeshProComponent : IComponent
    {
        public static Dictionary<string, TMP_FontAsset> FontAssetsMap = new Dictionary<string, TMP_FontAsset>()
        {
            { "Anton SDF", LoadFontAsset("Anton SDF") },
            { "Bangers SDF", LoadFontAsset("Bangers SDF") },
            { "Oswald Bold SDF", LoadFontAsset("Oswald Bold SDF") },
            { "Roboto-Bold SDF", LoadFontAsset("Roboto-Bold SDF") },
            { "LiberationSans SDF", LoadFontAsset("LiberationSans SDF") },
        };
        
        private static TMP_FontAsset LoadFontAsset(string fontAssetName)
        {
            Debug.Log($"Loading fontAssetName:{fontAssetName}");
            var fontPath = $"Assets/Bettr/Editor/fonts/{fontAssetName}.asset";
            var fontAsset =AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);;
            Debug.Log($"Loaded font asset:{fontAsset.name}");
            return fontAsset;
        }
            
        
        public string Text { get; set; }
        public float FontSize { get; set; }
        public Color FontColor { get; set; }
        
        public Rect? Rect { get; set; }
        
        public TMP_FontAsset FontAsset { get; set; }

        public TextMeshProComponent(string text, float fontSize, string colorHex, Rect? rect = null, string fontAssetName = "Roboto-Bold SDF")
        {
            Text = text;
            FontSize = fontSize;
            Rect = rect;
            
            // ReSharper disable once InlineOutVariableDeclaration
            TMP_FontAsset tmpFontAsset;
            FontAsset = FontAssetsMap.TryGetValue(fontAssetName, out tmpFontAsset) ? tmpFontAsset : FontAssetsMap["Roboto-Bold SDF"];
            Debug.Log($"TextMeshProComponent Retrieved FontAsset:{FontAsset.name}");
            
            FontColor = Color.white;
            if (ColorUtility.TryParseHtmlString(colorHex, out var tempColor))
            {
                FontColor = tempColor;
            }
            else
            {
                Debug.LogWarning($"Failed to parse color hex: {colorHex}. Using default color.");
            }
        }

        public void AddComponent(GameObject gameObject)
        {
            AddTextMeshPro(gameObject);
        }

        private void AddTextMeshPro(GameObject gameObject)
        {
            var textMeshPro = gameObject.AddComponent<TextMeshPro>();
            textMeshPro.text = Text;
            textMeshPro.fontSize = FontSize;
            textMeshPro.enableAutoSizing = false; // Ensure fixed font size
            textMeshPro.color = FontColor;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.enableWordWrapping = false;
            
            if (Rect is not null)
            {
                var rect = (Rect) Rect;
                textMeshPro.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                textMeshPro.rectTransform.pivot = new Vector2(rect.x, rect.y);
            }
            
            Debug.Log($"TextMeshProComponent Setting FontAsset:{FontAsset.name}");
            textMeshPro.fontMaterial = FontAsset.material;
        }
    }
    
    [Serializable]
    public class TextMeshProUIComponent : TextMeshProComponent
    {
        public TextMeshProUIComponent(string text, float fontSize, string colorHex, Rect? rect = null, string fontAssetName = "Roboto-Bold SDF") : base(text, fontSize, colorHex, rect, fontAssetName)
        {
        }
        
        public new void AddComponent(GameObject gameObject)
        {
            AddTextMeshProUI(gameObject);
        }

        private void AddTextMeshProUI(GameObject gameObject)
        {
            var textMeshPro = gameObject.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = Text;
            textMeshPro.fontSize = FontSize;
            textMeshPro.enableAutoSizing = false; // Ensure fixed font size
            textMeshPro.color = FontColor;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.enableWordWrapping = false;
            
            if (Rect is not null)
            {
                var rect = (Rect) Rect;
                textMeshPro.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                textMeshPro.rectTransform.pivot = new Vector2(rect.x, rect.y);
            }
        }
    }
    
    [Serializable]
    public class ImageComponent : IComponent
    {
        public Color Color { get; set; }
        public Rect? Rect { get; set; }
        
        public string TextureName { get; set; }
        
        public string RuntimeAssetPath { get; set; }
        
        // Constructor that takes a path to a Texture2D
        public ImageComponent(string runtimeAssetPath, string textureName, string colorHex, Rect? rect = null)
        {
            RuntimeAssetPath = runtimeAssetPath;
            TextureName = textureName;
            Color = Color.white;
            if (!string.IsNullOrEmpty(colorHex))
            {
                if (ColorUtility.TryParseHtmlString(colorHex, out var tempColor))
                {
                    Color = tempColor;
                }
                else
                {
                    Debug.LogWarning($"Failed to parse color hex: {colorHex}. Using default color.");
                }
            }
            Rect = rect;
        }

        public void AddComponent(GameObject gameObject)
        {
            // Add the Image component
            var image = gameObject.AddComponent<Image>();
            image.raycastTarget = true;
            image.maskable = true;

            if (!string.IsNullOrEmpty(TextureName))
            {
                string sourcePath = Path.Combine("Assets", "Bettr", "Editor", "textures", TextureName);
                var destPath = $"{RuntimeAssetPath}/Textures/{TextureName}";
                string extension = Path.GetExtension(sourcePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = File.Exists($"{sourcePath}.jpg") ? ".jpg" : ".png";
                    sourcePath += extension;
                    destPath += extension;
                }
                Utils.ImportTexture2D(sourcePath, destPath);
                AssetDatabase.Refresh();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
                image.sprite = sprite;
                image.type = Image.Type.Simple;
            }
            else
            {
                image.color = Color;
            }
            
            // Configure the RectTransform
            if (Rect is not null)
            {
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(Rect.Value.x, Rect.Value.y);
                rectTransform.sizeDelta = new Vector2(Rect.Value.width, Rect.Value.height);
            }
        }
    }
    
    [Serializable]
    public class RectTransformComponent : IComponent
    {
        public Rect? Rect { get; set; }

        // Constructor that takes a path to a Texture2D
        public RectTransformComponent(string runtimeAssetPath, string textureName, string colorHex, Rect? rect = null)
        {
            Rect = rect;
        }

        public void AddComponent(GameObject gameObject)
        {
            // Configure the RectTransform
            if (Rect is not null)
            {
                RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(Rect.Value.x, Rect.Value.y);
                rectTransform.sizeDelta = new Vector2(Rect.Value.width, Rect.Value.height);
            }
        }
    }
    
    [Serializable]
    public class CanvasComponent : IComponent
    {
        public Camera RenderCamera { get; set; }
        
        // Canvas settings
        public static float DefaultMatchWidthOrHeight = 0f;
        public static CanvasScaler.ScreenMatchMode DefaultScreenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        public static CanvasScaler.ScaleMode DefaultScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        public static RenderMode DefaultRenderMode = RenderMode.ScreenSpaceCamera;
        public static Vector2 DefaultReferenceResolution = new Vector2(800, 600);
        public static int DefaultSortOrder = 0;
        public static int DefaultPlaneDistance = 100;
        public static bool DefaultPixelPerfect = false;
        public static string DefaultSortingLayerName = "Default";

        // Scaler settings
        public static int DefaultReferencePixelsPerUnit = 100;
        
        // Raycaster settings
        public static bool DefaultIgnoreReversedGraphics = true;

        // Constructor
        public CanvasComponent(
            Camera renderCamera = null)
        {
            RenderCamera = renderCamera;
        }

        public void AddComponent(GameObject gameObject)
        {
            // Ensure the GameObject has a RectTransform component
            if (gameObject.GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
            }

            // Add the Canvas component
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.pixelPerfect = DefaultPixelPerfect;
            canvas.worldCamera = RenderCamera;
            canvas.planeDistance = DefaultPlaneDistance;
            canvas.renderMode = DefaultRenderMode;
            canvas.sortingLayerName = DefaultSortingLayerName;
            canvas.sortingOrder = DefaultSortOrder;

            // Add the Canvas Scaler component
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = DefaultScaleMode;
            scaler.referenceResolution = DefaultReferenceResolution;
            scaler.screenMatchMode = DefaultScreenMatchMode;
            scaler.matchWidthOrHeight = DefaultMatchWidthOrHeight;
            scaler.referencePixelsPerUnit = DefaultReferencePixelsPerUnit;

            // Add the Graphic Raycaster component
            GraphicRaycaster raycaster = gameObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = DefaultIgnoreReversedGraphics;
        }
    }
}