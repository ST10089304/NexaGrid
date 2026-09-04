namespace NexaGrid.Shared.Structures;

public static class TelemetryBatchProcessor
{
    public static T[][] CreateBatches<T>(
        IReadOnlyList<T> source,
        int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "The batch size must be greater than zero.");
        }

        if (source.Count == 0)
        {
            return [];
        }

        int batchCount =
            (int)Math.Ceiling(
                source.Count / (double)batchSize);

        // Jagged array: each inner array is a telemetry batch.
        var batches = new T[batchCount][];

        for (int batchIndex = 0;
             batchIndex < batchCount;
             batchIndex++)
        {
            int startingIndex = batchIndex * batchSize;

            int currentBatchSize = Math.Min(
                batchSize,
                source.Count - startingIndex);

            batches[batchIndex] =
                new T[currentBatchSize];

            for (int itemIndex = 0;
                 itemIndex < currentBatchSize;
                 itemIndex++)
            {
                batches[batchIndex][itemIndex] =
                    source[startingIndex + itemIndex];
            }
        }

        return batches;
    }

    public static List<T> FlattenToList<T>(
        T[][] batches)
    {
        int totalCapacity =
            batches.Sum(batch => batch.Length);

        var optimisedCollection =
            new List<T>(totalCapacity);

        foreach (T[] batch in batches)
        {
            optimisedCollection.AddRange(batch);
        }

        return optimisedCollection;
    }
}