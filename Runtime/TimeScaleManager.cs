using UnityEngine;
using CupkekGames.Services;

namespace CupkekGames.TimeSystem
{
  public class TimeScaleManager : ServiceProvider
  {
    private float fixedDeltaTime;
    private float _previousTimeScale = 1f;

    protected override void Awake()
    {
      base.Awake();

      // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
      this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    public void SetTimeScale(float timeScale)
    {
      _previousTimeScale = Time.timeScale;
      Time.timeScale = timeScale;
      Time.fixedDeltaTime = fixedDeltaTime * timeScale;
    }

    public void ResetTimeScale()
    {
      _previousTimeScale = Time.timeScale;
      Time.timeScale = 1f;
      Time.fixedDeltaTime = fixedDeltaTime;
    }

    public void PauseTimeScale()
    {
      _previousTimeScale = Time.timeScale;
      Time.timeScale = 0f;
    }

    public void ResumeTimeScale()
    {
      SetTimeScale(_previousTimeScale);
    }

    public override void RegisterServices()
    {
      ServiceLocator.Register(this);
    }

    public override void UnregisterServices()
    {
      ServiceLocator.Remove(this);
    }
  }
}
