using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers;
using FinancialChat.Core.Helpers.Messaging;
using FinancialChat.Core.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialChat.Bot.Receivers
{
    public class BotMessageReceiver : BackgroundService
    {
        private readonly RabbitConfiguration _rabbitMqOptions;
        private readonly IBotService _botService;
        private readonly IMessageSender _messageSender;
        private IConnection _connection;
        private IModel _channel;

        public BotMessageReceiver(RabbitConfiguration rabbitMqOptions, IBotService botService,
            IMessageSender messageSender)
        {
            _rabbitMqOptions = rabbitMqOptions;
            _botService = botService;
            _messageSender = messageSender;

            InitializeRabbitListener();
        }

        private void InitializeRabbitListener()
        {
            ConnectionFactory factory = new()
            {
                HostName = _rabbitMqOptions.HostName,
                UserName = _rabbitMqOptions.Username,
                Password = _rabbitMqOptions.Password,
            };

            _connection = factory.CreateConnection();
            _connection.ConnectionShutdown += ConnectionShutdown;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _rabbitMqOptions.ListenToQueueName, durable: false, exclusive: false,
                autoDelete: false, arguments: null);
        }

        private void ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void SendBotMessage(ChatMessage message)
        {
            ChatMessage defaultMessage = new()
            {
                SentBy = "Financial Bot",
                SentOn = DateTime.Now,
                Message = "Could not get stock quote."
            };

            try
            {
                StockQuote quote = _botService.GetStockQuote(message);

                if (quote is null)
                {
                    _messageSender.SendMessage(defaultMessage);
                    return;
                }

                _messageSender.SendMessage(new ChatMessage
                {
                    SentBy = "Financial Bot",
                    SentOn = DateTime.Now,
                    Message = $"{quote.Symbol} quote is ${quote.Close} per share."
                });
            }
            catch (Exception)
            {
                _messageSender.SendMessage(defaultMessage);
            }
        }

        private void OnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (_, eventArgs) =>
            {
                string content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                ChatMessage chatMessage = JsonConvert.DeserializeObject<ChatMessage>(content);

                SendBotMessage(chatMessage);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
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