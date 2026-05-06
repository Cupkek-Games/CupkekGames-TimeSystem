using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.TimeSystem
{
    /// <summary>
    /// Manages a list of PrimeTween Tweens and updates their time scale according to a TimeContext.
    /// </summary>
    public class TimeScaleParticleSystem
    {
        public TimeContext Context;
        [SerializeField] private Dictionary<ParticleSystem, float> _collection = new Dictionary<ParticleSystem, float>();

        public TimeScaleParticleSystem(TimeContext context)
        {
            Context = context ?? TimeManager.Instance?.Global;
            if (Context != null)
                Context.OnTimeScaleChanged += OnTimeScaleChanged;
        }

        public void Add(ParticleSystem particleSystem)
        {
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                float original;
                if (!_collection.ContainsKey(particleSystem))
                {
                    original = main.simulationSpeed;
                    _collection[particleSystem] = original;
                }
                else
                {
                    original = _collection[particleSystem];
                }

                main.simulationSpeed = original * Context.TimeScale;
            }
        }

        public void Add(GameObject gameObject)
        {
            if (gameObject != null)
            {
                Add(gameObject.GetComponent<ParticleSystem>());
                foreach (var particleSystem in gameObject.GetComponentsInChildren<ParticleSystem>())
                {
                    Add(particleSystem);
                }
            }
        }

        public void Remove(ParticleSystem particleSystem)
        {
            if (particleSystem != null && _collection.ContainsKey(particleSystem))
            {
                _collection.Remove(particleSystem);
            }
        }

        public void Clear()
        {
            foreach (var vfx in _collection.Keys)
            {
                if (vfx != null)
                {
                    var main = vfx.main;
                    main.simulationSpeed = _collection[vfx];
                }
            }
            _collection.Clear();
        }

        public void ClearInactive()
        {
            Dictionary<ParticleSystem, float> collection = new();
            foreach (var particle in _collection.Keys)
            {
                if (particle != null)
                {
                    if (particle.isPlaying)
                    {
                        collection.Add(particle, _collection[particle]);
                    }
                    else
                    {
                        var main = particle.main;
                        main.simulationSpeed = _collection[particle];
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
                    var main = particle.main;
                    main.simulationSpeed = _collection[particle] * timeScale;
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