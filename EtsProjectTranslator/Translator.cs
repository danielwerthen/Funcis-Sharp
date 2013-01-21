using Knx.Infrastructure.DataTypes;
using Knx.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EtsProjectTranslator
{
	public static class Translator
	{
		const string namesp = "http://www.gaia.se/TranslatedEts/v.0.1";
		static XNamespace ns = XNamespace.Get(namesp);
		public static XDocument Translate(Knx.ObjectModel.Model model)
		{
			var project = model.Project;
			var doc = new XDocument(new XDeclaration("0.1", "UTF-8", "yes"));
			var devices = project.Devices.SelectMany(device => Translate(device, project));
			var ele = new XElement(ns + "Translation");
			doc.Add(ele);
			ele.Add(new XElement(XName.Get("Devices", namesp), devices.ToArray()));
			ele.Add(new XElement(ns + "Gruppadresser", TranslateGroups(project)));
			return doc;
		}

		public static IEnumerable<XElement> TranslateGroups(Project project)
		{
			var gas = project.GroupRanges.SelectMany(row => row.Ranges).SelectMany(row => row.Addresses);
			foreach (var ga in gas)
			{
				XElement element = new XElement(ns + "Gruppadress");
				var ea = new EnmxAddress(ga.Value);
				var enmx = ea.Address.Split('/');
				element.Add(Create("Huvudgrupp", enmx[0]));
				element.Add(Create("Huvudgruppnamn", ga.ParentRange.ParentRange.Name));
				element.Add(Create("Huvudgruppbeskrivning", ga.ParentRange.ParentRange.Description));
				element.Add(Create("Mellangrupp", enmx[1]));
				element.Add(Create("Mellangruppnamn", ga.ParentRange.Name));
				element.Add(Create("Mellangruppbeskrivning", ga.ParentRange.Description));
				element.Add(Create("Gruppadress", ea.Address));
				element.Add(Create("Gruppadressnamn", ga.Name));
				element.Add(Create("Gruppadressbeskrivning", ga.Description));
				element.Add(Create("Längd", ga.ComObjectInstances.Select(row => row["ObjectSize"]).FirstOrDefault()));
				element.Add(Create("Antal förbindelser", ga.ComObjectInstances.Count()));
				element.Add(Create("Centralfunktion", "Nej?"));
				element.Add(Create("Passera genom linjekopplare", "Nej?"));
				yield return element;
			}
		}

		public static XElement Create(string name, object val)
		{
			var fname = name.Replace(" ", "_");
			var fval = val == null ? "" : val.ToString();
			if (fval == null) fval = "";
			return new XElement(XName.Get(fname, namesp), fval);
		}

		public static IEnumerable<XElement> Translate(DeviceInstance dev, Project project)
		{
			foreach (var obj in dev.ComObjectInstances)
			{
				XElement element = new XElement(XName.Get("Device", namesp));
				element.Add(Create("Område", dev.Line.Area.Address));
				element.Add(Create("Områdesnamn", dev.Line.Area.Name));
				element.Add(Create("Områdesbeskrivning", dev.Line.Area.Description));
				element.Add(Create("Linje", dev.Line.Address));
				element.Add(Create("Linjenamn", dev.Line.Name));
				element.Add(Create("Linjebeskrivning", dev.Line.Description));
				element.Add(Create("Device id", dev.FullAddress));
				element.Add(Create("Devicenamn", dev.Name));
				element.Add(Create("Devicebeskrivning", dev.Description));
				element.Add(Create("Produkt", dev.Product.Text));
				element.Add(Create("Rum", dev.RoomValue()));
				element.Add(Create("Funktion", "Okänd attribut"));
				element.Add(Create("Program", dev.Product.Program));
				element.Add(Create("Tillverkare", dev.Product.Manufacturer));
				element.Add(Create("Beställningsnummer", dev.Product.OrderNumber));
				element.Add(Create("Adr", dev.IndividualAddressLoaded == "1" ? "X" : "-"));
				element.Add(Create("Prg", dev.ApplicationProgramLoaded == "1" ? "X" : "-"));
				element.Add(Create("Par", dev.ParametersLoaded == "1" ? "X" : "-"));
				element.Add(Create("Grp", (obj.GroupAddresses.Count > 0) ? "X" : "-"));
				element.Add(Create("Kfg", dev.MediumConfigLoaded == "1" ? "X" : "-"));
				foreach (var attr in TranslateObject(obj))
					element.Add(attr);
				yield return element;
			}
		}
		private static List<string> _comAttributes = new List<string> { "ObjectFunction", 
			"Name", "Text", "VisibleDescription", "WriteFlag", 
			"UpdateFlag", "TransmitFlag", "ReadOnInitFlag", "ReadFlag", 
			"CommunicationFlag", "Priority", "ObjectSize", "Number", 
			"FunctionText", "Description" };
		public static IEnumerable<XElement> TranslateObject(ComObjectInstance obj)
		{
			yield return Create("Objektnummer", obj["Number"]);
			yield return Create("Objektfunktion", obj["FunctionText"]);
			yield return Create("Objektbeskrivning", obj["Description"] ?? obj["VisibleDescription"]);
			yield return Create("Antal förbindelser", obj.GroupAddresses.Count);
			yield return Create("Groupadresser", obj.Addresses);
			yield return Create("Längd", obj["ObjectSize"]);
			yield return Create("Datatyp", ReadDataType(obj));
			yield return Create("Kommunikation", obj["CommunciationFlag"]);
			yield return Create("Läs", obj["ReadFlag"]);
			yield return Create("Skriv", obj["WriteFlag"]);
			yield return Create("Överför", obj["TransmitFlag"]);
			yield return Create("Uppdatera", obj["UpdateFlag"]);
			yield return Create("I", obj["ReadOnInitFlag"]);
			yield return Create("Prioritet", obj["Priority"]);

		}

		public static string ReadDataType(ComObjectInstance obj)
		{
			if (obj == null || obj.DatapointType == null)
				return null;
			return string.Format("{0}.{1} {2}", new object[] { obj.DatapointType.Type.Number, int.Parse(obj.DatapointType.Number).ToString("D3"), obj.DatapointType.Text});
		}

		public static string RoomValue(this DeviceInstance dev)
		{
			var rooms = new BuildingPart[] { dev.Building, dev.Building.Parent, (dev.Building.Parent == null ? null : dev.Building.Parent.Parent) };
			return string.Join(" - ", rooms.Where(row => row != null).Select(row => row.Name).ToArray());
		}
	}
}
