using FinancialChat.Core.Contracts;
using FinancialChat.Core.Helpers.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialChat.Messaging.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageSender _messageSender;

        private static IList<ChatUser> _chatUsers = new List<ChatUser>();
        private static IList<ChatMessage> _messages = new List<ChatMessage>();

        public ChatHub(IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }

        public override Task OnConnectedAsync()
        {
            string userName = Context.User.Identity?.Name;
            string connectionId = Context.ConnectionId;

            if (!_chatUsers.Any(connectedUser => connectedUser.Username == userName))
            {
                _chatUsers.Add(new ChatUser
                {
                    ConnectionId = connectionId,
                    Username = userName, 
                    CurrentRoom = "Teams"
                });

                Groups.AddToGroupAsync(Context.ConnectionId, "Teams");
                var roomUsers = _chatUsers.Where(user => user.CurrentRoom == "Teams").ToList();

                Clients.Caller.SendAsync("ChatUsersChanged", roomUsers);
            }
            else
            {
                Clients.Caller.SendAsync("ChatUsersChanged", _chatUsers);
            }

            var user = _chatUsers.FirstOrDefault(connectedUser => connectedUser.Username == userName);

            Clients.Caller.SendAsync("CurrentMessages", _messages.Where(message => message.Room == user.CurrentRoom));

            return base.OnConnectedAsync();
        }

        private void AddNewMessage(ChatMessage message)
        {
            _messages.Add(message);

            if (_messages.Count > 50)
                _messages.RemoveAt(0);
        }

        
        public async Task Join(string roomName)
        {
            switch (roomName)
            {
                case "Slack":
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Slack");
                    break;
                case "Discord":
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Discord");
                    break;
                default:
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Teams");
                    break;
            }

            var user = _chatUsers.FirstOrDefault(connectedUser => connectedUser.Username == Context.User.Identity?.Name);
            user.CurrentRoom = roomName;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("NewMessage", new ChatMessage { Message = $"{user.Username} joined.", SentBy = "", SentOn = System.DateTime.Now });
            await Clients.Caller.SendAsync("CurrentMessages", _messages.Where(message => message.Room == user.CurrentRoom));
        }


        public async Task SendMessage(ChatMessage message)
        {
            var user = _chatUsers.FirstOrDefault(connectedUser => connectedUser.Username == Context.User.Identity?.Name);
            message.Room = user.CurrentRoom;
            await Clients.Group(user.CurrentRoom).SendAsync("NewMessage", message);

            if (message.Message.Contains("/stock="))
            {
                _messageSender.SendMessage(message);
                return;
            }

            AddNewMessage(message);
        }

        public void SendBotMessage(ChatMessage message)
        {
            AddNewMessage(message);
        }

        public async Task Disconnect(string userName)
        {
            if (_chatUsers.Any(currentUser => currentUser.Username == userName))
            {
                _chatUsers = _chatUsers.Where(currentUser => currentUser.Username != userName).ToList();
                await Clients.All.SendAsync("ChatUsersChanged", _chatUsers);
            }
        }
    }
}
