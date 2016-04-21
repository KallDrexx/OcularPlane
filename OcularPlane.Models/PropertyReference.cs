namespace OcularPlane.Models
{
    public class PropertyReference
    {
        public string Name { get; set; }
        public string ValueAsString { get; set; }
        public string TypeName { get; set; }
        public bool IsWritable { get; set; }
        public bool IsReadable { get; set; }
    }
}
