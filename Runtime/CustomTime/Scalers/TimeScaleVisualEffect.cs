using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Manages a list of Visual Effect Graph components and updates their play rate according to a TimeContext.
    /// </summary>
    public class TimeScaleVisualEffect
    {
        public TimeContext Context;
        [SerializeField] private Dictionary<VisualEffect, float> _collection = new Dictionary<VisualEffect, float>();

        public TimeScaleVisualEffect(TimeContext context)
        {
            Context = context ?? TimeManager.Instance?.Global;
            if (Context != null)
                Context.OnTimeScaleChanged += OnTimeScaleChanged;
        }

        public void Add(VisualEffect visualEffect)
        {
            if (visualEffect != null)
            {
                float original;
                if (!_collection.ContainsKey(visualEffect))
                {
                    original = visualEffect.playRate;
                    _collection[visualEffect] = original;
                }
                else
                {
                    original = _collection[visualEffect];
                }

                visualEffect.playRate = original * Context.TimeScale;
            }
        }

        public void Add(GameObject gameObject)
        {
            if (gameObject != null)
            {
                Add(gameObject.GetComponent<VisualEffect>());
                foreach (var vfx in gameObject.GetComponentsInChildren<VisualEffect>())
                {
                    Add(vfx);
                }
            }
        }

        public void Remove(VisualEffect visualEffect)
        {
            if (visualEffect != null && _collection.ContainsKey(visualEffect))
            {
                _collection.Remove(visualEffect);
            }
        }

        public void Clear()
        {
            foreach (var vfx in _collection.Keys)
            {
                if (vfx != null)
                {
                    vfx.playRate = _collection[vfx];
                }
            }
            _collection.Clear();
        }

        public void ClearInactive()
        {
            Dictionary<VisualEffect, float> collection = new();
            foreach (var vfx in _collection.Keys)
            {
                if (vfx != null)
                {
                    if (vfx.isActiveAndEnabled)
                    {
                        collection.Add(vfx, _collection[vfx]);
                    }
                    else
                    {
                        vfx.playRate = _collection[vfx];
                    }
                }
            }

            _collection = collection;
        }

        private void OnTimeScaleChanged(float timeScale)
        {
            foreach (var vfx in _collection.Keys)
            {
                if (vfx != null)
                {
                    vfx.playRate = _collection[vfx] * timeScale;
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