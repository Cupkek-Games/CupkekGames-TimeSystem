using System;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Turns scaled frame time into whole, equal ticks: a simulation that steps only on
    /// <see cref="OnTick"/> plays out the same whatever the frame rate. A faster time scale
    /// runs more ticks per frame, never longer ones, and a paused context runs none.
    /// Feed it by hand with <see cref="Advance"/> (or <see cref="Accumulate"/> then
    /// <see cref="Step"/>), or bind it to a <see cref="TimeContext"/> so the context's scaled
    /// updates feed it (dispose to unbind).
    /// </summary>
    public sealed class FixedStepClock : IDisposable
    {
        private TimeContext _context;
        private readonly double _step;
        private double _accumulator;

        /// <param name="ticksPerSecond">Ticks in one second of scaled time.</param>
        /// <param name="maxTicksPerAdvance">
        /// The most ticks one frame's time (one <see cref="Advance"/> or <see cref="Accumulate"/>)
        /// may make due; time beyond it is dropped, so a long hitch slows the simulation instead
        /// of freezing the frame. 0 means no limit.
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
        public float Alpha => (float)Math.Min(1.0, _accumulator / _step);

        /// <summary>Whole ticks the carried time completes, not yet run.</summary>
        public int Due => (int)(_accumulator / _step);

        /// <summary>
        /// Adds <paramref name="scaledDeltaSeconds"/> and runs every tick it completes.
        /// Returns how many ran.
        /// </summary>
        public int Advance(float scaledDeltaSeconds)
        {
            Accumulate(scaledDeltaSeconds);
            int ran = 0;
            while (Step()) ran++;
            return ran;
        }

        /// <summary>
        /// Adds <paramref name="scaledDeltaSeconds"/> without running ticks; <see cref="Step"/>
        /// runs them. Several clocks fed this way can be stepped in turn, one tick each, so
        /// their ticks interleave the same way at any frame rate. The cap applies here.
        /// </summary>
        public void Accumulate(float scaledDeltaSeconds)
        {
            if (scaledDeltaSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(scaledDeltaSeconds), scaledDeltaSeconds, "Time only runs forward.");

            _accumulator += scaledDeltaSeconds;
            int due = Due;
            if (MaxTicksPerAdvance > 0 && due > MaxTicksPerAdvance)
                _accumulator -= (due - MaxTicksPerAdvance) * _step;
        }

        /// <summary>Runs one due tick; false when none is due.</summary>
        public bool Step()
        {
            if (Due == 0) return false;

            _accumulator -= _step;
            if (_accumulator < 0.0) _accumulator = 0.0;
            Tick++;
            OnTick?.Invoke(Tick);
            return true;
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
