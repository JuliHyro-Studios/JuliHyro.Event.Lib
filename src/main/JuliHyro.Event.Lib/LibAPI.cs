
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using JuliHyro.Event.Lib.core.process;

namespace JuliHyro.Event.Lib
{
    /// <summary>
    /// Provides the main API for registering, listening to, and emitting events.
    /// </summary>
    public class LibAPI
    {
        /// <summary>
        /// Registers a new event with a limit on the number of listeners.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="maxListenEvent">The maximum number of listeners allowed for this event.</param>
        public static void RegisterEvent(String eventName, int maxListenEvent)
        {
            JuliHyro.Event.Lib.core.process.ManagerEvent.RegisterEventProcess(eventName, maxListenEvent);
        }

        /// <summary>
        /// Unregisters an event.
        /// </summary>
        /// <param name="eventName">The name of the event to unregister.</param>
        public static void UnregisterEvent(String eventName)
        {
            ManagerEvent.UnRegisterEventProcess(eventName);
        }

        /// <summary>
        /// Sends a payload to the listeners of an event.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload being sent.</typeparam>
        /// <param name="eventName">The name of the event to emit.</param>
        /// <param name="payload">The data to send to the event's listeners.</param>
        public static void EmitEvent<TPayload>(String eventName, TPayload payload)
        {
            ManagerEmit.EmitEventProcess(eventName, payload);
        }

        /// <summary>
        /// Registers a callback to receive an event's payload.
        /// </summary>
        /// <param name="eventName">The name of the event to listen to.</param>
        /// <param name="callback">The callback to invoke when the event is emitted.</param>
        public static void ListenEvent(
            String eventName,
            Action<String> callback)
        {
            ManagerListen.ListenEventProcess(eventName, callback);
        }

        /// <summary>
        /// Stops a callback from receiving an event's payload.
        /// </summary>
        /// <param name="eventName">The name of the event to unsubscribe from.</param>
        /// <param name="callback">The callback to remove.</param>
        public static void UnlistenEvent(
            String eventName,
            Action<String> callback)
        {
            ManagerListen.UnListenEventProcess(eventName, callback);
        }
        
        /// <summary>
        /// Gets the total number of registered events.
        /// </summary>
        /// <returns>The number of registered events.</returns>
        public static int GetTotalEvent()
        {
            return ManagerGetTotal.GetTotalEventProcess();
        }

        /// <summary>
        /// Gets the total number of listeners registered across all events.
        /// </summary>
        /// <returns>The total number of registered listeners.</returns>
        public static int GetTotalListen()
        {
            return ManagerGetTotal.GetTotalListenProcess();
        }

    }
}
