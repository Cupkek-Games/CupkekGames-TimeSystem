using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Manages a list of PrimeTween Tweens and updates their time scale according to a TimeContext.
    /// </summary>
    public class TimeScaleTrailRenderer
    {
        public TimeContext Context;
        [SerializeField] private Dictionary<TrailRenderer, float> _collection = new Dictionary<TrailRenderer, float>();

        public TimeScaleTrailRenderer(TimeContext context)
        {
            Context = context ?? TimeManager.Instance?.Global;
            if (Context != null)
                Context.OnTimeScaleChanged += OnTimeScaleChanged;
        }

        public void Add(TrailRenderer trailRenderer)
        {
            if (trailRenderer != null)
            {
                float original;
                if (!_collection.ContainsKey(trailRenderer))
                {
                    original = trailRenderer.time;
                    _collection[trailRenderer] = original;
                }
                else
                {
                    original = _collection[trailRenderer];
                }

                trailRenderer.time = original / Context.TimeScale;
            }
        }
        public void Add(GameObject gameObject)
        {
            if (gameObject != null)
            {
                Add(gameObject.GetComponent<TrailRenderer>());
                foreach (var trailRenderer in gameObject.GetComponentsInChildren<TrailRenderer>())
                {
                    Add(trailRenderer);
                }
            }
        }

        public void Remove(TrailRenderer particleSystem)
        {
            if (particleSystem != null && _collection.ContainsKey(particleSystem))
            {
                _collection.Remove(particleSystem);
            }
        }

        public void Clear()
        {
            foreach (var trailRenderer in _collection.Keys)
            {
                if (trailRenderer != null)
                {
                    trailRenderer.time = _collection[trailRenderer];
                }
            }
            _collection.Clear();
        }
        public void ClearInactive()
        {
            Dictionary<TrailRenderer, float> collection = new();
            foreach (var trailRenderer in _collection.Keys)
            {
                if (trailRenderer != null)
                {
                    if (trailRenderer.enabled)
                    {
                        collection.Add(trailRenderer, _collection[trailRenderer]);
                    }
                    else
                    {
                        trailRenderer.time = _collection[trailRenderer];
                    }
                }
            }

            _collection = collection;
        }

        private void OnTimeScaleChanged(float timeScale)
        {
            foreach (var particle in _collection.Keys)
            {
                if (particle != null)
                {
                    particle.time = _collection[particle] / timeScale;
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