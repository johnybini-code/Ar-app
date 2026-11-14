using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
namespace BiniarosAR {
[DefaultExecutionOrder(-10000)] public class RuntimeARBootstrap:MonoBehaviour {
 public static string Status="Starting AR..."; public ARSession session; public ARRaycastManager raycastManager; public ARPlaneManager planeManager; public ARCameraManager cameraManager; public ARCameraBackground cameraBackground; ARCoreLoader loader;
 IEnumerator Start(){ SetAR(false);
#if UNITY_ANDROID && !UNITY_EDITOR
 Status="Requesting camera permission..."; if(!Permission.HasUserAuthorizedPermission(Permission.Camera)){ Permission.RequestUserPermission(Permission.Camera); float t=Time.realtimeSinceStartup; while(!Permission.HasUserAuthorizedPermission(Permission.Camera)&&Time.realtimeSinceStartup-t<20f) yield return null; }
 if(!Permission.HasUserAuthorizedPermission(Permission.Camera)){ Status="Camera permission denied"; yield break; }
#endif
 Status="Initializing ARCore..."; loader=ScriptableObject.CreateInstance<ARCoreLoader>(); bool ok=false; try{ok=loader.Initialize();}catch(System.Exception e){Status="ARCore init error: "+e.GetType().Name; Debug.LogException(e);} if(!ok){Status="ARCore failed to initialize";yield break;} try{ok=loader.Start();}catch(System.Exception e){Status="ARCore start error: "+e.GetType().Name; Debug.LogException(e);} if(!ok){Status="ARCore failed to start";yield break;} SetAR(true); Status="ARCore started - scan the floor"; }
 void SetAR(bool e){ if(session)session.enabled=e; if(raycastManager)raycastManager.enabled=e; if(planeManager)planeManager.enabled=e; if(cameraManager)cameraManager.enabled=e; if(cameraBackground)cameraBackground.enabled=e; }
 void OnDestroy(){ if(loader==null)return; try{loader.Stop();}catch{} try{loader.Deinitialize();}catch{} Destroy(loader); loader=null; }
}}
