using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CalculatorLibrary.Tests.Unit;

public class CalculatorTests : IAsyncLifetime //IDisposable 
{
    // Arrange
    // System Under Test - System that is being tested for tests
    private readonly Calculator _sut = new();
    private readonly ITestOutputHelper _outputHelper;

    public CalculatorTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;

        //_outputHelper.WriteLine("Constructor");
    }

    [Theory]
    [InlineData(5, 5, 10)]
    [InlineData(-5, 5, 0)]
    [InlineData(-15, -5, -20)]
    public void Add_ShouldAddTwoNumbers_WhenTwoNumbersAreIntegers(int number1, int number2, int expected)
    {
        // Act
        var result = _sut.Add(number1, number2);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5, 5, 0)]
    [InlineData(15, 5, 10)]
    [InlineData(-5, -5, 0)]
    [InlineData(-15, -5, -10)]
    [InlineData(5, 10, -5)]
    public void Subtract_ShouldSubtractTwoNumbers_WhenTwoNumbersAreIntegers(int number1, int number2, int expected)
    {
        // Act
        var result = _sut.Subtract(number1, number2);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5, 5, 25)]
    [InlineData(5, 0, 0)]
    [InlineData(-5, 5, -25)]
    public void Multiply_ShouldMultiplyTwoNumbers_WhenTwoNumbersAreIntegers(int number1, int number2, int expected)
    {
        // Act
        var result = _sut.Multiply(number1, number2);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5, 5, 1)]
    [InlineData(15, 5, 3)]
    public void Divide_ShouldDivideTwoNumbers_WhenTwoNumbersAreIntegers(int number1, int number2, int expected)
    {
        // Act
        var result = _sut.Divide(number1, number2);

        // Assert
        Assert.Equal(expected, result);
    }

    public async Task InitializeAsync()
    {
        _outputHelper.WriteLine("InitializeAsync");
        await Task.FromResult(0);
        //throw new NotImplementedException();
    }

    public async Task DisposeAsync()
    {
        _outputHelper.WriteLine("DisposeAsync");
        await Task.FromResult(0);
        //throw new NotImplementedException();
    }

    //public void Dispose()
    //{
    //    _outputHelper.WriteLine("Dispose");
    //}
}
