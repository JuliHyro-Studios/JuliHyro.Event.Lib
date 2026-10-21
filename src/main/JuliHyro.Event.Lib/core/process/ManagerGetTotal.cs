using JuliHyro.Event.Lib.core.Registry;
using JuliHyro.Event.Lib.core.Debug;

namespace JuliHyro.Event.Lib.core.process;

/// <summary>
/// Handles event and listener count queries.
/// </summary>
internal static class ManagerGetTotal
{
    /// <summary>
    /// Gets the total number of registered events.
    /// </summary>
    /// <returns>The number of registered events.</returns>
    internal static int GetTotalEventProcess()
    {
        try
        {
            int TotalEvent = EventRegistry.EventCount;

            CodeReturn.AddSuccess(
                $"Retrieved the total number of registered events: {TotalEvent}."
            );

            return TotalEvent;
        }
        catch (Exception ex)
        {
            CodeReturn.AddError(
                $"Failed to retrieve the total number of registered events: {ex.Message}"
            );

            return 0;
        }
    }

    /// <summary>
    /// Gets the total number of listeners registered across all events.
    /// </summary>
    /// <returns>The total number of registered listeners.</returns>
    internal static int GetTotalListenProcess()
    {
        try
        {
            lock (EventRegistry.SyncRoot)
            {
                int TotalListen = 0;

                foreach (var ListenerList in EventRegistry.Listeners.Values)
                {
                    TotalListen += ListenerList.Count;
                }

                CodeReturn.AddSuccess(
                    $"Retrieved the total number of registered listeners: {TotalListen}."
                );

                return TotalListen;
            }
        }
        catch (Exception ex)
        {
            CodeReturn.AddError(
                $"Failed to retrieve the total number of registered listeners: {ex.Message}"
            );

            return 0;
        }
    }
}