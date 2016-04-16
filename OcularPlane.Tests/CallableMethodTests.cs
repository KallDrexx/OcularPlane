using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace OcularPlane.Tests
{
    public class CallableMethodTests
    {
        [Fact]
        public void Can_Add_Methods_To_Container_Manager()
        {
            var testClass = new TestClass();

            var containerManager = new ContainerManager();
            containerManager.AddMethodToContainer("container", () => testClass.SimpleMethod(0, null), "methodName");

            var methods = containerManager.GetMethodsInContainer("container");

            methods.Should().NotBeNull();
            methods.Length.Should().Be(1);
            methods[0].Should().NotBeNull();
            methods[0].MethodId.Should().NotBeEmpty();
            methods[0].Name.Should().Be("methodName");
            methods[0].Parameters.Should().NotBeNull();
            methods[0].Parameters.Length.Should().Be(2);
            methods[0].Parameters[0].ParameterName.Should().Be("param1");
            methods[0].Parameters[0].TypeName.Should().Be(typeof (int).FullName);
            methods[0].Parameters[1].ParameterName.Should().Be("param2");
            methods[0].Parameters[1].TypeName.Should().Be(typeof (string).FullName);
        }

        [Fact]
        public void Can_Execute_Method()
        {
            var testClass = new TestClass();
            var containerManager = new ContainerManager();
            containerManager.AddMethodToContainer("container", () => testClass.SimpleMethod(0, null), "methodName");

            var method = containerManager.GetMethodsInContainer("container").Single();
            var parameterValues = new Dictionary<string, string>
            {
                {"param1", "1"},
                {"param2", "two" }
            };

            containerManager.ExecuteMethod(method.MethodId, parameterValues);

            testClass.IntProperty.Should().Be(1);
            testClass.StringProperty.Should().Be("two");
        }

        [Fact]
        public void Can_Execute_Static_Method()
        {
            var testClass = new TestClass();
            var containerManager = new ContainerManager();
            containerManager.AddMethodToContainer("container", () => StaticTestMethod(0), "methodName");

            var method = containerManager.GetMethodsInContainer("container").Single();
            var parameterValues = new Dictionary<string, string>
            {
                {"value", "1"},
            };

            containerManager.ExecuteMethod(method.MethodId, parameterValues);

            StaticInt.Should().Be(1);
        }

        [Fact]
        public void Cannot_Add_Method_With_Non_IConvertible_Parameter_Types()
        {
            var testClass = new TestClass();
            var containerManager = new ContainerManager();

            try
            {
                containerManager.AddMethodToContainer("container", () => testClass.UnpassableMethodCall(null), "methodName");
                Assert.True(false, "Exception expected but none thrown");
            }
            catch(InvalidOperationException)
            {
                
            }
        }

        [Fact]
        public void Can_Set_Enum_Parameter()
        {
            var testClass = new TestClass();
            var containerManager = new ContainerManager();
            containerManager.AddMethodToContainer("container", () => testClass.SetEnumValue(0), "methodName");

            var method = containerManager.GetMethodsInContainer("container").Single();
            var parameterValues = new Dictionary<string, string>
            {
                {"value", "Specified"},
            };

            containerManager.ExecuteMethod(method.MethodId, parameterValues);

            testClass.EnumProperty.Should().Be(TestClass.TestEnum.Specified);
        }

        private class TestClass
        {
            public enum TestEnum
            {
                Unspecified,
                Specified
            };

            public int IntProperty { get; set; }
            public string StringProperty { get; set; }
            public TestEnum EnumProperty { get; set; }

            public string SimpleMethod(int param1, string param2)
            {
                IntProperty = param1;
                StringProperty = param2;

                return "test";
            }

            public void UnpassableMethodCall(CallableMethodTests test) { }

            public void SetEnumValue(TestEnum value)
            {
                EnumProperty = value;
            }
        }      

        private static int StaticInt { get; set; }
        private static void StaticTestMethod(int value)
        {
            StaticInt = value;
        }
    }
}
