using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Web;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Entities;
using TicTacToe.Enums;
using TicTacToe.Hubs;
using TicTacToe.Models;
using TicTacToe.Services;

namespace TicTacToe.Controllers
{
    [Controller]
    [Route("api")]
    public class GameController : Controller
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameService _gameService;
        private readonly TicTacToeDbContext _dbContext;

        public GameController(ILogger<GameController> logger, IGameService gameService, TicTacToeDbContext dbContext)
        {
            _logger = logger;
            _gameService = gameService;
            _dbContext = dbContext;
        }
        //
        // [HttpGet("player/all")]
        // public ActionResult<IEnumerable<Player>> GetAllPlayers()
        // {
        //     var players = _gameService.GetAllPlayers();
        //     return Ok(players);
        // }
        //
        // [HttpGet("player")]
        // public ActionResult<Player> GetPlayerById(int id)
        // {
        //     Player player = _gameService.GetPlayerById(id);
        //     if (player == null)
        //     {
        //         return BadRequest();
        //     }
        //
        //     return Ok(player);
        // }

        [HttpGet("player")]
        public ActionResult<Player> GetPlayerByName(string name)
        {
            Player player = _gameService.GetPlayerByName(name);
            return Ok(player);
        }

        [HttpPost("player")]
        public ActionResult<Player> CreateNewPlayer(string name)
        {
            Player player = _gameService.GetPlayerByName(name) ?? _gameService.CreateNewPlayer(name);
            return Ok(player);
        }

        [HttpGet("game")]
        public ActionResult<IEnumerable<Game>> GetAllGames()
        {
            var games = _gameService.GetAllGames();
            return Ok(games);
        }

        [HttpGet("games/player")]
        public ActionResult<IEnumerable<Game>> GetAllGamesFromPlayer([FromQuery] int id)
        {
            var games = _gameService.GetAllGamesFromPlayer(id);
            return Ok(games);
        }

        [HttpGet("game/{id:int}")]
        public ActionResult<Game> GetGameById(int id)
        {
            Game game = _gameService.GetGameById(id);
            return Ok(game);
        }

        [HttpGet("game/new")]
        public ActionResult GetNewGames()
        {
            var games = _gameService.GetNewGames();
            return PartialView("_FindGames", games);
        }

        [HttpPost("game/create")]
        public ActionResult CreateNewGame(CreateGameDto dto)
        {
            Game game = _gameService.CreateNewGame(dto.PlayerId, dto.BoardSize, dto.Name);
            game = _gameService.JoinGame(game.Id, dto.PlayerId);
            return PartialView("_Gameplay", game);
        }

        [HttpDelete("game/{id}")]
        public ActionResult DeleteGame(int id)
        {
            if (_gameService.DeleteGame(id))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete("game/all")]
        public ActionResult DeleteAllGames()
        {
            _gameService.DeleteAllGames();
            return Ok();
        }

        [HttpPost("game/join")]
        public ActionResult Join(int gameId, int playerId)
        {
            Game game = _gameService.JoinGame(gameId, playerId);
            return PartialView("_Gameplay", game);
        }

        [HttpPost("game/maketurn")]
        public ActionResult<TurnResult> MakeTurn(TurnDto dto)
        {
            // var dto = new TurnDto {X = x, Y = y, PlayerId = playerId};

            return Ok(_gameService.MakeTurn(dto));
            /*
            try
            {
                return Ok(_gameService.MakeTurn(turnDto));
            }
            catch (Exception e)
            {
                return BadRequest();
            }*/
        }
    }
}