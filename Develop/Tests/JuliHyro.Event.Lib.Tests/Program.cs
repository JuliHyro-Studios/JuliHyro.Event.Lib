
using System;
using System.Collections.Generic;
using System.Text.Json;
using JuliHyro.Event.Lib;

internal static class Program
{
    private sealed record TestResult(
        string Name,
        bool Passed,
        string? Error
    );

    private sealed class TestPayload
    {
        public string Message { get; set; } = "";
        public int Number { get; set; }
    }

    private static readonly List<TestResult> Results = new();

    private static int PassedCount =>
        Results.FindAll(result => result.Passed).Count;

    private static int FailedCount =>
        Results.FindAll(result => !result.Passed).Count;

    private static void Main()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(" JuliHyro.Event.Lib - Integration Test");
        Console.WriteLine("==================================================");
        Console.WriteLine();

        const string EventName = "tests.integration.full";
        const int MaxListeners = 2;

        int InitialEventCount = LibAPI.GetTotalEvent();
        int InitialListenerCount = LibAPI.GetTotalListen();

        int Listener1Calls = 0;
        int Listener2Calls = 0;
        int Listener3Calls = 0;

        string? Listener1Received = null;
        string? Listener2Received = null;

        Action<string> Listener1 = json =>
        {
            Listener1Calls++;
            Listener1Received = json;
            Console.WriteLine($"  [Listener 1] Received: {json}");
        };

        Action<string> Listener2 = json =>
        {
            Listener2Calls++;
            Listener2Received = json;
            Console.WriteLine($"  [Listener 2] Received: {json}");
        };

        Action<string> Listener3 = json =>
        {
            Listener3Calls++;
            Console.WriteLine($"  [Listener 3] Received: {json}");
        };

        // STEP 1: Read initial statistics.
        RunStep("01. Read initial event/listener counts", () =>
        {
            Check(InitialEventCount >= 0,
                "Initial event count is invalid.");

            Check(InitialListenerCount >= 0,
                "Initial listener count is invalid.");

            Console.WriteLine($"  Initial events: {InitialEventCount}");
            Console.WriteLine($"  Initial listeners: {InitialListenerCount}");
        });

        // STEP 2: Register an event.
        RunStep("02. Register event", () =>
        {
            LibAPI.RegisterEvent(EventName, MaxListeners);

            Check(
                LibAPI.GetTotalEvent() == InitialEventCount + 1,
                "Event count did not increase after registration."
            );
        });

        // STEP 3: Attempt to register the same event again.
        RunStep("03. Reject duplicate event registration", () =>
        {
            LibAPI.RegisterEvent(EventName, MaxListeners);

            Check(
                LibAPI.GetTotalEvent() == InitialEventCount + 1,
                "Duplicate registration changed the event count."
            );
        });

