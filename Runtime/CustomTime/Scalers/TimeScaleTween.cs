using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Manages a list of PrimeTween Tweens and updates their time scale according to a TimeContext.
    /// The tweens run on scaled time (PrimeTween's default), so the game's speed already reaches
    /// them: each tween's own scale is its context's alone.
    /// </summary>
    public class TimeScaleTween
    {
        public TimeContext Context;
        private List<Tween> _collection = new List<Tween>();

        public TimeScaleTween(TimeContext context)
        {
            Context = context ?? TimeManager.Instance?.Global;
            if (Context != null)
                Context.OnTimeScaleChanged += OnTimeScaleChanged;
        }

        public void Add(Tween tween)
        {
            if (tween.isAlive)
            {
                if (!_collection.Contains(tween))
                {
                    _collection.Add(tween);
                }

                tween.timeScale = Context.TimeScale;
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
                    tween.timeScale = 1f;
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
                    tween.timeScale = timeScale;
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
