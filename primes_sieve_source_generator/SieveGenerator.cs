using System;
using System.Collections;
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
        private static readonly Frame Frame = new Frame(FrameLevel);
        private static readonly Type TypeOfSieve = typeof(byte);
        private static readonly Func<string> SieveItemConstructor = TypeOfSieve switch
        {
            {} when TypeOfSieve == typeof(byte) => null,
            {} when TypeOfSieve == typeof(ulong) => null,
            {} when TypeOfSieve == typeof(BitArray) => () => $"new BitArray({Frame.CandidatesPerFrame.Length})",
            _ => throw new NotImplementedException($"Not implemented work with sieve of type {TypeOfSieve.FullName}"),
        };

        private static readonly Func<ulong, int, string> MarkSieveItemCode = TypeOfSieve switch
        {
            {} when TypeOfSieve == typeof(byte) => (n, i) => $" |= 0x{1 << i:x2}",
            {} when TypeOfSieve == typeof(ulong) => (n, i) => $" |= 0x{1UL << i:x16}",
            {} when TypeOfSieve == typeof(BitArray) => (n, i) => $".Set({i:0000}, true)",
            _ => throw new NotImplementedException($"Not implemented work with sieve of type {TypeOfSieve.FullName}"),
        };
        
        private static readonly Func<ulong, int, string> GetSieveItemCode = TypeOfSieve switch
        {
            {} when TypeOfSieve == typeof(byte) => (n, i) => $"(sieve[i] & 0x{1UL << i:x2}) == 0",
            {} when TypeOfSieve == typeof(ulong) => (n, i) => $"(sieve[i] & 0x{1UL << i:x16}) == 0",
            {} when TypeOfSieve == typeof(BitArray) => (n, i) => $"!sieve[i].Get({i:0000})",
            _ => throw new NotImplementedException($"Not implemented work with sieve of type {TypeOfSieve.FullName}"),
        };
        
        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine("Executing code generator...");
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", "Executing");
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            string source = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using {TypeOfSieve.Namespace};

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    partial class {mainMethod.ContainingType.Name}
    {{
        public static partial int FrameLevel() => {FrameLevel};
        public static partial ulong SieveSize() => {SieveSize};
        
        public static partial IEnumerable<ulong> Primes()
        {{
            {string.Join("\n            ", Frame.PrimesInFrame0.Select(n => $"yield return {n};"))}

            const ulong Frame = {Frame.FrameVolume};
            const ulong N = {SieveSize.ToString("### ### ### ### ### ###").Trim().Replace(' ', '_')};
            const ulong MaxNumber = Frame * N;
            var lim = (ulong)(Math.Sqrt(MaxNumber) / Frame) + 1;
            var sieve = new {TypeOfSieve.Name}[N];
            {(SieveItemConstructor == null ? "" : $@"
            for (var i = 0; i < sieve.Length; ++i)
                sieve[i] = {SieveItemConstructor()};")}

            void MarkSieve(ulong prime)
            {{
                for (var n = {Frame.PrimesInFrame0.Skip(Frame.PrimesCount).First()} * prime; n < MaxNumber; n += prime)
                {{
                    switch (n % Frame)
                    {{
                        {PrintCode(24, (n, i) => $"case {n:0000}: sieve[n / Frame]{MarkSieveItemCode(n, i)}; break;")}
                    }}
                }}
            }}
            {string.Join("\n            ", Frame.PrimesInFrame0.Skip(Frame.PrimesCount).Select(n => $"MarkSieve({n});"))}
            var j = Frame;
            for (var i = 1U; i <= lim; ++i, j += Frame)
            {{
                {PrintCode(16, (n, i) => $"if ({GetSieveItemCode(n, i)}) {{ var prime = j + {n:0000}; yield return prime; MarkSieve(prime); }}")}
            }}

            j = (lim + 1) * Frame;
            for (var i = lim + 1; i < N; ++i, j += Frame)
            {{
                {PrintCode(16, (n, i) => $"if ({GetSieveItemCode(n, i)}) {{ yield return j + {n:0000}; }}")}
            }}
            
            yield break;
        }}
    }}
}}
";

            context.AddSource("generatedSource.cs", source);
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", source);
        }

        private static string PrintCode(int identation, Func<ulong, int, string> codeString)
        {
            return string.Join("\n" + new string(' ', identation), Frame.CandidatesPerFrame.Select(codeString));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Console.WriteLine("Initializing code generator...");
            System.IO.File.WriteAllText("/tmp/codegeneratorlog.txt", "Initializing");
        }
    }
}
