using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.IO;
using Knx.Integration;
using Knx.ObjectModel.Filtering;
using System.Diagnostics;
using Knx.ObjectModel.Patterns;

namespace Knx.ObjectModel.Tests
{
    [TestClass]
    public class TestSomethings
    {
        [TestMethod]
        public void TestSplit()
        {
            var result = FilterExtensions.Split("Hello mister").ToList();
            var res2 = FilterExtensions.Split("Hello mister \"Woo Tien\"").ToList();
            var t = res2;
        }
        [TestMethod]
        public void TestMethod1()
        {
            XElement xbase = new XElement("Installation", new XElement("Area"), new XElement("Area"));
            //var t = ObjectModelFactory.ConstructInstallations(xbase).First();
            //int c = 0;
            //foreach (var a in t.Areas)
            //{
            //    c++;
            //}

            //foreach (var b in t.Areas)
            //{
            //    c++;
            //}
        }

        [TestMethod]
        public void TestMemory()
        {
            Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            long initial = currentProcess.WorkingSet64;
            List<long> measurements = new List<long>() { initial };
            List<Model> models = new List<Model>();
            for (var i = 0; i < 15; i++)
            {
                models.Add(Load());
                measurements.Add(currentProcess.WorkingSet64);
            }
            var peak = currentProcess.PeakWorkingSet64;
            var min = measurements.Min();
            var max = measurements.Max();
            var final = currentProcess.WorkingSet64;
        }

        private Model Load()
        {
            using (var stream = File.OpenRead(@"C:\Users\dwn.GAIA2000\Dropbox\KNX Utveckling\Projekt\GAIA ver 7.0b.knxproj"))
            {
                var store = ProjectStore.Load(stream);
                var model = ObjectModelFactory.BuildModel(store);
                store = null;
                return model;
            }
        }

        [TestMethod]
        public void TestPatterns()
        {
            using (var stream = File.OpenRead(@"C:\Users\dwn.GAIA2000\Dropbox\KNX Utveckling\Projekt\GAIA ver 7.0b.knxproj"))
            {
                var store = ProjectStore.Load(stream);
                var model = ObjectModelFactory.BuildModel(store);
                var filter = new Filter();
                filter.Filters = new List<FilterDefinition>();
                var fd = new FilterDefinition(FilterKinds.GroupAddressDescription, FilterTypes.EqualTo, FilterActions.Include, "Kontor");
                //filter.Filters.Add(fd);
                var toTest = filter.GetFilterResult(model);
                var result = PatternFinder.FindPatterns<GroupAddressInstance>(toTest.Select(row => row.GroupAddress), (row => row.Ref.Name)).ToList();

            }
        }


        [TestMethod]
        public void TestFilters()
        {
            using (var stream = File.OpenRead(@"C:\Users\dwn.GAIA2000\Dropbox\KNX Utveckling\Projekt\GAIA ver 7.0b.knxproj"))
            {
                var store = ProjectStore.Load(stream);
                var model = ObjectModelFactory.BuildModel(store);
                var filter = new Filter();
                filter.Filters = new List<FilterDefinition>();
                var fd = new FilterDefinition(FilterKinds.GroupAddressDescription, FilterTypes.EqualTo, FilterActions.Include, "Kontor");
                string toMatch = "Kon";
                var sugg = Filter.GetSuggestions(fd, model);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var t1 = LevenshteinDistance.SuggestMatches(toMatch, sugg, 20).ToList();
                sw.Stop();
                var e1 = sw.ElapsedMilliseconds;
                sw.Reset();
                toMatch = "Konto";
                sw.Start();
                var t2 = LevenshteinDistance.SuggestMatches(toMatch, sugg, 20).ToList();
                sw.Stop();
                var e2 = sw.ElapsedMilliseconds;

                var r1 = filter.GetFilterResult(model);
            }
        }
    }
}
