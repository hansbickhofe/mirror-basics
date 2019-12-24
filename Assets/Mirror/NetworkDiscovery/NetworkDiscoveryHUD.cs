using System.Collections.Generic;
using UnityEngine;

namespace Mirror {

    public class NetworkDiscoveryHUD : MonoBehaviour {
        List<NetworkDiscovery.DiscoveryInfo> m_discoveredServers = new List<NetworkDiscovery.DiscoveryInfo> ();
        string[] m_headerNames = new string[] {
            "IP",
            NetworkDiscovery.kMapNameKey,
            NetworkDiscovery.kNumPlayersKey,
            NetworkDiscovery.kMaxNumPlayersKey
        };
        string[] m_gameName = { "Alpha", "Beta", "Gamma", "Delta" };

        public System.Action<NetworkDiscovery.DiscoveryInfo> connectAction;
        public bool IsRefreshing { get { return Time.realtimeSinceStartup - m_timeWhenRefreshed < this.refreshInterval; } }
        float m_timeWhenRefreshed = 0f;
        [Range (1, 5)] public float refreshInterval = 3f;

        void OnEnable () {
            NetworkDiscovery.onReceivedServerResponse += OnDiscoveredServer;
        }

        void OnDisable () {
            NetworkDiscovery.onReceivedServerResponse -= OnDiscoveredServer;
        }

        void Awake () {
            InvokeRepeating (nameof (Refresh), 0, 3);
        }

        void Update () {
            if (Input.GetKeyDown ("s")) StartServer ();
            if (Input.GetKeyDown ("l")) DisplayServers ();
            if (Input.GetKeyDown ("c")) ConnectToFirstServer ();
            if (Input.GetKeyDown ("d")) DisconnectFromGame ();
        }

        void StartServer () {
            print ("StartServer");
            CancelInvoke ();
            int serverNum = (Random.Range (0, 4));
            if (!NetworkClient.isConnected && !NetworkServer.active) {
                if (!NetworkClient.active) {
                    print ("Start Gameserver-" + m_gameName[serverNum]);
                    // LAN Host
                    m_discoveredServers.Clear ();
                    NetworkManager.singleton.StartHost ();
                }
            }
        }

        void ConnectToFirstServer () {
            print ("ConnectToFirstServer");

        }

        void DisconnectFromGame () {
            print ("DisconnectFromGame");
            // NetworkDiscovery.singleton.StopAllCoroutines ();
            if (!NetworkServer.active) NetworkManager.singleton.StopClient ();
            else NetworkManager.singleton.StopHost ();
            InvokeRepeating (nameof (Refresh), 0, 3);
        }

        public void DisplayServers () {
            foreach (var info in m_discoveredServers) {
                string hostline = "Gameinfo: " + info.EndPoint.Address.ToString ();
                for (int i = 1; i < m_headerNames.Length; i++) {
                    if (info.KeyValuePairs.ContainsKey (m_headerNames[i])) {
                        hostline += " ";
                        hostline += info.KeyValuePairs[m_headerNames[i]];
                    } else print ("empty");
                }
                print (hostline);
            }
        }

        public void Refresh () {
            print ("refresh");
            m_discoveredServers.Clear ();
            m_timeWhenRefreshed = Time.realtimeSinceStartup;
            NetworkDiscovery.SendBroadcast ();
            DisplayServers ();
        }

        void Connect (NetworkDiscovery.DiscoveryInfo info) {
            if (null == NetworkManager.singleton)
                return;
            if (null == Transport.activeTransport)
                return;
            if (!(Transport.activeTransport is TelepathyTransport)) {
                Debug.LogErrorFormat ("Only {0} is supported", typeof (TelepathyTransport));
                return;
            }

            // assign address and port
            NetworkManager.singleton.networkAddress = info.EndPoint.Address.ToString ();
            ((TelepathyTransport) Transport.activeTransport).port = ushort.Parse (info.KeyValuePairs[NetworkDiscovery.kPortKey]);

            NetworkManager.singleton.StartClient ();
        }

        void OnDiscoveredServer (NetworkDiscovery.DiscoveryInfo info) {
            int index = m_discoveredServers.FindIndex (item => item.EndPoint.Equals (info.EndPoint));
            if (index < 0) {
                // server is not in the list
                // add it
                m_discoveredServers.Add (info);
            } else {
                // server is in the list
                // update it
                m_discoveredServers[index] = info;
            }

        }

    }

}