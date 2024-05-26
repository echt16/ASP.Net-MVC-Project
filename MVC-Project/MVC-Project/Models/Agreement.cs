namespace MVC_Project.Models
{
    public class Agreement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime DateTime { get; set; }
        public double TotalSum { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int WorkerId { get; set; }
        public User Worker { get; set; }
        public virtual List<AgreementService> AgreementsServices { get; set; }
        public Agreement()
        {
            AgreementsServices = new List<AgreementService>();
        }
    }
}
