using System;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Turns scaled frame time into whole, equal ticks: a simulation that steps only on
    /// <see cref="OnTick"/> plays out the same whatever the frame rate. A faster time scale
    /// runs more ticks per frame, never longer ones, and a paused context runs none.
    /// Feed it by hand with <see cref="Advance"/>, or bind it to a <see cref="TimeContext"/>
    /// so the context's scaled updates feed it (dispose to unbind).
    /// </summary>
    public sealed class FixedStepClock : IDisposable
    {
        private TimeContext _context;
        private readonly double _step;
        private double _accumulator;

        /// <param name="ticksPerSecond">Ticks in one second of scaled time.</param>
        /// <param name="maxTicksPerAdvance">
        /// The most ticks one advance runs; time beyond it is dropped, so a long hitch slows the
        /// simulation instead of freezing the frame. 0 means no limit.
        /// </param>
        public FixedStepClock(int ticksPerSecond, int maxTicksPerAdvance = 0)
        {
            if (ticksPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticksPerSecond), ticksPerSecond, "A fixed-step clock needs at least one tick per second.");
            if (maxTicksPerAdvance < 0)
                throw new ArgumentOutOfRangeException(nameof(maxTicksPerAdvance), maxTicksPerAdvance, "Use 0 for no limit.");

            TicksPerSecond = ticksPerSecond;
            _step = 1.0 / ticksPerSecond;
            StepSeconds = (float)_step;
            MaxTicksPerAdvance = maxTicksPerAdvance;
        }

        /// <summary>A clock fed by <paramref name="context"/>'s scaled updates until disposed.</summary>
        public FixedStepClock(TimeContext context, int ticksPerSecond, int maxTicksPerAdvance = 0)
            : this(ticksPerSecond, maxTicksPerAdvance)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.OnUpdate += OnContextUpdate;
        }

        /// <summary>Raised once per tick with the tick's number (the first is 1).</summary>
        public event Action<int> OnTick;

        public int TicksPerSecond { get; }

        /// <summary>The length of one tick in scaled seconds.</summary>
        public float StepSeconds { get; }

        public int MaxTicksPerAdvance { get; }

        /// <summary>Ticks run since the clock started or was last reset.</summary>
        public int Tick { get; private set; }

        /// <summary>
        /// How far the time not yet ticked is toward the next tick, from 0 to 1: draw between
        /// the last tick's state and the next by this much.
        /// </summary>
        public float Alpha => (float)(_accumulator / _step);

        /// <summary>
        /// Adds <paramref name="scaledDeltaSeconds"/> and runs every tick it completes.
        /// Returns how many ran.
        /// </summary>
        public int Advance(float scaledDeltaSeconds)
        {
            if (scaledDeltaSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(scaledDeltaSeconds), scaledDeltaSeconds, "Time only runs forward.");

            _accumulator += scaledDeltaSeconds;
            int due = (int)(_accumulator / _step);
            if (due == 0) return 0;

            _accumulator -= due * _step;
            if (_accumulator < 0.0) _accumulator = 0.0;
            if (MaxTicksPerAdvance > 0 && due > MaxTicksPerAdvance) due = MaxTicksPerAdvance;

            for (int i = 0; i < due; i++)
            {
                Tick++;
                OnTick?.Invoke(Tick);
            }
            return due;
        }

        /// <summary>Back to tick 0 with no time carried.</summary>
        public void Reset()
        {
            Tick = 0;
            _accumulator = 0.0;
        }

        /// <summary>The whole number of ticks nearest to <paramref name="seconds"/>.</summary>
        public int ToTicks(float seconds) => (int)Math.Round(seconds * TicksPerSecond, MidpointRounding.AwayFromZero);

        public float ToSeconds(int ticks) => (float)(ticks * _step);

        public void Dispose()
        {
            if (_context == null) return;
            _context.OnUpdate -= OnContextUpdate;
            _context = null;
        }

        private void OnContextUpdate(float scaledDeltaSeconds) => Advance(scaledDeltaSeconds);
    }
}
