BINIAROS ROMA AR - APK BUILD

This project is prepared for Unity 6.0.62f1 + Android/ARCore.
Target: Android ARM64, ARCore, real-scale ROMA model.

WHAT THE FIRST APK DOES
- live camera AR
- horizontal floor detection
- tap to place ROMA at true 1:1 scale
- two-finger twist to rotate
- 90-degree button
- reset/reposition button
- environment depth/occlusion request when the device supports it

MODEL SIZE USED
Approx. 5.006 m width x 4.002 m projection x 2.723 m height.

ONE-TIME BUILD REQUIREMENT
Install Unity Hub -> Unity 6.0.62f1 -> modules:
Android Build Support + Android SDK & NDK Tools + OpenJDK.
Then double click BUILD_APK_WINDOWS.bat.
The result will be Build\BINIAROS_ROMA_AR.apk.

This first APK is for real-world placement testing on the Samsung A56.
Parametric width/projection, columns, louvers, ZIP, RAL, LEDs and tablet-to-glasses sync are the next layer after placement stability is validated.
