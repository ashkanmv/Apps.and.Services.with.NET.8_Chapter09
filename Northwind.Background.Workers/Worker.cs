using System.Text.Json;
using Northwind.Queue.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Northwind.Background.Workers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string queueNameAndRoutingKey = "product";
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly EventingBasicConsumer _consumer;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _factory = new(){HostName = "localhost"};
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumer = new EventingBasicConsumer(_channel);

            _consumer.Received += (model, args) =>
            {
                byte[] body = args.Body.ToArray();
                ProductQueueMessage? message = JsonSerializer
                    .Deserialize<ProductQueueMessage>(body);
                if (message is not null)
                {
                    _logger.LogInformation($"Received product. Id: {message.Product.ProductId}, Name: {message.Product.ProductName}, Message: {message.Text}");
                }
                else
                {
                    _logger.LogInformation("Received unknown: {0}.",
                        args.Body.ToArray());
                }
            };
            // Start consuming as messages arrive in the queue.
            _channel.BasicConsume(queue: queueNameAndRoutingKey,
                autoAck: true, consumer: _consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
