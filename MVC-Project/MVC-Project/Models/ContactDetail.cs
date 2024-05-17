namespace MVC_Project.Models
{
    public class ContactDetail
    {
        public int Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public virtual List<WorkerAdditional> WorkersAdditionals { get; set; }
        public ContactDetail()
        {
            WorkersAdditionals = new List<WorkerAdditional>();
        }
    }
}
