using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class Hardware
    {
        internal Hardware()
        {

        }

        public List<Product> Products { get; internal set; }
    }
}
