using System;

namespace OcularPlane.Exceptions
{
    public class NoKnownWayToParseToTypeException : Exception
    {
        public Type DestinationType { get; }

        public NoKnownWayToParseToTypeException(Type destinationType)
            :base($"No known way to parse a string to type {destinationType}")
        {
            DestinationType = destinationType;
        }
    }
}
