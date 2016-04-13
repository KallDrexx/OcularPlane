using System;
using System.Linq;
using FluentAssertions;
using OcularPlane.Exceptions;
using Xunit;

namespace OcularPlane.Tests
{
    public class ContainerManagerTests
    {
        [Fact]
        public void No_Instances_In_Empty_Container()
        {
            var containerManager = new ContainerManager();
            var results = containerManager.GetInstancesInContainer("container");

            results.Should().NotBeNull();
            results.Length.Should().Be(0);
        }

        [Fact]
        public void Can_Add_Object_To_Container()
        {
            var containerManager = new ContainerManager();

            containerManager.AddObjectToContainer("container", this, "obj");
            var results = containerManager.GetInstancesInContainer("container");

            results.Should().NotBeNull();
            results.Length.Should().Be(1);
            results[0].Should().NotBeNull();
            results[0].Name.Should().Be("obj");
            results[0].Type.Should().Be(this.GetType());
            results[0].InstanceId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void No_Containers_Listed_Before_Adding_An_Object()
        {
            var containerManager = new ContainerManager();
            var results = containerManager.GetContainerNames();

            results.Should().NotBeNull();
            results.Length.Should().Be(0);
        }

        [Fact]
        public void Container_Added_When_Item_Added_To_It()
        {
            var containerManager = new ContainerManager();

            containerManager.AddObjectToContainer("container", this, "obj");
            var results = containerManager.GetContainerNames();

            results.Should().NotBeNull();
            results.Length.Should().Be(1);
            results[0].Should().Be("container");
        }

        [Fact]
        public void Can_Delete_Container()
        {
            var containerManager = new ContainerManager();

            containerManager.AddObjectToContainer("container", this, "obj");
            containerManager.DeleteContainer("container");
            var results = containerManager.GetContainerNames();

            results.Should().NotBeNull();
            results.Length.Should().Be(0);
        }

        [Fact]
        public void Storing_A_New_Object_With_The_Same_Name_In_The_Same_Container_Replaces_Object()
        {
            var containerManager = new ContainerManager();
            var testObj1 = new TestClass();
            var testObj2 = new TestClass();

            containerManager.AddObjectToContainer("container", testObj1, "obj");
            var instanceId = containerManager.GetInstancesInContainer("container")
                .Select(x => x.InstanceId)
                .FirstOrDefault();

            containerManager.AddObjectToContainer("container", testObj2, "obj");
            var results = containerManager.GetInstancesInContainer("container");

            results.Should().NotBeNull();
            results.Length.Should().Be(1);
            results[0].Should().NotBeNull();
            results[0].Name.Should().Be("obj");
            results[0].Type.Should().Be(typeof(TestClass));
            results[0].InstanceId.Should().NotBe(instanceId);
        }

        [Fact]
        public void Same_Object_In_Different_Containers_Has_Different_Instance_Id()
        {
            var containerManager = new ContainerManager();
            var testObj1 = new TestClass();

            containerManager.AddObjectToContainer("container1", testObj1, "obj");
            containerManager.AddObjectToContainer("container2", testObj1, "obj");

            var instance1 = containerManager.GetInstancesInContainer("container1")
                .Select(x => x.InstanceId)
                .FirstOrDefault();

            var instance2 = containerManager.GetInstancesInContainer("container2")
                .Select(x => x.InstanceId)
                .FirstOrDefault();

            instance1.Should().NotBeEmpty();
            instance2.Should().NotBeEmpty();
            instance1.Should().NotBe(instance2);
        }

        [Fact]
        public void Can_Get_Details_For_Instance()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass
            {
                NumberProperty = 123,
                StringProperty = "abc"
            };

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            var details = containerManager.GetInstanceDetails(reference.InstanceId);

            details.Should().NotBeNull();
            details.InstanceId.Should().Be(reference.InstanceId);
            details.Name.Should().Be(reference.Name);
            details.Properties.Should().NotBeNull();
            details.Properties.Length.Should().Be(2);

            var stringProperty = details.Properties.Single(x => x.Name == nameof(TestClass.StringProperty));
            stringProperty.Type.Should().Be(typeof (string));
            stringProperty.ValueAsString.Should().Be(testObject.StringProperty);

            var numberProperty = details.Properties.Single(x => x.Name == nameof(TestClass.NumberProperty));
            numberProperty.Type.Should().Be(typeof (int));
            numberProperty.ValueAsString.Should().Be(testObject.NumberProperty.ToString());
        }

        [Fact]
        public void Exception_When_Attempting_To_Set_Property_That_Doesnt_Exist()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

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
                ex.InstanceType.Should().Be(typeof (TestClass2));
                ex.PropertyName.Should().Be("SomeInvalidProperty");
            }
        }

        [Fact]
        public void Can_Set_String_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass2.StringProperty), "Test");

            testObject.StringProperty.Should().Be("Test");
        }

        [Fact]
        public void Can_Set_Int_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass2.IntProperty), "151");

            testObject.IntProperty.Should().Be(151);
        }

        [Fact]
        public void Can_Set_Float_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass2.FloatProperty), "3.33");

            testObject.FloatProperty.Should().Be(3.33f);
        }

        [Fact]
        public void Can_Set_Decimal_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass2.DecimalProperty), "3.33");

            testObject.DecimalProperty.Should().Be(3.33m);
        }

        [Fact]
        public void Can_Set_Bool_Value()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass2.BoolProperty), "true");

            testObject.BoolProperty.Should().BeTrue();
        }

        [Fact]
        public void Exception_Thrown_When_Value_Cant_Be_Converted()
        {
            var containerManager = new ContainerManager();
            var testObject = new TestClass2();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            try
            {
                containerManager.SetPropertyValue(reference.InstanceId, nameof(TestClass2.IntProperty), "abc");
                Assert.True(false, "Exception not thrown but expected");
            }
            catch (InvalidValueConversionException ex)
            {
                ex.InstanceType.Should().Be(typeof(TestClass2));
                ex.PropertyName.Should().Be(nameof(TestClass2.IntProperty));
                ex.PropertyType.Should().Be(typeof (int));
                ex.AttemptedValue.Should().Be("abc");
            }
        }

        private class TestClass
        {
            public string StringProperty { get; set; }
            public int NumberProperty { get; set; }
        }

        public class TestClass2
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public float FloatProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public bool BoolProperty { get; set; }
        }
    }
}
