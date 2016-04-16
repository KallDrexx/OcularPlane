using System;

namespace OcularPlane.Exceptions
{
    public class InvalidParameterValueConversionException : Exception
    {
        public string MethodName { get; }
        public string ParameterName { get; }
        public string ParameterTypeName { get; }
        public string AttemptedValue { get; }

        public InvalidParameterValueConversionException(string methodName, string parameterName, Type parameterType, string attemptedValue)
            :base($"Cannot convert '{attemptedValue}' into parameter '{parameterName}' of type '{parameterType.FullName}' for method '{methodName}'")
        {
            MethodName = methodName;
            ParameterName = parameterName;
            ParameterTypeName = parameterType.FullName;
            AttemptedValue = attemptedValue;
        }
    }
}
