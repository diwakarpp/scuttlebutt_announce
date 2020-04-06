using System;
using Xunit;
using Scuttlebutt.Announce;

namespace Scuttlebutt.Tests.Announce
{
    public class ScuttlebuttAnnounce
    {
        [Fact]
        public void Builds()
        {
            var ex = Record.Exception(() => new PresenceAnnouncer(8008, 1));

            Assert.Null(ex);
        }

        [Fact]
        public void Runs()
        {
            var announcer = new PresenceAnnouncer(8008, 1);

            var ex = Record.Exception(() => announcer.Run());

            Assert.Null(ex);
        }

        [Fact]
        public void DoesNotRunTwice()
        {
            var announcer = new PresenceAnnouncer(8008, 1);

            announcer.Run();
            var ex = Assert.Throws<InvalidOperationException>(() => announcer.Run());

            Assert.Equal("Announce loop already started", ex.Message);
        }

        [Fact]
        public void Stops()
        {
            var announcer = new PresenceAnnouncer(8008, 1);
            announcer.Run();

            var ex = Record.Exception(() => announcer.Stop());

            Assert.Null(ex);
        }

        [Fact]
        public void DoesNotStopTwice()
        {
            var announcer = new PresenceAnnouncer(8008, 1);
            announcer.Run();

            announcer.Stop();
            var ex = Assert.Throws<InvalidOperationException>(() => announcer.Stop());

            Assert.Equal("Announce loop not started", ex.Message);
        }

        [Fact]
        public void DoesNotStopBeforeStarting()
        {
            var announcer = new PresenceAnnouncer(8008, 1);

            var ex = Assert.Throws<InvalidOperationException>(() => announcer.Stop());

            Assert.Equal("Announce loop not started", ex.Message);
        }
    }
}
