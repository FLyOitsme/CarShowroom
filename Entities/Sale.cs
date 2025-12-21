using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Sale
{
    public int Id { get; set; }

    public int? ManagerId { get; set; }

    public DateOnly? Date { get; set; }

    public float? Cost { get; set; }

    public long CarId { get; set; }

    public long? ClientId { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual Client? Client { get; set; }

    public virtual User? Manager { get; set; }
}
