using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

namespace Network
{
    public class Network:MonoBehaviour
    {
        private static Network instance;
        
        private NetworkManager manager;
        
        public NetworkSetting setting;

        public NetworkManager Manager
        {
            get { return manager; }
        }

        public bool IsServer
        {
            get { return manager.ServerManager != null; }
        }

        public bool IsClient
        {
            get { return manager.ClientManager != null; }
        }

        public bool IsHost
        {
            get { return IsServer && IsClient; }
        }
        
        private void Awake()
        {
            if (manager == null)
            {
                manager = GetComponent<NetworkManager>();
            }

            if (instance != null)
            {
                Destroy(gameObject);
            }

            instance = this;
            manager.ServerManager.OnRemoteConnectionState += OnRemoteConnection;
            manager.SceneManager.OnClientLoadedStartScenes += OnClientLoadStartScenes;

        }

        private void OnDestroy()
        {
            manager.ServerManager.OnRemoteConnectionState -= OnRemoteConnection;
            manager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadStartScenes;
        }

        private void OnClientLoadStartScenes(NetworkConnection connection, bool v)
        {
            Debug.Log("收到clietn加入场景请求");
        }

        public void OnRemoteConnection(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            Debug.Log("收到clietn连接请求");
        }

        private void Start()
        {
            
        }

        public static Network Get()
        {
            return instance;
        }

        public void StartServer()
        {
            manager.ServerManager.StartConnection(setting.port);
            Debug.Log("Server started");
        }

        public void StartClient()
        {
            manager.ClientManager.StartConnection(setting.ip,setting.port);
            Debug.Log("Client started");
        }

        public void StartHost()
        {
            manager.ServerManager.StartConnection(setting.port);
            manager.ClientManager.StartConnection(setting.ip,setting.port);
            Debug.Log("Host started");
        }
        
        
        
        
    }
}