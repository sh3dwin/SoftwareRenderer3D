using BenchmarkDotNet.Running;
using SoftwareRenderer3D.FragmentShaders;

namespace Benchmark;
public class Benchmark
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting benchmark");
        BenchmarkRunner.Run<SimpleFragmentShader>();
    }
}