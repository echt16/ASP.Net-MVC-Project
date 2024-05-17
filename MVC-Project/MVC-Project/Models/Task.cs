namespace MVC_Project.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public string? Description { get; set; }
        public int? Complexity { get; set; }
        public int? Urgency {  get; set; }
        public int WorkerId {  get; set; }
        public User Worker { get; set; }
        public int? CustomerId {  get; set; }
        public User? Customer { get; set; }
        public int? AddressId { get; set; }
        public Address? Address { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
