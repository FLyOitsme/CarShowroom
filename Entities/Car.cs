using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Car
{
    public long Id { get; set; }

    public int? Year { get; set; }

    public bool? Stock { get; set; }

    public int? ConditionId { get; set; }

    public float? Mileage { get; set; }

    public int? TypeId { get; set; }

    public int? WdId { get; set; }

    public int? TransmissionId { get; set; }

    public float? EngVol { get; set; }

    public float? Power { get; set; }

    public string? Color { get; set; }

    public int? EngTypeId { get; set; }

    public float? Cost { get; set; }

    public int? ModelId { get; set; }

    public virtual ConditionType? Condition { get; set; }

    public virtual EngineType? EngType { get; set; }

    public virtual Model? Model { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual Transmission? Transmission { get; set; }

    public virtual CarType? Type { get; set; }

    public virtual Wdtype? Wd { get; set; }
}
