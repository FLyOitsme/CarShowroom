using System;
using System.Collections.Generic;

namespace Dom;

public partial class SaleAddition
{
    public int? SaleId { get; set; }

    public int? AddId { get; set; }

    public virtual Addition? Add { get; set; }

    public virtual Sale? Sale { get; set; }
}
