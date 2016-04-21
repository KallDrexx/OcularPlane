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

            var instanceId = containerManager.AddObjectToContainer("container", this, "obj");
            var results = containerManager.GetInstancesInContainer("container");

            results.Should().NotBeNull();
            results.Length.Should().Be(1);
            results[0].InstanceId.Should().Be(instanceId);
            results[0].Should().NotBeNull();
            results[0].Name.Should().Be("obj");
            results[0].TypeName.Should().Be(this.GetType().FullName);
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
            results[0].TypeName.Should().Be(typeof(TestClass).FullName);
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
            stringProperty.TypeName.Should().Be(typeof (string).FullName);
            stringProperty.ValueAsString.Should().Be(testObject.StringProperty);

            var numberProperty = details.Properties.Single(x => x.Name == nameof(TestClass.NumberProperty));
            numberProperty.TypeName.Should().Be(typeof (int).FullName);
            numberProperty.ValueAsString.Should().Be(testObject.NumberProperty.ToString());
        }

        [Fact]
        public void Fields_Are_Included_In_Instance_Details()
        {
            var containerManager = new ContainerManager();
            var testObject = new ClassWithField {IntField = 1};

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            var details = containerManager.GetInstanceDetails(reference.InstanceId);

            details.Should().NotBeNull();
            details.Properties.Length.Should().Be(1);
            details.Properties[0].Name.Should().Be(nameof(ClassWithField.IntField));
            details.Properties[0].ValueAsString.Should().Be("1");
        }

        [Fact]
        public void Can_Remove_Instance_From_Container()
        {
            var containerManager = new ContainerManager();

            containerManager.AddObjectToContainer("container", this, "obj");
            var instance = containerManager.GetInstancesInContainer("container").First();
            containerManager.RemoveInstance(instance.InstanceId);

            var results = containerManager.GetInstancesInContainer("container");
            results.Length.Should().Be(0);
        }

        [Fact]
        public void Can_Tell_If_Properties_Are_Read_Or_Write_Only()
        {
            var containerManager = new ContainerManager();
            var testObject = new ReadWriteOnlyTestClass {WriteOnlyProperty = 33};

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            var details = containerManager.GetInstanceDetails(reference.InstanceId);

            details.Properties.Length.Should().Be(2);

            var readProperty = details.Properties.Single(x => x.Name == nameof(ReadWriteOnlyTestClass.ReadOnlyProperty));
            readProperty.IsReadable.Should().BeTrue();
            readProperty.IsWritable.Should().BeFalse();
            readProperty.ValueAsString.Should().Be("33");

            var writeProperty = details.Properties.Single(x => x.Name == nameof(ReadWriteOnlyTestClass.WriteOnlyProperty));
            writeProperty.IsReadable.Should().BeFalse();
            writeProperty.IsWritable.Should().BeTrue();
            writeProperty.ValueAsString.Should().BeNull();
        }

        [Fact]
        public void Indexer_Properties_Are_Ignored()
        {
            var containerManager = new ContainerManager();
            var testObject = new IndexerClass();

            containerManager.AddObjectToContainer("container", testObject, "obj");
            var reference = containerManager.GetInstancesInContainer("container")
                .Single(x => x.Name == "obj");

            var details = containerManager.GetInstanceDetails(reference.InstanceId);

            details.Properties.Length.Should().Be(0);
        }

        [Fact]
        public void Can_Mark_Instance_As_Child_Of_Another_Instance()
        {
            var containerManager = new ContainerManager();

            var parentId = containerManager.AddObjectToContainer("container", this, "parent");
            var childId = containerManager.AddObjectToContainer("container", this, "child", parentId);

            var details = containerManager.GetInstanceDetails(childId);

            details.ParentInstanceId.Should().Be(parentId);
        }

        private class TestClass
        {
            public string StringProperty { get; set; }
            public int NumberProperty { get; set; }
            public void Method() { }
        }

        private class ClassWithField
        {
            public int IntField;
        }

        private class ReadWriteOnlyTestClass
        {
            private int _internalValue;

            public int ReadOnlyProperty => _internalValue;
            public int WriteOnlyProperty { set { _internalValue = value; } }
        }

        private class IndexerClass
        {
            private readonly int[] _array = new[] {1, 2, 3, 4, 5};

            public int this[int index]
            {
                get { return _array[index]; }
                set { _array[index] = value; }
            }
        }
    }
}
