using System;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Capability 内部数学工具。
/// </summary>
internal static class MovementMath
{
    private const float DegToRad = MathF.PI / 180f;

    /// <summary>
    /// 从角度生成 Godot 2D 方向，0=右、90=下、正值顺时针。
    /// </summary>
    public static Vector2Value DirectionFromAngle(float degrees)
    {
        var radians = degrees * DegToRad;
        return new Vector2Value(MathF.Cos(radians), MathF.Sin(radians));
    }

    /// <summary>
    /// 从方向向量推导 Godot 2D 角度。
    /// </summary>
    public static float AngleFromDirection(Vector2Value direction)
    {
        return MathF.Atan2(direction.Y, direction.X) / DegToRad;
    }

    /// <summary>
    /// 采样正弦波偏移。
    /// </summary>
    public static float SineOffset(float amplitude, float frequency, float elapsedTime, float phaseDegrees)
    {
        return amplitude * MathF.Sin(((elapsedTime * frequency * 360f) + phaseDegrees) * DegToRad);
    }

    /// <summary>
    /// 解析 Movement 速度。
    /// </summary>
    public static float ResolveSpeed(in MovementParams movementParams, float fallback)
    {
        if (movementParams.Speed > 0f)
        {
            return movementParams.Speed;
        }

        if (movementParams.MaxDistance > 0f && movementParams.MaxDuration > 0f)
        {
            return movementParams.MaxDistance / movementParams.MaxDuration;
        }

        return fallback;
    }
}
