using System;
using System.Collections.Generic;

namespace Dom;

public partial class SaleDiscount
{
    public int? SaleId { get; set; }

    public int? DiscountId { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual Sale? Sale { get; set; }
}
