﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Knx.Integration.Xml
{
    public class MasterXmlDataStore : XmlDataStore
    {
        public MasterXmlDataStore(Stream s, string fileName)
            : base(s, fileName)
        {

        }
    }
}
