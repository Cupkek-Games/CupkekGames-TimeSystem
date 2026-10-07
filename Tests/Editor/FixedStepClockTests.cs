using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CupkekGames.TimeSystem.Tests
{
    public class FixedStepClockTests
    {
        [Test]
        public void Advance_RunsOnlyWholeTicks_AndCarriesTheRest()
        {
            var clock = new FixedStepClock(20);
            Assert.AreEqual(0, clock.Advance(0.04f));
            Assert.AreEqual(0.8f, clock.Alpha, 1e-4f);
            Assert.AreEqual(1, clock.Advance(0.02f));
            Assert.AreEqual(1, clock.Tick);
            Assert.AreEqual(0.2f, clock.Alpha, 1e-4f);
        }

        [Test]
        public void TheSameTime_RunsTheSameTicks_WhateverTheFrameRate()
        {
            int TicksAt(float frameSeconds, int frames)
            {
                var clock = new FixedStepClock(20);
                for (int i = 0; i < frames; i++) clock.Advance(frameSeconds);
                return clock.Tick;
            }

            // Ten seconds at 30, 60 and 144 frames a second.
            Assert.AreEqual(200, TicksAt(1f / 30f, 300));
            Assert.AreEqual(200, TicksAt(1f / 60f, 600));
            Assert.AreEqual(200, TicksAt(1f / 144f, 1440), 1);
        }

        [Test]
        public void TicksAreNumberedFromOne_InOrder()
        {
            var clock = new FixedStepClock(10);
            var seen = new List<int>();
            clock.OnTick += seen.Add;
            clock.Advance(0.35f);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, seen);
        }

        [Test]
        public void AFasterScale_RunsMoreTicks_NeverLongerOnes()
        {
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            try
            {
                TimeContext context = manager.CreateContext();
                using var clock = new FixedStepClock(context, 20);

                for (int i = 0; i < 60; i++) context.Update(1f / 60f);
                Assert.AreEqual(20, clock.Tick, "1x: one second is 20 ticks.");

                context.TimeScale = 3f;
                for (int i = 0; i < 60; i++) context.Update(1f / 60f);
                Assert.AreEqual(80, clock.Tick, "3x: one second is 60 more ticks.");
                Assert.AreEqual(0.05f, clock.StepSeconds, 1e-6f);

                context.Pause();
                for (int i = 0; i < 60; i++) context.Update(1f / 60f);
                Assert.AreEqual(80, clock.Tick, "Paused: no ticks.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }

        [Test]
        public void Dispose_StopsTheContextFeedingTheClock()
        {
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            try
            {
                TimeContext context = manager.CreateContext();
                var clock = new FixedStepClock(context, 20);
                context.Update(0.1f);
                Assert.AreEqual(2, clock.Tick);

                clock.Dispose();
                context.Update(0.1f);
                Assert.AreEqual(2, clock.Tick);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }

        [Test]
        public void TheCap_DropsTimePastIt()
        {
            var clock = new FixedStepClock(20, maxTicksPerAdvance: 5);
            Assert.AreEqual(5, clock.Advance(2f), "A two-second hitch runs five ticks, not forty.");
            Assert.Less(clock.Alpha, 1f, "The dropped time is not carried into later frames.");
            Assert.AreEqual(1, clock.Advance(0.05f));
        }

        [Test]
        public void ClocksFedThenStepped_Interleave_OneTickEach()
        {
            var a = new FixedStepClock(20);
            var b = new FixedStepClock(20);
            var order = new List<string>();
            a.OnTick += t => order.Add("a" + t);
            b.OnTick += t => order.Add("b" + t);

            // One long frame makes two ticks due on each; stepping in turn interleaves them.
            a.Accumulate(0.1f);
            b.Accumulate(0.1f);
            Assert.AreEqual(2, a.Due);
            bool ran;
            do
            {
                ran = a.Step() | b.Step();
            } while (ran);

            CollectionAssert.AreEqual(new[] { "a1", "b1", "a2", "b2" }, order);
            Assert.AreEqual(0, a.Due);
            Assert.IsFalse(a.Step());
        }

        [Test]
        public void Reset_StartsAgainFromTickZero()
        {
            var clock = new FixedStepClock(20);
            clock.Advance(0.12f);
            clock.Reset();
            Assert.AreEqual(0, clock.Tick);
            Assert.AreEqual(0f, clock.Alpha);
            Assert.AreEqual(1, clock.Advance(0.05f));
            Assert.AreEqual(1, clock.Tick);
        }

        [Test]
        public void SecondsAndTicks_ConvertAtTheRate()
        {
            var clock = new FixedStepClock(20);
            Assert.AreEqual(17, clock.ToTicks(0.85f));
            Assert.AreEqual(1, clock.ToTicks(0.025f), "Halfway rounds up.");
            Assert.AreEqual(1.5f, clock.ToSeconds(30), 1e-6f);
        }

        [Test]
        public void BadInput_FailsLoud()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FixedStepClock(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FixedStepClock(20, -1));
            Assert.Throws<ArgumentNullException>(() => new FixedStepClock(null, 20));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FixedStepClock(20).Advance(-0.1f));
        }
    }
}
