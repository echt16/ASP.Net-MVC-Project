﻿namespace MVC_Project.Models
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Agreement> Agreements { get; set; }
        public Status()
        {
            Agreements = new List<Agreement>();
        }
    }
}
