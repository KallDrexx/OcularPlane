using FluentAssertions;
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
    }
}
