using System;

namespace OcularPlane.Exceptions
{
    public class InvalidPropertyValueConversionException : Exception
    {
        public Type InstanceType { get; }
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public string AttemptedValue { get; }

        public InvalidPropertyValueConversionException(Type instanceType, string propertyName, Type propertyType, string attemptedValue)
            :base($"Cannot convert '{attemptedValue}' into type '{propertyType}' for {instanceType}.{propertyName} ")
        {
            InstanceType = instanceType;
            PropertyName = propertyName;
            PropertyType = propertyType;
            AttemptedValue = attemptedValue;
        }
    }
}
