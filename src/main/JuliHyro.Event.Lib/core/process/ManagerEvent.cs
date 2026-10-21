
using System;
using JuliHyro.Event.Lib.core.Registry;
using JuliHyro.Event.Lib.core.Debug;

namespace JuliHyro.Event.Lib.core.process
{
    /// <summary>
    /// Handles registering and unregistering events.
    /// </summary>
    internal class ManagerEvent
    {
        /// <summary>
        /// Registers an event with a maximum listener limit.
        /// The event won't be registered if its name already exists
        /// or the listener limit is less than one.
        /// </summary>
        /// <param name="EventName">The name of the event to register.</param>
        /// <param name="MaxListenEvent">The maximum number of listeners allowed for this event.</param>
        internal static void RegisterEventProcess(String EventName, int MaxListenEvent)
        {
            if (String.IsNullOrWhiteSpace(EventName))
            {
                CodeReturn.AddError("Cannot register an event: the event name is empty.");
                return;
            }

            if (EventRegistry.Events.ContainsKey(EventName))
            {
                CodeReturn.AddWarning(
                    $"Cannot register event '{EventName}': the event is already registered."
                );
                return;
            }

            if (MaxListenEvent < 1)
            {
                CodeReturn.AddError(
                    $"Cannot register event '{EventName}': the maximum listener limit must be greater than zero."
                );
                return;
            }

            EventRegistry.Events.Add(EventName, MaxListenEvent);

            CodeReturn.AddSuccess(
                $"Event '{EventName}' registered successfully with a listener limit of {MaxListenEvent}."
            );
        }

        /// <summary>
        /// Unregisters an event if it exists.
        /// </summary>
        /// <param name="EventName">The name of the event to unregister.</param>
        internal static void UnRegisterEventProcess(String EventName)
        {
            if (String.IsNullOrWhiteSpace(EventName))
            {
                CodeReturn.AddError("Cannot unregister an event: the event name is empty.");
                return;
            }

            if (!EventRegistry.Events.ContainsKey(EventName))
            {
                CodeReturn.AddWarning(
                    $"Cannot unregister event '{EventName}': the event is not registered."
                );
                return;
            }

            EventRegistry.Events.Remove(EventName);

            CodeReturn.AddSuccess(
                $"Event '{EventName}' unregistered successfully."
            );
        }
    }
}
