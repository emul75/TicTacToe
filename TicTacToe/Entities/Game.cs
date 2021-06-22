using System;
using System.ComponentModel.DataAnnotations.Schema;
using TicTacToe.Enums;

namespace TicTacToe.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GameStatus Status { get; set; }
        public int TurnCounter { get; set; }
        public DateTime GameCreated { get; set; }
        public int BoardId { get; set; }
        public virtual Board Board { get; set; }

        [ForeignKey("PlayerX"), Column(Order = 0)]
        public int? PlayerXId { get; set; }

        [ForeignKey("PlayerO"), Column(Order = 1)]
        public int? PlayerOId { get; set; }

        public virtual Player PlayerX { get; set; }
        public virtual Player PlayerO { get; set; }
        public char Winner { get; set; }
    }
}