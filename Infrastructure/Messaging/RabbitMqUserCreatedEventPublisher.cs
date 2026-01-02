using Application.Interfaces.Messaging;
using Domain.Events;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Messaging
{
    public class RabbitMqUserCreatedEventPublisher : IUserCreatedEventPublisher, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _queueName = "user-created";

        public RabbitMqUserCreatedEventPublisher(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"] ?? "rabbitmq",
                UserName = config["RabbitMQ:User"] ?? "user",
                Password = config["RabbitMQ:Pass"] ?? "pass"
            };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }
        public async Task PublishAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(@event);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = (DeliveryModes)2
            };
            // aca se publica al exchange
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: _queueName,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);
        }
        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
