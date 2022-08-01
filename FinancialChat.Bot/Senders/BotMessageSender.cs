using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers;
using FinancialChat.Core.Helpers.Messaging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace FinancialChat.Bot.Senders
{
    public class BotMessageSender : IMessageSender
    {
        private readonly RabbitConfiguration _rabbitMqOptions;
        private IConnection _connection;

        public BotMessageSender(RabbitConfiguration rabbitMqOptions)
        {
            _rabbitMqOptions = rabbitMqOptions;
            CreateRabbitConnection();
        }

        private void CreateRabbitConnection()
        {
            try
            {
                ConnectionFactory factory = new()
                {
                    HostName = _rabbitMqOptions.HostName,
                    UserName = _rabbitMqOptions.Username,
                    Password = _rabbitMqOptions.Password
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create connection: {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection is not null)
                return true;

            CreateRabbitConnection();

            return _connection is not null;
        }

        public void SendMessage(ChatMessage message)
        {
            if (!ConnectionExists())
                return;

            using IModel channel = _connection.CreateModel();
            channel.QueueDeclare(queue: _rabbitMqOptions.QueueName, durable: false, exclusive: false,
                autoDelete: false, arguments: null);

            string json = JsonConvert.SerializeObject(message);
            byte[] body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: _rabbitMqOptions.QueueName, basicProperties: null,
                body: body);
        }
    }
}