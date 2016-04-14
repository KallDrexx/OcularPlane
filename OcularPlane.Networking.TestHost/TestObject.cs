namespace OcularPlane.Networking.TestHost
{
    class TestObject
    {

        public float FloatField;

        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public MyEnum EnumValue { get; set; }

        public enum MyEnum
        {
            Unspecified,
            Value1,
            Value2
        }
    }
}
