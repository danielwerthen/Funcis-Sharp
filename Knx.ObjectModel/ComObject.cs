using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knx.ObjectModel
{
    public class ComObject
    {
        internal ComObject()
        {
            _values = new Dictionary<string, object>();
        }

        public string Id { get; internal set; }

        protected Dictionary<string, object> _values;
        public object this[string name]
        {
            get
            {
                if (_values.ContainsKey(name))
                    return _values[name];
                return null;
            }
            internal set
            {
                _values[name] = value;
            }
        }

        public Dictionary<string, object> Attributes
        {
            get { return _values; }
        }

        public ApplicationProgram Program { get; internal set; }

        private List<ComObjectRef> _comObjectRefs;
        public List<ComObjectRef> ComObjectRefs
        {
            get
            {
                return _comObjectRefs;
            }
            internal set
            {
                if (_comObjectRefs != null)
                {
                    foreach (var instance in _comObjectRefs)
                        instance.Ref = null;
                }
                _comObjectRefs = value;
                if (_comObjectRefs != null)
                {
                    foreach (var instance in _comObjectRefs)
                        instance.Ref = this;
                }
            }
        }
    }
}
