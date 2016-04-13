using System;

namespace OcularPlane.Exceptions
{
    public class InvalidValueConversionException : Exception
    {
        public Type InstanceType { get; }
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public string AttemptedValue { get; }

        public InvalidValueConversionException(Type instanceType, string propertyName, Type propertyType, string attemptedValue)
            :base($"Cannot convert '{attemptedValue}' into type '{propertyType}' for {instanceType}.{propertyName} ")
        {
            InstanceType = instanceType;
            PropertyName = propertyName;
            PropertyType = propertyType;
            AttemptedValue = attemptedValue;
        }
    }
}
