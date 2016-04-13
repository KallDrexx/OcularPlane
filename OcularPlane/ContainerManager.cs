using System;
using System.Collections.Concurrent;
using System.Linq;
using OcularPlane.Models;

namespace OcularPlane
{
    public class ContainerManager
    {
        private readonly ConcurrentDictionary<string, Container> _containers = new ConcurrentDictionary<string, Container>();

        public void AddObjectToContainer(string containerName, object obj, string objectName)
        {
            _containers.TryAdd(containerName, new Container());

            var container = _containers[containerName];
            container.AddObject(obj, objectName);
        }

        public string[] GetContainerNames()
        {
            return _containers.Keys.ToArray();
        }

        public void DeleteContainer(string containerName)
        {
            Container container;
            _containers.TryRemove(containerName, out container);
        }

        public InstanceReference[] GetInstancesInContainer(string containerName)
        {
            Container container;
            _containers.TryGetValue(containerName, out container);

            return container != null
                ? container.GetInstances()
                : new InstanceReference[0];
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            // TODO: Implement caching for performance
            return _containers.Select(x => x.Value.GetInstanceDetails(instanceId))
                .SingleOrDefault();
        }
    }
}
