namespace MVC_Project.Models
{
    public class RoleViewModel
    {
        public Role Role { get; set; }
        public List<AppAccess> AllAppAccess { get; set; }
        public List<AppAccess> AppAccessForRole { get; set; }
    }
}
