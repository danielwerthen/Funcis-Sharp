using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class BuildingPart
    {
        internal BuildingPart()
        {

        }

        public BuildingPart Parent { get; internal set; }

        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Comment { get; internal set; }
        public string Number { get; internal set; }
        public string Type { get; internal set; }

        private List<BuildingPart> _parts;
        public List<BuildingPart> Parts
        {
            get { return _parts; }
            set
            {
                if (_parts != null)
                {
                    foreach (var part in _parts)
                    {
                        part.Parent = null;
                    }
                }
                _parts = value;
                if (_parts != null)
                {
                    foreach (var part in _parts)
                    {
                        part.Parent = this;
                    }
                }
            }
        }
        private List<DeviceInstance> _devices;
        public List<DeviceInstance> Devices
        {
            get { return _devices; }
            internal set 
            {
                if (_devices != null)
                {
                    foreach (var dev in _devices)
                        dev.Building = null;
                }
                _devices = value; 
                if (_devices != null)
                {
                    foreach (var dev in _devices)
                        dev.Building = this;
                }
            }
        }


        public IEnumerable<object> Children
        {
            get 
            {
                if (Parts == null)
                    return Devices;
                if (Devices == null)
                    return Parts;
                return Parts.AsEnumerable<object>().Union(Devices.AsEnumerable<object>()); 
            }
        }
    }
}
