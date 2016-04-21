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
                try
                {
                    var reference = new PropertyReference();
                    reference.Name = propertyInfo.Name;
                    reference.TypeName = propertyInfo.PropertyType.FullName;
                    var valueAsObject = propertyInfo.GetValue(RawObject);
                    reference.ValueAsString = Convert.ToString(valueAsObject);

                    toReturn.Add(reference);
                }
                // This can happen if the property 
                catch(Exception e)
                {
                    int m = 3;
                }
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
