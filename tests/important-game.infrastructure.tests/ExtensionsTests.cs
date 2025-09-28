using System.ComponentModel;
using FluentAssertions;
using important_game.infrastructure.Extensions;
using Xunit;

namespace important_game.infrastructure.tests.Extensions;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Paris Saint-Germain", "paris-saint-germain")]
    [InlineData(" Atlético  Madrid! ", "atletico-madrid")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void GenerateSlug_NormalizesInputs(string? input, string expected)
    {
        var result = SlugHelper.GenerateSlug(input ?? string.Empty);
        result.Should().Be(expected);
    }
}

public class EnumExtensionTests
{
    private enum SampleEnum
    {
        [Description("First value")]
        ValueWithDescription,
        ValueWithoutDescription
    }

    [Fact]
    public void GetDescription_ReturnsAttributeWhenPresent()
    {
        SampleEnum.ValueWithDescription.GetDescription().Should().Be("First value");
    }

    [Fact]
    public void GetDescription_ReturnsNullWhenMissing()
    {
        SampleEnum.ValueWithoutDescription.GetDescription().Should().BeNull();
    }
}
