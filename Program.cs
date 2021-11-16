using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace primes_sieve
{
    class Program
    {
        static void Main(string[] args)
        {
            //args.Dump("args"); // "16252321" for example or "144449533"
            
            //int skip = 0, take = 30;
            //Primes3().Skip(skip).Take(take).Zip(Primes(CandidatesInPrimes0).Skip(skip).Take(take)).Dump();
            //return;
            
            //Candidates2()
            //	.Take(200)
            //	.Where(x => x.Number % 5 == 0 /*&& x.Index == 1*/)
            //	.Select(x => new { x.Frame, x.FrameId, x.Index, x.Number, div5 = x.Number / 5 })
            //	.Dump();
            //Candidates3().Take(200).Dump();
            
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
            Primes3().Skip(skipParsed).Take(5).ToList().Dump($"Primes3 after {skipStr} with (" + swtime() + ")");

            sw.Stop();
        }

        record Candidate(uint FrameId, uint Frame, uint Index, uint Number)
        { public Candidate() : this(0, 0, 0, 0) { } }

        IEnumerable<Candidate> Candidates2()
        {
            // Frame = 2 * 3
            
            // yield return 2;
            // yield return 3;
            // yield return 5;
            
            var i = 6U;
            while (true)
            {
                yield return new Candidate { FrameId = i / 6, Frame = i, Index = 1, Number = i + 1 };
                yield return new Candidate { FrameId = i / 6, Frame = i, Index = 5, Number = i + 5 };
                i += 6;
            }
        }

        private static IEnumerable<ulong> CandidatesInPrimes0()
        {
            for (ulong i = 2; i < ulong.MaxValue; ++i)
                yield return i;
        }
        private static IEnumerable<ulong> CandidatesInPrimes1()
        {
            yield return 2;
            for (ulong i = 3; i < ulong.MaxValue - 2; i += 2)
                yield return i;
        }
        private static IEnumerable<ulong> CandidatesInPrimes2()
        {
            yield return 2;
            yield return 3;
            yield return 5;
            const ulong frame = 2 * 3;
            for (ulong i = frame; i < ulong.MaxValue - frame; i += frame)
            {
                yield return i + 1;
                yield return i + 5;
            }
        }
        private static IEnumerable<ulong> CandidatesInPrimes3()
        {
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
            const ulong frame = 2 * 3 * 5;
            for (var i = frame; i < ulong.MaxValue - frame; i += frame)
            {
                yield return i + 1;
                yield return i + 7;
                yield return i + 11;
                yield return i + 13;
                yield return i + 17;
                yield return i + 19;
                yield return i + 23;
                yield return i + 29;
            }
        }

        private static IEnumerable<ulong> Primes(Func<IEnumerable<ulong>> getCandidates)
        {
            foreach (var n in getCandidates())
            {
                var sqrtn = (ulong)Math.Sqrt(n);
                var isPrime = true;
                foreach (var i in getCandidates())
                {
                    if (i > sqrtn)
                        break;
                    if (n % i == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                    yield return n;
            }
        }

        private static IEnumerable<ulong> Primes2()
        {
            yield return 2;
            yield return 3;
            yield return 5;

            const int N = 1_000_000;
            const int MaxNumber = N * 6;
            var lim = (int)(Math.Sqrt(MaxNumber) / 6);
            var sieve = new byte[N];
            /*
                [i] - candidate
                // 2 candidates per frame
                [0]&1 - 1 * 6 + 1 prime?0:1
                [0]&2 - 1 * 6 + 5 prime?0:1
                [1]&1 - 2 * 6 + 1 prime?0:1
                [1]&2 - 2 * 6 + 5 prime?0:1
                [2]&1 - 3 * 6 + 1 prime?0:1
                [2]&2 - 3 * 6 + 5 prime?0:1
                [3]&1 - 4 * 6 + 1 prime?0:1
                [3]&2 - 4 * 6 + 5 prime?0:1
            */

            // 0. отсеять 5 (из Frame0)
            // 1. пройти по решётке

            IEnumerable<(int frameId, byte index)> Filter5()
            {
                var i = 4;
                while (i < N - 6)
                {
                    yield return (i + 0, 1);
                    yield return (i + 1, 2);
                    yield return (i + 5, 1);
                    yield return (i + 6, 2);
                    i += 10;
                }
                if (i + 0 < N) yield return (i + 0, 1);
                if (i + 1 < N) yield return (i + 1, 2);
                if (i + 5 < N) yield return (i + 5, 1);
            }

            foreach (var x in Filter5())
            {
                sieve[x.frameId] |= x.index;
            }

            void MarkSieve(ulong prime)
            {
                // [ x = i*6 + 1]: если k*x % 6 == 1 или 5, то помечаем соответствующий элемент
                for (var n = prime + prime; n < N * 6; n += prime)
                {
                    switch (n % 6)
                    {
                        case 1:
                            sieve[n / 6] |= 1;
                            break;
                        case 5:
                            sieve[n / 6] |= 2;
                            break;
                    }
                }
            }

            for (var i = 1; i <= lim; ++i)
            {
                if ((sieve[i] & 1) == 0)
                {
                    // i * 6 + 1 is prime
                    var prime = ((ulong)i) * 6 + 1;
                    yield return prime;
                    MarkSieve(prime);
                }
                if ((sieve[i] & 2) == 0)
                {
                    // i * 6 + 5 is prime
                    var prime = ((ulong)i) * 6 + 5;
                    yield return prime;
                    MarkSieve(prime);
                }
            }

            for (var i = lim + 1; i < N; ++i)
            {
                if ((sieve[i] & 1) == 0)
                {
                    // i * 6 + 1 is prime
                    var prime = ((uint)i) * 6 + 1;
                    yield return prime;
                }
                if ((sieve[i] & 2) == 0)
                {
                    // i * 6 + 5 is prime
                    var prime = ((uint)i) * 6 + 5;
                    yield return prime;
                }
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

            const ulong N = 10_000_000;
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

            // ulong NumberSevenTimes(ulong prime) => prime + prime + prime + prime + prime + prime + prime;

            void MarkSieve(ulong prime)
            {
                // starting with 7*prime couse lesser prime multiplier is apriory excluded
                for (var n = 7 * prime; n < MaxNumber; n += prime)
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
            /*var bitsQueue = new ConcurrentQueue<(ulong frameId, byte bitIndex)>();
            void QueueMarkSieve(ulong prime)
            {
                for (var n = prime + prime; n < MaxNumber; n += prime)
                {
                    switch (n % Frame)
                    {
                        case 01: bitsQueue.Enqueue((n / Frame, 0x1)); break;
                        case 07: bitsQueue.Enqueue((n / Frame, 0x2)); break;
                        case 11: bitsQueue.Enqueue((n / Frame, 0x4)); break;
                        case 13: bitsQueue.Enqueue((n / Frame, 0x8)); break;
                        case 17: bitsQueue.Enqueue((n / Frame, 0x10)); break;
                        case 19: bitsQueue.Enqueue((n / Frame, 0x20)); break;
                        case 23: bitsQueue.Enqueue((n / Frame, 0x40)); break;
                        case 29: bitsQueue.Enqueue((n / Frame, 0x80)); break;
                    }
                }
            }

            for (var i = 1U; i <= lim; ++i, j += Frame)
            {
                var tasks = new Task<ulong>[8];
                tasks[0] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x01) == 0) { var prime = j + 01;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[1] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x02) == 0) { var prime = j + 07;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[2] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x04) == 0) { var prime = j + 11;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[3] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x08) == 0) { var prime = j + 13;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[4] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x10) == 0) { var prime = j + 17;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[5] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x20) == 0) { var prime = j + 19;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[6] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x40) == 0) { var prime = j + 23;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);
                tasks[7] = Task.Factory.StartNew<ulong>(() => { if ((sieve[i] & 0x80) == 0) { var prime = j + 29;  QueueMarkSieve(prime); return prime; } else return 0; }, TaskCreationOptions.LongRunning);

                using var cs = new CancellationTokenSource();
                var controlTask = Task.Factory.StartNew(o => {
                    var token = (CancellationToken)o;
                    while (!token.IsCancellationRequested)
                    {
                        while (bitsQueue.TryDequeue(out var bitsItem))
                        {
                            sieve[bitsItem.frameId] |= bitsItem.bitIndex;
                        }
                    }
                    while (bitsQueue.TryDequeue(out var bitsItem))
                    {
                        sieve[bitsItem.frameId] |= bitsItem.bitIndex;
                    }
                },
                cs.Token,
                TaskCreationOptions.LongRunning);

                Task.WaitAll(tasks);
                cs.Cancel();
                for (var ii = 0; ii < tasks.Length; ++ii)
                    if (tasks[ii].Result != 0)
                        yield return tasks[ii].Result;
                controlTask.Wait();
            }*/

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
