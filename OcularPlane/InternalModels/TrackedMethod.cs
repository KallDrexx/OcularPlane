using System;
using System.Reflection;
using OcularPlane.Models;

namespace OcularPlane.InternalModels
{
    class TrackedMethod
    {
        public Guid MethodId { get; set; }
        public object RelvantObject { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public MethodReference Reference { get; set; }
    }
}
