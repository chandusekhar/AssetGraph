using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundleGraph {
	public static class TypeUtility {
		public static readonly List<string> KeyTypes = new List<string>{
			// empty
			AssetBundleGraphSettings.DEFAULT_FILTER_KEYTYPE,
			
			// importers
			typeof(TextureImporter).ToString(),
			typeof(ModelImporter).ToString(),
			typeof(AudioImporter).ToString(),
			
			// others(Assets)
			typeof(Animation).ToString(),
			typeof(Animator).ToString(),
			typeof(AvatarMask).ToString(),
			typeof(Cubemap).ToString(),
			typeof(Flare).ToString(),
			typeof(Font).ToString(),
			typeof(GUISkin).ToString(),
			// typeof(LightmapParameters).ToString(),
			typeof(Material).ToString(),
			typeof(PhysicMaterial).ToString(),
			typeof(PhysicsMaterial2D).ToString(),
			typeof(RenderTexture).ToString(),
			// typeof(SceneAsset).ToString(),
			typeof(Shader).ToString(),
			typeof(Scene).ToString(),
		};
		
		public static readonly Dictionary<string, Type> AssumeTypeBindingByExtension = new Dictionary<string, Type>{
			// others(Assets)
			{".anim", typeof(Animation)},
			{".controller", typeof(Animator)},
			{".mask", typeof(AvatarMask)},
			{".cubemap", typeof(Cubemap)},
			{".flare", typeof(Flare)},
			{".fontsettings", typeof(Font)},
			{".guiskin", typeof(GUISkin)},
			// typeof(LightmapParameters).ToString(),
			{".mat", typeof(Material)},
			{".physicMaterial", typeof(PhysicMaterial)},
			{".physicsMaterial2D", typeof(PhysicsMaterial2D)},
			{".renderTexture", typeof(RenderTexture)},
			// typeof(SceneAsset).ToString(),
			{".shader", typeof(Shader)},
			{".unity", typeof(Scene)},
			{".prefab", typeof(UnityEngine.Object)}

			// {"", typeof(Sprite)},
		};

		public static readonly List<string> IgnoredExtension = new List<string>{
			string.Empty,
			".manifest",
			".assetbundle",
			".sample",
			".cs",
			".sh",
			".json",
			".js",
		};

		/**
		 * Get type of asset from give path.
		 */
		public static Type GetTypeOfAsset (string assetPath) {
			Profiler.BeginSample("AssetBundleGraph.GetTypeOfAsset");

			if (assetPath.EndsWith(AssetBundleGraphSettings.UNITY_METAFILE_EXTENSION)) {
				return typeof(string);
			}

			Type t = null;
			#if (UNITY_5_4_OR_NEWER && !UNITY_5_4_0 && !UNITY_5_4_1)

			t = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

			#else

			UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

			if (asset != null) {
				t = asset.GetType();
				Resources.UnloadAsset(asset);
			}
			#endif

			Profiler.EndSample();
			return t;
		}

		/**
		 * Get type of asset from give path.
		 */
		public static Type FindTypeOfAsset (string assetPath) {
			// check by asset importer type.
			var importer = AssetImporter.GetAtPath(assetPath);
			if (importer == null) {
				LogUtility.Logger.LogWarning(LogUtility.kTag, "Failed to assume assetType of asset. The asset will be ignored: " + assetPath);
				return typeof(object);
			}

			var assumedImporterType = importer.GetType();
			var importerTypeStr = assumedImporterType.ToString();
			
			switch (importerTypeStr) {
				case "UnityEditor.TextureImporter":
				case "UnityEditor.ModelImporter":
				case "UnityEditor.AudioImporter": {
					return assumedImporterType;
				}
			}
			
			// not specific type importer. should determine their type by extension.
			var extension = Path.GetExtension(assetPath);
			if (AssumeTypeBindingByExtension.ContainsKey(extension)) {
				return AssumeTypeBindingByExtension[extension];
			}

			if (IgnoredExtension.Contains(extension)) {
				return null;
			}
			
			// unhandled.
			LogUtility.Logger.LogWarning(LogUtility.kTag, "Unknown file type found:" + extension + "\n. AssetReference:" + assetPath + "\n Assume 'object'.");
			return typeof(object);
		}			

		public static Type FindFirstIncomingAssetType(List<AssetReference> assets) {

			if(assets.Any()) {
				return assets.First().filterType;
			}

			return null;
		}

		public static MonoScript LoadMonoScript(string className) {
			var t = Type.GetType(className);
			if(t == null) {
				return null;
			}

			string[] guids = AssetDatabase.FindAssets ("t:MonoScript " + className);

			MonoScript s = null;

			if(guids.Length > 0 ) {
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				s = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			}

			return s;
		}
	}

	public class AssetBundleReference {}
	public class AssetBundleManifestReference {}
}
