using UnityEngine;

namespace Network
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "MyTPS/Network Setting")]
    public class NetworkSetting:ScriptableObject
    {
        public string ip = "127.0.0.1";
        public ushort port = 9000;

        public static NetworkSetting Get()
        {
            return Network.Get().setting;
        }
    }
}