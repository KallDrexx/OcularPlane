using System;

namespace OcularPlane.Models
{
    public class InstanceDetails
    {
        public Guid InstanceId { get; set; }
        public string Name { get; set; }
        public PropertyReference[] Properties { get; set; }

        public InstanceDetails()
        {
            Properties = new PropertyReference[0];
        }
    }
}
