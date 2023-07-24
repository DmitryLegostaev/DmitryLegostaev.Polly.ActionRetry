using DmitryLegostaev.Polly.ConditionalWait.Configuration;
using Microsoft.Extensions.Logging;

namespace DmitryLegostaev.Polly.ActionRetry;

public interface IActionRetry
{
    public void DoWithRetry(Action action,
        int? retryCount = null, TimeSpan? backOffDelay = null, Action? actionOnRetry = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null,
        bool strictCheck = true);

    public void DoWithRetry(Action action,
        IWaitConfiguration waitConfiguration, Action? actionOnRetry = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null,
        bool strictCheck = true);

    public T DoWithRetry<T>(Func<T> function,
        int? retryCount = null, TimeSpan? backOffDelay = null, Action? actionOnRetry = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null,
        bool strictCheck = true);

    public T DoWithRetry<T>(Func<T> function,
        IWaitConfiguration waitConfiguration, Action? actionOnRetry = null,
        IList<Type>? exceptionsToHandle = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null,
        bool strictCheck = true);
}
