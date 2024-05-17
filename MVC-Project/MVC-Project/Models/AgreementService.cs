namespace MVC_Project.Models
{
    public class AgreementService
    {
        public int Id { get; set; } 
        public int AgreementId { get; set; }
        public Agreement Agreement { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
