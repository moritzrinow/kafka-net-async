// Copyright (c) 2022 Moritz Rinow

namespace Kafka.Async;

using Confluent.Kafka;

/// <summary>
/// Interface providing asynchronous message consumption.
/// </summary>
/// <typeparam name="TKey">Type of the message key.</typeparam>
/// <typeparam name="TValue">Type of the message value.</typeparam>
public interface IAsyncConsumer<TKey, TValue> : IDisposable
{
  /// <summary>
  /// Consumes the next message asynchronously.
  /// </summary>
  /// <param name="cancellationToken">Token used for cancelling consumption.</param>
  /// <exception cref="OperationCanceledException">cancellationToken was cancelled.</exception>
  /// <exception cref="ConsumeException">Other consumption error.</exception>
  /// <exception cref="ObjectDisposedException">Instance was disposed.</exception>
  /// <returns>Task that completes when a message was consumed, an error occured or a cancellation was triggered.</returns>
  Task<ConsumeResult<TKey, TValue>> ConsumeAsync(CancellationToken cancellationToken = default);
}