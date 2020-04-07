// Copyright © 2020 Pedro Gómez Martín <zentauro@riseup.net>
//
// This file is part of the library scuttlebutt_announce which
// is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this library. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Text;

namespace Scuttlebutt.Announce
{
    /// A class that anounces a presence periodically
    public class PresenceAnnouncer
    {
        private int sleepTime;
        private Thread handle;
        // Conection
        private Socket udpSocket;
        private IPAddress localIp;
        private IPAddress destIp;
        private int destinationPort;
        private IPEndPoint connectionPoint;
        private string publicKey;
        private CancellationTokenSource cancelTokenSrc;

        public PresenceAnnouncer (
            int port,
            IPAddress localIp,
            IPAddress destIp,
            int sleepTime)
        {
            this.sleepTime = sleepTime;
            this.handle = null;

            // Connection to server
            this.destinationPort = port;
            this.udpSocket = new Socket (AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);
            this.localIp = localIp;
            this.destIp  = destIp;

            this.connectionPoint = new IPEndPoint(this.destIp,
                                                  this.destinationPort);

            // TODO: Scuttlebutt doesn't use RSA
            var rsa = new RSACryptoServiceProvider();
            this.publicKey = rsa.ToXmlString(false);
        }

        /// Starts the announcing loop
        public void Run ()
        {
            if (this.handle == null) {
                this.cancelTokenSrc = new CancellationTokenSource();
                this.udpSocket.Connect(connectionPoint);
                this.handle = new Thread(() => Loop(cancelTokenSrc.Token));
                this.handle.Start();
            } else {
                throw new InvalidOperationException("Announce loop already started");
            }
        }

        /// Stops the announcing loop
        public void Stop ()
        {
            if (this.handle != null)
            {
                this.cancelTokenSrc.Cancel();
                this.udpSocket.Disconnect(true);
                this.handle = null;
                this.cancelTokenSrc = null;
            } else {
                throw new InvalidOperationException("Announce loop not started");
            }
        }

        void Loop(CancellationToken cancelToken)
        {
            while(true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }
                LoopBody();
            }
        }

        void LoopBody()
        {
            String destinationMsg = "net:" + localIp.ToString() + ":" + destinationPort + "~shs:" + publicKey;
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
                this.Stop();
            }
        }
    }
}
