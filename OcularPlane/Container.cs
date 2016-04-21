using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OcularPlane.Exceptions;
using OcularPlane.InternalModels;
using OcularPlane.Models;

namespace OcularPlane
{
    class Container
    {
        private readonly ConcurrentDictionary<Guid, TrackedInstance> _objects = new ConcurrentDictionary<Guid, TrackedInstance>();
        private readonly ConcurrentDictionary<string, TrackedMethod> _methods = new ConcurrentDictionary<string, TrackedMethod>();

        public void AddObject(object obj, string name)
        {
            var previousInstances = _objects.Values.Where(x => x.Name == name).ToArray();
            foreach (var previousInstance in previousInstances)
            {
                RemoveInstance(previousInstance.Id);
            }

            var instance = new TrackedInstance(obj) {Name = name};
            _objects.TryAdd(instance.Id, instance);
        }

        public void AddMethod(Expression<Action> methodExpression, string name)
        {
            var method = MethodExpressionParser.ParseToReference(methodExpression, name);
            _methods.AddOrUpdate(name, method, (key, reference) => method);
        }

        public InstanceReference[] GetInstances()
        {
            var results = new List<InstanceReference>();
            foreach (var pair in _objects)
            {
                results.Add(new InstanceReference
                {
                    InstanceId = pair.Value.Id,
                    Name = pair.Value.Name,
                    TypeName = pair.Value.RawObject.GetType().FullName
                });
            }

            return results.ToArray();
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            var foundObject = _objects[instanceId];

            if(foundObject != null)
            {
                var toReturn = new InstanceDetails
                {
                    InstanceId = instanceId,
                    Name = foundObject.Name,
                };

                toReturn.Properties = foundObject.GetProperties();

                return toReturn;
            }
            return null;
            
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

            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;
            GetMemberInfo(propertyName, instance, out propertyInfo, out fieldInfo);


            Type memberType = GetMemberType(propertyName, instance, propertyInfo, fieldInfo);

            object valueToSet = GetValueToSet(propertyName, value, instance, ref memberType);

            if(fieldInfo != null)
            {
                // todo: handle setting on structs:
                // http://stackoverflow.com/questions/6280506/is-there-a-way-to-set-properties-on-struct-instances-using-reflection
                fieldInfo.SetValue(instance.RawObject, valueToSet);
            }
            else // propertyInfo != null
            {
                // todo: handle setting on structs:
                // http://stackoverflow.com/questions/6280506/is-there-a-way-to-set-properties-on-struct-instances-using-reflection
                propertyInfo.SetValue(instance.RawObject, valueToSet);
            }
        }

        public MethodReference[] GetMethods()
        {
            return _methods.Values.Select(x => x.Reference).ToArray();
        }

        public void ExecuteMethod(Guid methodId, Dictionary<string, string> parameterValues)
        {
            if (parameterValues == null)
            {
                throw new ArgumentNullException(nameof(parameterValues));
            }

            var method = _methods.Values
                .Where(x => x.MethodId == methodId)
                .SingleOrDefault();

            if (method == null)
            {
                return;
            }

            ExecuteMethod(method, parameterValues);
        }

        private object GetValueToSet(string propertyName, string value, TrackedInstance instance, ref Type memberType)
        {
            object valueToSet = null;
            if (value == null && (IsNullableType(memberType) || !memberType.IsValueType))
            {
                // keep it null
                valueToSet = null;
            }
            else
            {
                memberType = ExtractNullableUnderlyingType(memberType);
                var typeCode = GetCodeForType(memberType);
                if (typeCode != TypeCode.Empty)
                {
                    valueToSet = ConvertType(propertyName, value, typeCode, instance, memberType);

                }
                else
                {
                    if (memberType.IsEnum)
                    {
                        valueToSet = SetEnumPropertyValue(propertyName, value, memberType, instance);
                    }
                    else
                    {
                        throw new NoKnownWayToParseToTypeException(memberType);
                    }
                }
            }

            return valueToSet;
        }

