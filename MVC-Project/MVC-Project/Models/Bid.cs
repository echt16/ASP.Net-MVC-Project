namespace MVC_Project.Models
{
    public class Bid
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int UserId {  get; set; }
        public User User { get; set; }
    }
}
