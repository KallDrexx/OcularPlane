using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OcularPlane.InternalModels;
using OcularPlane.Models;

namespace OcularPlane
{
    class Container
    {
        private readonly ConcurrentDictionary<string, TrackedInstance> _objects = new ConcurrentDictionary<string, TrackedInstance>();

        public void AddObject(object obj, string name)
        {
            var instance = new TrackedInstance(obj);
            _objects.AddOrUpdate(name, instance, (key, oldObject) => instance);
        }

        public InstanceReference[] GetInstances()
        {
            var results = new List<InstanceReference>();
            foreach (var pair in _objects)
            {
                results.Add(new InstanceReference
                {
                    InstanceId = pair.Value.Id,
                    Name = pair.Key,
                    Type = pair.Value.RawObject.GetType()
                });
            }

            return results.ToArray();
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            return _objects.Where(x => x.Value.Id == instanceId)
                .Select(x => new InstanceDetails
                {
                    InstanceId = instanceId,
                    Name = x.Key,
                    Properties = x.Value.GetProperties()
                })
                .SingleOrDefault();
        }
    }
}
