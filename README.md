# HoloLens Multi-Finger Keyboard
For prototyping this with the HoloLens v2

This Project contains the full HoloLens experiment.

To renew certificate:
  1. Delete certificate from folder hierarchy
  2. Run new build in Unity
  3. New valid certificate is created


### Profile Configuration
Before deploying to the Hololens, ensure your project has the profile configured correctly. To configure:
  1. Select the "MixedRealityToolkit" object from the scene hierarchy.
  2. In the inspector, there will be a dropdown menu directly underneath the "MixedRealityToolkit" script. Select the "MultiFingerProfile" option.
  3. (optional) If you want to configure your own custom profile, note that this projects' profile was based on the "DefaultHololens2ConfigurationProfile", and that eye tracking must be enabled for this project to function correctly on the Hololens 2 (NOTE: Eye tracking is disabled in all default profiles).
