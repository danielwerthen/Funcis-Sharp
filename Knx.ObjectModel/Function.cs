using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Knx.ObjectModel
{
    public class Function : FunctionBase
    {
        public Function()
        {
            _comObjects = new System.Collections.ObjectModel.ObservableCollection<ComObjectInstance>();
            _subFunctions = new System.Collections.ObjectModel.ObservableCollection<Function>();
        }

        private System.Collections.ObjectModel.ObservableCollection<ComObjectInstance> _comObjects;
        private System.Collections.ObjectModel.ObservableCollection<Function> _subFunctions;
        public IEnumerable<ComObjectInstance> ComObjects
        {
            get { return _comObjects; }
        }

        public IEnumerable<Function> SubFunctions
        {
            get { return _subFunctions; }
        }

        public void AddSubFunction(Function f)
        {
            if (_subFunctions.Contains(f) || f == this)
                return;
            _subFunctions.Add(f);
            OnPropertyChanged("SubFunctions");
        }

        public void RemoveSubFunction(Function f)
        {
            if (!_subFunctions.Contains(f))
                return;
            _subFunctions.Remove(f);
            OnPropertyChanged("SubFunctions");
        }

        public void AddComObject(ComObjectInstance i)
        {
            if (_comObjects.Contains(i))
                return;
            _comObjects.Add(i);
            OnPropertyChanged("ComObjects");
        }

        public void RemoveComObject(ComObjectInstance i)
        {
            if (!_comObjects.Contains(i))
                return;
            _comObjects.Remove(i);
            OnPropertyChanged("ComObjects");
        }
    }

    public static class FunctionExtensions
    {
        public static IEnumerable<ComObjectInstance> GetAllComObjects(this Function f)
        {
            return f.ComObjects.Union(f.GetSubFunctions().SelectMany(row => row.ComObjects));
        }
    }
}
