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
        private readonly ConcurrentDictionary<Guid, TrackedMethod> _methods = new ConcurrentDictionary<Guid, TrackedMethod>();

        public Guid AddObject(object obj, string name, Guid? parentId)
        {
            // We shouldn't make the name the unique ID - items can exist in a list and have the same name
            //var previousInstances = _objects.Values.Where(x => x.Name == name).ToArray();
            //foreach (var previousInstance in previousInstances)
            //{
            //    RemoveInstance(previousInstance.Id);
            //}

            var instance = new TrackedInstance(obj)
            {
                Name = name,
                ParentId = parentId
            };

            _objects.TryAdd(instance.Id, instance);
            return instance.Id;
        }

        public Guid AddMethod(Expression<Action> methodExpression, string name)
        {
            var previousMethods = _methods.Values.Where(x => x.Name == name).ToArray();
            foreach (var previousMethod in previousMethods)
            {
                RemoveMethod(previousMethod.MethodId);
            }

            var method = MethodExpressionParser.ParseToReference(methodExpression, name);
            _methods.TryAdd(method.MethodId, method);

            return method.MethodId;
        }

        public void ClearObjects()
        {
            _objects.Clear();
        }

        public InstanceReference[] GetInstances()
        {
            return _objects.Values
                .Select(x => new InstanceReference
                {
                    InstanceId = x.Id,
                    Name = x.Name,
                    TypeName = x.RawObject.GetType().FullName
                })
                .ToArray();
        }

        public InstanceDetails GetInstanceDetails(Guid instanceId)
        {
            TrackedInstance foundObject;
            _objects.TryGetValue(instanceId, out foundObject);

            if (foundObject == null)
            {
                return null;
            }

            var toReturn = new InstanceDetails
            {
                InstanceId = instanceId,
                Name = foundObject.Name,
                ParentInstanceId = foundObject.ParentId,
                Properties = foundObject.GetProperties(),
            };

            return toReturn;
        }

        public void SetInstancePropertyValue(Guid instanceId, string propertyName, string value)
        {
            TrackedInstance instance;
            _objects.TryGetValue(instanceId, out instance);
            if (instance == null)
            {
                // We don't have this instance.  May need to eventually throw here but for now let it pass
                return;
            }

            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;
            GetMemberInfo(propertyName, instance, out propertyInfo, out fieldInfo);

            var memberType = GetMemberType(propertyName, instance, propertyInfo, fieldInfo);
            var valueToSet = GetValueToSet(propertyName, value, instance, ref memberType);

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

            TrackedMethod method;
            _methods.TryGetValue(methodId, out method);
            if (method != null)
            {
                ExecuteMethod(method, parameterValues);
            }
        }

        public void RemoveInstanceByObject(object objectToRemove)
        {
            var kvp = _objects.FirstOrDefault(item => item.Value.RawObject == objectToRemove);
            if (kvp.Value != null)
            {
                TrackedInstance throwaway;
                _objects.TryRemove(kvp.Key, out throwaway);
            }
        }

        public void RemoveInstance(Guid instanceId)
        {
            TrackedInstance removedInstance;
            _objects.TryRemove(instanceId, out removedInstance);
        }

        public void RemoveMethod(Guid methodId)
        {
            TrackedMethod removedMethod;
            _methods.TryRemove(methodId, out removedMethod);
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
    }
}
