using DmitryLegostaev.Polly.ConditionalWait.Configuration;
using Microsoft.Extensions.Logging;

namespace DmitryLegostaev.Polly.ActionRetry;

public interface IActionRetry
{
    public void DoWithRetry(Action action,
        int retryCount, TimeSpan backOffDelay,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null);

    public void DoWithRetry(Action action,
        IWaitConfiguration waitConfiguration,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null);

    public T DoWithRetry<T>(Func<T> function,
        int retryCount, TimeSpan backOffDelay,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null);

    public T DoWithRetry<T>(Func<T> function,
        IWaitConfiguration waitConfiguration,
        IList<Type>? exceptionsToIgnore = null, string? failReason = null, string? codePurpose = null, ILogger? logger = null);
}
