# HoloLens Multi-Finger Keyboard

### New HoloKeyboard project and TextSlinger server

The current keyboard is a separate Unity project in `HoloKeyboard/`. Open that directory
with Unity **2022.3.62f3** and load `Assets/_Scenes/Keyboard.unity`. The root-level Unity
project is the previous implementation described below.

The new keyboard uses the TextSlinger `latency3` version-1 WebSocket API. Select the scene
object with the `Client` component to configure:

| Setting | Default |
| --- | --- |
| Server URL | `wss://cheetah.textslinger.org:8443/api/v1/ws` |
| Model | `ngram` |
| Prediction Count | `3` |
| Deadline Ms | `10000` |

The model is a logical name advertised by the server's `/health` endpoint. For example,
the Cheetah deployment also offers `subword`. For a local mock server, use
`ws://127.0.0.1:8765/api/v1/ws` and model `default`. A headset must use the server's
reachable hostname/address; `127.0.0.1` refers to the headset itself. The mock server
also needs a network-facing listener to accept headset connections.

Predictions remain **noiseless autocomplete**: the exact typed text, including its partial
word, capitalization, and trailing spaces, is sent as `left_context`, with
`complete_prior: 0.0` and no `input_events`. The Gaussian weights are not sent. Suggestions
display full words; selection applies the server's word replacement and adds one space.
Returned offsets count Unicode code points. The UI accepts fewer than three suggestions
and discards suggestions invalidated by edits, clear, newer requests, or reconnection.

Word prediction is enabled after participant ID submission, as in the existing procedure.
The `Client` component's **Send data** context menu can exercise a request using **Test Input**.
The connection waits for `ready`, reconnects with bounded backoff, and resends the latest
requested text. TLS uses normal certificate validation. If authentication is enabled,
set `Client.BearerToken` at runtime before connecting; credentials are not serialized into
the scene. Request/accepted-response logs include request identity and response timings.
`Scripts/link.xml` preserves the protocol types used by JSON reflection in player builds
([Unity linker documentation](https://docs.unity3d.com/2022.3/Documentation/Manual/ManagedCodeStripping.html#LinkXMLAnnotation)).

### Prediction client checks

The standalone C# harness compiles the actual transport and text-state classes without
Unity. With a .NET 8 SDK, run from the repository root:

```sh
dotnet run --project Tests/PredictionClient/PredictionClient.csproj
```

For fragmentation, Unicode, out-of-order, disconnect, malformed-message, and timeout checks,
start the fixture in a separate terminal using the TextSlinger server's Python environment
(which provides `websockets`), then run:

```sh
python Tests/PredictionClient/websocket_fixture.py
# In another terminal:
dotnet run --project Tests/PredictionClient/PredictionClient.csproj -- --fixture ws://127.0.0.1:18766
```

To check an actual running server with three generic test contexts:

```sh
dotnet run --project Tests/PredictionClient/PredictionClient.csproj -- --server wss://cheetah.textslinger.org:8443/api/v1/ws --model ngram
```

Also validate in Unity Play Mode and on HoloLens: type quickly, accept a suggestion, backspace,
clear/advance a trial while a response is pending, and disconnect/reconnect the network.
Check that stale buttons disappear and loading stops on errors. The standalone harness
does not replace a Unity/UWP build or device test.

### Previous keyboard implementation

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
