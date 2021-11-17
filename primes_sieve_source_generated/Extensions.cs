using System;
using System.Collections;

namespace primes_sieve_source_generated
{
    static class Extensions
    {
        public static void Dump (this object data, string description)
        {
            Console.WriteLine(description);
            switch (data)
            {
                case IEnumerable collection when data is not string:
                    var count = 0;
                    foreach (var item in collection)
                    {
                        Console.Write("    - ");
                        Console.WriteLine(item);
                        ++count;
                    }
                    Console.WriteLine("Total {0} items", count);
                    break;
                default:
                    Console.WriteLine(data);
                    break;
            }
        }
    }
}
