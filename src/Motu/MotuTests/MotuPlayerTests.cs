using System;
using Xunit;

namespace MotuTests
{
    public class MotuPlayerTests
    {
        [Fact]
        public void ChangeBeastShot_GoingRight_Tests()
        {
            var p = new MotuPlayer();
            p.BeastActiveTargets[0] = true;
            p.ChangeActiveBeastTarget();
            Assert.True(p.BeastActiveTargets[1]);
        }

        /// <summary>
        /// When we get to the last shot we need to reverse the order
        /// </summary>
        [Fact]
        public void ChangeBeastShot_GoingLeftOnLastShot_Tests()
        {
            var p = new MotuPlayer();
            p.BeastActiveTargets[5] = true;
            p.ChangeActiveBeastTarget();
            Assert.True(p.BeastActiveTargets[4]);
        }
    }
}
