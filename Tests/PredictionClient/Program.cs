using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSWebSockets;
using CSWebSockets.Structs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal static class Program
{
    private static int _checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        _checks++;
    }

    private static void Reject(Action action, string message)
    {
        try { action(); }
        catch (InvalidDataException) { _checks++; return; }
        throw new Exception(message);
    }

    private static async Task RejectAsync(Func<Task> action, string message)
    {
        try { await action().WaitAsync(TimeSpan.FromSeconds(6)); }
        catch (TimeoutException error) when (error.Message.StartsWith("Timed out waiting")) { _checks++; return; }
        catch (Exception error) when (!(error is TimeoutException)) { _checks++; return; }
        throw new Exception(message);
    }

    private static PredictionResponse Reply(PredictionRequest request, string status = "ok")
    {
        return JsonConvert.DeserializeObject<PredictionResponse>(new JObject
        {
            ["version"] = 1, ["request_id"] = request.request_id,
            ["session_id"] = request.session_id, ["revision"] = request.revision, ["status"] = status,
            ["data"] = new JObject { ["words"] = new JObject { ["predictions"] = new JArray(
                new JObject { ["word"] = "text", ["replacement"] = new JObject { ["start"] = 0, ["end"] = 2, ["text"] = "text" },
                    ["probability"] = new JObject { ["log_retained_joint_mass"] = -2.5 } },
                new JObject { ["word"] = "test", ["replacement"] = new JObject { ["start"] = 0, ["end"] = 2, ["text"] = "test" },
                    ["probability"] = new JObject { ["log_retained_joint_mass"] = -1.25 } }) } }
        }.ToString());
    }

    private static void StateTests()
    {
        var session = new NoiselessPredictionSession();
        Check(session.Revision == 0 && session.Text == "", "Initial state");
        session.SetText("This is a te");
        var request = session.CreateRequest("ngram", 3, 10000);
        var wire = JObject.Parse(JsonConvert.SerializeObject(request));
        Check((string)wire["left_context"] == "This is a te", "Preserve text and case");
        Check(wire["input_events"] == null && (double)wire["input_channel"]["complete_prior"] == 0.0,
            "Request must be noiseless autocomplete");
        Check((string)wire["word_ending_characters"][0] == " " && (bool)wire["replaceable"], "Space ending and replacement policy");
        Check(!session.SetText(session.Text) && session.Revision == 1, "Repeated text must not increment revision");
        var response = Reply(request);
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
        try
        {
            Check(session.TryReadPredictions(request, response, out var set), "Read structured predictions");
            Check(set.predictions.Length == 2 && set.predictions[0].Contents == "test", "Fewer than three results and numeric ranking");
            var selected = set.predictions[0];
            Check(session.IsCurrent(selected), "Current suggestion is selectable");
            Check(NoiselessPredictionSession.TryApplyReplacement(session.Text, selected.Replacement, out string text) &&
                text + " " == "This is a test ", "Replace the word without duplicating its prefix");
            session.SetText("This is a tex");
            Check(!session.IsCurrent(selected), "Typing invalidates displayed suggestions");
            Check(!session.TryReadPredictions(request, response, out _), "Late responses must be discarded");
            session.SetText("This is a te");
            Check(!session.IsCurrent(selected), "Editing back to the same text must not revive an old suggestion");
        }
        finally { CultureInfo.CurrentCulture = culture; }
        request = session.CreateRequest("ngram", 3, 10000);
        var newer = session.CreateRequest("ngram", 3, 10000);
        Check(newer.revision == request.revision && newer.request_id != request.request_id, "Fresh request ID for the same revision");
        Check(!session.TryReadPredictions(request, Reply(request), out _), "A newer call supersedes an equal revision");
        Check(!session.TryReadPredictions(newer, Reply(newer, "replaced"), out _), "Replaced is not a prediction update");
        foreach (string status in new[] { "timeout", "overloaded", "unavailable", "model_error", "invalid_request", "cancelled", "conflict" })
            Reject(() => session.TryReadPredictions(newer, Reply(newer, status), out _), "Must surface status " + status);
        response = Reply(newer);
        response.data.words.predictions = Array.Empty<WordPrediction>();
        Check(session.TryReadPredictions(newer, response, out var empty) && empty.predictions.Length == 0, "Empty result is valid");
        response = Reply(newer);
        response.data.words.predictions[1].probability.log_retained_joint_mass = "-inf";
        Check(session.TryReadPredictions(newer, response, out var ranked) && ranked.predictions[0].Contents == "text", "Decode -inf ranking");
        response = Reply(newer); response.session_id = "another-session";
        Reject(() => session.TryReadPredictions(newer, response, out _), "Reject mismatched session");
        response = Reply(newer); response.revision++;
        Reject(() => session.TryReadPredictions(newer, response, out _), "Reject mismatched revision");
        response = Reply(newer); response.data.words.predictions[0].replacement.end = 999;
        Reject(() => session.TryReadPredictions(newer, response, out _), "Reject invalid replacement");
        session.Invalidate();
        Check(!session.IsCurrent(newer), "Disconnect invalidates the pending request");
        var reconnected = session.CreateRequest("ngram", 3, 10000);
        Check(reconnected.session_id == newer.session_id && reconnected.revision == newer.revision,
            "Reconnect preserves session and revision");
        session.SetText("");
        Check(!session.IsCurrent(reconnected), "Clear invalidates pending requests");
        session.SetText("hello ");
        Check(session.CreateRequest("ngram", 3, 10000).left_context == "hello ", "Preserve trailing word boundary");
        Check(NoiselessPredictionSession.TryApplyReplacement("a te", new WordReplacement { start = 2, end = 2, text = "st" }, out string suffix) &&
            suffix == "a test", "Support suffix replacement offsets");
        Check(NoiselessPredictionSession.TryApplyReplacement("hello 😀a", new WordReplacement { start = 1, end = 2, text = "b" }, out string unicode) &&
            unicode == "hello 😀b", "Offsets count Unicode code points, not UTF-16 units");
        Check(!NoiselessPredictionSession.TryApplyReplacement("te", new WordReplacement { start = -1, end = 1, text = "x" }, out _), "Reject negative offsets");
        Check(!NoiselessPredictionSession.TryApplyReplacement("te", new WordReplacement { start = 0, end = 3, text = "x" }, out _), "Reject offsets past word end");
        Console.WriteLine("PASS: text state, request schema, ranking, replacements, and stale-response checks");
    }

    private static async Task FixtureTests(string baseUrl)
    {
        using (var client = new WebSocketClient(baseUrl + "/fragment"))
        {
            await client.ConnectAsync();
            Check(client.IsReady, "Wait for ready handshake");
            var session = new NoiselessPredictionSession();
            session.SetText("hé");
            var request = session.CreateRequest("default", 3, 1000);
            var response = await client.PredictAsync(request);
            Check(session.TryReadPredictions(request, response, out var set) && set.predictions[0].Contents == "héllo😀",
                "Reassemble a fragmented UTF-8 response larger than 4096 bytes");
            await client.DisconnectAsync();
            Check(!client.IsReady, "Disconnect clears readiness");
        }
        using (var client = new WebSocketClient(baseUrl + "/reverse"))
        {
            await client.ConnectAsync();
            var session = new NoiselessPredictionSession(); session.SetText("te");
            var first = session.CreateRequest("default", 3, 1000);
            var firstTask = client.PredictAsync(first);
            session.SetText("tes");
            var second = session.CreateRequest("default", 3, 1000);
            var secondTask = client.PredictAsync(second);
            var secondReply = await secondTask;
            var firstReply = await firstTask;
            Check(secondReply.request_id == second.request_id && firstReply.request_id == first.request_id,
                "Correlate out-of-order responses");
            Check(!session.TryReadPredictions(first, firstReply, out _), "Ignore the older result arriving last");
            Check(session.TryReadPredictions(second, secondReply, out _), "Keep the latest result");
        }
        foreach (string mode in new[] { "close", "binary", "malformed", "silent" })
        {
            using (var client = new WebSocketClient(baseUrl + "/" + mode))
            {
                await client.ConnectAsync();
                var request = new NoiselessPredictionSession().CreateRequest("default", 3, 10);
                await RejectAsync(async () => { await client.PredictAsync(request); }, "Request should fail: " + mode);
            }
        }
        using (var client = new WebSocketClient(baseUrl + "/close-before-ready"))
            await RejectAsync(client.ConnectAsync, "Early close must fail the handshake");
        // A fresh transport after failure must still work.
        using (var client = new WebSocketClient(baseUrl + "/fragment"))
        {
            await client.ConnectAsync();
            Check(client.IsReady, "Reconnect with a new transport");
        }
        Console.WriteLine("PASS: WebSocket fragmentation, UTF-8, ordering, close, malformed frames, timeout, reconnect");
    }

    private static async Task ServerTests(string url, string model)
    {
        using (var client = new WebSocketClient(url))
        {
            await client.ConnectAsync();
            var session = new NoiselessPredictionSession();
            foreach (string text in new[] { "this is a te", "hello ", "" })
            {
                session.SetText(text);
                var request = session.CreateRequest(model, 3, 10000);
                var response = await client.PredictAsync(request);
                Check(session.TryReadPredictions(request, response, out var set), "Live server response must be valid");
                Check(set.predictions.Length > 0 && set.predictions.Length <= 3, "Live server returned suggestions");
                foreach (var prediction in set.predictions)
                    Check(NoiselessPredictionSession.TryApplyReplacement(text, prediction.Replacement, out _), "Live replacement is applicable");
                Console.WriteLine("PASS: " + model + " context '" + text + "' => " + string.Join(", ", set.predictions.Select(p => p.Contents)));
            }
            await client.DisconnectAsync();
        }
    }

    private static async Task<int> Main(string[] args)
    {
        try
        {
            StateTests();
            string model = "default";
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--model") model = args[i + 1];
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--fixture") await FixtureTests(args[++i]);
                else if (args[i] == "--server") await ServerTests(args[++i], model);
            }
            Console.WriteLine("PASS: " + _checks + " checks");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
