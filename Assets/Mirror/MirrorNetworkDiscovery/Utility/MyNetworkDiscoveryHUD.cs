using System.Collections.Generic;
using Assets.Scripts.NetworkMessages;
using Assets.Scripts.Utility.Serialisation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror {

    public class MyNetworkDiscoveryHUD : MonoBehaviour {

        Dictionary<string, DiscoveryInfo> m_discoveredServers = new Dictionary<string, DiscoveryInfo> ();
        string[] m_headerNames = new string[] { "IP", "Gamename" };
        string[] m_gameName = { "Alpha", "Beta", "Gamma", "Delta" };

        public NetworkManager manager;

        public bool isServer = false;
        public bool isClient = false;

        public int offsetX = 5;
        public int offsetY = 150;
        public int width = 500, height = 400;

        void OnEnable () {
            NetworkDiscovery.onReceivedServerResponse += OnDiscoveredServer;
        }

        void OnDisable () {
            NetworkDiscovery.onReceivedServerResponse -= OnDiscoveredServer;
        }

        void Start () {
            if (NetworkDiscovery.instance) NetworkDiscovery.instance.ClientRunActiveDiscovery ();
            print ("start discovery");
        }
        void Awake () {
            if (NetworkManager.singleton == null) {
                return;
            }
            if (NetworkServer.active || NetworkClient.active) {
                return;
            }
            if (!NetworkDiscovery.SupportedOnThisPlatform) {
                return;
            }
            if (NetworkDiscovery.instance) {
                m_discoveredServers.Clear ();
                return;
            }
            print ("awake discovery");

            if (NetworkDiscovery.instance) NetworkDiscovery.instance.ClientRunActiveDiscovery ();
        }

        void Update () {
            if (NetworkClient.active) print ("client active");
            if (Input.GetKeyDown ("x")) {
                DisconnectFromGame ();
            }

            if (NetworkServer.active) {
                print ("Server active");
                return;
            }

            if (Input.GetKeyDown ("s") && !NetworkClient.active) {
                StartServer ();
            }

            if (Input.GetKeyDown ("l")) DisplayServers ();

            if (Input.GetKeyDown ("1") && !NetworkClient.active) {
                ConnectServer (1);
            }
        }

        void StartServer () {
            int serverNum = (Random.Range (0, 4));
            if (NetworkDiscovery.instance) m_discoveredServers.Clear ();
            NetworkManager.singleton.StartHost ();

            // Wire in broadcaster pipeline here
            GameBroadcastPacket gameBroadcastPacket = new GameBroadcastPacket ();

            gameBroadcastPacket.serverAddress = NetworkManager.singleton.networkAddress;
            gameBroadcastPacket.port = ((TelepathyTransport) Transport.activeTransport).port;
            gameBroadcastPacket.hostName = m_gameName[serverNum];
            gameBroadcastPacket.serverGUID = NetworkDiscovery.instance.serverId;

            byte[] broadcastData = ByteStreamer.StreamToBytes (gameBroadcastPacket);
            NetworkDiscovery.instance.ServerPassiveBroadcastGame (broadcastData);
        }

        void ConnectServer (int serverNum) {
            print ("Connect To Server Num: " + serverNum);
            int serverCounter = 1;
            foreach (var info in m_discoveredServers.Values) {
                if (serverNum == serverCounter) Connect (info);
                else serverCounter++;
            }
        }

        void DisconnectFromGame () {
            print ("DisconnectFromGame");
            if (NetworkDiscovery.instance) m_discoveredServers.Clear ();
            if (!NetworkClient.isConnected && !NetworkServer.active) return;
            else {
                if (NetworkClient.active) {
                    NetworkManager.singleton.StopClient ();
                    NetworkManager.singleton.StopServer ();
                    // Destroy (GameObject.Find ("*NetMgr"));
                    // SceneManager.LoadScene ("OfflineScene");
                    NetworkManager.singleton.ServerChangeScene ("OfflineScene");

                } else if (NetworkServer.active) {
                    NetworkManager.singleton.StopServer ();
                    // Destroy (GameObject.Find ("*NetMgr"));
                    //SceneManager.LoadScene ("OfflineScene");
                    NetworkManager.singleton.ServerChangeScene ("OfflineScene");

                }

            }
        }

        public void DisplayServers () {            
            NetworkDiscovery.instance.ClientRunActiveDiscovery ();
            print ("start discovery");
            foreach (var info in m_discoveredServers.Values) {
                string hostline = "Gameinfo: " + info.EndPoint.Address.ToString ();
                for (int i = 1; i < m_headerNames.Length; i++) {
                    if (i == 0) {
                        hostline += " ";
                        hostline += info.unpackedData.serverAddress;
                    } else {
                        hostline += " ";
                        hostline += info.unpackedData.hostName;
                    }
                    print (hostline);
                }
            }
            m_discoveredServers.Clear ();
        }

        void Connect (DiscoveryInfo info) {
            if (NetworkManager.singleton == null ||
                Transport.activeTransport == null) {
                return;
            }
            if (!(Transport.activeTransport is TelepathyTransport)) {
                Debug.LogErrorFormat ("Only {0} is supported", typeof (TelepathyTransport));
                return;
            }

            // assign address and port
            NetworkManager.singleton.networkAddress = info.EndPoint.Address.ToString ();
            ((TelepathyTransport) Transport.activeTransport).port = (ushort) info.unpackedData.port;

            NetworkManager.singleton.StartClient ();
        }

        void OnDiscoveredServer (DiscoveryInfo info) {
            // Note that you can check the versioning to decide if you can connect to the server or not using this method
            m_discoveredServers[info.unpackedData.serverGUID] = info;
        }

    }

}