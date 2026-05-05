namespace SkilmeAI.GameOS.Runtime.Relationship;

/// <summary>
/// 父实体销毁时，直接归属子实体的处理策略。
/// </summary>
public enum ParentDestroyPolicy
{
    /// <summary>父实体销毁时递归销毁子实体。</summary>
    DestroyRecursively = 0,

    /// <summary>父实体销毁时只断开归属关系，子实体继续存活。</summary>
    Detach = 1
}
