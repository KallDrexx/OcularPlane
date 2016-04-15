using System;

namespace OcularPlane.Models
{
    public class MethodReference
    {
        public string Name { get; set; }
        public ParameterReference[] Parameters { get; set; }
        public Guid MethodId { get; set; }

        public MethodReference()
        {
            Parameters = new ParameterReference[0];
        }
    }
}
