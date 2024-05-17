namespace MVC_Project.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }   
        public virtual List<Service> Services { get; set; }
        public Product()
        {
            Services = new List<Service>();
        }
    }
}
