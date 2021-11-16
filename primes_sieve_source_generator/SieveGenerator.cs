using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace primes_sieve_source_generator
{
    [Generator]
    public class SieveGenerator : ISourceGenerator
    {
        public const int FrameLevel = 3;
        public const ulong SieveSize = 1_000_000;
        
        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine("Executing code generator...");
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", "Executing");
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            var frame = new Frame(FrameLevel);
            string source = $@"
using System;
using System.Collections.Generic;
using System.Linq;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    partial class {mainMethod.ContainingType.Name}
    {{
        public static partial IEnumerable<ulong> Primes()
        {{
            {string.Join(" ", frame.PrimesInFrame0.Select(n => $"yield return {n};"))}

            const ulong Frame = {frame.FrameVolume};
            const ulong N = {SieveSize.ToString("### ### ### ### ### ###").Trim().Replace(' ', '_')};
            const ulong MaxNumber = Frame * N;
            var lim = (ulong)(Math.Sqrt(MaxNumber) / Frame) + 1;
            var sieve = new byte[N];

            void MarkSieve(ulong prime)
            {{
                for (var n = {frame.PrimesInFrame0.Skip(frame.PrimesCount).First()} * prime; n < MaxNumber; n += prime)
                {{
                    switch (n % Frame)
                    {{
                        {string.Join(" ", Enumerable.Repeat(1UL, 1).Concat(frame.PrimesInFrame0.Skip(frame.PrimesCount)).Select((n, i) => $"case {n}: sieve[n / Frame] |= {1UL << i}; break;"))}
                        /*case 01: sieve[n / Frame] |= 0x1; break;
                        case 07: sieve[n / Frame] |= 0x2; break;
                        case 11: sieve[n / Frame] |= 0x4; break;
                        case 13: sieve[n / Frame] |= 0x8; break;
                        case 17: sieve[n / Frame] |= 0x10; break;
                        case 19: sieve[n / Frame] |= 0x20; break;
                        case 23: sieve[n / Frame] |= 0x40; break;
                        case 29: sieve[n / Frame] |= 0x80; break;*/
                    }}
                }}
            }}
            {string.Join(" ", frame.PrimesInFrame0.Skip(frame.PrimesCount).Select(n => $"MarkSieve({n});"))}
            var j = Frame;
            for (var i = 1U; i <= lim; ++i, j += Frame)
            {{
                {string.Join(" ", Enumerable.Repeat(1UL, 1).Concat(frame.PrimesInFrame0.Skip(frame.PrimesCount)).Select((n, i) => $"if ((sieve[i] & {1UL << i}) == 0) {{ var prime = j + {n}; yield return prime; MarkSieve(prime); }}"))}
                /*
                if ((sieve[i] & 0x01) == 0) {{ var prime = j + 01; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x02) == 0) {{ var prime = j + 07; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x04) == 0) {{ var prime = j + 11; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x08) == 0) {{ var prime = j + 13; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x10) == 0) {{ var prime = j + 17; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x20) == 0) {{ var prime = j + 19; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x40) == 0) {{ var prime = j + 23; yield return prime; MarkSieve(prime); }}
                if ((sieve[i] & 0x80) == 0) {{ var prime = j + 29; yield return prime; MarkSieve(prime); }}
                */
            }}

            j = (lim + 1) * Frame;
            for (var i = lim + 1; i < N; ++i, j += Frame)
            {{
                {string.Join(" ", Enumerable.Repeat(1UL, 1).Concat(frame.PrimesInFrame0.Skip(frame.PrimesCount)).Select((n, i) => $"if ((sieve[i] & {1UL << i}) == 0) {{ var prime = j + {n}; yield return prime; MarkSieve(prime); }}"))}
                /*
                if ((sieve[i] & 0x01) == 0) {{ yield return j + 01; }}
                if ((sieve[i] & 0x02) == 0) {{ yield return j + 07; }}
                if ((sieve[i] & 0x04) == 0) {{ yield return j + 11; }}
                if ((sieve[i] & 0x08) == 0) {{ yield return j + 13; }}
                if ((sieve[i] & 0x10) == 0) {{ yield return j + 17; }}
                if ((sieve[i] & 0x20) == 0) {{ yield return j + 19; }}
                if ((sieve[i] & 0x40) == 0) {{ yield return j + 23; }}
                if ((sieve[i] & 0x80) == 0) {{ yield return j + 29; }}
                */
            }}
            
            yield break;
        }}
    }}
}}
";

            context.AddSource("generatedSource.cs", SourceText.From(source, System.Text.Encoding.UTF8));
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Console.WriteLine("Initializing code generator...");
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", "Initializing");
        }
    }
}
