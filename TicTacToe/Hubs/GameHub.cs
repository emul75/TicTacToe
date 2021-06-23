using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Enums;
using TicTacToe.Models;
using TicTacToe.Services;

namespace TicTacToe.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService _service;

        public GameHub(IGameService service)
        {
            _service = service;
        }

        public string enemyId { get; set; }

        public Task HemloHub(string name)
        {
            return Clients.All.SendAsync("HemloJs", name);
        }


        public Task GetUserConnectionId()
        {
            string connectionId = Context.ConnectionId;
            return Clients.Caller.SendAsync("GetUserConnectionId", connectionId);
        }

        public Task NotifyEnemy(string gameId)
        {
            return Clients.OthersInGroup("gameGroup" + gameId).SendAsync("enemyJoined", gameId);
        }

        public Task JoinGroup(string gameId)
        {
            Clients.Caller.SendAsync("alert", "joining => gameGroup" + gameId);
            return Groups.AddToGroupAsync(Context.ConnectionId, "gameGroup" + gameId);
        }

        public Task MakeTurn(TurnDto dto)
        {
            _service.MakeTurn(dto);
            return Clients.OthersInGroup("gameGroup" + dto.GameId).SendAsync("enemyMove", dto);
        }
    }
}