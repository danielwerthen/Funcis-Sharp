using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Knx.Integration;

namespace Knx.ObjectModel
{
    public class ObjectModelFactory
    {
        public static Model BuildModel(ProjectStore store)
        {
            var project = ObjectModelFactory.BuildProject(store.Projects.Select(row => row.Document));
            return new Model()
            {
                Project = project,
                Hardware = ObjectModelFactory.BuildHardware(store.Hardwares.Select(row => row.Document), project),
                Programs = ObjectModelFactory.BuildProgramming(store.Manufacturers.Select(row => row.Document), project),
                Types = BuildTypes(store.Masters.Select(row => row.Document))
            };
        }

        private static IEnumerable<DatapointType> BuildTypes(IEnumerable<XDocument> doc)
        {
            XNamespace ns = doc.Elements().First().Name.Namespace;
            XElement root = new XElement(ns + "Programs", doc.SelectMany(row => row.Elements()));
            return BuildMany<DatapointType>("DatapointType", root, BuildDatapointType);
        }

        private static DatapointType BuildDatapointType(XElement e)
        {
            return new DatapointType()
            {
                Id = GetAttribute("Id", e),
                Name = GetAttribute("Name", e),
                Text = GetAttribute("Text", e),
                Number = GetAttribute("Number", e),
                SizeInBits = GetAttributeShort("SizeInBit", e),
                SubTypes = BuildMany<DatapointSubType>("DatapointSubtype", e, BuildDatapointSubType)
            };
        }

        private static DatapointSubType BuildDatapointSubType(XElement e)
        {
            bool def = false;
            string defVal = GetAttribute("Default", e);
            if (!string.IsNullOrEmpty(defVal))
                bool.TryParse(defVal, out def);
            return new DatapointSubType()
            {
                Id = GetAttribute("Id", e),
                Name = GetAttribute("Name", e),
                Text = GetAttribute("Text", e),
                Number = GetAttribute("Number", e),
                Default = def
            };
        }

        private static Programming BuildProgramming(IEnumerable<XDocument> doc, Project project)
        {
            XNamespace ns = doc.Elements().First().Name.Namespace;
            XElement root = new XElement(ns + "Programs", doc.SelectMany(row => row.Elements()));
            var objInstances = project.Devices
                .SelectMany(row => row.ComObjectInstances)
                .GroupBy(row => row.Id)
                .ToDictionary(row => row.Key);
            
            var p = new Programming()
            {
                Programs = BuildMany<ApplicationProgram>("ApplicationProgram", root, BuildApplicationProgramFunc(objInstances))
            };
            return p;
        }

        private static Func<XElement, ApplicationProgram> BuildApplicationProgramFunc(Dictionary<string, IGrouping<string, ComObjectInstance>> objInstances)
        {
            return (e) =>
            {
                XNamespace ns = e.Name.Namespace;
                var language = e.Parent.Parent.Element(ns + "Languages").Elements().FirstOrDefault(row => row.Attribute("Identifier") != null && row.Attribute("Identifier").Value == "sv-SE");
                var ap = new ApplicationProgram()
                {
                    Id = GetAttribute("Id", e),
                    Name = GetAttribute("Name", e),
                    VisibleDescription = GetAttribute("VisibleDescription", e),
                    ComObjects = BuildMany<ComObject>("ComObject", e, BuildComObject),
                    ComObjectRefs = BuildMany<ComObjectRef>("ComObjectRef", e, BuildComObjectRef(language))
                };

                foreach (var comObject in ap.ComObjects)
                {
                    comObject.ComObjectRefs = ap.ComObjectRefs.Where(row => row.RefId == comObject.Id).ToList();
                }

                foreach (var comObjectRef in ap.ComObjectRefs)
                {
                    if (objInstances.ContainsKey(comObjectRef.Id))
                        comObjectRef.Instances = objInstances[comObjectRef.Id].ToList();
                }

                return ap;
            };
        }

        private static IEnumerable<ComObjectInstance> UpdateComObjectInstance(IEnumerable<ComObjectInstance> instance, XElement e)
        {
            foreach (var attr in _comAttributes)
            {
                var val = GetAttribute(attr, e);
                if (string.IsNullOrEmpty(val))
                    continue;
                foreach (var item in instance)
                    item[attr].Value = val;
            }
            return instance;
        }

        private static List<string> _comAttributes = new List<string> {"ObjectFunction", "Name", "Text", "VisibleDescription", "WriteFlag", "UpdateFlag", "TransmitFlag", "ReadOnInitFlag", "ReadFlag", "CommunicationFlag", "Priority", "ObjectSize", "Number", "FunctionText", "Description" };

        private static ComObject BuildComObject(XElement e)
        {
            var obj = new ComObject()
            {
                Id = GetAttribute("Id", e)
            };

            foreach (var attr in _comAttributes)
            {
                obj[attr] = GetAttribute(attr, e);
            }

            return obj;
        }

