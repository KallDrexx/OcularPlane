using System.Collections.Concurrent;
using System.Collections.Generic;
using OcularPlane.Models;

namespace OcularPlane
{
    class Container
    {
        private readonly ConcurrentDictionary<string, object> _objects = new ConcurrentDictionary<string, object>();

        public void AddObject(object obj, string name)
        {
            _objects.TryAdd(name, obj);
        }

        public InstanceReference[] GetInstances()
        {
            var results = new List<InstanceReference>();
            foreach (var pair in _objects)
            {
                results.Add(new InstanceReference
                {
                    Name = pair.Key,
                    Type = pair.Value.GetType()
                });
            }

            return results.ToArray();
        }
    }
}
