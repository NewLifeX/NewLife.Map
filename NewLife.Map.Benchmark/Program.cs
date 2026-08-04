using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using NewLife.Map.Benchmark;

// 使用 InProcess 工具链：避免生成外部项目重编译被测库（NewLife.Map 通配符 AssemblyVersion 与确定性冲突）
var config = ManualConfig.Create(DefaultConfig.Instance);
config.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));

BenchmarkRunner.Run<ConvertBenchmark>(config);