        private static string FindTranslation(XElement language, string attributeName, string refId)
        {
            if (!(attributeName == "Text" || attributeName == "FunctionText"))
                return null;
            if (language == null)
                return null;
            var ns = language.Name.Namespace;
            return language.Descendants(ns + "TranslationElement").Where(row => row.Attribute("RefId") != null && row.Attribute("RefId").Value == refId).SelectMany(row => row.Elements()).Where(row => row.Attribute("AttributeName") != null && row.Attribute("AttributeName").Value == attributeName).Select(row => row.Attribute("Text") != null ? row.Attribute("Text").Value : null).FirstOrDefault();
        }

        private static Func<XElement, ComObjectRef> BuildComObjectRef(XElement language)
        {
            return (e) =>
            {
                string refId;
                var obj = new ComObjectRef()
                {
                    Id = GetAttribute("Id", e),
                    RefId = refId = GetAttribute("RefId", e)
                };

                foreach (var attr in _comAttributes)
                {
                    var val = GetAttribute(attr, e);
                    if (string.IsNullOrEmpty(val))
                        continue;
                    obj[attr].Value = FindTranslation(language, attr, refId) ?? val;
                }

                return obj;
            };
        }

        private static Hardware BuildHardware(IEnumerable<XDocument> doc, Project project)
        {
            XNamespace ns = doc.Elements().First().Name.Namespace;
            XElement root = new XElement(ns + "Hardwares", doc.SelectMany(row => row.Elements()));
            Hardware hw = new Hardware()
            {
                Products = BuildMany<Product>("Product", root, BuildProductFunc(project.Devices))
            };
            return hw;
        }

        private static Func<XElement, Product> BuildProductFunc(IEnumerable<DeviceInstance> devices)
        {
            return (e) =>
                new Product()
                {
                    Id = GetAttribute("Id", e),
                    Text = GetAttribute("Text", e),
                    Devices = devices.Where(row => row.ProductRefId == GetAttribute("Id", e)).ToList()
                };
        }

        private static Project BuildProject(IEnumerable<XDocument> doc)
        {
            XNamespace ns = doc.Elements().First().Name.Namespace;
            XElement root = new XElement(ns + "Projects", doc.SelectMany(row => row.Elements()));
            Project p = new Project()
            {
                Installations = BuildMany<Installation>("Installation", root, BuildInstallation)
            };
            var groupAddresses = p.Devices
                .SelectMany(row => row.ComObjectInstances)
                .SelectMany(row => row.GroupAddresses)
                .Where(row => row != null);
            p.GroupRanges = BuildManyStrict<GroupRange>("GroupRange", root, BuildGroupRangeFunc(groupAddresses));
            p.Buildings = BuildManyStrict<BuildingPart>("BuildingPart", root, BuildBuildingPartFunc(p.Devices));
            return p;
        }

        private static Func<XElement, BuildingPart> BuildBuildingPartFunc(IEnumerable<DeviceInstance> devices)
        {
            return new Func<XElement, BuildingPart>((e) =>
            {
                bool hasSubParts = e.Elements(e.Name.Namespace + "BuildingPart").Count() > 0;
                var bp = new BuildingPart()
                {
                    Name = GetAttribute("Name", e),
                    Description = GetAttribute("Description", e),
                    Comment = GetAttribute("Comment", e),
                    Id = GetAttribute("Id", e),
                    Number = GetAttribute("Number", e),
                    Type = GetAttribute("Type", e)
                };

                if (hasSubParts)
                {
                    bp.Parts = BuildManyStrict<BuildingPart>("BuildingPart", e, BuildBuildingPartFunc(devices));
                }
                bp.Devices = BuildManyStrict<DeviceInstance>("DeviceInstanceRef", e, AttachDevicesFunc(devices), false)
                    .Where(row => row != null)
                    .ToList();

                return bp;
            });
        }

        private static Func<XElement, DeviceInstance> AttachDevicesFunc(IEnumerable<DeviceInstance> devices)
        {
            return (e) =>
                {
                    var id = e.Attribute("RefId").Value;
                    return devices.FirstOrDefault(row => row.Id == id);
                };
        }

        private static Func<XElement, GroupRange> BuildGroupRangeFunc(IEnumerable<GroupAddressInstance> addresses)
        {
            return new Func<XElement, GroupRange>((e) =>
            {
                bool hasSubRange = e.Elements(e.Name.Namespace + "GroupRange").Count() > 0;
                var gr = new GroupRange()
                {
                    Name = GetAttribute("Name", e),
                    Description = GetAttribute("Description", e),
                    RangeStart = GetAttributeInt("RangeStart", e),
                    RangeEnd = GetAttributeInt("RangeEnd", e)
                };

                if (hasSubRange)
                {
                    gr.Ranges = BuildManyStrict<GroupRange>("GroupRange", e, BuildGroupRangeFunc(addresses));
                }
                else
                {
                    gr.Addresses = BuildManyStrict<GroupAddress>("GroupAddress", e, BuildGroupAddressFunc(addresses), false);
                }

                return gr;
            });
        }

