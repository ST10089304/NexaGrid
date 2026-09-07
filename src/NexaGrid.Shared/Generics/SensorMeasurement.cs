using System.Numerics;

namespace NexaGrid.Shared.Generics;
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
public readonly record struct SensorMeasurement<T>(
    string SensorIdentifier,
    T Value,
    string Unit)
    where T : struct, INumber<T>
{
    public static SensorMeasurement<T> operator +(
        SensorMeasurement<T> left,
        SensorMeasurement<T> right)
    {
        ValidateUnits(left, right);

        return new SensorMeasurement<T>(
            $"{left.SensorIdentifier}+{right.SensorIdentifier}",
            left.Value + right.Value,
            left.Unit);
    }
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
    public static SensorMeasurement<T> operator -(
        SensorMeasurement<T> left,
        SensorMeasurement<T> right)
    {
        ValidateUnits(left, right);

        return new SensorMeasurement<T>(
            $"{left.SensorIdentifier}-{right.SensorIdentifier}",
            left.Value - right.Value,
            left.Unit);
    }
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
    public static bool operator >(
        SensorMeasurement<T> left,
        SensorMeasurement<T> right)
    {
        ValidateUnits(left, right);
        return left.Value > right.Value;
    }

    public static bool operator <(
        SensorMeasurement<T> left,
        SensorMeasurement<T> right)
    {
        ValidateUnits(left, right);
        return left.Value < right.Value;
    }
/*Stack Overflow Community (2018) Best practices for structuring Models, Structs, and Generics in C# projects*/
    private static void ValidateUnits(
        SensorMeasurement<T> left,
        SensorMeasurement<T> right)
    {
        if (!string.Equals(
                left.Unit,
                right.Unit,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Sensor measurements must use the same unit.");
        }
    }
}