        private static Type GetMemberType(string propertyName, TrackedInstance instance, PropertyInfo property, FieldInfo field)
        {
            Type memberType;
            if (property == null && field == null)
            {
                throw new PropertyDoesntExistException(instance.RawObject.GetType(), propertyName);
            }
            else if (property != null)
            {
                memberType = property.PropertyType;
            }
            else // if field != null
            {
                memberType = field.FieldType;
            }

            return memberType;
        }

        private static void GetMemberInfo(string propertyName, TrackedInstance instance, out PropertyInfo property, out FieldInfo field)
        {
            property = null;
            field = null;
            var rawObjectType = instance.RawObject.GetType();

            property = rawObjectType
                .GetProperties()
                .Where(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            if (property == null)
            {
                field = rawObjectType.GetFields().Where(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
            }
        }

        private object SetEnumPropertyValue(string propertyName, string value, Type propertyType, TrackedInstance instance)
        {
            if (propertyType.IsEnumDefined(value))
            {
                var parsedValue = Enum.Parse(propertyType, value);
                return parsedValue;
            }
            else
            {
                throw new InvalidPropertyValueConversionException(instance.RawObject.GetType(), propertyName, propertyType, value);
            }


        }

        private static object ConvertType(string propertyName,
            string value,
            TypeCode typeCode,
            TrackedInstance instance,
            Type propertyType)
        {
            object convertedValue;

            try
            {
                convertedValue = Convert.ChangeType(value, typeCode);
            }
            catch
            {
                throw new InvalidPropertyValueConversionException(instance.RawObject.GetType(), propertyName, propertyType, value);
            }

            return convertedValue;
        }


        private static Type ExtractNullableUnderlyingType(Type memberType)
        {
            if (IsNullableType(memberType))
            {
                memberType = Nullable.GetUnderlyingType(memberType);
            }
            return memberType;
        }

        private static bool IsNullableType(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof (Nullable<>);
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

        private static void ExecuteMethod(TrackedMethod trackedMethod, Dictionary<string, string> parameterValues)
        {
            var actualParameters = new List<object>();
            foreach (var paramInfo in trackedMethod.MethodInfo.GetParameters())
            {
                var valueAsString = parameterValues[paramInfo.Name];

                if (paramInfo.ParameterType.IsEnum)
                {
                    if (paramInfo.ParameterType.IsEnumDefined(valueAsString))
                    {
                        var enumValue = Enum.Parse(paramInfo.ParameterType, valueAsString);
                        actualParameters.Add(enumValue);
                    }
                    else
                    {
                        throw new InvalidParameterValueConversionException(trackedMethod.MethodInfo.Name, paramInfo.Name, paramInfo.ParameterType, valueAsString);
                    }
                }
                else
                {
                    var typeCode = GetCodeForType(paramInfo.ParameterType);
                    var realValue = Convert.ChangeType(valueAsString, typeCode);
                    actualParameters.Add(realValue);
                }
            }

            trackedMethod.MethodInfo.Invoke(trackedMethod.RelvantObject, actualParameters.ToArray());
        }

        public void RemoveInstanceByObject(object objectToRemove)
        {
            var kvp = _objects.FirstOrDefault(item => item.Value.RawObject == objectToRemove);
            if(kvp.Value != null)
            {
                TrackedInstance throwaway;
                _objects.TryRemove(kvp.Key, out throwaway);
            }
        }

        public void RemoveInstance(Guid instanceId)
        {
            var key =_objects.Where(x => x.Value.Id == instanceId)
                .Select(x => x.Key)
                .SingleOrDefault();

            if (key != null)
            {
                TrackedInstance removedInstance;
                _objects.TryRemove(key, out removedInstance);
            }
        }

        public void RemoveMethod(Guid methodId)
        {
            var key = _methods.Where(x => x.Value.MethodId == methodId)
                .Select(x => x.Key)
                .SingleOrDefault();

            if (key != null)
            {
                TrackedMethod removedMethod;
                _methods.TryRemove(key, out removedMethod);
            }
        }
    }
}
