using UnityEngine;

namespace CupkekGames.TimeSystem
{
    public class TimeScaleParticleSystemMono : MonoBehaviour
    {
        public TimeContext Context;
        [Header("Particle System, empty to use the one on this GameObject")]
        [SerializeField] private ParticleSystem _particleSystem;
        private float _originalSimSpeed;

        private void Awake()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }
            _originalSimSpeed = _particleSystem.main.simulationSpeed;

            if (Context == null && TimeManager.Instance != null)
                Context = TimeManager.Instance.Global;
        }

        private void OnEnable()
        {
            if (Context != null)
                Context.OnTimeScaleChanged += OnTimeScaleChanged;
        }

        private void OnDisable()
        {
            if (Context != null)
                Context.OnTimeScaleChanged -= OnTimeScaleChanged;
        }

        private void OnTimeScaleChanged(float timeScale)
        {
            if (_particleSystem != null)
            {
                var main = _particleSystem.main;
                main.simulationSpeed = _originalSimSpeed * timeScale;
            }
        }
    }
}