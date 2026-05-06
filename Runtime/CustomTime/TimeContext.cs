using System;
using System.Threading;
using System.Threading.Tasks;
#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif

namespace CupkekGames.TimeSystem
{
    public class TimeContext
    {
        private float _timeScale = 1f;
        public float TimeScale
        {
            get
            {
                return _timeScale;
            }
            set
            {
                if (Math.Abs(_timeScale - value) > float.Epsilon)
                {
                    bool wasPaused = Math.Abs(_timeScale) < float.Epsilon;
                    _timeScale = value;
                    OnTimeScaleChanged?.Invoke(_timeScale);

                    bool isPaused = Math.Abs(_timeScale) < float.Epsilon;
                    if (!wasPaused && isPaused)
                    {
                        OnPaused?.Invoke();
                    }
                    else if (wasPaused && !isPaused)
                    {
                        OnResumed?.Invoke();
                    }
                }
            }
        }
        private float _time;
        private float _unscaledTime;
        private float _lastTime;

        // Events
        public event Action<float> OnUpdate;
        public event Action<float> OnTimeScaleChanged;
        public event Action OnPaused;
        public event Action OnResumed;

        internal TimeContext()
        {
            _timeScale = 1f;
            _time = 0f;
            _unscaledTime = 0f;
            _lastTime = 0f;
        }
        public void Update(float deltaTime)
        {
            _unscaledTime += deltaTime;
            float scaledDelta = deltaTime * TimeScale;
            _time += scaledDelta;
            _lastTime = scaledDelta;

            OnUpdate?.Invoke(scaledDelta);
        }

        public float DeltaTime => _lastTime;
        public float UnscaledDeltaTime => TimeScale == 0 ? 0 : _lastTime / TimeScale;
        public float Time => _time;

        public void Pause()
        {
            TimeScale = 0f;
        }

        public void Resume()
        {
            TimeScale = 1f;
        }

#if UNITASK_INSTALLED
        // UniTask-based delay helpers — only compiled when UniTask is installed.
        // Consumers without UniTask still get the rest of TimeContext (TimeScale,
        // events, Update); the async overloads simply aren't available.

        /// <summary>
        /// Delays execution for the specified duration using this time context's time scale.
        /// The delay will be affected by time scaling and will pause when TimeScale is 0.
        /// </summary>
        /// <param name="seconds">Duration to delay in seconds</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task that completes after the specified delay</returns>
        public async UniTask DelayAsync(float seconds, CancellationToken cancellationToken = default)
        {
            if (seconds <= 0f) return;

            float targetTime = _time + seconds;

            while (_time < targetTime && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Delays execution for the specified duration using unscaled time.
        /// This delay will NOT be affected by time scaling or pausing.
        /// </summary>
        /// <param name="seconds">Duration to delay in seconds</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task that completes after the specified delay</returns>
        public async UniTask DelayUnscaledAsync(float seconds, CancellationToken cancellationToken = default)
        {
            if (seconds <= 0f) return;

            float targetTime = _unscaledTime + seconds;

            while (_unscaledTime < targetTime && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
#endif
    }
}