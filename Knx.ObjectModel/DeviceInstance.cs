using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Knx.ObjectModel
{
	public sealed class DeviceInstance
	{
		public Line Line { get; internal set; }
		public BuildingPart Building { get; set; }

		public string Name { get; internal set; }

		public string Id { get; internal set; }

		public string Description { get; internal set; }

		public string Comment { get; internal set; }

		public short Address { get; internal set; }

		public string Hardware2ProgramRefId { get; internal set; }

		public string ProductRefId { get; internal set; }
		public Product Product { get; internal set; }

		public string IsComVis { get; set; }
		public string CommunicationPartLoaded { get; set; }
		public string MediumConfigLoaded { get; set; }
		public string ParametersLoaded { get; set; }
		public string ApplicationProgramLoaded { get; set; }
		public string IndividualAddressLoaded { get; set; }

		public string FullAddress
		{
			get
			{
				return string.Format("{0}.{1}.{2}", Line.Area.Address, Line.Address, Address);
			}
		}

		private List<ComObjectInstance> _comObjectInstances;
		public List<ComObjectInstance> ComObjectInstances
		{
			get { return _comObjectInstances; }
			internal set
			{
				if (_comObjectInstances != null)
					foreach (var coi in _comObjectInstances)
						coi.ParentDevice = null;
				_comObjectInstances = value;
				if (_comObjectInstances != null)
					foreach (var coi in _comObjectInstances)
						coi.ParentDevice = this;
			}
		}

		internal DeviceInstance()
		{

		}
	}
}
