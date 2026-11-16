using System;
using Newtonsoft.Json.Linq;

namespace CSWebSockets.Structs
{
    // No input_events: the entire typed string is fixed context for autocomplete.
    public sealed class PredictionRequest
    {
        public int version = 1;
        public string request_id;
        public string session_id;
        public long revision;
        public string operation = "words";
        public string model;
        public string lane = "keyboard-current";
        public bool replaceable = true;
        public string left_context;
        public int nbest;
        public int deadline_ms;
        public string[] word_ending_characters = { " " };
        public InputChannel input_channel = new InputChannel();
    }

    public sealed class InputChannel
    {
        public double complete_prior = 0.0;
    }

    public sealed class PredictionResponse
    {
        public int version;
        public string type;
        public string request_id;
        public string session_id;
        public long? revision;
        public string status;
        public PredictionData data;
        public JToken error;
        public JToken timings;
    }

    public sealed class PredictionData
    {
        public WordResults words;
    }

    public sealed class WordResults
    {
        public WordPrediction[] predictions;
    }

    public sealed class WordPrediction
    {
        public string word;
        public WordReplacement replacement;
        public WordProbability probability;
    }

    public sealed class WordProbability
    {
        // The API represents negative infinity as the JSON string "-inf".
        public JToken log_retained_joint_mass;
    }

    [Serializable]
    public sealed class WordReplacement
    {
        public int start;
        public int end;
        public string text;
    }

    [Serializable]
    public struct PredictionSet
    {
        public long ID;
        public string RequestId;
        public Prediction[] predictions;
    }

    [Serializable]
    public struct Prediction
    {
        public long ID;
        public int Order;
        // Contents is now the whole display word, rather than a suffix.
        public string Contents;
        public string RequestId;
        public string SessionId;
        public string SourceText;
        public WordReplacement Replacement;
    }
}
