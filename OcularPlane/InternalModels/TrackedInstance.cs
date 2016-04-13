using System;
using System.Linq;
using OcularPlane.Models;

namespace OcularPlane.InternalModels
{
    class TrackedInstance
    {
        public Guid Id { get; }
        public object RawObject { get; }

        public TrackedInstance(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Id = Guid.NewGuid();
            RawObject = obj;
        }

        public PropertyReference[] GetProperties()
        {
            // TODO: Should cache for performance
            return RawObject.GetType()
                .GetProperties()
                .Select(x => new PropertyReference
                {
                    Name = x.Name,
                    Type = x.PropertyType,
                    ValueAsString = Convert.ToString(x.GetValue(RawObject))
                })
                .ToArray();
        }
    }
}
