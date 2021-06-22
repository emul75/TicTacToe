using Microsoft.EntityFrameworkCore;

namespace TicTacToe.Entities
{
    public class TicTacToeDbContext : DbContext
    {
        private string _connectionString =
            "Data Source=.\\TicTacToe.db;";

        public DbSet<Game> Games { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .Property((r => r.Status))
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}