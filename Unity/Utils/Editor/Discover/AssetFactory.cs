namespace Scellecs.Morpeh.Utils.Editor.Discover {
	using System;
	using System.IO;
	using UnityEditor;
	using UnityEditor.ProjectWindowCallback;
	using UnityEngine;

	public class AssetFactory {
		public static void CreateAssetInProjectWindow<T>(string iconName, string fileName) where T : ScriptableObject {
			var icon = EditorGUIUtility.FindTexture(iconName);

			var namingInstance = new DoCreateGenericAsset();
			namingInstance.type = typeof(T);
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, namingInstance, fileName, icon, null);
		}

		public static ScriptableObject CreateAssetAtPath(string path, Type type) {
			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			asset.name = Path.GetFileName(path);
			AssetDatabase.CreateAsset(asset, path);
			return asset;
		}

		class DoCreateGenericAsset : EndNameEditAction {
			public Type type;

			public override void Action(int instanceId, string pathName, string resourceFile) {
				ScriptableObject asset = CreateAssetAtPath(pathName, this.type);
				ProjectWindowUtil.ShowCreatedAsset(asset);
			}
		}
	}
}