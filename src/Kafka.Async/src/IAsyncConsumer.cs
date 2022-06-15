namespace Kafka.Async;

using Confluent.Kafka;

/// <summary>
/// Interface providing asynchronous message consumption.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public interface IAsyncConsumer<TKey, TValue> : IDisposable
{
  /// <summary>
  /// Consumes the next message asynchronously.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns>Task that completes when a message was consumed or an error occured.</returns>
  Task<ConsumeResult<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken = default);
}