using System.Net.WebSockets;
using System.Text;
using CSWebSockets.Structs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;
namespace CSWebSockets{
    public class Main : MonoBehaviour{
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _ = MainLoop();
            }
        }
        readonly string address = "cheetah.sabu.mtu.edu:8765";
        public async Task MainLoop(){
            Debug.Log("Starting");
            WebSocketClient client = new WebSocketClient("ws://" + address);
            try{
                Debug.Log("Awaiting");
                await client.ConnectAsync();
            }catch (WebSocketException){
                Debug.LogError($"Unable to connect to server {address}");
                return;
            }
            Debug.Log("Connected");
            
            // Put into struct
            Message msg;
            msg.ID = 0;
            msg.message = "Hello There!";
            
            // Convert to string with SerializeObject, then to bytes
            await client.SendBinaryAsync(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg)));
            // Wait for response
            await Task.Delay(10000);
            await client.DisconnectAsync();
           Debug.Log("Disconnected");
        }
    }
}