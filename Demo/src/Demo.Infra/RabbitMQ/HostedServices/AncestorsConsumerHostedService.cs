﻿using Demo.Domain.Entities;
using Demo.Infra.Contracts.Repository;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Infra.RabbitMQ.HostedServices
{
    // TODO: Verificar o ponto para implementar o BasicNack para retornar com a msg para a fila em caso de falha
    public class AncestorsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IAncestorsTreeRepository _ancestorsRepository;
        private readonly IOptions<RabbitMQSettings> _rabbitMQSettings;
        private IConnection _connection;
        private IModel _channel;

        public AncestorsConsumerHostedService(ILoggerFactory loggerFactory
            , IAncestorsTreeRepository ancestorsRepository
            , IOptions<RabbitMQSettings> rabbitMQSettings)
        {
            this._logger = loggerFactory.CreateLogger<AncestorsConsumerHostedService>();            
            _ancestorsRepository = ancestorsRepository;
            _rabbitMQSettings = rabbitMQSettings;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.Value.Hostname,
                UserName = "user",
                Password = "pass",
                VirtualHost = "challenge-dev"
            };

            // create connection
            _connection = factory.CreateConnection();

            // create channel
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("ancestors.exchange", ExchangeType.Topic);
            _channel.QueueDeclare("ancestors.queue", false, false, false, null);
            _channel.QueueBind("ancestors.queue", "ancestors.exchange", "ancestors.queue*", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message
                var body = ea.Body.ToArray();
                var content = System.Text.Encoding.UTF8.GetString(body);

                // handle the received message
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("ancestors.queue", false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
            _logger.LogInformation($"consumer received {content}");

            var responseAncestorsReport = JsonConvert.DeserializeObject<AncestorsTree>(content);
            _ancestorsRepository.AddAsync(responseAncestorsReport);
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"connection shut down {e.ReplyText}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer cancelled {e.ConsumerTags}");
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer unregistered {e.ConsumerTags}");
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation($"consumer registered {e.ConsumerTags}");
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"consumer shutdown {e.ReplyText}");
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
