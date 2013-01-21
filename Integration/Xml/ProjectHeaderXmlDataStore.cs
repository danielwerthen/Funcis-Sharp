using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Knx.Integration.Xml
{
    public class ProjectHeaderXmlDataStore : XmlDataStore
    {
        public ProjectHeaderXmlDataStore(Stream s, string fileName)
            : base(s, fileName)
        {

        }
    }
}
