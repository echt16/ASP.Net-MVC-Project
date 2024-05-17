﻿namespace MVC_Project.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<RoleAppAccess> RolesAppAccesses { get; set; }
        public virtual List<User> Users { get; set; }
        public Role()
        {
            RolesAppAccesses = new List<RoleAppAccess>();
            Users = new List<User>();
        }
    }
}
