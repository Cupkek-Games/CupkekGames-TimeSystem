using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace CupkekGames.TimeSystem.Tests
{
    public class CountdownTimeContextTests
    {
        [Test]
        public void ACancelledToken_CompletesTheCountdown()
        {
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            using var cancel = new CancellationTokenSource();
            try
            {
                TimeContext context = manager.CreateContext();
                var countdown = new CountdownTimeContext(context, 5f, 0f, 0.1f, cancel.Token);
                int completed = 0;
                countdown.OnComplete += () => completed++;
                countdown.Start();

                cancel.Cancel();
                context.Update(0.05f);
                Assert.AreEqual(1, completed, "Cancelled while running: it completes.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }

        [Test]
        public void ACountdownStoppedInTheSameUpdate_NeverCompletes()
        {
            // A unit's poison kills it on this update: its death stops its stun's countdown
            // and cancels the stun's token, and the context still calls the stun's handler.
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            using var cancel = new CancellationTokenSource();
            try
            {
                TimeContext context = manager.CreateContext();
                CountdownTimeContext countdown = null;
                context.OnUpdate += _ =>
                {
                    countdown.Dispose();
                    cancel.Cancel();
                };
                countdown = new CountdownTimeContext(context, 5f, 0f, 0.1f, cancel.Token);
                int completed = 0;
                countdown.OnComplete += () => completed++;
                countdown.Start();

                context.Update(0.05f);
                context.Update(0.05f);
                Assert.AreEqual(0, completed, "A stopped countdown does nothing more.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }

        [Test]
        public void ALongFrame_RunsEveryStepItCovers()
        {
            // At 3x and 30 fps a frame is 0.1 s of fight; a slower frame covers several steps.
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            try
            {
                TimeContext context = manager.CreateContext();
                var countdown = new CountdownTimeContext(context, 1f, 0f, 0.1f);
                int ticks = 0, completed = 0;
                countdown.OnTick += _ => ticks++;
                countdown.OnComplete += () => completed++;
                countdown.Start();

                context.Update(0.55f);
                Assert.AreEqual(5, ticks, "Half a second in one frame is five steps, not one.");
                Assert.AreEqual(0.5f, countdown.Value, 0.0001f);

                context.Update(0.6f);
                Assert.AreEqual(1, completed, "It ends on the fight's time.");
                Assert.IsFalse(countdown.IsPlaying());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }

        [Test]
        public void AStepThatStopsIt_EndsTheCatchUp()
        {
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            try
            {
                TimeContext context = manager.CreateContext();
                var countdown = new CountdownTimeContext(context, 1f, 0f, 0.1f);
                int ticks = 0, completed = 0;
                countdown.OnTick += _ =>
                {
                    if (++ticks == 2) countdown.Dispose();
                };
                countdown.OnComplete += () => completed++;
                countdown.Start();

                context.Update(1f);
                Assert.AreEqual(2, ticks, "A step's handler stopped it: no more steps.");
                Assert.AreEqual(0, completed);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }

        [Test]
        public void ANonPositiveInterval_FailsLoud()
        {
            var manager = new GameObject("TimeManager").AddComponent<TimeManager>();
            try
            {
                TimeContext context = manager.CreateContext();
                Assert.Throws<System.ArgumentOutOfRangeException>(() => new CountdownTimeContext(context, 1f, 0f, 0f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }
    }
}
