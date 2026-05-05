namespace SkilmeAI.GameOS.Runtime.Pool;

/// <summary>
/// Optional lifecycle contract for pooled objects.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called after an object is acquired.
    /// </summary>
    void OnPoolAcquire()
    {
    }

    /// <summary>
    /// Called before an object is returned.
    /// </summary>
    void OnPoolRelease()
    {
    }

    /// <summary>
    /// Called after release cleanup to reset state.
    /// </summary>
    void OnPoolReset()
    {
    }
}
