using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Strategy 注册表。
/// </summary>
public static class MovementStrategyRegistry
{
    private static readonly Dictionary<MoveMode, Func<IMovementStrategy>> Factories = new();

    /// <summary>注册策略工厂。</summary>
    public static void Register(MoveMode mode, Func<IMovementStrategy> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Factories[mode] = factory;
    }

    /// <summary>创建策略实例。</summary>
    public static IMovementStrategy? Create(MoveMode mode)
    {
        return Factories.TryGetValue(mode, out var factory) ? factory.Invoke() : null;
    }

    /// <summary>注册内置策略。</summary>
    public static void RegisterBuiltIns()
    {
        if (!Factories.ContainsKey(MoveMode.Charge))
        {
            Register(MoveMode.Charge, static () => new ChargeMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.Orbit))
        {
            Register(MoveMode.Orbit, static () => new OrbitMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.SineWave))
        {
            Register(MoveMode.SineWave, static () => new SineWaveMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.BezierCurve))
        {
            Register(MoveMode.BezierCurve, static () => new BezierCurveMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.Boomerang))
        {
            Register(MoveMode.Boomerang, static () => new BoomerangMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.AttachToHost))
        {
            Register(MoveMode.AttachToHost, static () => new AttachToHostMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.PlayerInput))
        {
            Register(MoveMode.PlayerInput, static () => new PlayerInputMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.AIControlled))
        {
            Register(MoveMode.AIControlled, static () => new AIControlledMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.Parabola))
        {
            Register(MoveMode.Parabola, static () => new ParabolaMovementStrategy());
        }

        if (!Factories.ContainsKey(MoveMode.CircularArc))
        {
            Register(MoveMode.CircularArc, static () => new CircularArcMovementStrategy());
        }
    }

    /// <summary>清空注册表，供隔离测试使用。</summary>
    public static void Clear()
    {
        Factories.Clear();
    }
}
