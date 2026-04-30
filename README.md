# CupkekGames TimeSystem

Per-context time scaling for Unity. Run multiple independent timelines (combat, UI, cinematic) with their own pause/slow/fast-forward state instead of toggling `Time.timeScale` globally.

## What's inside

**Runtime** (`CupkekGames.TimeSystem.asmdef`)

- `TimeManager` — singleton MonoBehaviour, root of the time-context tree
- `TimeContext` / `TimeBundle` — per-context delta-time provider; nest contexts to scale a subtree
- `ITimeScaler` — apply a `TimeContext` to anything that has a "speed" knob
- Built-in scalers: `TimeScaleParticleSystem(Mono)`, `TimeScaleTrailRenderer(Mono)`, `TimeScaleVisualEffect`, `TimeScaleTween` (PrimeTween)
- `Countdown` / `CountdownMono` / `CountdownTimeContext` — pausable countdowns bound to a `TimeContext`

## Dependencies

- `com.cysharp.unitask` (asmdef reference, not a UPM dep)
- `com.kyrylokuzyk.primetween` (asmdef reference, used by `TimeScaleTween`)

Both are referenced as asmdef GUIDs — bring your own copy via UPM or `.tgz`.
