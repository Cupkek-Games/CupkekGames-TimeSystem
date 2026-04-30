using UnityEngine;
using System.Collections.Generic;

namespace CupkekGames.TimeSystem
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }
        private List<TimeContext> _updateContexts = new List<TimeContext>();
        private List<TimeContext> _fixedUpdateContexts = new List<TimeContext>();
        public TimeContext Global { get; private set; }
        public bool GlobalUseFixedUpdate { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Global = new TimeContext();
        }

        private void Update()
        {
            float deltaTime = UnityEngine.Time.deltaTime;

            if (!GlobalUseFixedUpdate)
            {
                Global.Update(deltaTime);
            }

            // Iterate only contexts that use Update
            int contextCount = _updateContexts.Count;
            for (int i = 0; i < contextCount; i++)
            {
                _updateContexts[i].Update(deltaTime);
            }
        }

        private void FixedUpdate()
        {
            float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;

            if (GlobalUseFixedUpdate)
            {
                Global.Update(fixedDeltaTime);
            }

            // Iterate only contexts that use FixedUpdate
            int contextCount = _fixedUpdateContexts.Count;
            for (int i = 0; i < contextCount; i++)
            {
                _fixedUpdateContexts[i].Update(fixedDeltaTime);
            }
        }

        public TimeContext CreateContext(bool useFixedUpdate = false)
        {
            var ctx = new TimeContext();

            // Add to appropriate specialized collection
            if (useFixedUpdate)
                _fixedUpdateContexts.Add(ctx);
            else
                _updateContexts.Add(ctx);

            return ctx;
        }

        public void RemoveContext(TimeContext ctx)
        {
            if (ctx != Global)
            {
                // Remove from specialized collections
                _updateContexts.Remove(ctx);
                _fixedUpdateContexts.Remove(ctx);
            }
        }
    }
}