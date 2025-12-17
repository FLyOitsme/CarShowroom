using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class EngineType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
