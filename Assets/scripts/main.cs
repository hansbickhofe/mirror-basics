using System.Collections.Generic;
using Assets.Scripts.NetworkMessages;
using Assets.Scripts.Utility.Serialisation;
using Mirror;
using UnityEngine;

public class main : MonoBehaviour {

    public NetworkManager NetMngr;
    Dictionary<string, DiscoveryInfo> m_discoveredServers = new Dictionary<string, DiscoveryInfo> ();
    string[] m_headerNames = new string[] { "IP", "Host" };

    string[] m_gameName = { "Alpha", "Beta", "Gamma", "Delta" };

    void OnEnable () {
        NetworkDiscovery.onReceivedServerResponse += OnDiscoveredServer;
    }

    void OnDisable () {
        NetworkDiscovery.onReceivedServerResponse -= OnDiscoveredServer;
    }

    // Start is called before the first frame update
    void Start () {
        m_discoveredServers.Clear ();
        NetworkDiscovery.instance.ClientRunActiveDiscovery ();
    }

    // Update is called once per frame
    void Update () {
        if (!NetMngr) return;
        // if (!isLocalPlayer) return;
        if (Input.GetKeyDown ("s")) StartServer ();
        if (Input.GetKeyDown ("r")) RefreshServerList ();
        if (Input.GetKeyDown ("c")) ConnectToFirstServer ();
        if (Input.GetKeyDown ("d")) DisconnectFromGame ();
    }

    void StartServer () {
        int serverNum = (Random.Range (0, 4));
        if (!NetworkClient.isConnected && !NetworkServer.active) {
            if (!NetworkClient.active) {
                print ("Start Gameserver-" + m_gameName[serverNum]);
                // LAN Host
                m_discoveredServers.Clear ();
                NetworkManager.singleton.StartHost ();

                // Wire in broadcaster pipeline here
                GameBroadcastPacket gameBroadcastPacket = new GameBroadcastPacket ();

                gameBroadcastPacket.serverAddress = NetworkManager.singleton.networkAddress;
                gameBroadcastPacket.port = ((TelepathyTransport) Transport.activeTransport).port;
                gameBroadcastPacket.hostName = "Gameserver-" + m_gameName[serverNum];
                gameBroadcastPacket.serverGUID = NetworkDiscovery.instance.serverId;

                byte[] broadcastData = ByteStreamer.StreamToBytes (gameBroadcastPacket);
                NetworkDiscovery.instance.ServerPassiveBroadcastGame (broadcastData);
            }
        }
    }

    void RefreshServerList () {
        NetworkDiscovery.instance.ClientRunActiveDiscovery ();
        print ("RefreshServerList");

        foreach (var info in m_discoveredServers.Values) {
            print ("Info " + info.unpackedData.serverGUID);
            print ("Info " + info.unpackedData.hostName);
            print ("Info " + info.unpackedData.serverAddress);
            
            for (int i = 0; i < m_headerNames.Length; i++) {
                if (i == 0) {
                    print ("server Name: " + info.unpackedData.hostName + " " + " GUID: " + info.unpackedData.serverGUID +
                        " server Address: " + info.unpackedData.serverAddress);
                }
            }
        }

        m_discoveredServers.Clear ();
    }

    void ConnectToFirstServer () {
        print ("ConnectToFirstServer ");

        if (NetworkManager.singleton == null ||
            Transport.activeTransport == null) {
            return;
        }
        if (!(Transport.activeTransport is TelepathyTransport)) {
            Debug.LogErrorFormat ("Only {0} is supported", typeof (TelepathyTransport));
            return;
        }

    }

    void DisconnectFromGame () {
        print ("DisconnectFromGame");
    }

    void OnDiscoveredServer (DiscoveryInfo info) {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        m_discoveredServers[info.unpackedData.serverGUID] = info;
    }
}