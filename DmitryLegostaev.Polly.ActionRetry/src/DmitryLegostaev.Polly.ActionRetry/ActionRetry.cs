using DmitryLegostaev.Polly.ActionRetry.Configuration;
using DmitryLegostaev.Polly.ActionRetry.Policies;
using DmitryLegostaev.Polly.ConditionalWait.Configuration;
using Microsoft.Extensions.Logging;

namespace DmitryLegostaev.Polly.ActionRetry;

public class ActionRetry : IActionRetry
{
    public ActionRetry(int? defaultRetryCount = null, TimeSpan? defaultBackOffDelay = null, IList<Type>? exceptionsToHandle = null, ILogger? logger = null)
    {
        TimeSpan GetTimeSpanValue(TimeSpan? initialTimeSpanValue, string environmentVariableName, TimeSpan defaultTimeSpan) =>
            initialTimeSpanValue ??
            (TimeSpan.TryParse(Environment.GetEnvironmentVariable(environmentVariableName),
                out var parsedTimeSpanFromEnvironmentVariables)
                ? parsedTimeSpanFromEnvironmentVariables
                : defaultTimeSpan);

        int GetIntValue(int? initialIntValue, string environmentVariableName, int defaultInt) =>
            initialIntValue ??
            (int.TryParse(Environment.GetEnvironmentVariable(environmentVariableName),
                out var parsedTimeSpanFromEnvironmentVariables)
                ? parsedTimeSpanFromEnvironmentVariables
                : defaultInt);

        this.defaultRetryCount = GetIntValue(defaultRetryCount, DefaultRetryCountEnvironmentVariableName, 5);

        this.defaultBackOffDelay =
            GetTimeSpanValue(defaultBackOffDelay, DefaultBackOffDelayEnvironmentVariableName, TimeSpan.FromSeconds(1));

        this.exceptionsToHandle = exceptionsToHandle;

        this.logger = logger;
    }

    private const string DefaultRetryCountEnvironmentVariableName = $"{nameof(ActionRetry)}__{nameof(defaultRetryCount)}";
    private const string DefaultBackOffDelayEnvironmentVariableName = $"{nameof(ActionRetry)}__{nameof(defaultBackOffDelay)}";
    private readonly int defaultRetryCount;
    private readonly TimeSpan defaultBackOffDelay;
    private readonly ILogger? logger;
    private readonly IList<Type>? exceptionsToHandle;

    public void DoWithRetry(Action action,
        int? retryCount = null, TimeSpan? backOffDelay = null, Action? actionOnRetry = null, Action? resetAction = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null, bool strictCheck = true)
    {
        var waitConfiguration = InitActionRetryConfiguration(retryCount, backOffDelay);

        DoWithRetry(action, waitConfiguration, actionOnRetry, resetAction, exceptionsToHandle ?? this.exceptionsToHandle, failReason, codePurpose, logger, strictCheck);
    }

    public void DoWithRetry(Action action,
        IWaitConfiguration waitConfiguration, Action? actionOnRetry = null, Action? resetAction = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null, bool strictCheck = true)
    {
        PollyPolicies
            .ActionRetryPolicy(waitConfiguration, actionOnRetry, resetAction, exceptionsToHandle ?? this.exceptionsToHandle, failReason, codePurpose, logger ?? this.logger, strictCheck)
            .Execute(action);
    }

    public T DoWithRetry<T>(Func<T> function,
        int? retryCount = null, TimeSpan? backOffDelay = null, Action? actionOnRetry = null, Action? resetAction = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null, bool strictCheck = true)
    {
        var waitConfiguration = InitActionRetryConfiguration(retryCount, backOffDelay);

        return DoWithRetry(function, waitConfiguration, actionOnRetry, resetAction, exceptionsToHandle ?? this.exceptionsToHandle, failReason, codePurpose, logger, strictCheck);
    }

    public T DoWithRetry<T>(Func<T> function,
        IWaitConfiguration waitConfiguration, Action? actionOnRetry = null, Action? resetAction = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null, bool strictCheck = true)
    {
        return PollyPolicies
            .ActionRetryPolicy(waitConfiguration, actionOnRetry, resetAction, exceptionsToHandle ?? this.exceptionsToHandle, failReason, codePurpose, logger ?? this.logger, strictCheck)
            .Execute(function);
    }

    private IWaitConfiguration InitActionRetryConfiguration(int? retryCount = null,
        TimeSpan? backoffDelay = null)
    {
        return new ActionRetryConfiguration(retryCount ?? defaultRetryCount, backoffDelay ?? defaultBackOffDelay);
    }
}
