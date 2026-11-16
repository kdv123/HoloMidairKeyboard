using System.Threading.Tasks;
using UnityEngine;

namespace CSWebSockets
{
    // Optional scene test helper: exercise the same connection and protocol as typing.
    public class Main : MonoBehaviour
    {
        [SerializeField] private string _testInput = "this is a te";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) _ = MainLoop();
        }

        public async Task MainLoop()
        {
            if (Client.Instance == null) return;
            if (await Client.Instance.TryConnect())
                await Client.Instance.CreateMessage(_testInput);
        }
    }
}
