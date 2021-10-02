using System.ComponentModel;

namespace EnumExample
{
    [GenerateEnumDescription]
    public enum TestEnum
    {
        [Description("First Test Enum")]
        FirstTestEnum,
        SecondTestEnum,
        [Description("Third Test Enum")]
        ThirdTestEnum
    }
}