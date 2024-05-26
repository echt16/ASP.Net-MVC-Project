namespace MVC_Project.Models
{
    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<WorkerAdditional> WorkerAdditionals { get; set; }
        public Position() { WorkerAdditionals = new List<WorkerAdditional>(); }
    }
}
