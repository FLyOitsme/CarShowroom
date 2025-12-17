using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Addition
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public float? Cost { get; set; }
}
