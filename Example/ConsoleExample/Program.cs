using System;

namespace EnumExample
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(TestEnum.FirstTestEnum.GetDescription());
            Console.WriteLine(TestEnum.SecondTestEnum.GetDescription());
            Console.WriteLine(TestEnum.ThirdTestEnum.GetDescription());

            Console.WriteLine(AnotherTestEnum.ValueOne.GetDescription());
            Console.WriteLine(AnotherTestEnum.ValueTwo.GetDescription());
            Console.WriteLine(AnotherTestEnum.ValueThree.GetDescription());
        }
    }
}


