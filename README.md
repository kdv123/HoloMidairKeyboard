# HoloLens Multi-Finger Keyboard
Contains the full interface used in The Impact of Surface Co-location and Eye-tracking on Mixed Reality Typing paper during Experiment 2.

The keyboard is stationary and spawns relative to the headset location when the program is launched. Initially, the user will be prompted to enter a participant number. This determines the order of the four conditions. The four conditions are as follows:
Index - Users can type only with their index fingers.
IndexEye - Users can type only with their index fingers. They must be looking near a key in order to press it.
Ten - Users can type with all ten fingers.
TenEye - Users can type with all ten fingers. They must be looking near a key in order to press it.

Each condition consists of two practice sentences and eight test sentences. The users WPM and Error % are displayed after they complete each sentence. A 30 second break is enforced between each condition.

Log files are created automatically, and stored on the HoloLens in the Pictures folder. A new folder will be created named "Output" that will contain the log files.

### Certificate Renewal
To renew certificate:
  1. Delete certificate from folder hierarchy
  2. Run new build in Unity
  3. New valid certificate is created


### Profile Configuration
Before deploying to the Hololens, ensure your project has the profile configured correctly. To configure:
  1. Select the "MixedRealityToolkit" object from the scene hierarchy.
  2. In the inspector, there will be a dropdown menu directly underneath the "MixedRealityToolkit" script. Select the "MultiFingerProfile" option.
  3. (optional) If you want to configure your own custom profile, note that this projects' profile was based on the "DefaultHololens2ConfigurationProfile", and that eye tracking must be enabled for this project to function correctly on the Hololens 2 (NOTE: Eye tracking is disabled in all default profiles).

This material is based upon work supported by the NSF under Grant No. IIS-1909089.
