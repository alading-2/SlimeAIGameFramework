using SlimeAI.GameOS.Runtime.Entity;

internal static class EntityIdTests
{
    public static void ValueEqualityWorksAcrossSameKey()
    {
        var left = new EntityId("player");
        var right = new EntityId("player");
        AssertEqual("typed equality", left, right);
        AssertEqual("typed != different", false, left == new EntityId("enemy"));
        AssertEqual("hash equality", left.GetHashCode(), right.GetHashCode());
    }

    public static void EmptyDefaultAndFromNullAllEqual()
    {
        var defaultId = default(EntityId);
        AssertEqual("default == Empty", EntityId.Empty, defaultId);
        AssertEqual("From(null) == Empty", EntityId.Empty, EntityId.From(null));
        AssertEqual("From(\"\") == Empty", EntityId.Empty, EntityId.From(string.Empty));
        AssertEqual("new EntityId(null) == Empty", EntityId.Empty, new EntityId(null!));
        AssertEqual("new EntityId(\"\") == Empty", EntityId.Empty, new EntityId(string.Empty));
    }

    public static void IsEmptyTreatsNullAndEmptyAsEmpty()
    {
        AssertEqual("Empty.IsEmpty", true, EntityId.Empty.IsEmpty);
        AssertEqual("default.IsEmpty", true, default(EntityId).IsEmpty);
        AssertEqual("From(null).IsEmpty", true, EntityId.From(null).IsEmpty);
        AssertEqual("From(empty).IsEmpty", true, EntityId.From(string.Empty).IsEmpty);
        AssertEqual("non-empty IsEmpty", false, new EntityId("player").IsEmpty);
        // 空白字符串按设计 *不* 视为 empty（spec 仅要求 null / "" 拦截）。
        AssertEqual("whitespace IsEmpty", false, new EntityId(" ").IsEmpty);
    }

    public static void ValuePreservesUnderlyingString()
    {
        AssertEqual("Value preserves stable id", "player-001", new EntityId("player-001").Value);
        AssertEqual("Value of From preserves", "enemy", EntityId.From("enemy").Value);
        AssertEqual("ToString matches Value", "player", new EntityId("player").ToString());
        AssertEqual("ToString of Empty is empty", string.Empty, EntityId.Empty.ToString());
        AssertEqual("ToString of default is empty", string.Empty, default(EntityId).ToString());
    }

    private static void AssertEqual<T>(string name, T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{name}: expected {expected}, actual {actual}");
        }
    }
}
