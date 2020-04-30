using System;
using Xunit;
using Scuttlebutt.Announce;
using System.Net;
using Sodium;

namespace Scuttlebutt.Tests.Announce
{
    public class ScuttlebuttAnnounce
    {
        static private PresenceAnnouncer BuildAnnouncer()
        {
            var localAddr = IPAddress.Parse("127.0.0.1");
            var broadAddr = IPAddress.Parse("127.255.255.255");

            var keypair = PublicKeyAuth.GenerateKeyPair();

            var ret = new PresenceAnnouncer(
                8008,
                localAddr,
                broadAddr,
                1,
                keypair.PublicKey
            );

            return ret;
        }

        [Fact]
        public void Builds()
        {
            var ex = Record.Exception(() => BuildAnnouncer());

            Assert.Null(ex);
        }

        [Fact]
        public void Runs()
        {
            var announcer = BuildAnnouncer();

            var ex = Record.Exception(() => announcer.Run());

            Assert.Null(ex);
        }

        [Fact]
        public void DoesNotRunTwice()
        {
            var announcer = BuildAnnouncer();

            announcer.Run();
            var ex = Assert.Throws<InvalidOperationException>(() => announcer.Run());

            Assert.Equal("Announce loop already started", ex.Message);
        }

        [Fact]
        public void Stops()
        {
            var announcer = BuildAnnouncer();
            announcer.Run();

            var ex = Record.Exception(() => announcer.Stop());

            Assert.Null(ex);
        }

        [Fact]
        public void DoesNotStopTwice()
        {
            var announcer = BuildAnnouncer();
            announcer.Run();

            announcer.Stop();
            var ex = Assert.Throws<InvalidOperationException>(() => announcer.Stop());

            Assert.Equal("Announce loop not started", ex.Message);
        }

        [Fact]
        public void DoesNotStopBeforeStarting()
        {
            var announcer = BuildAnnouncer();

            var ex = Assert.Throws<InvalidOperationException>(() => announcer.Stop());

            Assert.Equal("Announce loop not started", ex.Message);
        }
    }
}
