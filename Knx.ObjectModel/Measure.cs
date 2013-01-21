using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Knx.ObjectModel
{
    public enum MeasureTypes
    {
        Entry,
        Converter,
        Aggregator
    }

    public class Measure : FunctionBase
    {
        private MeasureTypes? _type;
        public MeasureTypes? Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

    }
}
