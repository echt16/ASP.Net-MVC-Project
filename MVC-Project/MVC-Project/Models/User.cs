namespace MVC_Project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int LoginPasswordId { get; set; }
        public LoginPassword LoginPassword { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public virtual List<WorkerAdditional> WorkersAdditionals { get; set; }
        public virtual List<CustomerAdditional> CustomersAdditionals { get; set; }
        public virtual List<Task> Tasks { get; set; }
        public virtual List<Agreement> Agreements { get; set; }
        public User()
        {
            WorkersAdditionals = new List<WorkerAdditional>();
            CustomersAdditionals = new List<CustomerAdditional>();
            Tasks = new List<Task>();   
            Agreements = new List<Agreement>();
        }
    }
}
