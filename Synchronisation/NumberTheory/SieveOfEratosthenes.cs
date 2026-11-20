using System.Threading;

namespace NumberTheory;

public static class SieveOfEratosthenes
{
    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a sequential approach.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersSequentialAlgorithm(int n)
    {
        List<int> returnList;
        bool[] listOfBools = GenerateList(n);
        int squareRootOfLength = (int)Math.Sqrt(n);
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "The given argument was null!");
        }

        EditTruthTable(listOfBools, squareRootOfLength, n);

        returnList = FillList(n, listOfBools);
        return returnList;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a modified sequential approach.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersModifiedSequentialAlgorithm(int n)
    {
        List<int> returnList;
        bool[] truthTable = GenerateList(n);
        int squareRootOfLength = (int)Math.Sqrt(n);
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "The given argument was null!");
        }

        // Phase 1
        EditTruthTable(truthTable, squareRootOfLength, squareRootOfLength);
        returnList = FillList(squareRootOfLength + 1, truthTable);

        // Phase 2
        EditSegmentTable(truthTable, returnList, squareRootOfLength, n);

        returnList = FillList(n, truthTable);
        return returnList;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a concurrent approach by data decomposition.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentDataDecomposition(int n)
    {
        List<int> returnList;
        bool[] truthTable = GenerateList(n);
        int squareRootOfLength = (int)Math.Sqrt(n);

        int segmentCount = 2;
        int[][] segmentIndexes = new int[segmentCount][];
        int segmentLength;
        int leftoverLength = n - squareRootOfLength;
        int baseSize = leftoverLength / segmentCount;
        int remainder = leftoverLength % segmentCount;
        int start;
        int end;
        int currentStart = squareRootOfLength + 1;
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "The given argument was null!");
        }

        EditTruthTable(truthTable, squareRootOfLength, squareRootOfLength);
        returnList = FillList(squareRootOfLength + 1, truthTable);

        // Phase 2: Data parralelsim
        for (int i = 0; i < segmentCount; i++)
        {
            segmentLength = baseSize + (i < remainder ? 1 : 0);
            start = currentStart;
            end = start + segmentLength - 1;
            segmentIndexes[i] = new int[2] { start, end };
            currentStart = end + 1;
        }

        var threadCollcetion = new List<Thread>();
        int numberOfAvaliableThreads = Environment.ProcessorCount;
        Queue<int[]> queue = new Queue<int[]>(segmentIndexes);
        object locker = new object();

        for (int i = 0; i < numberOfAvaliableThreads; i++)
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    int[] coords;
                    lock (locker)
                    {
                        if (queue.Count == 0)
                        {
                            break;
                        }

                        coords = queue.Dequeue();
                    }

                    EditSegmentTable(truthTable, returnList, coords[0], coords[1]);
                }
            });

            threadCollcetion.Add(thread);
            thread.Start();
        }

        foreach (var j in threadCollcetion)
        {
            j.Join();
        }

        returnList = FillList(n, truthTable);
        return returnList;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using a concurrent approach by "basic" primes decomposition.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentBasicPrimesDecomposition(int n)
    {
        List<int> returnList;
        bool[] truthTable = GenerateList(n);
        int squareRootOfLength = (int)Math.Sqrt(n);
        int segmentCount;
        int remainder;
        int baseSize;
        int segmentLength;
        int start;
        int end;
        int currentStart = 0;
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "The given argument was null!");
        }

        // Phase 1
        EditTruthTable(truthTable, squareRootOfLength, squareRootOfLength);
        returnList = FillList(squareRootOfLength + 1, truthTable);

        segmentCount = returnList.Count > 3 ? returnList.Count / 2 : returnList.Count;
        baseSize = segmentCount > 1 ? returnList.Count / segmentCount : 1;
        remainder = returnList.Count % segmentCount;

        int[][] segmentIndexes = new int[segmentCount][];
        for (int i = 0; i < segmentCount; i++)
        {
            segmentLength = baseSize + (i < remainder ? 1 : 0);
            start = currentStart;
            end = start + segmentLength - 1;
            segmentIndexes[i] = new int[2] { start, end };
            currentStart = end + 1;
        }

        var threadCollcetion = new List<Thread>();
        int numberOfAvaliableThreads = Environment.ProcessorCount;
        Queue<int[]> queue = new Queue<int[]>(segmentIndexes);
        object locker = new object();

        for (int i = 0; i < numberOfAvaliableThreads; i++)
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    int[] coords;
                    lock (locker)
                    {
                        if (queue.Count == 0)
                        {
                            break;
                        }

                        coords = queue.Dequeue();
                    }

                    for (int k = coords[0]; k <= coords[1]; k++)
                    {
                        EditSingleElement(truthTable, returnList[k], squareRootOfLength, n);
                    }
                }
            });

            threadCollcetion.Add(thread);
            thread.Start();
        }

        foreach (var j in threadCollcetion)
        {
            j.Join();
        }

        returnList = FillList(n, truthTable);
        return returnList;
    }

    /// <summary>
    /// Generates a sequence of prime numbers up to the specified limit using thread pool and signaling construct.
    /// </summary>
    /// <param name="n">The upper limit for generating prime numbers.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing prime numbers up to the specified limit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the input <paramref name="n"/> is less than or equal to 0.</exception>
    public static IEnumerable<int> GetPrimeNumbersConcurrentWithThreadPool(int n)
    {
        List<int> returnList;
        bool[] truthTable = GenerateList(n);
        int squareRootOfLength = (int)Math.Sqrt(n);

        int segmentCount = 2;
        int[][] segmentIndexes = new int[segmentCount][];
        int segmentLength;
        int leftoverLength = n - squareRootOfLength;
        int baseSize = leftoverLength / segmentCount;
        int remainder = leftoverLength % segmentCount;
        int start;
        int end;
        int currentStart = squareRootOfLength + 1;
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "The given argument was null!");
        }

        EditTruthTable(truthTable, squareRootOfLength, squareRootOfLength);
        returnList = FillList(squareRootOfLength + 1, truthTable);

        // Phase 2: Data parralelsim
        for (int i = 0; i < segmentCount; i++)
        {
            segmentLength = baseSize + (i < remainder ? 1 : 0);
            start = currentStart;
            end = start + segmentLength - 1;
            segmentIndexes[i] = new int[2] { start, end };
            currentStart = end + 1;
        }

        using (var countDownEventSync = new CountdownEvent(segmentCount))
        {
            foreach (var i in segmentIndexes)
            {
                _ = ThreadPool.QueueUserWorkItem(_ =>
                {
                    EditSegmentTable(truthTable, returnList, i[0], i[1]);
                    _ = countDownEventSync.Signal();
                });
            }

            countDownEventSync.Wait();
            countDownEventSync.Dispose();
        }

        returnList = FillList(n, truthTable);
        return returnList;
    }

    private static bool[] GenerateList(int n)
    {
        return Enumerable.Repeat(true, n + 1).ToArray();
    }

    private static List<int> FillList(int n, bool[] listOfBools)
    {
        List<int> returnList = new List<int>();
        for (int i = 2; i < n; i++)
        {
            if (listOfBools[i])
            {
                returnList.Add(i);
            }
        }

        return returnList;
    }

    private static void EditTruthTable(bool[] truthTable, int n, int j)
    {
        int index;
        int m = 0;
        for (int i = 2; i <= n; i++)
        {
            if (truthTable[i])
            {
                while (true)
                {
                    index = (int)Math.Pow(i, 2) + (i * m);
                    if (index > j)
                    {
                        break;
                    }

                    truthTable[index] = false;
                    m++;
                }

                m = 0;
            }
        }
    }

    private static void EditSingleElement(bool[] truthTable, int element, int n, int j)
    {
        int start;
        int index;
        int m = 1;
        start = Math.Max(element * element, (n + element - 1) / element * element);
        truthTable[start] = false;
        while (true)
        {
            index = start + (m * element);
            if (index > j)
            {
                break;
            }

            truthTable[index] = false;
            m++;
        }
    }

    private static void EditSegmentTable(bool[] segmentTable, List<int> elementList, int n, int j)
    {
        foreach (int element in elementList)
        {
            EditSingleElement(segmentTable, element, n, j);
        }
    }
}
