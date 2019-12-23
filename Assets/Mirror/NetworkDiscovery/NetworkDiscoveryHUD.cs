using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        Vector2 m_scrollViewPos = Vector2.zero;
        public bool IsRefreshing { get { return Time.realtimeSinceStartup - m_timeWhenRefreshed < this.refreshInterval; } }
        float m_timeWhenRefreshed = 0f;
        bool m_displayBroadcastAddresses = false;

        IPEndPoint m_lookupServer = null; // server that we are currently looking up
        string m_lookupServerIP = "";
        string m_lookupServerPort = NetworkDiscovery.kDefaultServerPort.ToString ();
        float m_timeWhenLookedUpServer = 0f;
        bool IsLookingUpAnyServer {
            get {
                return Time.realtimeSinceStartup - m_timeWhenLookedUpServer < this.refreshInterval &&
                    m_lookupServer != null;
            }
        }

        GUIStyle m_centeredLabelStyle;

        public bool drawGUI = true;
        public int offsetX = 5;
        public int offsetY = 150;
        public int width = 500, height = 400;
        [Range (1, 5)] public float refreshInterval = 3f;

        public System.Action<NetworkDiscovery.DiscoveryInfo> connectAction;

        NetworkDiscoveryHUD () {
            this.connectAction = this.Connect;
        }

        void OnEnable () {
            NetworkDiscovery.onReceivedServerResponse += OnDiscoveredServer;
        }

        void OnDisable () {
            NetworkDiscovery.onReceivedServerResponse -= OnDiscoveredServer;
        }

        void Start () {

        }

        // void OnGUI () {

        //     if (null == m_centeredLabelStyle) {
        //         m_centeredLabelStyle = new GUIStyle (GUI.skin.label);
        //         m_centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
        //     }

        //     if (this.drawGUI)
        //         this.Display (new Rect (offsetX, offsetY, width, height));

        // }

        void Update () {
            if (Input.GetKeyDown ("s")) StartServer ();
            if (Input.GetKeyDown ("r")) {
                Refresh ();
                DisplayServers ();
            }
            if (Input.GetKeyDown ("c")) ConnectToFirstServer ();
            if (Input.GetKeyDown ("d")) DisconnectFromGame ();
        }

        void StartServer () {
            print ("StartServer");
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
        }

        // public void Display (Rect displayRect) {
        //     if (null == NetworkManager.singleton)
        //         return;
        //     if (NetworkServer.active || NetworkClient.active)
        //         return;
        //     if (!NetworkDiscovery.SupportedOnThisPlatform)
        //         return;

        //     // GUILayout.BeginArea (displayRect);

        //     // this.DisplayRefreshButton ();

        //     // lookup a server

        //     // GUILayout.Label ("Lookup server: ");
        //     // GUILayout.BeginHorizontal ();
        //     // GUILayout.Label ("IP:");
        //     m_lookupServerIP = GUILayout.TextField (m_lookupServerIP, GUILayout.Width (120));
        //     // GUILayout.Space (10);
        //     // GUILayout.Label ("Port:");
        //     m_lookupServerPort = GUILayout.TextField (m_lookupServerPort, GUILayout.Width (60));
        //     // GUILayout.Space (10);
        //     if (IsLookingUpAnyServer) {
        //         GUILayout.Button ("Lookup...", GUILayout.Height (25), GUILayout.MinWidth (80));
        //     } else {
        //         if (GUILayout.Button ("Lookup", GUILayout.Height (25), GUILayout.MinWidth (80)))
        //             LookupServer ();
        //     }
        //     GUILayout.FlexibleSpace ();
        //     GUILayout.EndHorizontal ();

        //     GUILayout.BeginHorizontal ();
        //     m_displayBroadcastAddresses = GUILayout.Toggle (m_displayBroadcastAddresses, "Display broadcast addresses", GUILayout.ExpandWidth (false));
        //     if (m_displayBroadcastAddresses) {
        //         GUILayout.Space (10);
        //         GUILayout.Label (string.Join (", ", NetworkDiscovery.GetBroadcastAdresses ().Select (ip => ip.ToString ())));
        //     }
        //     GUILayout.EndHorizontal ();

        //     GUILayout.Label (string.Format ("Servers [{0}]:", m_discoveredServers.Count));

        //     this.DisplayServers ();

        //     GUILayout.EndArea ();

        // }

        // public void DisplayRefreshButton () {
        //     if (IsRefreshing) {
        //         GUILayout.Button ("Refreshing...", GUILayout.Height (25), GUILayout.ExpandWidth (false));
        //     } else {
        //         if (GUILayout.Button ("Refresh LAN", GUILayout.Height (25), GUILayout.ExpandWidth (false))) {
        //             Refresh ();
        //         }
        //     }
        // }

        public void DisplayServers () {

            // int elemWidth = this.width / m_headerNames.Length - 5;

            // header
            // GUILayout.BeginHorizontal ();
            // foreach (string str in m_headerNames) {
            //     // print (str);
            // }

            foreach (var info in m_discoveredServers) {
                // if (GUILayout.Button (info.EndPoint.Address.ToString (), GUILayout.Width (elemWidth)))
                //     this.connectAction (info);

                for (int i = 1; i < m_headerNames.Length; i++) {
                    if (info.KeyValuePairs.ContainsKey (m_headerNames[i])) {
                        print (info.KeyValuePairs[m_headerNames[i]]);
                        // GUILayout.Label (info.KeyValuePairs[m_headerNames[i]], m_centeredLabelStyle, GUILayout.Width (elemWidth));
                    } else print ("empty");
                }
            }
        }

        public void Refresh () {
            m_discoveredServers.Clear ();
            m_timeWhenRefreshed = Time.realtimeSinceStartup;
            NetworkDiscovery.SendBroadcast ();

        }

        bool IsLookingUpServer (IPEndPoint endPoint) {
            return Time.realtimeSinceStartup - m_timeWhenLookedUpServer < this.refreshInterval &&
                m_lookupServer != null &&
                m_lookupServer.Equals (endPoint);
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
            print ("OnDiscoveredServer " + info.EndPoint.ToString ());
            if (!IsRefreshing && !IsLookingUpServer (info.EndPoint))
                return;

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