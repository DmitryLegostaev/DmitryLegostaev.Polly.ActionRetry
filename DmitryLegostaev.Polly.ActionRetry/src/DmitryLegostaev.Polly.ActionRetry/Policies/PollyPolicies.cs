using DmitryLegostaev.Polly.ConditionalWait.Configuration;
using DmitryLegostaev.Polly.ConditionalWait.Utilities;
using DmitryLegostaev.Polly.HandleFromList.Utilities;
using Humanizer;
using Microsoft.Extensions.Logging;
using Polly;

namespace DmitryLegostaev.Polly.ActionRetry.Policies;

public static class PollyPolicies
{
    public static Policy ActionRetryPolicy(
        IWaitConfiguration actionRetryConfiguration,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null,
        bool strictCheck = true)
    {
        var handleResultPolicyBuilder = PolicyBuilderUtilities.HandleFromList(exceptionsToIgnore);

        var waitAndRetryPolicy = handleResultPolicyBuilder
            .WaitAndRetry(
                BackoffUtilities.CalculateBackoff(actionRetryConfiguration),
                (exception, timeSpan, i, context) =>
                {
                    // Do not handle exceptions during last retry
                    if (i < actionRetryConfiguration.RetryCount + 1)
                    {
                        logger?.LogDebug(
                            "An exception {ExceptionType} has occured during action execution. Retry #{RetryAttempt} (Execution #{ExecutionAttempt}): {CodePurpose}",
                            exception.GetType(), i, i + 1, codePurpose);
                        return;
                    }

                    // Do nothing if there are no exceptions during last retry
                    if (exception is null) return;

                    logger?.LogDebug(
                        "ActionRetry ran out of {RetryCount} retries with {BackOffDelay} BackOff Delay, {BackOffType} BackOff Type, and {BackOffFactor} BackOff Factor. Fail reason: {FailReason}. Executed code purpose: {CodePurpose}",
                        actionRetryConfiguration.RetryCount, actionRetryConfiguration.BackOffDelay.Humanize(),
                        actionRetryConfiguration.BackoffType, actionRetryConfiguration.Factor, failReason ?? "Not Specified",
                        codePurpose ?? "Not Specified");

                    // Re-throw the occured exception if strictCheck is true
                    if (strictCheck)
                    {
                        logger?.LogDebug(
                            "An exception {ExceptionType} has occured during last retry #{RetryCount} attempt with {ExceptionMessage} message, re-throwing the exception",
                            exception.GetType(), i, exception.Message);
                        throw exception;
                    }

                    // Log warning if strictCheck is false
                    logger?.LogWarning(
                        "An exception {Exception} has occured during last retry attempt with {ExceptionMessage} message, stack trace: {ExceptionStackTrace}",
                        exception, exception.Message, exception.StackTrace);
                });

        return waitAndRetryPolicy;
    }
}
