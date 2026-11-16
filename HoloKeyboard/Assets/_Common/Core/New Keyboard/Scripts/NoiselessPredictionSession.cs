using System;
using System.IO;
using System.Linq;
using CSWebSockets.Structs;
using Newtonsoft.Json.Linq;

namespace CSWebSockets
{
    // Text state belongs to the keyboard, not to a particular socket. No Unity dependencies.
    public sealed class NoiselessPredictionSession
    {
        public string SessionId { get; } = Guid.NewGuid().ToString("N");
        public long Revision { get; private set; }
        public string Text { get; private set; } = "";
        public string LatestRequestId { get; private set; }

        public bool SetText(string text)
        {
            text = text ?? "";
            if (text == Text) return false;
            Text = text;
            Revision++;
            Invalidate();
            return true;
        }

        public void Invalidate()
        {
            LatestRequestId = null;
        }

        public PredictionRequest CreateRequest(string model, int count, int deadlineMs)
        {
            if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("Choose a server model.");
            if (count < 1 || deadlineMs < 1) throw new ArgumentOutOfRangeException();
            LatestRequestId = Guid.NewGuid().ToString("N");
            return new PredictionRequest
            {
                request_id = LatestRequestId, session_id = SessionId, revision = Revision,
                model = model, left_context = Text, nbest = count, deadline_ms = deadlineMs
            };
        }

        public bool IsCurrent(PredictionRequest request)
        {
            return request != null && LatestRequestId != null &&
                request.session_id == SessionId && request.request_id == LatestRequestId &&
                request.revision == Revision && request.left_context == Text;
        }

        public bool IsCurrent(Prediction prediction)
        {
            return LatestRequestId != null && prediction.SessionId == SessionId &&
                prediction.RequestId == LatestRequestId && prediction.ID == Revision &&
                prediction.SourceText == Text;
        }

        public bool TryReadPredictions(PredictionRequest request, PredictionResponse response,
            out PredictionSet set)
        {
            set = default(PredictionSet);
            if (!IsCurrent(request)) return false;
            if (response == null || response.version != 1 ||
                response.request_id != request.request_id || response.session_id != SessionId)
                throw new InvalidDataException("Prediction response identity did not match the request.");
            if (response.status == "replaced") return false;
            if (response.status != "ok")
                throw new InvalidDataException($"Prediction failed ({response.status}): {response.error}");
            if (response.revision != Revision)
                throw new InvalidDataException("Prediction response revision did not match the request.");
            var words = response.data?.words?.predictions;
            if (words == null) throw new InvalidDataException("The response has no word prediction list.");

            var predictions = words.Where(word => word != null && !string.IsNullOrEmpty(word.word))
                .OrderByDescending(Score).Take(request.nbest).Select((word, index) =>
                {
                    if (!TryApplyReplacement(Text, word.replacement, out _))
                        throw new InvalidDataException("The server returned an invalid word replacement.");
                    return new Prediction
                    {
                        ID = Revision, Order = index + 1, Contents = word.word,
                        RequestId = request.request_id, SessionId = SessionId,
                        SourceText = Text, Replacement = word.replacement
                    };
                }).ToArray();
            set = new PredictionSet { ID = Revision, RequestId = request.request_id, predictions = predictions };
            return true;
        }

        private static double Score(WordPrediction word)
        {
            var value = word.probability?.log_retained_joint_mass;
            if (value == null || (value.Type == JTokenType.String && (string)value == "-inf"))
                return double.NegativeInfinity;
            if (value.Type != JTokenType.Float && value.Type != JTokenType.Integer)
                throw new InvalidDataException("Invalid prediction probability.");
            double score = value.Value<double>();
            if (double.IsNaN(score) || double.IsInfinity(score))
                throw new InvalidDataException("Invalid prediction probability.");
            return score;
        }

        public static bool TryApplyReplacement(string text, WordReplacement replacement, out string result)
        {
            result = text;
            if (text == null || replacement == null || replacement.text == null ||
                replacement.start < 0 || replacement.end < replacement.start) return false;
            // TextSlinger's offsets count code points within the word after the last literal space.
            int wordStart = text.LastIndexOf(' ') + 1;
            int start = CodePointOffset(text, wordStart, replacement.start);
            int end = CodePointOffset(text, wordStart, replacement.end);
            if (start < 0 || end < 0) return false;
            result = text.Substring(0, start) + replacement.text + text.Substring(end);
            return true;
        }

        private static int CodePointOffset(string text, int start, int count)
        {
            int index = start;
            for (int i = 0; i < count; i++)
            {
                if (index >= text.Length) return -1;
                index += char.IsSurrogatePair(text, index) ? 2 : 1;
            }
            return index;
        }
    }
}
