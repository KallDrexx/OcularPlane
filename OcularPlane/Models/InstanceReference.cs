﻿using System;

namespace OcularPlane.Models
{
    public class InstanceReference
    {
        public Guid InstanceId { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
