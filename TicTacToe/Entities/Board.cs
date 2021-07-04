namespace TicTacToe.Entities
{
    public class Board
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public string BoardArrayString { get; set; }
        public string MovesHistory { get; set; }
    }
}