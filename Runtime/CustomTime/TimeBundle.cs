using System;
using System.Collections.Generic;

namespace CupkekGames.TimeSystem
{
    public class TimeBundle
    {
        private TimeContext _timeContext;
        public TimeContext TimeContext => _timeContext;
        private TimeScaleParticleSystem _timeScaleParticleSystem;
        public TimeScaleParticleSystem TimeScaleParticleSystem => _timeScaleParticleSystem;
        private TimeScaleTrailRenderer _timeScaleTrailRenderer;
        public TimeScaleTrailRenderer TimeScaleTrailRenderer => _timeScaleTrailRenderer;
#if PRIMETWEEN_INSTALLED
        private TimeScaleTween _timeScaleTween;
        public TimeScaleTween TimeScaleTween => _timeScaleTween;
#endif
        private TimeScaleVisualEffect _timeScaleVisualEffect;
        public TimeScaleVisualEffect TimeScaleVisualEffect => _timeScaleVisualEffect;

        private readonly Dictionary<Type, ITimeScaler> _extensionScalers = new Dictionary<Type, ITimeScaler>();

        public void AddScaler<T>(T scaler) where T : class, ITimeScaler
        {
            _extensionScalers[typeof(T)] = scaler;
        }

        public T GetScaler<T>() where T : class, ITimeScaler
        {
            if (_extensionScalers.TryGetValue(typeof(T), out var scaler))
                return scaler as T;
            return null;
        }

        public TimeBundle(TimeManager timeManager)
        {
            _timeContext = timeManager.CreateContext();
            _timeScaleParticleSystem = new TimeScaleParticleSystem(_timeContext);
            _timeScaleTrailRenderer = new TimeScaleTrailRenderer(_timeContext);
#if PRIMETWEEN_INSTALLED
            _timeScaleTween = new TimeScaleTween(_timeContext);
#endif
            _timeScaleVisualEffect = new TimeScaleVisualEffect(_timeContext);
        }

        public void Clear()
        {
            _timeScaleParticleSystem.Clear();
            _timeScaleTrailRenderer.Clear();
#if PRIMETWEEN_INSTALLED
            _timeScaleTween.Clear();
#endif
            _timeScaleVisualEffect.Clear();

            foreach (var scaler in _extensionScalers.Values)
                scaler.Clear();

            _timeContext.Resume();
        }

        public void ClearInactive()
        {
            _timeScaleParticleSystem.ClearInactive();
            _timeScaleTrailRenderer.ClearInactive();
            _timeScaleVisualEffect.ClearInactive();

            foreach (var scaler in _extensionScalers.Values)
                scaler.ClearInactive();
        }

        public void Dispose()
        {
            _timeScaleParticleSystem.Dispose();
            _timeScaleTrailRenderer.Dispose();
#if PRIMETWEEN_INSTALLED
            _timeScaleTween.Dispose();
#endif
            _timeScaleVisualEffect.Dispose();

            foreach (var scaler in _extensionScalers.Values)
                scaler.Dispose();

            _timeContext.Resume();
        }
    }
}