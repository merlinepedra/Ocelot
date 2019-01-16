using System;
using System.Collections.Generic;

namespace Ocelot.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Validators;
    using Configuration;
    using Configuration.Builder;
    using Configuration.Creator;
    using Configuration.File;
    using DownstreamRouteFinder.UrlMatcher;
    using Microsoft.AspNetCore.Http;
    using Tests.Utility;

    [Config(typeof(UrlPathToUrlPathTemplateMatcherBenchmarks))]
    public class UrlPathToUrlPathTemplateMatcherBenchmarks : ManualConfig
    {
        private RegExUrlMatcher _urlPathMatcher;
        private ReRoute _reRoute;
        private string _downstreamUrlPath;
        private string _upstreamQuery;

        public UrlPathToUrlPathTemplateMatcherBenchmarks()
        {
            Add(StatisticColumn.AllStatistics);
            Add(MemoryDiagnoser.Default);
            Add(BaselineValidator.FailOnError);
        }

        [GlobalSetup]
        public void SetUp()
        {
            _urlPathMatcher = new RegExUrlMatcher();
            _reRoute = new ReRouteBuilder().WithUpstreamPathTemplate(
                    new UpstreamTemplatePatternCreator().Create(
                        new FileReRoute
                        {
                            UpstreamPathTemplate = "/api/product/products/{productId}/variants/"
                        }
                    )
                )
                .WithUpstreamHttpMethod(new List<string>())
                .WithUpstreamHost(string.Empty)
                .Build();
            _downstreamUrlPath = "/api/product/products/1/variants/?soldout=false";
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            _urlPathMatcher.Match(
                new FakeHttpRequest
                {
                    Path = new PathString(_downstreamUrlPath),
                    QueryString = new QueryString(_upstreamQuery)
                }, _reRoute);
        }

        // * Summary *

        // BenchmarkDotNet=v0.10.13, OS=macOS 10.12.6 (16G1212) [Darwin 16.7.0]
        // Intel Core i5-4278U CPU 2.60GHz (Haswell), 1 CPU, 4 logical cores and 2 physical cores
        // .NET Core SDK=2.1.4
        //   [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.0.0, CoreFX 4.6.26212.01), 64bit RyuJIT
        //   DefaultJob : .NET Core 2.0.6 (CoreCLR 4.6.0.0, CoreFX 4.6.26212.01), 64bit RyuJIT
        //      Method |     Mean |     Error |    StdDev |    StdErr |      Min |       Q1 |   Median |       Q3 |      Max |      Op/s |
        // ----------- |---------:|----------:|----------:|----------:|---------:|---------:|---------:|---------:|---------:|----------:|
        //  Benchmark1 | 3.133 us | 0.0492 us | 0.0460 us | 0.0119 us | 3.082 us | 3.100 us | 3.122 us | 3.168 us | 3.233 us | 319,161.9 |
    }
}
