using Microsoft.EntityFrameworkCore;

namespace EquipRentApi.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Equipment> Equipments { get; set; } = null!;
        public DbSet<PickUpPoint> PickUpPoints { get; set; } = null!;
        public DbSet<PickUpPointEquipment> PickUpPointEquipments { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;


        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}
