namespace CSWebSockets.Structs{
    public struct ServerPrediction{
        public int ID;
        public string[] predictions;
    }

    public struct Message{
        public int ID;
        public string message;
    }
    public struct PredictionSet
    {
        public int ID;
        public Prediction[] predictions;
    }
    public struct Prediction
    {
        public int ID;
        public int Order;
        public string Contents;
    }
}