namespace MVC_Project.Models
{
    public class UserViewModel
    {
        public User User { get; set; }
        public string Login {  get; set; }
        public WorkerAdditional? WorkerAdditional { get; set; }
        public ContactDetail? ContactDetail { get; set; }
        public List<Role> Roles { get; set; }
        public List<Position>? Positions { get; set; }
    }
}
