using FinancialChat.Core.Helpers;
using FinancialChat.Core.Helpers.Messaging;
using FinancialChat.Messaging.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialChat.Messaging.Receivers
{
    public class ChatMessageReceiver : BackgroundService
    {
        private readonly RabbitConfiguration _rabbitMqOptions;
        private readonly IHubContext<ChatHub> _hubContext;
        private IConnection _connection;
        private IModel _channel;

        public ChatMessageReceiver(RabbitConfiguration rabbitMqOptions, IHubContext<ChatHub> hubContext)
        {
            _rabbitMqOptions = rabbitMqOptions;
            _hubContext = hubContext;

            InitializeRabbitListener();
        }

        private void InitializeRabbitListener()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqOptions.HostName,
                UserName = _rabbitMqOptions.Username,
                Password = _rabbitMqOptions.Password
            };

            _connection = factory.CreateConnection();
            _connection.ConnectionShutdown += ConnectionShutdown;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _rabbitMqOptions.ListenToQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        private void ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        private async Task HandleMessage(ChatMessage message)
        {
            await _hubContext.Clients.All.SendAsync("NewMessage", message);
        }

        private void OnConsumerCancelled(object sender, ConsumerEventArgs e) { }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var chatMessage = JsonConvert.DeserializeObject<ChatMessage>(content);

                await HandleMessage(chatMessage);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerCancelled;

            _channel.BasicConsume(_rabbitMqOptions.ListenToQueueName, false, consumer);

            return Task.CompletedTask;
        }
    }
}
