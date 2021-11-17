using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace primes_sieve_source_generated
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();

            TimeSpan swtime()
            {
                sw.Stop();
                var result = sw.Elapsed;
                sw.Restart();
                return result;
            }
            sw.Start();

            var skipParsed = (args.Length == 1 && int.TryParse(args[0], out var skipRawParsed)) ? skipRawParsed : 200_000;
            var skipStr = skipParsed.ToString("### ### ### ### ###");

            //Primes(CandidatesInPrimes0).Skip(200_000).Take(5).ToList().Dump("Primes after 200 000 with candidates 0 (" + swtime() + ")");
            //Primes(CandidatesInPrimes1).Skip(200_000).Take(5).ToList().Dump("Primes after 200 000 with candidates 1 (" + swtime() + ")");
            //Primes(CandidatesInPrimes2).Skip(200_000).Take(5).ToList().Dump("Primes after 200 000 with candidates 2 (" + swtime() + ")");
            //Primes(CandidatesInPrimes3).Skip(skipParsed).Take(5).ToList().Dump($"Primes after {skipStr} with candidates 3 (" + swtime() + ")");

            //Primes2().Skip(200_000).Take(5).ToList().Dump("Primes2 after 200 000 with (" + swtime() + ")");

            Primes().Skip(skipParsed).Take(5).ToList().Dump($"Primes3 after {skipStr} with (" + swtime() + ")");

            sw.Stop();
        }

        public static partial IEnumerable<ulong> Primes();

        public static partial int FrameLevel();
        public static partial ulong SieveSize();
    }
}
