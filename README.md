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
