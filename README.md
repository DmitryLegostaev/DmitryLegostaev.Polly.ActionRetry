## DmitryLegostaev.Polly.ActionRetry

A small class library to provide Action Retry functionality using Polly v7 library.

### Usage
#### Obtaining ActionRetry object instance
Explicitly create ActionRetry object
```csharp
var actionRetrier = new ActionRetry();
```
or use a DI to obtain ActionRetry object.


#### (optional) Configuration of default RetryCount and BackOffDelay
You can configure default retry count and backoffdelay for ActionRetry object through its constructor. Defaults are 5/1s
```csharp
var actionRetrier = new ActionRetry(20, TimeSpan.FromSeconds(2));
```
or set environment variables
```csharp
Environment.SetEnvironmentVariable("ActionRetry__defaultRetryCount", 20.ToString());
Environment.SetEnvironmentVariable("ActionRetry__defaultBackOffDelay", TimeSpan.FromSeconds(2).ToString());
```
Configuration priority is: Constructor parameters -> Environment variables -> Pre-defined defaults (5/1s)

#### Actual usage
Each method require Action or Func to execute within its body. WaitForPredicateAndGetResult requires a predicate to be passed as argument in addition to Func.
```csharp
// Do action with default amount of retries and default back off delay
actionRetrier.DoWithRetry(() => 2 + 2 == 4);

// Do action with default amount of retries and default back off delay and return action result
var actionResult = actionRetrier.DoWithRetry<T>(() => 2 + 2);
```

Each method could consume IWaitConfiguration object or RetryCount/BackOffDelay. 
If 2nd option is chosen but RetryCount or BackOffDelay is missing, then it will be obtained from ActionRetry defaults defined during ActionRetry object instantiation.
```csharp
actionRetrier.DoWithRetry(() => 2 + 2 == 4,
    new ActionRetryConfiguration(20, TimeSpan.FromSeconds(2)));

actionRetrier.DoWithRetry(() => 2 + 2 == 4,
    20, TimeSpan.FromSeconds(2));
    
actionRetrier.DoWithRetry(() => 2 + 2 == 4,
    retryCount: 20);
    
actionRetrier.DoWithRetry(() => 2 + 2 == 4,
    backoffDelay: TimeSpan.FromSeconds(2));
```

Also ActionRetry methods consume some optional arguments:

| Argument name      | Type          | Purpose                                                                                              |
|--------------------|---------------|------------------------------------------------------------------------------------------------------|
| exceptionsToIgnore | IList\<Type\> | List with Exception types (derived from System.Exception) to be ignored during ActionRetry execution |
| failReason         | string        | String to be added to ActionRetry timed out exception message                                    |
| codePurpose        | string        | String to be added to each retry attempt message (doesn't work without logger)                       |
| logger             | ILogger       | Microsoft.Extensions.Logging object to add debug outputs during ActionRetry execution            |

#### ActionRetryConfiguration
To create ActionRetryConfiguration object you should pass int RetryCount and TimeSpan BackOffDelay to its constructor.
Also you can set Factor and BackoffType to customize main ActionRetry behaviour based on WaitAndRetry policy.

ActionRetryConfiguration could be mapped from .NET Configuration by Microsoft.Extensions.Configuration.ConfigurationBinder.

To understand more about ActionRetryConfiguration capabilities visit https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry

