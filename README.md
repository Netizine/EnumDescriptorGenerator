# Enum to String Source Generator

The source generator generates a `GetDescription` method for enumerations that have the `[GenerateEnumDescription]` defined

## Get Started
Add a reference in your project to the EnumDescriptorGenerator project and then set `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"` as shown below.
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\EnumDescriptorGenerator\EnumDescriptorGenerator.csproj"
					OutputItemType="Analyzer"
				  ReferenceOutputAssembly="false"/>
</ItemGroup>
```
All enums with `[GenerateEnumDescription]` will now be selected to be processed by the Source Generator

> You can customize the enum value using the `[Description("First Test Enum")]` attribute. If none is set `nameof(EnumExample.TestEnum.SecondTestEnum)` will be returned

## Example

```csharp
[GenerateEnumDescription]
public enum TestEnum
{
	[Description("First Test Enum")]
	FirstTestEnum,
	SecondTestEnum,
	[Description("Third Test Enum")]
	ThirdTestEnum
}
```

This example will then generate the following code for you:

```csharp
namespace System
{
    public static class EnumStringExtensions
    {

        public static string GetDescription(this EnumExample.TestEnum value)
        {
            return value switch
            {
                EnumExample.TestEnum.FirstTestEnum => "First Test Enum",
                EnumExample.TestEnum.SecondTestEnum => nameof(EnumExample.TestEnum.SecondTestEnum),
                EnumExample.TestEnum.ThirdTestEnum => "Third Test Enum",

                _ => value.ToString()
            };
        }

    }
}
```

To test the output, you can simply call `GetDescription()` as shown below
```csharp
TestEnum.FirstTestEnum.GetDescription();
```