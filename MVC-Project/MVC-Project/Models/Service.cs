namespace MVC_Project.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Count {  get; set; }
        public double Sum { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public virtual List<AgreementService> AgreementsServices { get; set; }
        public Service()
        {
            AgreementsServices = new List<AgreementService>();
        }
    }
}
