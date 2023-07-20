using DmitryLegostaev.Polly.ActionRetry.Configuration;
using DmitryLegostaev.Polly.ActionRetry.Policies;
using DmitryLegostaev.Polly.ConditionalWait.Configuration;
using Microsoft.Extensions.Logging;

namespace DmitryLegostaev.Polly.ActionRetry;

public class ActionRetry : IActionRetry
{
    public ActionRetry(int? defaultRetryCount = null, TimeSpan? defaultBackOffDelay = null, ILogger? logger = null)
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

        this.logger = logger;
    }

    private const string DefaultRetryCountEnvironmentVariableName = $"{nameof(ActionRetry)}__{nameof(defaultRetryCount)}";
    private const string DefaultBackOffDelayEnvironmentVariableName = $"{nameof(ActionRetry)}__{nameof(defaultBackOffDelay)}";
    private readonly int defaultRetryCount;
    private readonly TimeSpan defaultBackOffDelay;
    private readonly ILogger? logger;

    public void DoWithRetry(Action action,
        int retryCount, TimeSpan backOffDelay,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null)
    {
        var waitConfiguration = InitActionRetryConfiguration(retryCount, backOffDelay);

        DoWithRetry(action, waitConfiguration, exceptionsToIgnore, failReason, codePurpose, logger);
    }

    public void DoWithRetry(Action action,
        IWaitConfiguration waitConfiguration,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null)
    {
        PollyPolicies
            .ActionRetryPolicy(waitConfiguration, exceptionsToIgnore, failReason, codePurpose, logger ?? this.logger)
            .Execute(action);
    }

    public T DoWithRetry<T>(Func<T> function,
        int retryCount, TimeSpan backOffDelay,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null)
    {
        var waitConfiguration = InitActionRetryConfiguration(retryCount, backOffDelay);

        return DoWithRetry(function, waitConfiguration, exceptionsToIgnore, failReason, codePurpose, logger);
    }

    public T DoWithRetry<T>(Func<T> function,
        IWaitConfiguration waitConfiguration,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null)
    {
        return PollyPolicies
            .ActionRetryPolicy(waitConfiguration, exceptionsToIgnore, failReason, codePurpose, logger ?? this.logger)
            .Execute(function);
    }

    private IWaitConfiguration InitActionRetryConfiguration(int? retryCount = null,
        TimeSpan? backoffDelay = null)
    {
        return new ActionRetryConfiguration(retryCount ?? defaultRetryCount, backoffDelay ?? defaultBackOffDelay);
    }
}
