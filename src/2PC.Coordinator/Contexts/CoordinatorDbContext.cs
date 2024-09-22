using _2PC.Coordinator.Entities;
using Microsoft.EntityFrameworkCore;

namespace _2PC.Coordinator.Contexts
{
    public class CoordinatorDbContext : DbContext
    {
        public CoordinatorDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeState> NodeStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().HasData(
                   new Node { Id = 1, Name = "Order.API", Url = "https://localhost:7153" },
                   new Node { Id = 2, Name = "Stock.API", Url = "https://localhost:7199" },
                   new Node { Id = 3, Name = "Payment.API", Url = "https://localhost:7092" }
               );
        }
    }
}