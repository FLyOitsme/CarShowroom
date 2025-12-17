using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class RoleType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
