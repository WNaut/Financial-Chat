using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers;
using FinancialChat.Core.Helpers.Messaging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace FinancialChat.Messaging.Senders
{
    public class ChatMessageSender : IMessageSender
    {
        private readonly RabbitConfiguration _rabbitMqOptions;
        private IConnection _connection;

        public ChatMessageSender(RabbitConfiguration rabbitMqOptions)
        {
            _rabbitMqOptions = rabbitMqOptions;

            CreateConnection();
        }

        private void CreateConnection()
        {
            try
            { 
                var factory = new ConnectionFactory
                {
                    HostName = _rabbitMqOptions.HostName,
                    UserName = _rabbitMqOptions.Username,
                    Password = _rabbitMqOptions.Password,
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

            CreateConnection();

            return _connection is not null;
        }

        public void SendMessage(ChatMessage message)
        {
            if (!ConnectionExists())
                return;

            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: _rabbitMqOptions.QueueName, durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: _rabbitMqOptions.QueueName, basicProperties: null,
                body: body);
        }
    }
}