using System;

namespace OcularPlane.Exceptions
{
    public class PropertyDoesntExistException : Exception
    {
        public Type InstanceType { get; }
        public string PropertyName { get; }

        public PropertyDoesntExistException(Type instanceType, string propertyName)
            :base($"Property '{propertyName}' does not exist for type {instanceType}")
        {
            InstanceType = instanceType;
            PropertyName = propertyName;
        }
    }
}
