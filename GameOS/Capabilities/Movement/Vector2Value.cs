using System;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// GameOS 运行时二维向量值，避免 Capability 内核直接依赖 Godot 类型。
/// </summary>
public readonly record struct Vector2Value(float X, float Y)
{
    /// <summary>零向量。</summary>
    public static readonly Vector2Value Zero = new(0f, 0f);

    /// <summary>向量长度。</summary>
    public float Length => MathF.Sqrt((X * X) + (Y * Y));

    /// <summary>返回归一化向量；零向量保持为零。</summary>
    public Vector2Value Normalized()
    {
        var length = Length;
        return length <= float.Epsilon ? Zero : new Vector2Value(X / length, Y / length);
    }

    /// <summary>
    /// 计算两点距离。
    /// </summary>
    public static float Distance(Vector2Value left, Vector2Value right)
    {
        return (left - right).Length;
    }

    /// <summary>
    /// 向量加法。
    /// </summary>
    public static Vector2Value operator +(Vector2Value left, Vector2Value right)
    {
        return new Vector2Value(left.X + right.X, left.Y + right.Y);
    }

    /// <summary>
    /// 向量减法。
    /// </summary>
    public static Vector2Value operator -(Vector2Value left, Vector2Value right)
    {
        return new Vector2Value(left.X - right.X, left.Y - right.Y);
    }

    /// <summary>
    /// 标量乘法。
    /// </summary>
    public static Vector2Value operator *(Vector2Value vector, float value)
    {
        return new Vector2Value(vector.X * value, vector.Y * value);
    }

    /// <summary>
    /// 标量除法。
    /// </summary>
    public static Vector2Value operator /(Vector2Value vector, float value)
    {
        return new Vector2Value(vector.X / value, vector.Y / value);
    }
}
