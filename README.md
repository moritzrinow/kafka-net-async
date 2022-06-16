# kafka-net-async
Async wrapper for Confluent's .NET Kafka consumer client.

## Installation
Nuget package: https://www.nuget.org/packages/Kafka.Async

Targets:
- netstandard2.1

## Example

```csharp
using Kafka.Async;

IConsumer<string, string> consumer = myConsumerInstance;

using IAsyncConsumer<string, string> asyncConsumer = consumer.CreateAsyncConsumer();

// Consume message asynchronously
ConsumeResult<string, string> message = await asyncConsumer.ConsumeAsync(cancellationToken).ConfigureAwait(false);

// Consume messages continuously
await foreach (ConsumeResult<string, string> result in asyncConsumer.AsAsyncEnumerable(stoppingToken).ConfigureAwait(false))
{
  // Handle message
}
```