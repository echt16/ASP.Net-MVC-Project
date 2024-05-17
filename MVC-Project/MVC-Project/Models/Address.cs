namespace MVC_Project.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<CustomerAdditional> CustomersAdditionals { get; set; }
        public virtual List<Task> Tasks { get; set; }
        public Address()
        {
            CustomersAdditionals = new List<CustomerAdditional>();
            Tasks = new List<Task>();
        }
    }
}
