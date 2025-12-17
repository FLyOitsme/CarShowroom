using System;
using System.Collections.Generic;

namespace DataLayer.Entities;

public partial class Brand
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? CountryId { get; set; }

    public virtual Country? Country { get; set; }

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}
