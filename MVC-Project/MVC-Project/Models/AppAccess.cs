namespace MVC_Project.Models
{
    public class AppAccess
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public virtual List<RoleAppAccess> RolesAppAccesses { get; set; }
        public AppAccess()
        {
            RolesAppAccesses = new List<RoleAppAccess>();
        }
    }
}
