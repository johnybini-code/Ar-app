using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

namespace BiniarosAR.Editor
{
    public static class BiniarosBuildAndroid
    {
        const string ScenePath = "Assets/Scenes/BINIAROS_AR.unity";
        const string ModelPath = "Assets/BiniarosAR/Models/ROMA_AR_MOBILE.obj";
        const string ApkPath = "Build/BINIAROS_ROMA_AR.apk";

        [MenuItem("BINIAROS/1 - Setup AR Project")]
        public static void SetupProject()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.applicationIdentifier = "gr.biniaros.romaar";
            PlayerSettings.productName = "BINIAROS ROMA AR";
            PlayerSettings.companyName = "BINIAROS";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.ARCoreEnabled = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

            ConfigureXR();
            CreateScene();
            AssetDatabase.SaveAssets();
            Debug.Log("BINIAROS AR setup complete.");
        }

        static void ConfigureXR()
        {
            var perTarget = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(BuildTarget.Android);
            if (perTarget == null)
                throw new Exception("XR General Settings are not available yet. Let Unity finish importing packages, then run Setup again.");
            var settings = perTarget.AssignedSettings;
            if (settings == null)
                throw new Exception("XR Manager Settings are missing.");
            settings.automaticLoading = true;
            settings.automaticRunning = true;
            XRPackageMetadataStore.AssignLoader(settings, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android);
            EditorUtility.SetDirty(settings);
        }

        static void CreateScene()
        {
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
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = 0.05f;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<ARCameraManager>();
            cameraGO.AddComponent<ARCameraBackground>();
            var occ = cameraGO.AddComponent<AROcclusionManager>();
            occ.requestedEnvironmentDepthMode = EnvironmentDepthMode.Medium;
            cameraGO.AddComponent<TrackedPoseDriver>();

            origin.CameraFloorOffsetObject = cameraOffset;
            origin.Camera = cam;

            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (modelAsset == null)
                throw new Exception("ROMA model not imported yet at " + ModelPath);
            var pergola = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            pergola.name = "ROMA_AR_1_TO_1";
            pergola.transform.position = Vector3.zero;
            pergola.transform.rotation = Quaternion.identity;
            pergola.transform.localScale = Vector3.one;
            pergola.SetActive(false);

            // Give the CAD a neutral anthracite material for the first AR test.
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "ROMA_RAL7016_RUNTIME" };
            mat.color = new Color(0.18f, 0.19f, 0.20f, 1f);
            foreach (var r in pergola.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = mat;

            var managerGO = new GameObject("BINIAROS_AR_MANAGER");
            var placement = managerGO.AddComponent<ARPlacementController>();
            placement.raycastManager = raycast;
            placement.planeManager = planes;
            placement.arCamera = cam;
            placement.pergolaRoot = pergola.transform;

            var ui = managerGO.AddComponent<BiniarosRuntimeUI>();
            ui.placement = placement;
            ui.pergola = pergola.transform;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        [MenuItem("BINIAROS/2 - Build Android APK")]
        public static void BuildAPK()
        {
            SetupProject();
            Directory.CreateDirectory("Build");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("APK build failed: " + report.summary.result);
            Debug.Log("APK ready: " + Path.GetFullPath(ApkPath));
        }
    }
}
