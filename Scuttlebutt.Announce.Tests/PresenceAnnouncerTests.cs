using System;
using System.Net;
using System.Threading;

using Microsoft.Extensions.Logging;

using Xunit;
using Scuttlebutt.Announce;

namespace Scuttlebutt.Tests.Announce
{
    public class ScuttlebuttAnnounce
    {
        static private PresenceAnnouncer BuildAnnouncer()
        {
            var localAddr = IPAddress.Parse("127.0.0.1");
            var broadAddr = IPAddress.Parse("127.255.255.255");

            var logger = new NullLoggerFactory()
                .CreateLogger<PresenceAnnouncer>();

            var ret = new PresenceAnnouncer(8008,
                                            localAddr,
                                            broadAddr,
                                            1,
                                            logger);

            return ret;
        }

        [Fact]
        public void Builds()
        {
            var ex = Record.Exception(() => BuildAnnouncer());

            Assert.Null(ex);
        }

        [Fact]
        public async void Runs()
        {
            var announcer = BuildAnnouncer();

            var canceller = new CancellationTokenSource();

            var ex = await Record.ExceptionAsync(() => announcer.StartAsync(canceller.Token));

            Assert.Null(ex);
        }

        [Fact]
        public async void Stops()
        {
            var announcer = BuildAnnouncer();

            var canceller = new CancellationTokenSource();

            await announcer.StartAsync(canceller.Token);

            var ex = Record.ExceptionAsync(() => announcer.StopAsync(canceller.Token));

            Assert.Null(ex);
        }
    }
}
