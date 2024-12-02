using System.Buffers;

int[] leftArray = null!, rightArray = null!;

var filePath = Path.Combine(Directory.GetCurrentDirectory(), "file.txt");

try
{ 
    (leftArray, rightArray, var count) = ParseFile(filePath);
    var difference = CalculateDifference(leftArray, rightArray, count);
    Console.WriteLine(difference);
    var similarity = CalculateSimilarityScore(leftArray, rightArray, count);
    Console.WriteLine(similarity);
}
finally
{
  ArrayPool<int>.Shared.Return(leftArray);
  ArrayPool<int>.Shared.Return(rightArray);  
}

return;

static long CalculateSimilarityScore(int[] leftList, int[] rightList, int count)
{
    // Build a frequency dictionary for the right list
    var frequencyMap = new Dictionary<int, int>();
    for (var i = 0; i < count; i++)
    {
        if (frequencyMap.TryGetValue(rightList[i], out var freq))
            frequencyMap[rightList[i]] = freq + 1;
        else
            frequencyMap[rightList[i]] = 1;
    }

    // Calculate the similarity score
    long similarityScore = 0;
    for (var i = 0; i < count; i++)
    {
        if (frequencyMap.TryGetValue(leftList[i], out var freq))
            similarityScore += (long)leftList[i] * freq;
    }

    return similarityScore;
}

static (int[] leftList, int[] rightList, int count) ParseFile(string filePath)
{
    var contentSpan = File.ReadAllText(filePath).AsSpan();

    // Initial buffer size (can grow as needed)
    var leftList = ArrayPool<int>.Shared.Rent(1001);
    var rightList = ArrayPool<int>.Shared.Rent(1001);

    var count = 0;

    try
    {
        // Parse the file
        foreach (var line in contentSpan.EnumerateLines())
        {
            if (line.IsEmpty) continue;

            var lineSpan = line.Trim();
            var spaceIndex = lineSpan.IndexOf(' ');

            if (spaceIndex < 0)
                throw new FormatException("Invalid line format. Expected two numbers separated by a space.");

            // Resize buffers if necessary
            if (count >= leftList.Length)
            {
                var newSize = leftList.Length * 2;
                Array.Resize(ref leftList, newSize);
                Array.Resize(ref rightList, newSize);
            }

            // Parse two numbers
            leftList[count] = int.Parse(lineSpan[..spaceIndex]);
            rightList[count] = int.Parse(lineSpan.Slice(spaceIndex + 1));
            count++;
        }

        // Return parsed data
        return (leftList, rightList, count);
    }
    catch
    {
        // Return arrays to the pool if an exception occurs
        ArrayPool<int>.Shared.Return(leftList);
        ArrayPool<int>.Shared.Return(rightList);
        return ([], [], 0);
    }
}

static int CalculateDifference(int[] buffer1, int[] buffer2, int count)
{
        // Sort the columns
        Array.Sort(buffer1, 0, count);
        Array.Sort(buffer2, 0, count);

        // Calculate sum of absolute differences
        var sum = 0;
        for (var i = 0; i < count; i++)
        {
            sum += Math.Abs(buffer1[i] - buffer2[i]);
        }
        return sum;
}
