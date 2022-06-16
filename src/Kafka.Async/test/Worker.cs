// Copyright (c) 2022 Moritz Rinow

namespace Kafka.Async.Test;

using Confluent.Kafka;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> logger;

  private readonly IConfiguration configuration;

  private readonly IProducer<string, string> producer;

  private readonly IAsyncConsumer<string, string> consumer;
  
  public Worker(ILogger<Worker> logger,
    IAsyncConsumer<string, string> consumer,
    IProducer<string, string> producer,
    IConfiguration configuration)
  {
    this.logger = logger;
    this.consumer = consumer;
    this.producer = producer;
    this.configuration = configuration;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    async Task ThreadFunc()
    {
      await foreach (ConsumeResult<string, string> result in this.consumer.AsAsyncEnumerable(stoppingToken).ConfigureAwait(false))
      {
        this.logger.LogInformation("Thread [{thread}] consumed message from {timestamp} Key: '{key}' Value: '{value}'", Environment.CurrentManagedThreadId, result.Message.Timestamp.UtcDateTime, result.Message.Key, result.Message.Value);
      }
    }

    string topic = this.configuration.GetValue<string>("topic");
    
    foreach (var i in Enumerable.Range(0, 10000))
    {
      this.producer.Produce(topic, new Message<string, string>
      {
        Key = $"Message key {i}",
        Value = $"Message value {i}"
      });
    }

    // Launch a couple workers on the thread pool consuming messages asynchronously
    IEnumerable<Task> threads = Enumerable.Range(0, 10).Select(_ => Task.Run(ThreadFunc, stoppingToken));

    try
    {
      await Task.WhenAll(threads).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
    }
  }
}