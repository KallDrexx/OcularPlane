using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OcularPlane.Exceptions;
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

        public void SetInstancePropertyValue(Guid instanceId, string propertyName, string value)
        {
            var instance = _objects.Where(x => x.Value.Id == instanceId)
                .Select(x => x.Value)
                .SingleOrDefault();

            if (instance == null)
            {
                // We don't have this instance.  May need to eventually throw here but for now let it pass
                return;
            }

            var property = instance.RawObject
                .GetType()
                .GetProperties()
                .Where(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            if (property == null)
            {
                throw new PropertyDoesntExistException(instance.RawObject.GetType(), propertyName);
            }

            var typeCode = GetCodeForType(property.PropertyType);
            if (typeCode != TypeCode.Empty)
            {
                object convertedValue;

                try
                {
                    convertedValue = Convert.ChangeType(value, typeCode);
                }
                catch
                {
                    throw new InvalidValueConversionException(instance.RawObject.GetType(), propertyName, property.PropertyType, value);
                }

                property.SetValue(instance.RawObject, convertedValue);
            }
            else
            {
                if (property.PropertyType.IsEnum)
                {
                    if (property.PropertyType.IsEnumDefined(value))
                    {
                        var parsedValue = Enum.Parse(property.PropertyType, value);
                        property.SetValue(instance.RawObject, parsedValue);
                    }
                    else
                    {
                        throw new InvalidValueConversionException(instance.RawObject.GetType(), propertyName, property.PropertyType, value);
                    }
                }
                else
                {
                    throw new NoKnownWayToParseToTypeException(property.PropertyType);
                }
            }
        }

        private static TypeCode GetCodeForType(Type type)
        {
            var map = new Dictionary<Type, TypeCode>
            {
                {typeof (bool), TypeCode.Boolean},
                {typeof (byte), TypeCode.Byte},
                {typeof (char), TypeCode.Char},
                {typeof (DateTime), TypeCode.DateTime},
                {typeof (DBNull), TypeCode.DBNull},
                {typeof (decimal), TypeCode.Decimal},
                {typeof (double), TypeCode.Double},
                {typeof (short), TypeCode.Int16},
                {typeof (int), TypeCode.Int32},
                {typeof (long), TypeCode.Int64},
                {typeof (object), TypeCode.Object},
                {typeof (sbyte), TypeCode.SByte},
                {typeof (Single), TypeCode.Single},
                {typeof (string), TypeCode.String},
                {typeof (UInt16), TypeCode.UInt16},
                {typeof (UInt32), TypeCode.UInt32},
                {typeof (UInt64), TypeCode.UInt64}
            };

            TypeCode code;
            return map.TryGetValue(type, out code)
                ? code
                : TypeCode.Empty;
        }
    }
}
