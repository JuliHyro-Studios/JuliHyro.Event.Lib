
using System;
using System.Collections.Generic;
using JuliHyro.Event.Lib.core.Registry;
using JuliHyro.Event.Lib.core.Debug;

namespace JuliHyro.Event.Lib.core.process;

/// <summary>
/// Handles registering and removing event listeners.
/// </summary>
internal class ManagerListen
{
    /// <summary>
    /// Adds a callback to an event's listener list.
    /// The callback won't be added if the event doesn't exist
    /// or has reached its listener limit.
    /// </summary>
    /// <param name="EventName">The name of the event to listen to.</param>
    /// <param name="Callback">The callback to invoke when the event is emitted.</param>
    internal static void ListenEventProcess(String EventName, Action<String> Callback)
    {
        if (!EventRegistry.Events.ContainsKey(EventName))
        {
            CodeReturn.AddError(
                $"Cannot listen to event '{EventName}': the event is not registered."
            );
            return;
        }

        if (!EventRegistry.Listeners.ContainsKey(EventName))
        {
            EventRegistry.Listeners.Add(EventName, new List<Action<String>>());
        }

        if (EventRegistry.Listeners[EventName].Count >= EventRegistry.Events[EventName])
        {
            CodeReturn.AddWarning(
                $"Cannot listen to event '{EventName}': the listener limit has been reached."
            );
            return;
        }

        EventRegistry.Listeners[EventName].Add(Callback);

        CodeReturn.AddSuccess(
            $"Listener registered successfully for event '{EventName}'."
        );
    }

    /// <summary>
    /// Removes a callback from an event's listener list.
    /// The listener list is removed when no callbacks remain.
    /// </summary>
    /// <param name="EventName">The name of the event to unsubscribe from.</param>
    /// <param name="Callback">The callback to remove.</param>
    internal static void UnListenEventProcess(String EventName, Action<String> Callback)
    {
        if (!EventRegistry.Listeners.ContainsKey(EventName))
        {
            CodeReturn.AddWarning(
                $"Cannot remove listener from event '{EventName}': no listeners are registered."
            );
            return;
        }

        if (!EventRegistry.Listeners[EventName].Remove(Callback))
        {
            CodeReturn.AddWarning(
                $"Cannot remove listener from event '{EventName}': the callback was not found."
            );
            return;
        }

        if (EventRegistry.Listeners[EventName].Count == 0)
        {
            EventRegistry.Listeners.Remove(EventName);
        }

        CodeReturn.AddSuccess(
            $"Listener removed successfully from event '{EventName}'."
        );
    }
}
