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
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Scuttlebutt.Announce
{
    /// <summary>
    ///   Announces a presence periodically
    /// </summary>
    public class PresenceAnnouncer : BackgroundService
    {
        private int sleepTime;
        // Conection
        private Socket udpSocket;
        private IPAddress localIp;
        private IPAddress destIp;
        private int destinationPort;
        private IPEndPoint connectionPoint;
        private string publicKey;

        // Logging
        private readonly ILogger<PresenceAnnouncer> _logger;

        public PresenceAnnouncer (
            int port,
            IPAddress localIp,
            IPAddress destIp,
            int sleepTime,
            ILogger<PresenceAnnouncer> logger
            )
        {
            this._logger = logger;

            this.sleepTime = sleepTime;

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

        /// <summary>
        ///   Stops the announcing loop
        /// </summary>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            var e = new SocketAsyncEventArgs();
            this.udpSocket.DisconnectAsync(e);

            return base.StopAsync(cancellationToken);
        }

        /// <summary>
        ///   Runs the LoopBody while the service is not canceled
        /// </summary>
        /// <param name="cancelToken">Token that signals that the service should be stopped</param>
        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            await this.udpSocket.ConnectAsync(connectionPoint);

            while(!cancelToken.IsCancellationRequested)
            {
                var message = CraftMessage(localIp, destinationPort, publicKey);

                // Send message
                udpSocket.SendTo(message, connectionPoint);
                await Task.Delay(this.sleepTime, cancelToken);

                _logger.LogInformation("Broadcasted presence: {message}", message);
            }
        }

        private static byte[] CraftMessage(IPAddress localIp, int destinationPort, string publicKey)
        {
            String destinationMsg = "net:" + localIp.ToString() + ":" + destinationPort + "~shs:" + publicKey;
            // Preparation to send the message
            byte[] bufferedMsg = Encoding.ASCII.GetBytes(destinationMsg);

            return bufferedMsg;
        }
    }
}
