namespace MVC_Project.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string ImageSrc { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public virtual List<CustomerAdditional> CustomersAdditionals { get; set; }
        public virtual List<Task> Tasks { get; set; }
        public virtual List<Agreement> Agreements { get; set; }
        public Customer()
        {
            CustomersAdditionals = new List<CustomerAdditional>();
            Tasks = new List<Task>();
            Agreements = new List<Agreement>();
        }
    }
}
