using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

namespace scuttlebutt_announce
{
    /// A class that anounces a presence periodically
    public class PresenceAnnouncer
    {
        private int sleepTime;
        private Thread handle;
        // Conection
        private Socket udpSocket;
        private IPAddress locapIp;
        private IPAddress serverIp;
        private int destinationPort;
        private IPEndPoint connectionPoint;
        private string publicKey;
        private string privateKey;


        public PresenceAnnouncer (
            int destinationPort,
            int sleepTime)
        {
            this.sleepTime = sleepTime;
            this.handle = null;

            // Connection to server
            this.destinationPort = destinationPort;
            this.udpSocket = new Socket (AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Udp);
            this.locapIp  = IPAddress.Parse (GetLocalIPAddress());
            this.serverIp = IPAddress.Parse ("255.255.255.255");
            this.connectionPoint = new IPEndPoint(this.serverIp,
                                                  this.destinationPort);

            // TODO: Scuttlebutt doesn't use RSA
            var rsa = new RSACryptoServiceProvider();
            this.publicKey = rsa.ToXmlString(false);
        }

        /// Starts the announcing loop
        public void Run ()
        {
            if (this.handle == null) {
                this.handle = new Thread(Loop);
            } else {
                throw new InvalidOperationException("Announce loop already started");
            }
        }

        /// Stops the announcing loop
        public void Stop ()
        {
            if (this.handle != null)
            {
                this.handle.Abort();
                this.handle = null;
            } else {
                throw new InvalidOperationException("Announce loop not started");
            }
        }

        void Loop()
        {
            while(true)
            {
                InnerLoop();
            }
        }

        void InnerLoop()
        {
            String destinationMsg = "net:" + locapIp.ToString() + ":" + destinationPort + "~shs:" + publicKey;
            // Preparation to send the message
            byte[] bufferedMsg = Encoding.ASCII.GetBytes(destinationMsg);
            // Send message
            udpSocket.SendTo(bufferedMsg, connectionPoint);

            Thread.Sleep(this.sleepTime);
        }

        ~PresenceAnnouncer()
        {
            if (this.handle != null)
            {
                this.handle.Abort();
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry( Dns.GetHostName() );
            foreach ( var ip in host.AddressList )
            {
                if ( ip.AddressFamily == AddressFamily.InterNetwork )
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
