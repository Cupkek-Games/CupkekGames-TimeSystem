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
    }
}
