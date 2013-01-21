using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Knx.ObjectModel
{
    public sealed class Installation
    {
        public string Name { get; internal set; }

        private List<Area> _areas;
        public List<Area> Areas 
        {
            get { return _areas; }
            internal set
            {
                if (_areas != null && _areas != value)
                {
                    foreach (var area in _areas)
                        area.Installation = null;
                }
                if (value != null)
                {
                    foreach (var area in value)
                        area.Installation = this;
                }
                _areas = value;
            }
        }

        internal Installation()
        {

        }
    }
}
