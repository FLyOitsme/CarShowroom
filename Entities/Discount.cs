using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Discount
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public float? Cost { get; set; }
}
