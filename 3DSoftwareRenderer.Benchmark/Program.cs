using BenchmarkDotNet.Running;
using SoftwareRenderer3D.FragmentShaders;
using SoftwareRenderer3D.Rasterizers;

namespace Benchmark;
public class Benchmark
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting benchmark");
        //BenchmarkRunner.Run<SimpleFragmentShader>();
        BenchmarkRunner.Run<ScanLineRasterizer>();

    }
}