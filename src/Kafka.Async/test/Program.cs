using Confluent.Kafka;
using Kafka.Async;
using Kafka.Async.Test;

IHost host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                   services.AddSingleton(provider =>
                   {
                     IConfiguration configuration = provider.GetRequiredService<IConfiguration>();

                     IProducer<string, string> producer = new ProducerBuilder<string, string>(configuration.GetSection("producer").Get<Dictionary<string, string>>()).Build();

                     return producer;
                   });
                   
                   services.AddSingleton(provider =>
                   {
                     IConfiguration configuration = provider.GetRequiredService<IConfiguration>();

                     IConsumer<string, string> consumer = new ConsumerBuilder<string, string>(configuration.GetSection("consumer").Get<Dictionary<string, string>>()).Build();
                     
                     consumer.Subscribe(configuration.GetValue<string>("topic"));

                     return consumer;
                   });

                   services.AddSingleton<IAsyncConsumer<string, string>>(provider =>
                   {
                     IConsumer<string, string> consumer = provider.GetRequiredService<IConsumer<string, string>>();

                     return new AsyncConsumer<string, string>(consumer);
                   });
                   
                   services.AddHostedService<Worker>();
                 })
                 .Build();

await host.RunAsync();