using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SceneSelector.Editor.SceneUtils;
using SceneSelector.Editor.Toolbar;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSelector.Editor {
    public struct SceneData {
        [NotNull] public readonly string SceneName;
        [NotNull] public readonly string ScenePath;
        [NotNull] public readonly string SelectDisplayPath;
        [NotNull] public readonly string FancyDisplayPath;
        public readonly int FolderDepth;
        public readonly bool IsMain;

        public SceneData(string path) {
            ScenePath = path;
            SceneName = System.IO.Path.GetFileNameWithoutExtension(ScenePath);
            SelectDisplayPath = ScenePaths.GetSceneDisplayPath(path);
            FolderDepth = GetFolderDepth(SelectDisplayPath);
            FancyDisplayPath = SelectDisplayPath.Replace("/", " \\ ");
            IsMain = SceneName.ToLower().StartsWith("main");
        }

        private static int GetFolderDepth(string filePath) {
            var folderDepth = filePath?.Split('/', '\\').Length;
            return (folderDepth ?? 1) - 1;
        }

        public override int GetHashCode() {
            return ScenePath.GetHashCode();
        }
    }

    [InitializeOnLoad]
    public class SceneSwitchLeftButton {
        static SceneSwitchLeftButton() {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;


            if (SceneManager.GetActiveScene().path != string.Empty) {
                CurrentScene = new SceneData(SceneManager.GetActiveScene().path);
            }

            SceneIcon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene) {
            CurrentScene = new SceneData(newScene.path);
        }

        public static SceneData? CurrentScene;


        private static List<SceneData> _scenesData = new();

        private static SceneData? _loadSceneAfterPlay;
        private static readonly Texture SceneIcon;


        private static void OnSceneSelected(SceneData scene) {
            if (Application.isPlaying) {
                EditorApplication.ExitPlaymode();
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                _loadSceneAfterPlay = scene;
            }
            else {
                if (SceneManager.GetActiveScene().isDirty) {
                    var canProceed = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    if (!canProceed) return;
                }

                EditorSceneManager.OpenScene(scene.ScenePath);
                _loadSceneAfterPlay = null;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                if (_loadSceneAfterPlay != null) {
                    EditorSceneManager.OpenScene(_loadSceneAfterPlay.Value.ScenePath);
                    _loadSceneAfterPlay = null;
                }
            }
            else {
                _loadSceneAfterPlay = null;
            }
        }

        private static void RefreshSceneList() {
            var sceneGUIDs = AssetDatabase.FindAssets("t:Scene");
            _scenesData.Clear();

            foreach (var sceneGuid in sceneGUIDs) {
                _scenesData.Add(new SceneData(AssetDatabase.GUIDToAssetPath(sceneGuid)));
            }

            _scenesData = _scenesData.OrderBy(x => x.FolderDepth)
                .ThenBy(x=> !x.IsMain)
                .ThenBy(x => x.SelectDisplayPath).ToList();
        }

        static void OnToolbarGUI() {
            if (CurrentScene == null) return;
            

            GUILayout.FlexibleSpace();

            var toolbarPopup = new GUIStyle(EditorStyles.toolbarPopup) {
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft,
                // margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 16, 2, 2),
                // border = new RectOffset(0,0,0,0),
                // overflow = new RectOffset(0,0,0,0),
                stretchWidth = false,
            };

            if (EditorGUILayout.DropdownButton(new GUIContent(CurrentScene.Value.FancyDisplayPath),
                    FocusType.Keyboard, toolbarPopup, new GUILayoutOption[] {
                        GUILayout.MinWidth(100)
                    })) {
                RefreshSceneList();

                var sceneMenu = new GenericMenu();

                foreach (var sceneData in _scenesData) {
                    var data = sceneData;
                    var isCurrentScene = data.GetHashCode() == CurrentScene.GetHashCode();
                    sceneMenu.AddItem(new GUIContent(sceneData.SelectDisplayPath), isCurrentScene,
                        () => OnSceneSelected(data));
                }

                sceneMenu.ShowAsContext();
            }
        }
    }
}