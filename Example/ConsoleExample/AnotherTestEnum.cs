using System.ComponentModel;

namespace EnumExample
{
    [GenerateEnumDescription]
    public enum AnotherTestEnum
    {
        [Description("Value One")]
        ValueOne,
        ValueTwo,
        [Description("Value Three")]
        ValueThree
    }
}