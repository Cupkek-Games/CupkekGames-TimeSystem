using UnityEngine;

namespace CupkekGames.TimeSystem
{
  public class CountdownMono : MonoBehaviour
  {
    [SerializeField] private int _start;
    [SerializeField] private int _target;
    [SerializeField] private int _value;
    private Countdown _countdown;

    private void Awake()
    {
      _countdown = new(this, _start, _target);

      _countdown.OnStart += SetValue;
      _countdown.OnTick += SetValue;
      _countdown.OnComplete += OnComplete;
    }

    public void SetValue(int value)
    {
      _value = value;
    }

    public void OnComplete()
    {

    }

    private void OnDestroy()
    {
      _countdown.Stop();
    }

    public void Resume()
    {
      _countdown.Resume();
    }

    public void Stop()
    {
      _countdown.Stop();
    }

    public void IsPlaying()
    {
      Debug.Log(_countdown.IsPlaying() + "");
    }
  }
}