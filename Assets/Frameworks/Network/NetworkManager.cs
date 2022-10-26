using System.Collections.Generic;
using System;
using UnityEngine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Framework.Network
{
    public enum SessionType
    {
        TCP, UDP,
    }

    public enum ADDRESSFAM
    {
        IPv4, IPv6
    }

    public static partial class NetworkManager
    {
        private static Dictionary<string, Session> _sessions = new Dictionary<string, Session>();
        internal static void RegisterSession(string name, Session session)
        {
            _sessions.Add(name, session);
        }
        public static Session GetSession(string name)
        {
            Session session;
            _sessions.TryGetValue(name, out session);
            return session;
        }

        static NetworkManager()
        {
            // knight.msg.thrift.CSharpNetMsgRegisterHelper.RegisterAllMsg();
        }

        public static void Tick()
        {
            UpdateReachability();
            foreach (Session session in _sessions.Values)
            {
                session.Update();
            }
        }

        public static void OnApplicationQuit()
        {
            foreach (Session session in _sessions.Values)
            {
                session.OnApplicationQuit();
            }
        }

        public static volatile NetworkReachability Reachability;
        public static volatile bool IsCarrierDataNetwork;

        public static bool isNetworkReachable()
        {
            return NetworkManager.Reachability == UnityEngine.NetworkReachability.NotReachable;
        }

        public static void UpdateReachability()
        {
            /*
            if (Reachability != Application.internetReachability)
            {
                Reachability = Application.internetReachability;
                if (Reachability == NetworkReachability.NotReachable)
                {
                    for (int i = 0; i < _ListSession.Count; ++i)
                    {
                        _ListSession[i].OnNotReachability();
                    }
                }
            }
            */
            IsCarrierDataNetwork = (Reachability == NetworkReachability.ReachableViaCarrierDataNetwork);
        }

        public static string GetIPByDomain(string domain)//根据域名返回ip
        {
            domain = domain.Replace("http://", "").Replace("https://", "");
            IPHostEntry hostEntry = Dns.GetHostEntry(domain);
            if (hostEntry.AddressList.Length > 0)
            {
                IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
                return ipEndPoint.Address.ToString();
            }
            return "";
        }

        public static string GetIP()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return GetIPAddressByUDP();
#else
            return GetLocalIP(ADDRESSFAM.IPv4);
#endif
        }

        private static string GetIPAddressByUDP()
        {
            /*
            string hostName = System.Net.Dns.GetHostName();
            string localIP = System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString();
            return localIP;
            */
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        private static string GetLocalIP(ADDRESSFAM Addfam)//不支持多网卡
        {
            //如果是ipv6但是系统不支持
            if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
            {
                return null;
            }

            string output = "";

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                bool isCandidate = (item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                //MacOS (10.13) and iOS (12.1), OperationalStatus 可能会一直是 "Unknown".
                isCandidate = isCandidate && item.OperationalStatus == OperationalStatus.Up;
#endif

                if (isCandidate)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        //IPv4
                        if (Addfam == ADDRESSFAM.IPv4)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }

                        //IPv6
                        else if (Addfam == ADDRESSFAM.IPv6)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return output;
        }
    }
}
