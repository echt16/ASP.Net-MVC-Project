namespace MVC_Project.Models
{
    public class AdditionalAppAccess
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Href { get; set; }
        public int AppAccessId { get; set; }
        public AppAccess AppAccess { get; set; }
        public virtual List<RoleAdditionalAppAccess> RoleAdditionalAppAccess { get; set; }
        public AdditionalAppAccess()
        {
            RoleAdditionalAppAccess = new List<RoleAdditionalAppAccess>();
        }
    }
}
