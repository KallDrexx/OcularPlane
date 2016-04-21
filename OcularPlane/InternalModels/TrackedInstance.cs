using System;
using System.Linq;
using OcularPlane.Models;
using System.Collections.Generic;

namespace OcularPlane.InternalModels
{
    class TrackedInstance
    {
        public Guid Id { get; }
        public object RawObject { get; }
        public string Name { get; set; }

        public TrackedInstance(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            Id = Guid.NewGuid();
            RawObject = obj;
        }

        public PropertyReference[] GetProperties()
        {
            var type = RawObject.GetType();
            var propertyInfos = type.GetProperties();
            List<PropertyReference> toReturn = new List<PropertyReference>();

            foreach(var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetIndexParameters().Any())
                {
                    // Ignore indexer properties
                    continue;
                }

                var reference = new PropertyReference
                {
                    Name = propertyInfo.Name,
                    IsWritable = propertyInfo.CanWrite,
                    IsReadable = propertyInfo.CanRead,
                    TypeName = propertyInfo.PropertyType.FullName
                };

                if (propertyInfo.CanRead)
                {
                    var valueAsObject = propertyInfo.GetValue(RawObject);
                    reference.ValueAsString = Convert.ToString(valueAsObject);
                }

                toReturn.Add(reference);
            }

            var fields = type
                .GetFields()
                .Select(x => new PropertyReference
                {
                    Name = x.Name,
                    TypeName = x.FieldType.FullName,
                    ValueAsString = Convert.ToString(x.GetValue(RawObject))
                })
                .ToList();

            toReturn.AddRange(fields);
            
            return toReturn.ToArray();

        }

    }
}
