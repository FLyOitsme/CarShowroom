using System;
using System.Collections.Generic;

namespace Dom;

public partial class Country
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
}
