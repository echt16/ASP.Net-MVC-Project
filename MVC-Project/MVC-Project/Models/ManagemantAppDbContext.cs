using Microsoft.EntityFrameworkCore;

namespace MVC_Project.Models
{
    public class ManagemantAppDbContext : DbContext
    {
        public ManagemantAppDbContext(DbContextOptions<ManagemantAppDbContext> options) : base(options) { }
    }
}
