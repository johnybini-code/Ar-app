# BINIAROS ROMA AR – starter

## What is already prepared
- Source: 21-10-2026_ASL-150-00170-01_ROMA.stp
- Mobile AR model: Assets/BiniarosAR/Models/ROMA_AR_MOBILE.glb
- Real CAD extents detected: approx. 5.006 m x 4.002 m x 2.723 m
- Assembly names/hierarchy retained during CAD -> glTF conversion.
- Small fasteners under ~50 mm were removed from the mobile version to reduce real-time load.
- Mobile model: ~8.2 MB, ~548k triangles.

## Chosen architecture
- Unity 6 + AR Foundation
- Android: Google ARCore
- Horizontal/vertical plane detection
- Real-world placement at 1 Unity unit = 1 meter
- Depth/occlusion can be enabled with AROcclusionManager on supported devices
- Tablet controller and AR-glasses viewer are designed as separate front ends sharing the same pergola state.

## First Unity scene
1. Create/open an AR project in Unity 6.
2. Install/enable AR Foundation + ARCore XR Plugin.
3. Create AR Session and XR Origin (Mobile AR).
4. On XR Origin add ARRaycastManager and ARPlaneManager.
5. On AR Camera add ARCameraManager, ARCameraBackground and AROcclusionManager.
6. Import the GLB with a glTF importer (Unity glTFast or equivalent) and make it a prefab named ROMA_AR.
7. Add an empty GameObject `BINIAROS_AR_MANAGER` and attach `ARPlacementController`.
8. Assign Raycast Manager, Plane Manager, AR Camera and the ROMA root.
9. Verify orientation. The source CAD is Z-up; if the imported model lies on its side, rotate only the visual child ±90° around X, never scale the AR origin.
10. Keep ROMA scale at 1,1,1 for true-size placement.

## Important design rule
Do NOT resize the whole pergola with arbitrary XYZ scaling for customer dimensions. That distorts profile sections. The next step is to map the ROMA assembly into semantic groups (columns, width beams, projection beams, louvers, LED/ZIP) and drive dimensions parametrically by repositioning/repeating parts.

## Controls already scaffolded
- tap to place/move on a detected floor
- two-finger twist to rotate
- reset and rotate 90° methods for UI buttons
- column visibility count
- louver angle 0–135°
- RAL material color
- LED on/off
- ZIP visibility hooks
- Day/Night virtual-light switch
- optional local-WiFi state broadcast from tablet to glasses

## AR glasses
The glasses build should use the platform provider supported by the chosen hardware (OpenXR/vendor plugin). The model/configuration layer is intentionally independent from the AR provider so the same ROMA configurator can be reused.
