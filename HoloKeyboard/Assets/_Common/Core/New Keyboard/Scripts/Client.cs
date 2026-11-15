using CSWebSockets;
using CSWebSockets.Structs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class Client : MonoBehaviour
{
    public static Client Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public Action<List<PredictionSet>> OnPredictionsUpdated;
    public Action OnOutboundServerRequest;
    private WebSocketClient _socket = null;

    //readonly string _address = "cheetah.sabu.mtu.edu:8765";
    //readonly string _address = "6.tcp.ngrok.io:10127";
    readonly string _address = "141.219.186.170:8765";
    //141.219.186.255
    private void Start()
    {
        Task<bool> connectTask = TryConnect();
    }
    [ContextMenu("Send data")]
    public void Test()
    {
        _ = CreateMessage(TestInput);
    }
    public string TestInput;

    [SerializeField] private List<PredictionSet> _predictionList = new List<PredictionSet>();




    public async Task<bool> TryConnect()
    {
        Debug.Log("Attempting connection...");
        _socket = new WebSocketClient("ws://" + _address);
        try
        {
            await _socket.ConnectAsync();
        }
        catch (WebSocketException)
        {
            Debug.LogError($"Unable to connect to server {_address}");
            return false;
        }
        _socket.OnRecieveResponse += RecieveMessageFromServer;
        return true;
    }
    public async Task Disconnect()
    {
        if (_socket == null) Debug.LogWarning("Cannot disconnect as the socket is null");
        await _socket.DisconnectAsync();
        Debug.Log("Disconnected");
    }
    public async Task<bool> CreateMessage(string msg)
    {
        //Log a warning and return if the socket isn't open for sending
        if (_socket == null || _socket.ClientState != WebSocketState.Open)
        {
            Debug.LogWarning("Cannot send message; socket is null");
        }
        // Put into struct
        Message data;
        data.ID = _socket.GetNextID();
        data.message = msg;
        Debug.Log($"Trying to send message |{data.message}| with ID {data.ID}");
        // Convert to string with SerializeObject, then to bytes
        await _socket.SendBinaryAsync(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data)));
        OnOutboundServerRequest?.Invoke();
        LogManager.Instance.CreateServerRequestLog(msg);
        return true;

    }
    private void RecieveMessageFromServer(ServerPrediction p)
    {
        //Debug.Log(JsonConvert.SerializeObject(p));
        
        Debug.Log($"Recieved message - ID {p.ID}, predictions:");
        //First need to split each prediction into its own struct
        PredictionSet set = new PredictionSet
        {
            ID = p.ID,
            predictions = new Prediction[p.predictions.Length]
        };
        foreach (string s in p.predictions)
        {
            string[] contents = s.Split('_');
            if (contents.Length < 2)
            {
                Debug.LogError("Couldn't split string");
                return;
            }
            Prediction part = new Prediction
            {
                ID = p.ID,
                Order = int.Parse(contents[1]),
                Contents = contents[0]
            };
            //And add to group struct
            set.predictions[part.Order - 1] = part;
        }
        //After adding each prediction, add the prediction set to the ongoing list
        _predictionList.Add(set);
        //Compare the ID of the most recent added to the previous - if smaller ID, list is out of order, and needs to be re-sorted.
        if (_predictionList.Count > 2 && _predictionList[_predictionList.Count - 1].ID < _predictionList[_predictionList.Count - 2].ID)
        {
            _predictionList = _predictionList.OrderBy(x => x.ID).ToList();
        }
        //Broadcast that the list was updated
        OnPredictionsUpdated?.Invoke(_predictionList);
        LogManager.Instance.CreateServerResponseLog(set);
    }
}
