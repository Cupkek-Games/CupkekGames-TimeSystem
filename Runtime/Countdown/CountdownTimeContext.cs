using System;
using System.Threading;
using UnityEngine;

namespace CupkekGames.TimeSystem
{
  public class CountdownTimeContext
  {
    private Guid _id;
    private float _value = 10f;
    public float Value
    {
      get
      {
        return _value;
      }
      set
      {
        _value = value;
        if (_value <= _target)
        {
          Stop();
          OnComplete?.Invoke();
        }
      }
    }
    private float _target = 0;
    private float _intervalSeconds;
    private float _elapsedTime;
    private bool _isRunning;

    public event Action<float> OnStart;
    public event Action<float> OnTick;
    public event Action OnComplete;

    private TimeContext _timeContext;
    private CancellationToken _cancellationToken;

    private bool _debug = false;

    public CountdownTimeContext(
      TimeContext timeContext,
      float startValue,
      float target = 0,
      float intervalSeconds = 0.1f,
      CancellationToken cancellationToken = default,
      bool debug = false)
    {
      _id = Guid.NewGuid();
      _timeContext = timeContext ?? throw new ArgumentNullException(nameof(timeContext));
      _value = startValue;
      _target = target;
      _intervalSeconds = intervalSeconds;
      _elapsedTime = 0f;
      _isRunning = false;
      _cancellationToken = cancellationToken;
      _debug = debug;

      if (_debug)
      {
        Debug.Log("CountdownTimeContext value: " + _value);
        Debug.Log("CountdownTimeContext target: " + _target);
        Debug.Log("CountdownTimeContext intervalSeconds: " + _intervalSeconds);
        Debug.Log("CountdownTimeContext elapsedTime: " + _elapsedTime);
        Debug.Log("CountdownTimeContext isRunning: " + _isRunning);
        Debug.Log("CountdownTimeContext cancellationToken: " + _cancellationToken);
      }
    }

    public void SetValue(float value)
    {
      _value = value;
    }

    private void OnTimeUpdate(float deltaTime)
    {
      if (_cancellationToken.IsCancellationRequested)
      {
        if (_debug)
        {
          Debug.Log("OnTimeUpdate: cancellationToken is cancelled");
        }
        Stop();
        OnComplete?.Invoke();
        return;
      }

      if (!_isRunning || _value <= _target)
      {
        if (_debug)
        {
          Debug.Log("OnTimeUpdate: not running or value is less than target");
        }
        return;
      }

      if (_debug)
      {
        Debug.Log("OnTimeUpdate");
      }

      _elapsedTime += deltaTime;

      if (_elapsedTime >= _intervalSeconds)
      {
        _elapsedTime -= _intervalSeconds;
        _value -= _intervalSeconds;
        OnTick?.Invoke(_intervalSeconds);

        if (_value <= _target)
        {
          Stop();
          OnComplete?.Invoke();
        }
      }
    }

    public void Start()
    {
      if (_isRunning)
      {
        if (_debug)
        {
          Debug.Log("CountdownTimeContext is already running");
        }
        return;
      }

      if (_value <= _target)
      {
        if (_debug)
        {
          Debug.Log("CountdownTimeContext value is less than target");
        }
        OnComplete?.Invoke();
        return;
      }

      if (_debug)
      {
        Debug.Log("Start");
      }

      _isRunning = true;
      _elapsedTime = 0f;
      _timeContext.OnUpdate += OnTimeUpdate;
      OnStart?.Invoke(_value);
    }

    public void Resume()
    {
      Start();
    }

    public void Stop()
    {
      if (!_isRunning)
        return;

      if (_debug)
      {
        Debug.Log("Stop");
      }

      _isRunning = false;
      _timeContext.OnUpdate -= OnTimeUpdate;
    }

    public bool IsPlaying()
    {
      return _isRunning;
    }

    /// <summary>
    /// Clean up event subscriptions when the countdown is no longer needed
    /// </summary>
    public void Dispose()
    {
      Stop();
    }

    public override bool Equals(object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      CountdownTimeContext other = (CountdownTimeContext)obj;
      return _id.Equals(other._id);
    }

    public override int GetHashCode()
    {
      return _id.GetHashCode();
    }

    public static bool operator ==(CountdownTimeContext left, CountdownTimeContext right)
    {
      if (ReferenceEquals(left, right))
        return true;

      if (left is null || right is null)
        return false;

      return left.Equals(right);
    }

    public static bool operator !=(CountdownTimeContext left, CountdownTimeContext right)
    {
      return !(left == right);
    }
  }
}
