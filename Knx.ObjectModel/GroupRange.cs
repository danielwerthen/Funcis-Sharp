using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class GroupRange
    {
        internal GroupRange()
        {

        }

        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public int RangeStart { get; internal set; }
        public int RangeEnd { get; internal set; }

        private List<GroupAddress> _addresses;
        public List<GroupAddress> Addresses
        {
            get { return _addresses; }
            internal set 
            {
                if (_addresses != null)
                {
                    foreach (var addr in _addresses)
                    {
                        addr.ParentRange = null;
                    }
                }
                _addresses = value; 
                if (_addresses != null)
                {
                    foreach (var addr in _addresses)
                    {
                        addr.ParentRange = this;
                    }
                }
            }
        }


        public GroupRange ParentRange { get; set; }
        private List<GroupRange> _ranges;

        public List<GroupRange> Ranges
        {
            get { return _ranges; }
            internal set 
            {
                if (_ranges != null)
                {
                    foreach (var range in _ranges)
                    {
                        range.ParentRange = null;
                    }
                }
                _ranges = value; 
                if (_ranges != null)
                {
                    foreach (var range in _ranges)
                    {
                        range.ParentRange = this;
                    }
                }
            }
        }
        

        public IEnumerable<object> Children
        {
            get
            {
                if (Addresses == null)
                    return Ranges;
                if (Ranges == null)
                    return Addresses;
                return Addresses.AsEnumerable<object>().Union(Ranges.AsEnumerable<object>());
            }
        }
    }
}
