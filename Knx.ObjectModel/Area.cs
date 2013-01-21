using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Knx.ObjectModel
{
    public sealed class Area
    {
        public Installation Installation { get; internal set; }

        private List<Line> _lines;
        public List<Line> Lines
        {
            get { return _lines; }
            set
            {
                if (_lines != null && _lines != value)
                {
                    foreach (var line in _lines)
                        line.Area = null;
                }
                if (value != null)
                {
                    foreach (var line in value)
                        line.Area = this;
                }
                _lines = value;
            }
        }

        public string Name { get; internal set; }
        public string Description { get; internal set; }

        public short Address { get; internal set; }

        internal Area()
        {

        }
    }
}
