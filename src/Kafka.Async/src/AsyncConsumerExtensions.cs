namespace Kafka.Async;

using System.Runtime.CompilerServices;
using Confluent.Kafka;

public static class AsyncConsumerExtensions
{
  public static async IAsyncEnumerable<ConsumeResult<TKey, TValue>> AsAsyncEnumerable<TKey, TValue>(this IAsyncConsumer<TKey, TValue> consumer, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      yield return await consumer.ConsumeAsync(cancellationToken).ConfigureAwait(false);
    }
  }
}