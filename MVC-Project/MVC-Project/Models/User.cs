namespace MVC_Project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int LoginPasswordId { get; set; }
        public LoginPassword LoginPassword { get; set; }
        public int? RoleId { get; set; }
        public Role? Role { get; set; }
        public string ImageSrc { get; set; }
        public virtual List<WorkerAdditional> WorkerAdditionals { get; set; }
        public virtual List<Task> Tasks { get; set; }
        public virtual List<Agreement> Agreements { get; set; }
        public virtual List<Bid> Bids { get; set; }
        public User()
        {
            WorkerAdditionals = new List<WorkerAdditional>();
            Tasks = new List<Task>();
            Agreements = new List<Agreement>();
        }
    }
}