        // STEP 4: Register the first listener.
        RunStep("04. Register listener 1", () =>
        {
            LibAPI.ListenEvent(EventName, Listener1);

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount + 1,
                "Listener 1 was not registered."
            );
        });

        // STEP 5: Register the second listener.
        RunStep("05. Register listener 2", () =>
        {
            LibAPI.ListenEvent(EventName, Listener2);

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount + 2,
                "Listener 2 was not registered."
            );
        });

        // STEP 6: Test the maximum listener limit.
        RunStep("06. Enforce maximum listener limit", () =>
        {
            LibAPI.ListenEvent(EventName, Listener3);

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount + 2,
                "The library accepted more listeners than permitted."
            );

            Check(
                Listener3Calls == 0,
                "Listener 3 unexpectedly received a message."
            );
        });

        // STEP 7: Emit a JSON payload.
        RunStep("07. Emit event with JSON payload", () =>
        {
            var Payload = new TestPayload
            {
                Message = "Hello from integration test!",
                Number = 42
            };

            LibAPI.EmitEvent(EventName, Payload);

            Check(
                Listener1Calls == 1,
                $"Listener 1 expected 1 call, got {Listener1Calls}."
            );

            Check(
                Listener2Calls == 1,
                $"Listener 2 expected 1 call, got {Listener2Calls}."
            );

            Check(
                Listener3Calls == 0,
                "Listener 3 received an unexpected message."
            );

            Check(!string.IsNullOrWhiteSpace(Listener1Received),
                "Listener 1 received an empty payload.");

            Check(!string.IsNullOrWhiteSpace(Listener2Received),
                "Listener 2 received an empty payload.");

            Check(
                Listener1Received == Listener2Received,
                "Listeners received different JSON payloads."
            );

            TestPayload? ReceivedPayload =
                JsonSerializer.Deserialize<TestPayload>(
                    Listener1Received!,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            Check(ReceivedPayload is not null,
                "Received JSON could not be deserialized.");

            Check(
                ReceivedPayload!.Message == "Hello from integration test!",
                "The received Message value is incorrect."
            );

            Check(
                ReceivedPayload.Number == 42,
                "The received Number value is incorrect."
            );
        });

        // STEP 8: Remove listener 1.
        RunStep("08. Unlisten listener 1", () =>
        {
            LibAPI.UnlistenEvent(EventName, Listener1);

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount + 1,
                "Listener count did not decrease after unlistening."
            );
        });

        // STEP 9: Emit again and verify listener delivery.
        RunStep("09. Verify delivery after unlisten", () =>
        {
            int PreviousListener1Calls = Listener1Calls;
            int PreviousListener2Calls = Listener2Calls;

            LibAPI.EmitEvent(EventName, new TestPayload
            {
                Message = "Second message",
                Number = 99
            });

            Check(
                Listener1Calls == PreviousListener1Calls,
                "Listener 1 received a message after being removed."
            );

            Check(
                Listener2Calls == PreviousListener2Calls + 1,
                "Listener 2 did not receive the second message."
            );

            TestPayload? ReceivedPayload =
                JsonSerializer.Deserialize<TestPayload>(
                    Listener2Received!,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            Check(ReceivedPayload is not null,
                "Listener 2 received invalid JSON.");

            Check(
                ReceivedPayload!.Message == "Second message"
                    && ReceivedPayload.Number == 99,
                "Listener 2 received incorrect second-message data."
            );
        });

        // STEP 10: Remove the remaining listener.
        RunStep("10. Unlisten listener 2", () =>
        {
            LibAPI.UnlistenEvent(EventName, Listener2);

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount,
                "Listener count did not return to its initial value."
            );
        });

        // STEP 11: Verify that an event with no listeners delivers nothing.
        RunStep("11. Emit event without listeners", () =>
        {
            int PreviousListener1Calls = Listener1Calls;
            int PreviousListener2Calls = Listener2Calls;

            LibAPI.EmitEvent(EventName, new TestPayload
            {
                Message = "Nobody should receive this",
                Number = 123
            });

            Check(
                Listener1Calls == PreviousListener1Calls
                    && Listener2Calls == PreviousListener2Calls,
                "A removed listener received a message."
            );
        });

        // STEP 12: Unregister the event.
        RunStep("12. Unregister event", () =>
        {
            LibAPI.UnregisterEvent(EventName);

            Check(
                LibAPI.GetTotalEvent() == InitialEventCount,
                "Event count did not return to its initial value."
            );

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount,
                "Listeners remained after event unregistration."
            );
        });

        // STEP 13: Attempt to emit an unregistered event.
        RunStep("13. Reject emit on unregistered event", () =>
        {
            int PreviousListener1Calls = Listener1Calls;
            int PreviousListener2Calls = Listener2Calls;

            LibAPI.EmitEvent(EventName, new TestPayload
            {
                Message = "This event no longer exists",
                Number = -1
            });

            Check(
                Listener1Calls == PreviousListener1Calls
                    && Listener2Calls == PreviousListener2Calls,
                "A listener received data from an unregistered event."
            );
        });

        // STEP 14: Attempt to listen to an unregistered event.
        RunStep("14. Reject listener on unregistered event", () =>
        {
            LibAPI.ListenEvent(EventName, Listener3);

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount,
                "A listener was added to an unregistered event."
            );

            Check(
                Listener3Calls == 0,
                "Listener 3 unexpectedly received a message."
            );
        });

        // STEP 15: Verify final statistics.
        RunStep("15. Verify final statistics", () =>
        {
            Check(
                LibAPI.GetTotalEvent() == InitialEventCount,
                "Final event count differs from the initial count."
            );

            Check(
                LibAPI.GetTotalListen() == InitialListenerCount,
                "Final listener count differs from the initial count."
            );
        });

        PrintSummary();
    }

    private static void RunStep(string Name, Action Test)
    {
        Console.Write($"[RUN ] {Name} ... ");

        try
        {
            Test();

            Results.Add(new TestResult(Name, true, null));

            Console.WriteLine("PASS");
        }
        catch (Exception Exception)
        {
            Results.Add(new TestResult(
                Name,
                false,
                $"{Exception.GetType().Name}: {Exception.Message}"
            ));

            Console.WriteLine("FAIL");
            Console.WriteLine($"       Reason: {Exception.Message}");
        }

        Console.WriteLine();
    }

    private static void Check(bool Condition, string Message)
    {
        if (!Condition)
        {
            throw new InvalidOperationException(Message);
        }
    }

    private static void PrintSummary()
    {
        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine(" TEST SUMMARY");
        Console.WriteLine("==================================================");

        foreach (TestResult Result in Results)
        {
            Console.WriteLine(
                $"[{(Result.Passed ? "PASS" : "FAIL")}] {Result.Name}"
            );

            if (!Result.Passed)
            {
                Console.WriteLine($"       {Result.Error}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Total : {Results.Count}");
        Console.WriteLine($"Passed: {PassedCount}");
        Console.WriteLine($"Failed: {FailedCount}");

        Console.WriteLine();

        if (FailedCount == 0)
        {
            Console.WriteLine("RESULT: ALL TESTS PASSED!");
            Console.WriteLine("Your library survived the integration test. 🚀");
        }
        else
        {
            Console.WriteLine("RESULT: SOME TESTS FAILED!");
            Console.WriteLine("Review the failed steps above. 💀");
        }

        Console.WriteLine("==================================================");

        Environment.ExitCode = FailedCount == 0 ? 0 : 1;
    }
}
