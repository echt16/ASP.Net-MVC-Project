namespace MVC_Project.Models
{
    public class RoleAdditionalAppAccess
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int AdditionalAppAccessId { get; set; }
        public AdditionalAppAccess AdditionalAppAccess { get; set; }
    }
}
