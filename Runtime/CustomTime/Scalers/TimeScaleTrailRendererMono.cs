using UnityEngine;

namespace CupkekGames.TimeSystem
{
    [RequireComponent(typeof(TrailRenderer))]
    public class TimeScaleTrailRendererMono : MonoBehaviour
    {
        public TimeContext Context;
        [Header("Trail Renderer, empty to use the one on this GameObject")]
        [SerializeField] private TrailRenderer _trailRenderer;
        private float _originalTime;

        private void Awake()
        {
            if (_trailRenderer == null)
            {
                _trailRenderer = GetComponent<TrailRenderer>();
            }
            _originalTime = _trailRenderer.time;

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
            if (_trailRenderer != null)
            {
                _trailRenderer.time = _originalTime * timeScale;
            }
        }
    }
}