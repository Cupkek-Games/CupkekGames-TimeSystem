using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Manages a list of PrimeTween Tweens and updates their time scale according to a TimeContext.
    /// </summary>
    public class TimeScaleTween
    {
        public TimeContext Context;
        private List<Tween> _collection = new List<Tween>();
        private bool _withTimeScale = true;

        public TimeScaleTween(TimeContext context, bool withTimeScale = true)
        {
            Context = context ?? TimeManager.Instance?.Global;
            if (Context != null)
                Context.OnTimeScaleChanged += OnTimeScaleChanged;

            _withTimeScale = withTimeScale;
        }

        private float GetTimeScale(float timeScale)
        {
            if (_withTimeScale)
                return UnityEngine.Time.timeScale * timeScale;
            return timeScale;
        }

        public void Add(Tween tween)
        {
            if (tween.isAlive)
            {
                if (!_collection.Contains(tween))
                {
                    _collection.Add(tween);
                }

                tween.timeScale = GetTimeScale(Context.TimeScale);
            }
        }

        public void Remove(Tween tween)
        {
            _collection.Remove(tween);
        }

        public void Clear()
        {
            foreach (var tween in _collection)
            {
                if (tween.isAlive)
                {
                    tween.timeScale = GetTimeScale(1f);
                }
            }
            _collection.Clear();
        }

        private void OnTimeScaleChanged(float timeScale)
        {
            foreach (var tween in _collection)
            {
                if (tween.isAlive)
                {
                    tween.timeScale = GetTimeScale(timeScale);
                }
                else
                {
                    Remove(tween);
                }
            }
        }

        public void Dispose()
        {
            if (Context != null)
                Context.OnTimeScaleChanged -= OnTimeScaleChanged;
            Clear();
        }
    }
}