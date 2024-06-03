namespace MVC_Project.Models
{
    public class RoleViewModel
    {
        public Role Role { get; set; }
        public Dictionary<AppAccess, List<AdditionalAppAccess>> AppAccesses { get; set; }
        public Dictionary<AppAccess, List<AdditionalAppAccess>> AppAccessesForRole { get; set; }
    }
}
