using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            return _objects.Keys
                .Select(x => new InstanceReference {Name = x})
                .ToArray();
        }
    }
}
