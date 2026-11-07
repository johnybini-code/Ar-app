using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace BiniarosAR
{
    /// Optional local-WiFi bridge: tablet broadcasts configuration; glasses receive it.
    public class LocalPergolaSync : MonoBehaviour
    {
        [Serializable]
        public class State
        {
            public float louverAngle;
            public int columnCount = 4;
            public bool led;
            public string ral = "7016";
        }

        public enum Mode { TabletSender, GlassesReceiver }
        public Mode mode;
        public int port = 47777;
        public PergolaConfigurator configurator;

        UdpClient udp;
        Thread thread;
        volatile bool running;
        volatile string pendingJson;

        void Start()
        {
            if (mode == Mode.GlassesReceiver)
            {
                udp = new UdpClient(port);
                running = true;
                thread = new Thread(ReceiveLoop) { IsBackground = true };
                thread.Start();
            }
        }

        public void Broadcast(State state)
        {
            if (mode != Mode.TabletSender) return;
            string json = JsonUtility.ToJson(state);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (UdpClient sender = new UdpClient())
            {
                sender.EnableBroadcast = true;
                sender.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port));
            }
        }

        void ReceiveLoop()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            while (running)
            {
                try
                {
                    byte[] bytes = udp.Receive(ref ep);
                    pendingJson = Encoding.UTF8.GetString(bytes);
                }
                catch { }
            }
        }

        void Update()
        {
            if (string.IsNullOrEmpty(pendingJson) || configurator == null) return;
            string json = pendingJson;
            pendingJson = null;
            State s = JsonUtility.FromJson<State>(json);
            configurator.SetLouverAngle(s.louverAngle);
            configurator.SetColumnCount(s.columnCount);
            configurator.SetLED(s.led);
        }

        void OnDestroy()
        {
            running = false;
            udp?.Close();
            thread?.Join(100);
        }
    }
}
