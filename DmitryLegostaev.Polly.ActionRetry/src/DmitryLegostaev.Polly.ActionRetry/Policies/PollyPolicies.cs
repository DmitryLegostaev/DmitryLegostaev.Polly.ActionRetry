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
        IWaitConfiguration actionRetryConfiguration, Action? actionOnRetry = null, Action? resetAction = null,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null,
        bool strictCheck = true)
    {
        var handleResultPolicyBuilder = PolicyBuilderUtilities.HandleFromList(exceptionsToIgnore);

        var waitAndRetryPolicy = handleResultPolicyBuilder
            .WaitAndRetry(
                BackoffUtilities.CalculateBackoff(actionRetryConfiguration),
                (exception, timeSpan, i, context) =>
                {
                    actionOnRetry?.Invoke();
                    logger?.LogDebug(
                        "An exception {ExceptionType} has occured during action execution. Retry #{RetryAttempt} (Execution #{ExecutionAttempt}): {CodePurpose}",
                        exception.GetType(), i, i + 1, codePurpose);
                });

        var aggregateLastRetryResult = handleResultPolicyBuilder
            .Fallback(((exception, context, arg3) =>
            {
                if (strictCheck)
                {
                    throw new AggregateException(
                        $"An exception {exception.GetType()} has occured during Retry #{actionRetryConfiguration.RetryCount} attempt with {exception.Message} message",
                        exception);
                }

                // Log warning if strictCheck is false
                logger?.LogWarning(
                    "An exception {Exception} has occured during Retry #{RetryAttempt} attempt with {ExceptionMessage} message, stack trace: {ExceptionStackTrace}",
                    exception, actionRetryConfiguration.RetryCount, exception.Message, exception.StackTrace);
            }), (exception, context) =>
            {
                if (resetAction is not null)
                {
                    logger?.LogDebug("resetAction is not null. Invoking resetAction");
                    resetAction?.Invoke();
                }

                logger?.LogDebug(
                    "ActionRetry ran out of {RetryCount} retries with {BackOffDelay} BackOff Delay, {BackOffType} BackOff Type, and {BackOffFactor} BackOff Factor. Fail reason: {FailReason}. Executed code purpose: {CodePurpose}",
                    actionRetryConfiguration.RetryCount, actionRetryConfiguration.BackOffDelay.Humanize(),
                    actionRetryConfiguration.BackoffType, actionRetryConfiguration.Factor, failReason ?? "Not Specified",
                    codePurpose ?? "Not Specified");
            });

        return aggregateLastRetryResult.Wrap(waitAndRetryPolicy);
    }
}
