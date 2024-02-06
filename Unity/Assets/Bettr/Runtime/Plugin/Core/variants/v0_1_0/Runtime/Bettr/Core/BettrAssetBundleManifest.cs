using YamlDotNet.Serialization;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [YamlSerializable]
    public class BettrAssetBundleManifest
    {
        // ReSharper disable once UnusedMember.Global
        public long ManifestFileVersion { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public uint CRC { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Hashes Hashes { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public long HashAppended { get; set; }

        // ReSharper disable once UnusedMember.Global
        public ClassType[] ClassTypes { get; set; }

        // ReSharper disable once UnusedMember.Global
        public object[] SerializeReferenceClassIdentifiers { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string[] Assets { get; set; }

        // ReSharper disable once UnusedMember.Global
        public object[] Dependencies { get; set; }
        
        public string AssetBundleName { get; set; }
        
        public string AssetBundleVersion { get; set; }

        // ReSharper disable once EmptyConstructor
        public BettrAssetBundleManifest()
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [YamlSerializable]
    public class ClassType
    {
        public long Class { get; set; }

        public Script Script { get; set; }
        
        // ReSharper disable once EmptyConstructor
        public ClassType()
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [YamlSerializable]
    public class Script
    {
        [YamlMember(ApplyNamingConventions = false, Alias = "instanceID")]
        public long InstanceId { get; set; }
        
        [YamlMember(ApplyNamingConventions = false, Alias = "fileID")]
        public long FileId { get; set; }
        
        [YamlMember(ApplyNamingConventions = false, Alias = "guid")]
        // ReSharper disable once InconsistentNaming
        public string GUID { get; set; }
        
        [YamlMember(ApplyNamingConventions = false, Alias = "type")]
        public long Type { get; set; }

        // ReSharper disable once EmptyConstructor
        public Script()
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [YamlSerializable]
    public class Hashes
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public EHash AssetFileHash { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public EHash TypeTreeHash { get; set; }
        
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public EHash IncrementalBuildHash { get; set; }

        // ReSharper disable once EmptyConstructor
        public Hashes()
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [YamlSerializable]
    public class EHash
    {
        [YamlMember(ApplyNamingConventions = false, Alias = "serializedVersion")]
        public long SerializedVersion { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Hash { get; set; }
        
        // ReSharper disable once EmptyConstructor
        public EHash()
        {
        }
    }
}
