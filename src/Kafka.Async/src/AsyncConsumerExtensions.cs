// Copyright (c) 2022 Moritz Rinow

namespace Kafka.Async;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Confluent.Kafka;

public static class AsyncConsumerExtensions
{
  /// <summary>
  /// Creates an asynchronous wrapper for the specified <see cref="IConsumer{TKey,TValue}"/>.
  /// For details see: <see cref="AsyncConsumer{TKey,TValue}"/>.
  /// </summary>
  /// <param name="consumer">The consumer instance to wrap.</param>
  /// <returns></returns>
  public static IAsyncConsumer<string, string> CreateAsyncConsumer(this IConsumer<string, string> consumer)
    => new AsyncConsumer<string, string>(consumer);

  /// <summary>
  /// Returns an <see cref="IAsyncEnumerable{T}"/> representing continuous asynchronous message consumption. 
  /// </summary>
  /// <param name="consumer">The async consumer.</param>
  /// <param name="cancellationToken">Token used for cancelling consumption.</param>
  /// <typeparam name="TKey">Type of the message key.</typeparam>
  /// <typeparam name="TValue">Type of the message value.</typeparam>
  /// <returns></returns>
  public static async IAsyncEnumerable<ConsumeResult<TKey, TValue>> AsAsyncEnumerable<TKey, TValue>(this IAsyncConsumer<TKey, TValue> consumer, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      yield return await consumer.ConsumeAsync(cancellationToken).ConfigureAwait(false);
    }
  }
}