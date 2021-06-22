using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
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
        
        public void MakeTurn(TurnDto dto)
        {
        }
    }
}