namespace ChatApplication.Hub
{
    using Microsoft.AspNetCore.SignalR;
    public class ChatHub : Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connection;
        public ChatHub(IDictionary<string, UserRoomConnection> connection)
        {
            _connection = connection;
        }
        public async Task JoinRoom(UserRoomConnection userConnection)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room!);
            _connection[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.Room!)
                .SendAsync("ReveiveMessage", "", $"{userConnection.User} has joind the group");
            await SendConnectedUsers(userConnection.Room!);
        }

        public async Task SendMessage(string message)
        {
            if (_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userRoomConnection))
            {
                await Clients.Group(userRoomConnection.Room!)
                        .SendAsync("ReveiveMessage", userRoomConnection.User, message, DateTime.Now);
            }
        }

        public Task SendConnectedUsers(string room)
        {
            var users = _connection.Values.Where(u => u.Room == room).Select(u => u.User);
            return Clients.Group(room).SendAsync("ConnectedUser", users);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (!_connection.TryGetValue(Context.ConnectionId, out UserRoomConnection userConnection))
                return base.OnDisconnectedAsync(exception);

            Clients.Group(userConnection.Room!)
                .SendAsync("ReveiveMessage", "", $"{userConnection.User} has left the group");
            SendConnectedUsers(userConnection.Room!);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
