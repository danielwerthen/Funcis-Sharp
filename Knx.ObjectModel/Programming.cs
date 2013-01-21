using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class Programming
    {
        internal Programming()
        {

        }

        public List<ApplicationProgram> Programs { get; internal set; }

        public IEnumerable<ComObject> ComObjects
        {
            get
            {
                return Programs.SelectMany(row => row.ComObjects);
            }
        }
    }
}
