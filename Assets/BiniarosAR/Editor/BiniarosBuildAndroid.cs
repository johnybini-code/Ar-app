using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

namespace BiniarosAR.Editor
{
    public sealed class BiniarosPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return -1000; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            BiniarosBuildAndroid.SetupProject();
        }
    }

    public static class BiniarosBuildAndroid
    {
        const string ScenePath = "Assets/Scenes/BINIAROS_AR.unity";
        const string ModelPath = "Assets/BiniarosAR/Models/ROMA_AR_MOBILE.glb";

        [MenuItem("BINIAROS/Setup AR Project")]
        public static void SetupProject()
        {
            PlayerSettings.applicationIdentifier = "com.biniaros.ar";
            PlayerSettings.productName = "BINIAROS AR";
            PlayerSettings.companyName = "BINIAROS";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

            ConfigureXR();
            CreateScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("BINIAROS AR setup complete.");
        }

        static void ConfigureXR()
        {
            XRGeneralSettingsPerBuildTarget perTarget = XRGeneralSettingsPerBuildTarget.GetOrCreate();
            if (!perTarget.HasSettingsForBuildTarget(BuildTargetGroup.Android))
                perTarget.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            if (!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
                perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);

            var manager = perTarget.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            if (manager == null)
                throw new Exception("Could not create XR Manager Settings for Android.");

            manager.automaticLoading = true;
            manager.automaticRunning = true;
            XRPackageMetadataStore.AssignLoader(manager, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android);
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(perTarget);
        }

        static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sessionGO = new GameObject("AR Session");
            sessionGO.AddComponent<ARSession>();
            sessionGO.AddComponent<ARInputManager>();

            var originGO = new GameObject("XR Origin");
            var origin = originGO.AddComponent<XROrigin>();
            var raycast = originGO.AddComponent<ARRaycastManager>();
            var planes = originGO.AddComponent<ARPlaneManager>();
            planes.requestedDetectionMode = PlaneDetectionMode.Horizontal;

            var cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(originGO.transform, false);
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            cameraGO.transform.SetParent(cameraOffset.transform, false);
            var cam = cameraGO.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<ARCameraManager>();
            cameraGO.AddComponent<ARCameraBackground>();
            cameraGO.AddComponent<TrackedPoseDriver>();

            origin.CameraFloorOffsetObject = cameraOffset;
            origin.Camera = cam;

            GameObject pergola = TryLoadModel();
            if (pergola == null)
            {
                Debug.LogWarning("GLB importer unavailable in this first build. Using accurate-size procedural ROMA fallback for AR placement test.");
                pergola = CreateProceduralRoma();
            }
            pergola.name = "ROMA_AR_1_TO_1";
            pergola.transform.position = Vector3.zero;
            pergola.transform.rotation = Quaternion.identity;
            pergola.transform.localScale = Vector3.one;
            pergola.SetActive(false);

            var managerGO = new GameObject("BINIAROS_AR_MANAGER");
            var placement = managerGO.AddComponent<BiniarosAR.ARPlacementController>();
            placement.raycastManager = raycast;
            placement.planeManager = planes;
            placement.arCamera = cam;
            placement.pergolaRoot = pergola.transform;

            var ui = managerGO.AddComponent<BiniarosAR.BiniarosRuntimeUI>();
            ui.placement = placement;
            ui.pergola = pergola.transform;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static GameObject TryLoadModel()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (asset == null) return null;
            return PrefabUtility.InstantiatePrefab(asset) as GameObject;
        }

        static GameObject CreateProceduralRoma()
        {
            const float width = 5.006f;
            const float depth = 4.002f;
            const float height = 2.723f;
            const float post = 0.14f;
            const float beamH = 0.20f;

            var root = new GameObject("ROMA_Procedural_Fallback");
            var mat = new Material(Shader.Find("Standard"));
            mat.name = "RAL7016";
            mat.color = new Color(0.18f, 0.19f, 0.20f, 1f);

            AddBox(root.transform, "Column_FL", new Vector3(-width/2f + post/2f, height/2f, -depth/2f + post/2f), new Vector3(post, height, post), mat);
            AddBox(root.transform, "Column_FR", new Vector3(width/2f - post/2f, height/2f, -depth/2f + post/2f), new Vector3(post, height, post), mat);
            AddBox(root.transform, "Column_RL", new Vector3(-width/2f + post/2f, height/2f, depth/2f - post/2f), new Vector3(post, height, post), mat);
            AddBox(root.transform, "Column_RR", new Vector3(width/2f - post/2f, height/2f, depth/2f - post/2f), new Vector3(post, height, post), mat);

            float beamY = height - beamH/2f;
            AddBox(root.transform, "Beam_Front", new Vector3(0, beamY, -depth/2f + post/2f), new Vector3(width, beamH, post), mat);
            AddBox(root.transform, "Beam_Back", new Vector3(0, beamY, depth/2f - post/2f), new Vector3(width, beamH, post), mat);
            AddBox(root.transform, "Beam_Left", new Vector3(-width/2f + post/2f, beamY, 0), new Vector3(post, beamH, depth), mat);
            AddBox(root.transform, "Beam_Right", new Vector3(width/2f - post/2f, beamY, 0), new Vector3(post, beamH, depth), mat);

            int louverCount = 24;
            float usable = depth - 0.34f;
            float step = usable / louverCount;
            float louverY = height - beamH - 0.045f;
            for (int i = 0; i < louverCount; i++)
            {
                float z = -usable/2f + step * (i + 0.5f);
                AddBox(root.transform, "Louver_" + (i + 1).ToString("00"), new Vector3(0, louverY, z), new Vector3(width - 0.34f, 0.045f, step * 0.80f), mat);
            }
            return root;
        }

        static void AddBox(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            var c = go.GetComponent<Collider>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);
        }
    }
}
