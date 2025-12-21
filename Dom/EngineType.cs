using System;
using System.Collections.Generic;

namespace Dom;

public partial class EngineType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
