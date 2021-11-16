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
        public const int FrameLevel = 4;
        public const ulong SieveSize = 10_000_000;
        
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
            {string.Join("\n            ", frame.PrimesInFrame0.Select(n => $"yield return {n};"))}

            const ulong Frame = {frame.FrameVolume};
            const ulong N = {SieveSize.ToString("### ### ### ### ### ###").Trim().Replace(' ', '_')};
            const ulong MaxNumber = Frame * N;
            var lim = (ulong)(Math.Sqrt(MaxNumber) / Frame) + 1;
            var sieve = new ulong[N];

            void MarkSieve(ulong prime)
            {{
                for (var n = {frame.PrimesInFrame0.Skip(frame.PrimesCount).First()} * prime; n < MaxNumber; n += prime)
                {{
                    switch (n % Frame)
                    {{
                        {PrintCode(24, (n, i) => $"case {n:0000}: sieve[n / Frame] |= 0x{1UL << i:x4}; break;", frame)}
                    }}
                }}
            }}
            {string.Join("\n            ", frame.PrimesInFrame0.Skip(frame.PrimesCount).Select(n => $"MarkSieve({n});"))}
            var j = Frame;
            for (var i = 1U; i <= lim; ++i, j += Frame)
            {{
                {PrintCode(16, (n, i) => $"if ((sieve[i] & 0x{1UL << i:x4}) == 0) {{ var prime = j + {n:0000}; yield return prime; MarkSieve(prime); }}", frame)}
            }}

            j = (lim + 1) * Frame;
            for (var i = lim + 1; i < N; ++i, j += Frame)
            {{
                {PrintCode(16, (n, i) => $"if ((sieve[i] & 0x{1UL << i:x4}) == 0) {{ yield return j + {n:0000}; }}", frame)}
            }}
            
            yield break;
        }}
    }}
}}
";

            context.AddSource("generatedSource.cs", SourceText.From(source, System.Text.Encoding.UTF8));
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", source);
        }

        private static string PrintCode(int identation, Func<ulong, int, string> codeString, Frame frame)
        {
            return string.Join("\n" + new string(' ', identation), frame.CandidatesPerFrame.Select(codeString));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Console.WriteLine("Initializing code generator...");
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", "Initializing");
        }
    }
}
