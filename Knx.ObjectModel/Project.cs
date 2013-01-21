using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class Project
    {
        internal Project()
        {

        }

        public List<Installation> Installations { get; internal set; }

        public List<GroupRange> GroupRanges { get; internal set; }

        public List<BuildingPart> Buildings { get; internal set; }

        public IEnumerable<DeviceInstance> Devices
        {
            get
            {
                return Installations
                .SelectMany(row => row.Areas)
                .SelectMany(row => row.Lines)
                .SelectMany(row => row.Devices);
            }
        }
    }
}
