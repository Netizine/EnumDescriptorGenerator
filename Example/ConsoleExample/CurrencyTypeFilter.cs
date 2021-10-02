using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumExample
{
    public enum CurrencyTypeFilter
    {
        [Description("None")]
        None = 0,
        [Description("United States Dollar")]
        USD = 1,
        [Description("Pound Sterling")]
        GBP = 2,
        [Description("Euro")]
        EUR = 3

    }
}
