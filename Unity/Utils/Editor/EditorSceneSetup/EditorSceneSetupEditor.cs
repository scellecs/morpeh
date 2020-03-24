namespace Morpeh.Utils.Editor {
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

	[CustomEditor(typeof(EditorSceneSetup))]
	public class EditorSceneSetupEditor : UnityEditor.Editor {
		ReorderableList reorderableList;

		SerializedProperty loadedScenes;
		SerializedProperty activeScene;

		private void OnEnable() {
			activeScene = serializedObject.FindProperty("ActiveScene");
			loadedScenes = serializedObject.FindProperty("LoadedScenes");

			reorderableList = new ReorderableList(serializedObject, loadedScenes, true, true, true, true);
			reorderableList.drawElementCallback = OnDrawElement;
			reorderableList.drawHeaderCallback = OnDrawHeader;
		}

		private void OnDrawHeader(Rect rect) {
			GUI.Label(rect, "Scene List");
		}

		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused) {
			var toggleRect = rect;
			toggleRect.width = 16;
			toggleRect.yMin += 2;

			var sceneRect = rect;
			sceneRect.xMin += 24;
			sceneRect.xMax -= 80;
			sceneRect.yMin += 2;
			sceneRect.height = 16;

			var loadedRect = rect;
			loadedRect.xMin = rect.xMax - 80;
			loadedRect.yMin += 2;

			bool active = activeScene.intValue == index;
			bool newActive = GUI.Toggle(toggleRect, active, GUIContent.none);
			if (GUI.changed && newActive != active) {
				activeScene.intValue = index;
			}

			var sceneAsset = (SceneAsset) EditorGUI.ObjectField(sceneRect,
				loadedScenes.GetArrayElementAtIndex(index).FindPropertyRelative("Scene").objectReferenceValue,
				typeof(SceneAsset), false);
			if (GUI.changed) {
				loadedScenes.GetArrayElementAtIndex(index).FindPropertyRelative("Scene").objectReferenceValue =
					sceneAsset;
			}

			EditorGUI.BeginDisabledGroup(index == 0);
			int visible = loadedScenes.GetArrayElementAtIndex(index).FindPropertyRelative("Loaded").boolValue ? 1 : 0;
			visible = EditorGUI.IntPopup(loadedRect, visible, kLoadedItems, kLoadedIndices);

			if (GUI.changed) {
				loadedScenes.GetArrayElementAtIndex(index).FindPropertyRelative("Loaded").boolValue =
					visible == 1 ? true : false;
			}
			else if (index == 0) {
				loadedScenes.GetArrayElementAtIndex(index).FindPropertyRelative("Loaded").boolValue = true;
			}

			EditorGUI.EndDisabledGroup();
			serializedObject.ApplyModifiedProperties();
		}

		static readonly int[] kLoadedIndices = new int[2] {0, 1};

		static readonly GUIContent[] kLoadedItems = new GUIContent[2]
			{new GUIContent("Not Loaded"), new GUIContent("Loaded")};

		public override void OnInspectorGUI() {
			reorderableList.DoLayoutList();
		}
	}
}