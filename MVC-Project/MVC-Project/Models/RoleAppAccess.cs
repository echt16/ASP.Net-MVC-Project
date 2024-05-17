namespace MVC_Project.Models
{
    public class RoleAppAccess
    {
        public int Id { get; set; } 
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int AppAccessId { get; set; }
        public AppAccess AppAccess { get; set; }
    }
}
