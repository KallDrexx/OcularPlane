using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OcularPlane.Exceptions;
using Xunit;

namespace OcularPlane.Tests
{
    public class SetValueTests
    {
        [Fact]
        public void Exception_When_Attempting_To_Set_Property_That_Doesnt_Exist()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            try
            {
                containerManager.SetPropertyValue(reference.InstanceId, "SomeInvalidProperty", "Test");
                Assert.True(false, "No exception thrown");
            }
            catch (PropertyDoesntExistException ex)
            {
                ex.InstanceType.Should().Be(typeof(TestClass));
                ex.PropertyName.Should().Be("SomeInvalidProperty");
            }
        }

        [Fact]
        public void Exception_Thrown_When_Value_Cant_Be_Converted()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            try
            {
                containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.IntProperty), "abc");
                Assert.True(false, "Exception not thrown but expected");
            }
            catch (InvalidValueConversionException ex)
            {
                ex.InstanceType.Should().Be(typeof(TestClass));
                ex.PropertyName.Should().Be(nameof(TestClass.IntProperty));
                ex.PropertyType.Should().Be(typeof(int));
                ex.AttemptedValue.Should().Be("abc");
            }
        }

        [Fact]
        public void Exception_Thrown_When_Target_Type_Has_No_Known_Conversion_Method()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            try
            {
                containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.UnknownClassProperty), "abc");
                Assert.True(false, "Exception not thrown but expected");
            }
            catch (NoKnownWayToParseToTypeException ex)
            {
                ex.DestinationType.Should().Be(typeof (Unparseable));
            }
        }

        [Fact]
        public void Can_Set_String_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.StringProperty), "Test");

            testObject.StringProperty.Should().Be("Test");
        }

        [Fact]
        public void Can_Set_Int_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.IntProperty), "151");

            testObject.IntProperty.Should().Be(151);
        }

        [Fact]
        public void Can_Set_Float_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.FloatProperty), "3.33");

            testObject.FloatProperty.Should().Be(3.33f);
        }

        [Fact]
        public void Can_Set_Decimal_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.DecimalProperty), "3.33");

            testObject.DecimalProperty.Should().Be(3.33m);
        }

        [Fact]
        public void Can_Set_Bool_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.BoolProperty), "true");

            testObject.BoolProperty.Should().BeTrue();
        }

        [Fact]
        public void Can_Set_Enum_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.EnumProperty), "Specified");

            testObject.EnumProperty.Should().Be(TestEnum.Specified);
        }

        [Fact]
        public void Exception_When_Enum_Value_Not_Defined()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            try
            {
                containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.EnumProperty), "abc");
                Assert.True(false, "Exception not thrown but expected");
            }
            catch (InvalidValueConversionException ex)
            {
                ex.InstanceType.Should().Be(typeof(TestClass));
                ex.PropertyName.Should().Be(nameof(TestClass.EnumProperty));
                ex.PropertyType.Should().Be(typeof(TestEnum));
                ex.AttemptedValue.Should().Be("abc");
            }
        }

        [Fact]
        public void Can_Set_Nullable_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.NullableIntProperty), "151");

            testObject.NullableIntProperty.Should().Be(151);
        }

        [Fact]
        public void Can_Set_Reference_Type_To_Null()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass {UnknownClassProperty = new Unparseable()};

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass.UnknownClassProperty), null);

            testObject.UnknownClassProperty.Should().BeNull();
        }

        [Fact]
        public void Can_Set_Field_To_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new ClassWithField();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(ClassWithField.IntField), "151");

            testObject.IntField.Should().Be(151);
        }

        private class TestClass
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public float FloatProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public bool BoolProperty { get; set; }
            public TestEnum EnumProperty { get; set; }
            public Unparseable UnknownClassProperty { get; set; }
            public int? NullableIntProperty { get; set; }
        }

        private enum TestEnum { Unspecified, Specified }
        private class Unparseable { }
        private class ClassWithField { public int IntField; }
    }
}
