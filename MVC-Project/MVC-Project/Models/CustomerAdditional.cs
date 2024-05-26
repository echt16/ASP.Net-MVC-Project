namespace MVC_Project.Models
{
    public class CustomerAdditional
    {
        public int Id { get; set; } 
        public string Position { get; set; }
        public int ContactDetailId { get; set; }
        public ContactDetail ContactDetail { get; set; }
        public int AddressId {  get; set; }
        public Address Address { get; set; }
        public int CustomerId { get; set; } 
        public Customer Customer { get; set; }
    }
}
