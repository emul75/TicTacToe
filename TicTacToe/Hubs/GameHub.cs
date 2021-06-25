﻿using System;
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

        public Task HemloHub(string name)
        {
            //OnDisconnectedAsync(exception => Clients.OthersInGroup("enemyLeft"));

            return Clients.All.SendAsync("HemloJs", name);
        }

        public Task NotifyEnemy(string gameId)
        {
            return Clients.OthersInGroup("gameGroup" + gameId).SendAsync("enemyJoined", gameId);
        }

        public Task EnemyLeft(string gameId)
        {
            return Clients.OthersInGroup("gameGroup" + gameId).SendAsync("enemyLeft", gameId);
        }

        // public override Task OnDisconnectedAsync(Exception? exception)
        // {
        //     _service.LeaveGame(int.Parse(lastGameId));
        //     return Clients.OthersInGroup("gameGroup" + lastGameId).SendAsync("enemyLeft");
        // }

        public Task JoinGroup(string gameId)
        {
            Clients.Caller.SendAsync("consoleLog", "joining => gameGroup" + gameId);
            return Groups.AddToGroupAsync(Context.ConnectionId, "gameGroup" + gameId);
        }

        public Task MakeTurn(TurnDto dto)
        {
            TurnResult result = _service.MakeTurn(dto);
            return Clients.Group("gameGroup" + dto.GameId).SendAsync("turnResult", dto, result);
        }
    }
}