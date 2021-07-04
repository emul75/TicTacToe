using System;

namespace TicTacToe.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int Draws { get; set; }
        public DateTime AccountCreated { get; set; }
        public int GamesPlayed => Wins + Loses + Draws;
    }
}