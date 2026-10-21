
using System;
using System.Linq;
using System.Text.Json;
using JuliHyro.Event.Lib.core.Registry;
using JuliHyro.Event.Lib.core.Debug;

namespace JuliHyro.Event.Lib.core.process;

/// <summary>
/// Handles emitting events and validating their payloads.
/// </summary>
internal static class ManagerEmit
{
    /// <summary>
    /// Validates and sends a JSON payload to the listeners of a registered event.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload being sent.</typeparam>
    /// <param name="eventName">The name of the registered event.</param>
    /// <param name="payload">The data to serialize and send as JSON.</param>
    internal static void EmitEventProcess<TPayload>(String eventName, TPayload payload)
    {
        if (String.IsNullOrWhiteSpace(eventName))
        {
            CodeReturn.AddError("Cannot emit an event: the event name is empty.");
            return;
        }

        if (!EventRegistry.Events.ContainsKey(eventName))
        {
            CodeReturn.AddError(
                $"Cannot emit event '{eventName}': the event is not registered."
            );
            return;
        }

        if (payload is null)
        {
            CodeReturn.AddError(
                $"Cannot emit event '{eventName}': the payload is null."
            );
            return;
        }

        String JsonPayload;

        try
        {
            JsonPayload = JsonSerializer.Serialize(payload);
        }
        catch (JsonException ex)
        {
            CodeReturn.AddError(
                $"Failed to serialize the payload for event '{eventName}': {ex.Message}"
            );
            return;
        }
        catch (NotSupportedException ex)
        {
            CodeReturn.AddError(
                $"The payload type is not supported for event '{eventName}': {ex.Message}"
            );
            return;
        }

        try
        {
            using JsonDocument Document = JsonDocument.Parse(JsonPayload);
        }
        catch (JsonException ex)
        {
            CodeReturn.AddError(
                $"The payload for event '{eventName}' is not valid JSON: {ex.Message}"
            );
            return;
        }

        if (!EventRegistry.Listeners.TryGetValue(eventName, out var Listeners)
            || Listeners.Count == 0)
        {
            CodeReturn.AddWarning(
                $"Event '{eventName}' was emitted, but no listeners are registered."
            );
            return;
        }

        foreach (var Callback in Listeners.ToArray())
        {
            Callback(JsonPayload);
        }

        CodeReturn.AddSuccess(
            $"Event '{eventName}' emitted successfully to {Listeners.Count} listener(s)."
        );
    }
}
