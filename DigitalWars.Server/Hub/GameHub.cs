using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    public class GameHub : Hub
    {
        // Metoda, którą klient wywołuje, aby dołączyć do pokoju gry
        public async Task JoinGameRoomAsAdmin(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
        }

        // Metoda, którą klient wywołuje, aby opuścić pokój
        public async Task LeaveGameRoomAsAdmin(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game-{gameId}");
        }

        public async Task JoinGameRoomAsPlayer(string gameId, string teamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}-team-{teamId}");
        }

        public async Task LeaveGameRoomAsPlayer(string gameId, string teamId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game-{gameId}-team-{teamId}");
        }
    }
}
