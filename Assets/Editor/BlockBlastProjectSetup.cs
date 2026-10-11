using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BlockBlast.EditorTools
{
    /// <summary>
    /// Lần đầu mở project: tự tạo scene Assets/Scenes/BlockBlast.unity, thêm vào Build Settings
    /// và chỉnh Player Settings sang màn hình dọc.
    /// </summary>
    [InitializeOnLoad]
    static class BlockBlastProjectSetup
    {
        const string ScenePath = "Assets/Scenes/BlockBlast.unity";

        static BlockBlastProjectSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        static void AutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(ScenePath)) return;
            CreateScene();
        }

        [MenuItem("BlockBlast/Tạo lại scene game")]
        static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.2f);
            camGo.transform.position = new Vector3(0, 0, -10);

            new GameObject("BlockBlastGame").AddComponent<BlockBlastGame>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.productName = "Block Blast";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.resizableWindow = true;

            AssetDatabase.SaveAssets();
            Debug.Log("[BlockBlast] Đã tạo scene " + ScenePath + ". Bấm Play để chơi!");
        }
    }
}
