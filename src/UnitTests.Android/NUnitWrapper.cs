namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public class TestClassAttribute : NUnit.Framework.TestFixtureAttribute { }

    public class TestMethodAttribute : NUnit.Framework.TestAttribute { }

    public class Assert : NUnit.Framework.Assert
    {
        public static void IsInstanceOfType(object obj, System.Type type)
        {
            NUnit.Framework.Assert.IsInstanceOfType(type, obj, null);
        }
        public static void IsInstanceOfType(object obj, System.Type type, string message)
        {
            NUnit.Framework.Assert.IsInstanceOfType(type, obj, message);
        }
    }
}