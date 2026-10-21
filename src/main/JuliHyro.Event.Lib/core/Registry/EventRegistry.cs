namespace JuliHyro.Event.Lib.core.Registry;

/// <summary>
/// Stores registered events and their listeners.
/// </summary>
internal static class EventRegistry
{
    /// <summary>
    /// Contains registered event names and their listener limits.
    /// </summary>
    internal static readonly Dictionary<string, int> Events =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Contains the listeners registered for each event.
    /// </summary>
    internal static readonly Dictionary<string, List<Action<string>>> Listeners =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Used to synchronize access to the event registry.
    /// </summary>
    internal static readonly object SyncRoot = new();

    /// <summary>
    /// Gets the number of registered events.
    /// </summary>
    internal static int EventCount
    {
        get
        {
            lock (SyncRoot)
            {
                return Events.Count;
            }
        }
    }
}