using System;
using System.Collections.Generic;

namespace Dom;

public partial class Client
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Patronyc { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PassData { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
