using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TicTacToe.Entities;
using TicTacToe.Enums;
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

        [HttpGet("player/all")]
        public ActionResult<IEnumerable<Player>> GetAllPlayers()
        {
            var players = _gameService.GetAllPlayers();
            return Ok(players);
        }

        [HttpGet("player/{id:int}")]
        public ActionResult<Player> GetPlayerById(int id)
        {
            Player player = _gameService.GetPlayerById(id);
            if (player == null)
            {
                return BadRequest();
            }

            return Ok(player);
        }

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

        [HttpGet("games")]
        public ActionResult<IEnumerable<Game>> GetAllGamesFromPlayer(int playerId)
        {
            var games = _gameService.GetAllGamesFromPlayer(playerId);
            return Ok(games);
        }

        [HttpGet("game/{id:int}")]
        public ActionResult<Game> GetGameById(int id)
        {
            Game game = _gameService.GetGameById(id);
            return Ok(game);
        }

        [HttpGet("game/details/{id:int}")]
        public ActionResult GetGameDetails(int id)
        {
            Game game = _gameService.GetGameById(id);
            return PartialView("_gameDetails", game);
        }

        [HttpGet("game/new")]
        public ActionResult GetNewGames(int playerId)
        {
            Game game = _gameService.PlayerIsInNewOrStartedGame(playerId);
            if (game is not null) return PartialView("_ReconnectToGame", game);

            var games = _gameService.GetNewGames();
            return PartialView("_FindGames", games);
        }

        [HttpPost("game/create")]
        public ActionResult CreateNewGame(CreateGameDto dto)
        {
            Game game = _gameService.CreateNewGame(dto.PlayerId, dto.BoardSize, dto.Name);
            game = _gameService.Join(game.Id, dto.PlayerId);
            return PartialView("_Gameplay", game);
        }

        [HttpDelete("game/{id:int}")]
        public ActionResult DeleteGame(int id)
        {
            if (_gameService.DeleteGame(id))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete("game/leave")]
        public ActionResult LeaveGame(int id)
        {
            _gameService.LeaveGame(id);
            return Ok();
        }

        [HttpDelete("game/all")]
        public ActionResult DeleteAllGames()
        {
            _gameService.DeleteAllGames();
            return Ok();
        }

        [HttpPost("game/reconnect")]
        public ActionResult Reconnect(JoinGameDto dto)
        {
            Game game = _gameService.Reconnect(dto.GameId, dto.PlayerId);
            return PartialView("_Gameplay", game);
        }

        [HttpPost("game/join")]
        public ActionResult Join(JoinGameDto dto)
        {
            Game game = _gameService.Join(dto.GameId, dto.PlayerId);
            return PartialView("_Gameplay", game);
        }

        [HttpPost("game/maketurn")]
        public ActionResult<TurnResult> MakeTurn(TurnDto dto)
        {
            return Ok(_gameService.MakeTurn(dto));
        }
    }
}