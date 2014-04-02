AsyncPrimitives
===============

Async concurrency primitives for .NET

##Async Semaphore
```c#
public class SomeClass
{
    private readonly _semaphore = new AsyncSemaphore(3 /* starting count */);
    
    public async Task DoSomethingAsync()
    {
        using(await _semaphore.WaitAndSignal())
        {
            // only 3 callers can run this at a time
            // ...
        }
    }
}
```

##Async ReaderWriterLock
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
