using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System;
using Object = UnityEngine.Object;
using Microsoft.Win32.SafeHandles;


public class ServerConnector : MonoBehaviour
{
    public static ServerConnector instance;

    public TcpClient client;
    public Thread thread;

    [SerializeField]
    private string IP = "141.219.195.255";
    [SerializeField]
    private int PORT = 9999;

    public UnityEvent serverReply;

    private NamedPipeServerStream outPipe;
    private NamedPipeClientStream inPipe;
    private StreamWriter pipeWriter;
    private StreamReader pipeReader;
    private Reply currentReply = null;
    private Object resultLock = new Object();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (serverReply == null) serverReply = new UnityEvent();
        outPipe = new NamedPipeServerStream("send", PipeDirection.Out);
        inPipe = new NamedPipeClientStream("send");
        inPipe.Connect();
        pipeWriter = new StreamWriter(outPipe);
        pipeReader = new StreamReader(inPipe);
        thread = new Thread(new ThreadStart(Connect));

    }

    // Prediction count is currently not utilized on the server side, but should be soon
    public void message(string message, int predictionCount = 3)
    {
        Message messageHolder = new Message();
        messageHolder.words = message;
        messageHolder.predictionCount = predictionCount;
        pipeWriter.Write(JsonConvert.SerializeObject(messageHolder));
    }

    public Reply getReply()
    {
        Reply reply = new Reply();
        lock (resultLock)
        {
            reply.mode = currentReply.mode;
            reply.predictions = currentReply.predictions;
        }
        return reply;
    }

    private void Connect()
    {
        try
        {
            client = new TcpClient(IP, PORT);
            Debug.Log("Connected to server");
            SendMessage();
        }
        catch (System.Exception e)
        {
            Debug.Log("Error connecting to server: " + e.Message);
        }
    }

    private void SendMessage()
    {
        while (true)
        {
            if (client == null) return;
            NetworkStream stream = client.GetStream();
            string msg = pipeReader.ReadLine();
            byte[] message = Encoding.UTF8.GetBytes(msg);
            stream.Write(message, 0, message.Length);
            Debug.Log("Sent message");
            ReadMessage();
        }
    }

    private void ReadMessage()
    {
        if (client == null)
        {
            Debug.Log("Client is null");
            return;
        }
        byte[] bytes = new byte[1024];
        NetworkStream stream = client.GetStream();
        stream.Read(bytes, 0, bytes.Length);
        string message = Encoding.UTF8.GetString(bytes);
        lock (resultLock)
        {
            currentReply = JsonConvert.DeserializeObject<Reply>(message);
            Debug.Log("Received Message: " + currentReply.predictions);
        }
        serverReply.Invoke();
    }

    private class Message
    {
        public string words;
        public int predictionCount;
    }

    public class Reply
    {
        public string mode;
        public string[] predictions;
    }
}
