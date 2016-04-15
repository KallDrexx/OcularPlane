using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

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

        private class TestClass
        {
            public int IntProperty { get; set; }
            public string StringProperty { get; set; }

            public string SimpleMethod(int param1, string param2)
            {
                IntProperty = param1;
                StringProperty = param2;

                return "test";
            }
        }
    }
}
