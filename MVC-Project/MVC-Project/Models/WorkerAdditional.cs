namespace MVC_Project.Models
{
    public class WorkerAdditional
    {
        public int Id { get; set; }
        public string Position {  get; set; }
        public int ContactDetailId { get; set; }
        public ContactDetail ContactDetail { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
