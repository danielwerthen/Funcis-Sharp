using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
	public class Product
	{
		internal Product()
		{

		}

		public string Id { get; internal set; }
		public string Text { get; internal set; }
		public string Name { get; internal set; }
		public string Program { get; internal set; }
		public string Manufacturer { get; internal set; }
		public string OrderNumber { get; internal set; }
		public string HasApplicationProgram { get; internal set; }
		public string IsCoupler { get; internal set; }
		public string HasIndividualAddress { get; internal set; }

		public string ApplicationProgramRefId { get; internal set; }

		private List<DeviceInstance> _devices;
		public List<DeviceInstance> Devices
		{
			get { return _devices; }
			set
			{
				if (_devices != null)
				{
					foreach (var device in _devices)
						device.Product = null;
				}
				_devices = value;
				if (_devices != null)
				{
					foreach (var device in _devices)
						device.Product = this;
				}
			}
		}
	}
}
