using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace primes_sieve_source_generator
{
    class Frame
    {
        public int PrimesCount { get; }
        public ulong[] Primes { get; }
        public ulong FrameVolume { get; }
        public ulong[] PrimesInFrame0 { get; }
        private ulong[] _rawCandidatesPerFrame { get; }
        public ulong[] CandidatesPerFrame { get; }
        public double CandidatesPersent { get; }
        public double CandidatesPerBytesPersent { get; }

        public Frame(int primesCount)
        {
            PrimesCount = primesCount;
            Primes = Primes3().Take(PrimesCount).ToArray();
            FrameVolume = Primes.Aggregate(1UL, (seed, i) => seed * i);
            PrimesInFrame0 = Primes3().TakeWhile(n => n < FrameVolume).ToArray();
            _rawCandidatesPerFrame = PrimesInFrame0.Skip(PrimesCount).ToArray();
            CandidatesPerFrame = /*UserQuery.DistinctOrdererd*/(
                Enumerable.Repeat(1UL, 1)
                    .Concat(_rawCandidatesPerFrame)
                    .Concat(PrimesCombinations())
                    .OrderBy(e => e)
                    .Distinct()
                )
                .ToArray();
            CandidatesPersent = 100.0 * CandidatesPerFrame.Length / FrameVolume;
            CandidatesPerBytesPersent = 100.0 * CandidatesPerFrame.Length / (CandidatesPerFrame.Length % 8 == 0 ? CandidatesPerFrame.Length : (CandidatesPerFrame.Length + (8 - (CandidatesPerFrame.Length % 8))));
        }

        private IEnumerable<ulong> PrimesCombinations()
        {
            foreach (var element in _rawCandidatesPerFrame)
            {
                foreach (var x in PrimesCombinations(element))
                {
                    yield return x;
                }
            }
        }
        
        private IEnumerable<ulong> PrimesCombinations(ulong prime)
        {
            foreach (var element in _rawCandidatesPerFrame)
            {
                if (prime * element < FrameVolume)
                {
                    yield return prime * element;
                    foreach (var x in PrimesCombinations(prime * element))
                        yield return x;
                }
                else break;
            }
        }
        
        private static IEnumerable<ulong> Primes3()
        {
            const ulong Frame = 2 * 3 * 5; // = 30
            
            yield return 2;
            yield return 3;
            yield return 5;
            yield return 7;
            yield return 11;
            yield return 13;
            yield return 17;
            yield return 19;
            yield return 23;
            yield return 29;

            const ulong N = 1_000_000;
            const ulong MaxNumber = Frame * N;
            var lim = (ulong)(Math.Sqrt(MaxNumber) / Frame) + 1;
            //lim = N;
            var sieve = new byte[N];
            /*
                [i] - candidate
                // 8 candidates per frame
                [0]&001 - 1 * 30 + 01 prime?0:1
                [0]&002 - 1 * 30 + 07 prime?0:1
                [0]&004 - 1 * 30 + 11 prime?0:1
                [0]&008 - 1 * 30 + 13 prime?0:1
                [0]&016 - 1 * 30 + 17 prime?0:1
                [0]&032 - 1 * 30 + 19 prime?0:1
                [0]&064 - 1 * 30 + 23 prime?0:1
                [0]&128 - 1 * 30 + 29 prime?0:1
            */

            void MarkSieve(ulong prime)
            {
                for (var n = prime + prime; n < MaxNumber; n += prime)
                {
                    switch (n % Frame)
                    {
                        case 01: sieve[n / Frame] |= 0x1; break;
                        case 07: sieve[n / Frame] |= 0x2; break;
                        case 11: sieve[n / Frame] |= 0x4; break;
                        case 13: sieve[n / Frame] |= 0x8; break;
                        case 17: sieve[n / Frame] |= 0x10; break;
                        case 19: sieve[n / Frame] |= 0x20; break;
                        case 23: sieve[n / Frame] |= 0x40; break;
                        case 29: sieve[n / Frame] |= 0x80; break;
                    }
                }
            }
            MarkSieve(7);
            MarkSieve(11);
            MarkSieve(13);
            MarkSieve(17);
            MarkSieve(19);
            MarkSieve(23);
            MarkSieve(29);

            var j = Frame;
            for (var i = 1U; i <= lim; ++i, j += Frame)
            {
                if ((sieve[i] & 0x01) == 0) { var prime = j + 01; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x02) == 0) { var prime = j + 07; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x04) == 0) { var prime = j + 11; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x08) == 0) { var prime = j + 13; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x10) == 0) { var prime = j + 17; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x20) == 0) { var prime = j + 19; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x40) == 0) { var prime = j + 23; yield return prime; MarkSieve(prime); }
                if ((sieve[i] & 0x80) == 0) { var prime = j + 29; yield return prime; MarkSieve(prime); }
            }

            j = (lim + 1) * Frame;
            for (var i = lim + 1; i < N; ++i, j += Frame)
            {
                if ((sieve[i] & 0x01) == 0) { yield return j + 01; }
                if ((sieve[i] & 0x02) == 0) { yield return j + 07; }
                if ((sieve[i] & 0x04) == 0) { yield return j + 11; }
                if ((sieve[i] & 0x08) == 0) { yield return j + 13; }
                if ((sieve[i] & 0x10) == 0) { yield return j + 17; }
                if ((sieve[i] & 0x20) == 0) { yield return j + 19; }
                if ((sieve[i] & 0x40) == 0) { yield return j + 23; }
                if ((sieve[i] & 0x80) == 0) { yield return j + 29; }
            }
        }
    }

}
