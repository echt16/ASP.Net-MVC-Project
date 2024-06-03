namespace MVC_Project.Models
{
    public class CheckRoleViewModel
    {
        public Role Role { get; set; }
        public Dictionary<AppAccess, List<AppAccess>> AllAppAccess { get; set; }
        public Dictionary<AppAccess, List<AppAccess>> AppAccessForRole { get; set; }
    }
}
