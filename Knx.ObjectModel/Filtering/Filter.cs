using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Knx.ObjectModel;

namespace Knx.ObjectModel.Filtering
{
    public class Filter
    {
        public Filter()
        {
            Filters = new List<FilterDefinition>();
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public List<FilterDefinition> Filters { get; set; }

        public List<FilteredResult> GetFilterResult(Model model, bool permissive = true)
        {
            if (model == null || model.Project == null)
                return new List<FilteredResult>();
            var list = model.Project.Devices.SelectMany(row => row.ComObjectInstances);
            return list.ApplyFilters(Filters, permissive);
        }

        public static IEnumerable<string> GetSuggestions(FilterDefinition filter, Model model)
        {
            return model.Project.Devices.SelectMany(row => row.ComObjectInstances)
                .SelectMany(row => 
                    filter.Iterate(row)
                        .Where(r1 => !string.IsNullOrEmpty(r1)))
                    .OrderBy(row => row)
                    .Distinct();
        }

    }
}