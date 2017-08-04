using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class ApplicationInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var location = new System.IO.FileInfo(typeof(ApplicationInitializer).Assembly.Location).Directory.FullName;
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.InstallPath = location;
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
        }
    }
}
