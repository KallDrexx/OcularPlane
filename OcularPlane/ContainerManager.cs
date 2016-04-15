using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public void AddMethodToContainer(string containerName, Expression<Action> methodExpression, string methodName)
        {
            _containers.TryAdd(containerName, new Container());

            var container = _containers[containerName];
            container.AddMethod(methodExpression, methodName);
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

        public MethodReference[] GetMethodsInContainer(string containerName)
        {
            Container container;
            _containers.TryGetValue(containerName, out container);

            return container != null
                ? container.GetMethods()
                : new MethodReference[0];
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            // TODO: Implement caching for performance
            return _containers.Select(x => x.Value.GetInstanceDetails(instanceId))
                .Where(x => x != null)
                .SingleOrDefault();
        }

        public void SetPropertyValue(Guid instanceId, string propertyName, string value)
        {
            // TODO: Cache container<->InstanceId associations
            foreach (var containerPair in _containers)
            {
                containerPair.Value.SetInstancePropertyValue(instanceId, propertyName, value);
            }
        }

        public void ExecuteMethod(Guid methodId, Dictionary<string, string> parameters)
        {
            // TODO: Cache method id associations
            foreach (var containerPair in _containers)
            {
                containerPair.Value.ExecuteMethod(methodId, parameters);
            }
        }
    }
}