        private static Func<XElement, GroupAddress> BuildGroupAddressFunc(IEnumerable<GroupAddressInstance> with)
        {
            return (e) =>
            {
                var id = GetAttribute("Id", e);
                return new GroupAddress()
                {
                    Id = GetAttribute("Id", e),
                    Name = GetAttribute("Name", e),
                    Value = GetAttributeInt("Address", e),
                    Instances = with.Where(row => row.Id == id).ToList()
                };
            };
        }

        private static GroupAddress MergeGroupAddress(XElement e, IEnumerable<GroupAddress> with)
        {
            var id = GetAttribute("Id", e);
            var match = with.SingleOrDefault(row => row.Id == id);
            if (match == null)
                return null;
            match.Value = GetAttributeInt("Address", e);
            match.Name = GetAttribute("Name", e);
            return match;
        }

        private static Installation BuildInstallation(XElement e1)
        {
            return new Installation()
                {
                    Name = GetAttribute("Name", e1),
                    Areas = BuildMany<Area>("Area", e1, BuildArea)
                };
        }

        private static Area BuildArea(XElement e2)
        {
            return new Area()
            {
                Name = GetAttribute("Name", e2),
                Description = GetAttribute("Description", e2),
                Address = GetAttributeShort("Address", e2),
                Lines = BuildMany<Line>("Line", e2, BuildLine)
            };
        }

        private static Line BuildLine(XElement e3)
        {
            return new Line()
            {
                Name = GetAttribute("Name", e3),
                Description = GetAttribute("Description", e3),
                Address = GetAttributeShort("Address", e3),
                Devices = BuildMany<DeviceInstance>("DeviceInstance", e3, BuildDeviceInstance)
            };
        }

        private static DeviceInstance BuildDeviceInstance(XElement e4)
        {
            return new DeviceInstance()
                {
                    Name = GetAttribute("Name", e4),
                    Description = GetAttribute("Description", e4),
                    Comment = GetAttribute("Comment", e4),
                    Address = GetAttributeShort("Address", e4),
                    Hardware2ProgramRefId = GetAttribute("Hardware2ProgramRefId", e4),
                    ProductRefId = GetAttribute("ProductRefId", e4),
                    Id = GetAttribute("Id", e4),
                    ComObjectInstances = BuildMany<ComObjectInstance>("ComObjectInstanceRef", e4, BuildComObjectInstance)
                };
        }

        private static ComObjectInstance BuildComObjectInstance(XElement e)
        {
            var obj = new ComObjectInstance()
            {
                Id = e.Attribute("RefId").Value,
                GroupAddresses = BuildMany<GroupAddressInstance>("Send", e, BuildGroupAddressInstance)
            };

            foreach (var attr in _comAttributes)
            {
                var val = GetAttribute(attr, e);
                if (string.IsNullOrEmpty(val))
                    continue;
                obj[attr].Value = val;
            }

            return obj;
        }

        private static GroupAddressInstance BuildGroupAddressInstance(XElement e)
        {
            return new GroupAddressInstance()
            {
                Id = GetAttribute("GroupAddressRefId", e)
            };
        }

        private static List<TYPE> BuildMany<TYPE>(string name, XElement element, Func<XElement, TYPE> construct)
        {
            XNamespace ns = element.Name.Namespace;
            if (element.Descendants(ns + name).Count() > 0)
                return element.Descendants(ns + name).Select(row => construct(row)).ToList();
            else if (element.Name.LocalName == name)
                return new List<TYPE> { construct(element) };
            else
                return new List<TYPE>();
        }

        private static List<TYPE> BuildManyStrict<TYPE>(string name, XElement element, Func<XElement, TYPE> construct, bool recursive = true)
        {
            XNamespace ns = element.Name.Namespace;
            if (element.Elements(ns + name).Count() > 0)
                return element.Elements(ns + name).Select(row => construct(row)).ToList();
            else if (element.Name.LocalName == name)
                return new List<TYPE> { construct(element) };
            else if (element.Descendants(ns + name).Count() > 0 && recursive)
                return element.Elements().SelectMany(row => BuildManyStrict<TYPE>(name, row, construct)).ToList();
            else
                return new List<TYPE>();
        }

        private static string GetAttribute(XName name, XElement element)
        {
            var attr = element.Attribute(name);
            return attr != null ? attr.Value : null;
        }

        private static int GetAttributeInt(XName name, XElement element)
        {
            string read = GetAttribute(name, element);
            if (read == null)
                return default(int);
            int res;
            if (int.TryParse(read, out res))
                return res;
            return default(int);
        }

        private static short GetAttributeShort(XName name, XElement element)
        {
            string read = GetAttribute(name, element);
            if (read == null)
                return default(short);
            short res;
            if (short.TryParse(read, out res))
                return res;
            return default(short);
        }
    }
}
