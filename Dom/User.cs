using System;
using System.Collections.Generic;

namespace Dom;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Patronyc { get; set; }

    public int? RoleTypeId { get; set; }

    public virtual RoleType? RoleType { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
