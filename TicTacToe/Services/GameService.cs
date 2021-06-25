using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Entities;
using TicTacToe.Enums;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    public interface IGameService
    {
        Player GetPlayerById(int id);
        Player GetPlayerByName(string name);
        IEnumerable<Player> GetAllPlayers();
        IEnumerable<Game> GetAllGames();
        IEnumerable<Game> GetAllGamesFromPlayer(int id);
        Game GetGameById(int id);
        IEnumerable<Game> GetNewGames();
        Game CreateNewGame(int playerId, int size, string name);
        Player CreateNewPlayer(string name);
        Game Join(int gameId, int playerId);
        Game Reconnect(int gameId, int playerId);
        Game PlayerIsInNewOrStartedGame(int playerId);
        bool DeleteGame(int gameId);
        void DeleteAllGames();
        TurnResult MakeTurn(TurnDto dto);
        bool LeaveGame(int id);
    }

    public class GameService : IGameService
    {
        private readonly TicTacToeDbContext _ticTacToeDbContext;
        private readonly TicTacToeDbContext _dbContext;

        public GameService(TicTacToeDbContext ticTacToeDbContext, TicTacToeDbContext dbContext)
        {
            _ticTacToeDbContext = ticTacToeDbContext;
            _dbContext = dbContext;
        }

        public Player GetPlayerById(int id)
        {
            Player player = _dbContext
                .Players
                .FirstOrDefault(p => p.Id == id);
            return player;
        }

        public Player GetPlayerByName(string name)
        {
            Player player = _dbContext
                .Players
                .FirstOrDefault(p => p.Name == name);
            return player;
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            var players = _dbContext.Players.ToList();
            return players;
        }

        public IEnumerable<Game> GetAllGames()
        {
            var games = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .ToList();
            return games;
        }

        public IEnumerable<Game> GetAllGamesFromPlayer(int id)
        {
            var games = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .Where(p => p.PlayerX.Id == id || p.PlayerO.Id == id)
                .ToList();
            return games;
        }

        public Game GetGameById(int id)
        {
            Game game = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .FirstOrDefault(g => g.Id == id);
            return game;
        }

        public IEnumerable<Game> GetNewGames()
        {
            var games = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .Where(g => g.Status == GameStatus.Created)
                .ToList();
            return games;
        }


        public Game CreateNewGame(int playerId, int size, string name)
        {
            if (name == string.Empty)
            {
                name = _dbContext.Players.First(p => p.Id == playerId).Name.ToString() + "\'s game";
            }

            var game = new Game
            {
                Name = name,
                Status = GameStatus.Created,
                TurnCounter = 0,
                GameCreated = DateTime.Now,
                Board = new Board
                {
                    BoardArrayString = new string('_', size * size),
                    Size = size
                }
            };
            _dbContext.Add(game);
            _dbContext.SaveChanges();
            return game;
        }

        public Player CreateNewPlayer(string name)
        {
            var player = new Player {Name = name, AccountCreated = DateTime.Now};
            _dbContext.Add(player);
            _dbContext.SaveChanges();
            return player;
        }

        public Game Reconnect(int gameId, int playerId)
        {
            Game game = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .FirstOrDefault(g =>
                    g.Id == gameId && (g.PlayerOId == playerId || g.PlayerXId == playerId) &&
                    g.Status != GameStatus.Finished);


            return PlayerIsInNewOrStartedGame(playerId) is not null ? game : null;
        }

        public Game Join(int gameId, int playerId)
        {
            Game game = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .FirstOrDefault(g => g.Id == gameId);

            if (game is not null && PlayerIsInNewOrStartedGame(playerId) is null)
            {
                if (game.PlayerX is null)
                    game.PlayerX = _dbContext.Players.FirstOrDefault(p => p.Id == playerId);
                else
                {
                    game.PlayerO = _dbContext.Players.FirstOrDefault(p => p.Id == playerId);
                    StartGame(gameId);
                }
            }

            _dbContext.SaveChanges();
            return game;
        }

        public void StartGame(int gameId)
        {
            var random = new Random();
            Game game = _dbContext.Games.FirstOrDefault(g => g.Id == gameId) ?? throw new Exception("nima takiej gry");
            if (random.Next(2) == 0)
            {
                Player tempPlayer = game.PlayerX;
                game.PlayerX =
                    game.PlayerO;
                game.PlayerO = tempPlayer;
            }

            game.Status = GameStatus.InProgress;
            _dbContext.SaveChanges();
        }

        public bool DeleteGame(int gameId)
        {
            Game game = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .FirstOrDefault(g => g.Id == gameId);

            if (game is null) return false;

            _dbContext.Games.Remove(game);
            _dbContext.SaveChanges();
            return true;
        }

        public void DeleteAllGames()
        {
            foreach (Game entity in _dbContext.Games)
                _dbContext.Games.Remove(entity);
            _dbContext.SaveChanges();
        }

        public TurnResult MakeTurn(TurnDto dto)
        {
            var games = _dbContext.Games.ToList();
            Game game = _ticTacToeDbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .FirstOrDefault(g =>
                    (g.PlayerO.Id == dto.PlayerId || g.PlayerX.Id == dto.PlayerId) &&
                    g.Status == GameStatus.InProgress && g.Id == dto.GameId);
            if (game == null) throw new Exception("Game not found");
            int x = dto.X;
            int y = dto.Y;
            char[,] boardArray = StringToCharArray(game.Board.BoardArrayString);

            if (boardArray[x, y] != '_') throw new Exception("Location is occupied");
            boardArray[x, y] = game.TurnCounter % 2 == 0 ? 'X' : 'O';
            game.TurnCounter++;
            game.Board.BoardArrayString = CharArrayToString(boardArray);
            _dbContext.SaveChanges();
            switch (WinningConditionChecker(game, x, y))
            {
                case TurnResult.PlayerXWon:
                    game.Winner = 'X';
                    game.PlayerX.Wins++;
                    game.PlayerO.Loses++;
                    game.Status = GameStatus.Finished;
                    _dbContext.SaveChanges();
                    return TurnResult.PlayerXWon;
                case TurnResult.PlayerOWon:
                    game.Winner = 'O';
                    game.PlayerX.Loses++;
                    game.PlayerO.Wins++;
                    game.Status = GameStatus.Finished;
                    _dbContext.SaveChanges();
                    return TurnResult.PlayerOWon;
                case TurnResult.Draw:
                    game.Winner = 'D';
                    game.PlayerX.Draws++;
                    game.PlayerO.Draws++;
                    _dbContext.SaveChanges();
                    return TurnResult.Draw;
                case TurnResult.StillInProgress:
                    return TurnResult.StillInProgress;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool LeaveGame(int id)
        {
            Game game = _ticTacToeDbContext
                .Games
                .Include(g => g.Board)
                .FirstOrDefault(g => g.Id == id && g.Status != GameStatus.Finished);
            if (game is null) return false;
            _dbContext.Remove(game);
            _dbContext.SaveChanges();
            return true;
        }

        private TurnResult WinningConditionChecker(Game game, int turnIndexX, int turnIndexY)
        {
            char[,] boardArray = StringToCharArray(game.Board.BoardArrayString);
            int symbolsInARowToWin = game.Board.Size > 4 ? 5 : game.Board.Size;
            int maxBoardIndex = game.Board.Size - 1;
            char playerSymbol = (game.TurnCounter - 1) % 2 == 0 ? 'X' : 'O';
            /*
            Vertical - from up to down,
            Horizontal - from left to right,
            Diagonal_Positive - from down_left to up_right,
            Diagonal_Negative - from up_left to down_right
            */

            int symbolsInARowCount = 1,
                x = turnIndexX,
                y = turnIndexY;

            //      VERTICAL        //

            while (y > 0)
            {
                if (boardArray[x, y - 1] == playerSymbol) y--;
                else break;
            }

            while (y < maxBoardIndex)
            {
                if (boardArray[x, y + 1] == playerSymbol)
                {
                    y++;
                    symbolsInARowCount++;
                    if (symbolsInARowCount == symbolsInARowToWin)
                        return playerSymbol == 'X' ? TurnResult.PlayerXWon : TurnResult.PlayerOWon;
                }
                else break;
            }


            //      HORIZONTAL      //

            symbolsInARowCount = 1;
            x = turnIndexX;
            y = turnIndexY;

            while (x > 0)
            {
                if (boardArray[x - 1, y] == playerSymbol) x--;
                else break;
            }

            while (x < maxBoardIndex)
            {
                if (boardArray[x + 1, y] == playerSymbol)
                {
                    x++;
                    symbolsInARowCount++;
                    if (symbolsInARowCount == symbolsInARowToWin)
                        return playerSymbol == 'X' ? TurnResult.PlayerXWon : TurnResult.PlayerOWon;
                }
                else break;
            }


            //      Diagonal_Up       //

            symbolsInARowCount = 1;
            x = turnIndexX;
            y = turnIndexY;

            while (x > 0 && y < maxBoardIndex)
            {
                if (boardArray[x - 1, y + 1] == playerSymbol)
                {
                    x--;
                    y++;
                }
                else break;
            }

            while (x < maxBoardIndex && y > 0)
            {
                if (boardArray[x + 1, y - 1] == playerSymbol)
                {
                    x++;
                    y--;
                    symbolsInARowCount++;
                    if (symbolsInARowCount == symbolsInARowToWin)
                        return playerSymbol == 'X' ? TurnResult.PlayerXWon : TurnResult.PlayerOWon;
                }
                else break;
            }


            //      Diagonal_Down       //

            symbolsInARowCount = 1;
            x = turnIndexX;
            y = turnIndexY;

            while (x > 0 && y > 0)
            {
                if (boardArray[x - 1, y - 1] == playerSymbol)
                {
                    x--;
                    y--;
                }
                else break;
            }

            while (x < maxBoardIndex && y < maxBoardIndex)
            {
                if (boardArray[x + 1, y + 1] == playerSymbol)
                {
                    x++;
                    y++;
                    symbolsInARowCount++;
                    if (symbolsInARowCount == symbolsInARowToWin)
                        return playerSymbol == 'X' ? TurnResult.PlayerXWon : TurnResult.PlayerOWon;
                }
                else break;
            }


            //      Draw    OR    Continue Game       //

            return game.TurnCounter == (maxBoardIndex + 1) * (maxBoardIndex + 1)
                ? TurnResult.Draw
                : TurnResult.StillInProgress;
        }

        public Game PlayerIsInNewOrStartedGame(int playerId)
        {
            Game game = _dbContext
                .Games
                .Include(g => g.PlayerX)
                .Include(g => g.PlayerO)
                .Include(g => g.Board)
                .FirstOrDefault(g =>
                    (g.PlayerXId == playerId || g.PlayerOId == playerId) &&
                    (g.Status != GameStatus.Finished));
            return game;
        }

        private string CharArrayToString(char[,] czar)
        {
            return czar.Cast<char>().Aggregate(string.Empty, (current, c) => current + c);
        }

        private char[,] StringToCharArray(string boardString)
        {
            int size = (int) Math.Sqrt(boardString.Length);
            char[,] result = new char[size, size];
            int index = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    result[i, j] = boardString[index];
                    index++;
                }
            }

            return result;
        }
    }
}