using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.NetworkMessages;
using Assets.Scripts.Utility.Serialisation;
using Mirror;
using UnityEngine;

namespace Mirror {
    public class main : MonoBehaviour {

        public NetworkManager NetMngr;
        Dictionary<string, DiscoveryInfo> m_discoveredServers = new Dictionary<string, DiscoveryInfo> ();

        // Start is called before the first frame update
        void Start () {
            m_discoveredServers.Clear ();
            NetworkDiscovery.instance.ClientRunActiveDiscovery ();
        }

        // Update is called once per frame
        void Update () {
            if (!NetMngr) return;
            if (!isLocalPlayer) return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            if (Input.GetKeyDown ("s")) StartServer ();
            if (Input.GetKeyDown ("r")) RefreshServerList ();
            if (Input.GetKeyDown ("c")) ConnectToFirstServer ();
            if (Input.GetKeyDown ("d")) DisconnectFromGame ();

#endif

            void StartServer () {
                if (!NetworkClient.isConnected && !NetworkServer.active) {
                    if (!NetworkClient.active) {
                        // LAN Host
                        if (GUILayout.Button ("Passive Host", GUILayout.Height (25), GUILayout.ExpandWidth (false))) {
                            m_discoveredServers.Clear ();
                            NetworkManager.singleton.StartHost ();

                            // Wire in broadcaster pipeline here
                            GameBroadcastPacket gameBroadcastPacket = new GameBroadcastPacket ();

                            gameBroadcastPacket.serverAddress = NetworkManager.singleton.networkAddress;
                            gameBroadcastPacket.port = ((TelepathyTransport) Transport.activeTransport).port;
                            gameBroadcastPacket.hostName = "MyDistinctDummyPlayerName";
                            gameBroadcastPacket.serverGUID = NetworkDiscovery.instance.serverId;

                            byte[] broadcastData = ByteStreamer.StreamToBytes (gameBroadcastPacket);
                            NetworkDiscovery.instance.ServerPassiveBroadcastGame (broadcastData);
                        }
                    }
                }
            }

            void RefreshServerList () {

            }

            void ConnectToFirstServer () {

            }

            void DisconnectFromGame () {

            }
        }
    }
}