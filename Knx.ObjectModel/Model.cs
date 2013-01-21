using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class Model
    {
        public Project Project { get; set; }

        public Hardware Hardware { get; set; }

        public Programming Programs { get; set; }

        public IEnumerable<DatapointType> Types { get; set; }
    }
}
