// Copyright (c) 2022 Moritz Rinow

namespace Kafka.Async;

using System.Threading.Tasks;
using Confluent.Kafka;

/// <summary>
/// Asynchronous wrapper for <see cref="IConsumer{TKey,TValue}"/> instances.
/// A dedicated thread is launched in the background waiting on consumptions to be "requested".
/// <see cref="ConsumeResult{TKey,TValue}"/>s are written back to <see cref="TaskCompletionSource{TResult}"/>s being awaited on the consumer side.
/// Messages are not buffered internally 
/// The lifetime of the wrapped consumer instance is managed externally.
/// Disposing an instance of this type will not dispose the wrapped consumer instance.
/// <see cref="AsyncConsumer{TKey,TValue}"/> instances should be disposed before the wrapped consumer instances.
/// There can be multiple <see cref="AsyncConsumer{TKey,TValue}"/> instances wrapping one <see cref="IConsumer{TKey,TValue}"/> instance,
/// just like there can be multiple threads calling <see cref="IConsumer{TKey,TValue}.Consume(CancellationToken)"/>.
/// There can be multiple threads consuming from the same <see cref="IAsyncConsumer{TKey,TValue}"/> instance.
/// </summary>
/// <typeparam name="TKey">Type of the message key.</typeparam>
/// <typeparam name="TValue">Type of the message value.</typeparam>
public class AsyncConsumer<TKey, TValue> : IAsyncConsumer<TKey, TValue>
{
  private readonly SemaphoreSlim consumerSemaphore = new(1, 1);

  private readonly CancellationTokenSource disposeCts = new();

  private readonly object disposeLock = new();

  private readonly IConsumer<TKey, TValue> instance;

  private readonly Thread thread;

  private readonly SemaphoreSlim threadSemaphore = new(0, 1);

  private CancellationToken ct = CancellationToken.None;
  
  private bool disposed;

  private TaskCompletionSource<ConsumeResult<TKey, TValue>>? tcs;

  public AsyncConsumer(IConsumer<TKey, TValue> instance)
  {
    this.instance = instance;

    this.thread = new Thread(this.Run)
    {
      IsBackground = true
    };

    this.thread.Start();
  }

  public async Task<ConsumeResult<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken = default)
  {
    await this.consumerSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

    this.ct = cancellationToken;

    this.tcs = new TaskCompletionSource<ConsumeResult<TKey, TValue>>(TaskCreationOptions.RunContinuationsAsynchronously);

    this.threadSemaphore.Release();

    try
    {
      ConsumeResult<TKey, TValue> result = await this.tcs.Task.ConfigureAwait(false);
      
      return result;
    }
    finally
    {
      this.consumerSemaphore.Release();
    }
  }

  public void Dispose()
  {
    lock (this.disposeLock)
    {
      if (this.disposed)
      {
        return;
      }

      this.disposed = true;
    }
    
    this.disposeCts.Cancel();
    
    this.thread.Join();
    
    this.consumerSemaphore.Dispose();

    this.threadSemaphore.Dispose();
    
    this.disposeCts.Dispose();
  }

  private void Run()
  {
    while (true)
    {
      if (this.disposed)
      {
        break;
      }

      try
      {
        this.threadSemaphore.Wait(this.disposeCts.Token);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (ObjectDisposedException)
      {
        break;
      }
      
      if (this.disposed)
      {
        break;
      }

      // Should never be needed
      this.tcs ??= new TaskCompletionSource<ConsumeResult<TKey, TValue>>(TaskCreationOptions.RunContinuationsAsynchronously);

      using CancellationTokenSource localCts = CancellationTokenSource.CreateLinkedTokenSource(this.ct, this.disposeCts.Token);

      try
      {
        ConsumeResult<TKey, TValue> result = this.instance.Consume(localCts.Token);
        
        this.tcs.SetResult(result);
      }
      catch (OperationCanceledException ex)
      {
        if (this.disposed)
        {
          this.tcs.SetException(new ObjectDisposedException(nameof(AsyncConsumer<TKey, TValue>)));

          break;
        }
        
        this.tcs.TrySetCanceled(ex.CancellationToken);
      }
      catch (Exception ex)
      {
        this.tcs.SetException(ex);
      }
    }
  }
}