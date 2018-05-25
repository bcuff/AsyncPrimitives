AsyncPrimitives
===============

Async concurrency primitives for .NET

## Async Semaphore

### Methods

#### Wait
`public Task Wait()`
Waits until the count is greater than 0 then enters the semaphore. Decrements the count upon entry.

#### WaitAndRelease
`public Task<IDisposable> WaitAndRelease()`
Waits (as above) then returns an `IDisposable`. When the return value is disposed `void Release()` is called.

#### Release
`public void Release(int count = 1)`
Increments the count by the specified number and releases the corresponding number of waiters (if there are any waiters).

```c#
public class SomeClass
{
    private readonly _semaphore = new AsyncSemaphore(3 /* starting count */);
    
    public async Task DoSomethingAsync()
    {
        using(await _semaphore.WaitAndRelease())
        {
            // only 3 callers can run this at a time
            // ...
        }
    }
}
```

## Async ReaderWriterLock
```c#
_gate = new AsyncReaderWriterLock();

public async Task ModifyResource()
{
    using(await _gate.OpenWriter())
    {
        // ... do something to modify a protected resource ...
    }
}

public async Task<string> GetResource()
{
    using(await _gate.OpenReader())
    {
        // ... do something to obtain a protected resource ...
    }
}
```

## AsyncLazy&lt;T&gt;
```c#
var lazy = new AsyncLazy<string>(async () =>
{
	using(var client = new WebClient())
	{
		return await client.DownloadStringTaskAsync("http://google.com");
	}
});

// first time actually downloads http://google.com
var html = await lazy;

// second time gets it from memory
var html2 = await lazy;
```
