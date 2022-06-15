namespace Kafka.Async.Test;

using Confluent.Kafka;

public class Worker : BackgroundService
{
  private readonly ILogger<Worker> logger;

  private readonly IAsyncConsumer<string, string> consumer;

  public Worker(ILogger<Worker> logger,
    IAsyncConsumer<string, string> consumer)
  {
    this.logger = logger;
    this.consumer = consumer;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      ConsumeResult<string, string> result = await this.consumer.ConsumeAsync(stoppingToken).ConfigureAwait(false);
      
      this.logger.LogInformation("Consumed message from {timestamp}\nKey: '{key}'\nValue: '{value}'", result.Message.Timestamp.UtcDateTime, result.Message.Key, result.Message.Value);
    }
  }
}