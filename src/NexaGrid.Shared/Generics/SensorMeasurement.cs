using System.Numerics;

namespace NexaGrid.Shared.Generics;

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