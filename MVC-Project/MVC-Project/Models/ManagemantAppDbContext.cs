using Microsoft.EntityFrameworkCore;

namespace MVC_Project.Models
{
    public class ManagemantAppDbContext : DbContext
    {
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<AppAccess> AppAccesses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleAppAccess> RolesAppAccesses { get; set; }
        public DbSet<AgreementService> AgreementsServices { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<LoginPassword> LoginsPasswords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WorkerAdditional> WorkersAdditionals { get; set; }
        public DbSet<CustomerAdditional> CustomersAdditionals { get; set; }
        public DbSet<ContactDetail> ContactDetails { get; set; }
        public ManagemantAppDbContext(DbContextOptions<ManagemantAppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Agreement>()
                .HasOne(a => a.Customer)
                .WithMany(u => u.Agreements)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Agreement>()
                .HasOne(a => a.Worker)
                .WithMany(u => u.Agreements)
                .HasForeignKey(a => a.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Task>()
                .HasOne(a => a.Customer)
                .WithMany(u => u.Tasks)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Task>()
                .HasOne(a => a.Worker)
                .WithMany(u => u.Tasks)
                .HasForeignKey(a => a.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
