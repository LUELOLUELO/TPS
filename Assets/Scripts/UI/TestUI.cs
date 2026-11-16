using UnityEngine;
using DG.Tweening;
using FishNet.Managing;
using UnityEngine.UI;
namespace UI
{
    public class TestUI:MonoBehaviour
    {
        public Button startServer;
        public Button startClient;
        public Button startHost;
        private Network.Network network;
        private void Awake()
        {
            DOTween.Init();
            if (startServer != null)
            {
                startServer.onClick.AddListener(StartServer);
            }

            if (startClient != null)
            {
                startClient.onClick.AddListener(StartClient);
            }

            if (startHost != null)
            {
                startHost.onClick.AddListener(StartHost);
            }
        }

        private void Start()
        {
            network = Network.Network.Get();
        }

        public void StartServer()
        {
            if (network != null)
            {
                network.StartServer();
            }
        }

        public void StartClient()
        {
            if (network != null)
            {
                Debug.Log("UI Start Client");
                network.StartClient();
            }

        }

        public void StartHost()
        {
            if (network != null)
            {
                network.StartHost();
            }
        }
    }
}