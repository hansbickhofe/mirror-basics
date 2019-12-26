using System.Collections.Generic;
using Assets.Scripts.NetworkMessages;
using Assets.Scripts.Utility.Serialisation;
using UnityEngine;

namespace Mirror {

    public class NetworkDiscoveryHUD : MonoBehaviour {

        Dictionary<string, DiscoveryInfo> m_discoveredServers = new Dictionary<string, DiscoveryInfo> ();
        string[] m_headerNames = new string[] { "IP", "Gamename" };
        string[] m_gameName = { "Alpha", "Beta", "Gamma", "Delta" };

        Vector2 m_scrollViewPos = Vector2.zero;

        GUIStyle m_centeredLabelStyle;

        public int offsetX = 5;
        public int offsetY = 150;
        public int width = 500, height = 400;

        void OnEnable () {
            NetworkDiscovery.onReceivedServerResponse += OnDiscoveredServer;
        }

        void OnDisable () {
            NetworkDiscovery.onReceivedServerResponse -= OnDiscoveredServer;
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
        }

        void Update () {
            if (Input.GetKeyDown ("s")) StartServer ();
            if (Input.GetKeyDown ("l")) DisplayServers ();
            if (Input.GetKeyDown ("1")) ConnectServer (1);
            if (Input.GetKeyDown ("2")) ConnectServer (2);
            if (Input.GetKeyDown ("3")) ConnectServer (3);
            if (Input.GetKeyDown ("d")) DisconnectFromGame ();
        }

        void StartServer () {
            int serverNum = (Random.Range (0, 4));
            m_discoveredServers.Clear ();
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
            if (!NetworkServer.active) NetworkManager.singleton.StopClient ();
            else NetworkManager.singleton.StopHost ();
            m_discoveredServers.Clear ();
        }

        public void DisplayServers () {
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
        }

        public void Refresh () {
            m_discoveredServers.Clear ();
            NetworkDiscovery.instance.ClientRunActiveDiscovery ();
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