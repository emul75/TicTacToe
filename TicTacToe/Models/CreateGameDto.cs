namespace TicTacToe.Models
{
    public class CreateGameDto
    {
        public string Name { get; set; }
        public int PlayerId { get; set; }
        public int BoardSize { get; set; }
    }
}