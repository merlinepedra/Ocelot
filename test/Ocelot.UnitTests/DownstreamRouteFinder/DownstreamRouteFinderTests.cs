using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.DownstreamRouteFinder;
using Ocelot.DownstreamRouteFinder.Finder;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Tests.Utility;
using Ocelot.Values;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DownstreamRouteFinder
{
    public class DownstreamRouteFinderTests
    {
        private readonly IDownstreamRouteProvider _downstreamRouteFinder;
        private readonly Mock<IUrlPathToUrlTemplateMatcher> _mockMatcher;
        
        private FakeHttpRequest _fakeHttpRequest = new FakeHttpRequest();
        
        private string _upstreamUrlPath
        {
            get => _fakeHttpRequest.Path.Value;
            set => _fakeHttpRequest.Path = new PathString(value.StartsWith("/") ? value : $"/{value}");
        }

        private Response<DownstreamRoute> _result;
        private List<ReRoute> _reRoutesConfig;
        private InternalConfiguration _config;
        private Response<DownstreamRoute> _match;

        private string _upstreamHttpMethod
        {
            get => _fakeHttpRequest.Method;
            set => _fakeHttpRequest.Method = value;
        }

        private string _upstreamHost
        {
            get => _fakeHttpRequest.Host.Value;
            set => _fakeHttpRequest.Host = new HostString(value);
        }

        private string _upstreamQuery
        {
            get => _fakeHttpRequest.QueryString.Value;
            set => _fakeHttpRequest.QueryString = new QueryString(value);
        }

        public DownstreamRouteFinderTests()
        {
            _mockMatcher = new Mock<IUrlPathToUrlTemplateMatcher>();
            _downstreamRouteFinder = new Ocelot.DownstreamRouteFinder.Finder.DownstreamRouteFinder(_mockMatcher.Object);
        }

        [Fact]
        public void should_return_highest_priority_when_first()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath", new List<string>()))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath", new List<string>()))
                        .Build(),
                    new ReRouteBuilder()
                        .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 0, false, "someUpstreamPath", new List<string>()))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Post" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 0, false, "someUpstreamPath", new List<string>()))
                        .Build()
                }, string.Empty, serviceProviderConfig))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(x => x.ThenTheFollowingIsReturned(new DownstreamRoute(new Dictionary<string, string>(),
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath", new List<string>()))
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .Build())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("test", 1, false, "someUpstreamPath", new List<string>()))
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .Build()
                        )))
                .BDDfy();
        }

       
        [Fact]
        public void should_return_route()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                        .Build()
                }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRoute(
                            new Dictionary<string, string>(),
                            new ReRouteBuilder()
                                .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                    .WithDownstreamPathTemplate("someDownstreamPath")
                                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                                    .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                    .Build())
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build()
                )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_not_append_slash_to_upstream_url_path()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("/matchInUrlMatcher"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                            .WithDownstreamPathTemplate("/someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("/someUpstreamPath", 1, false, "/someUpstreamPath", new List<string>()))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("/someUpstreamPath", 1, false, "/someUpstreamPath", new List<string>()))
                        .Build()
                }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRoute(
                            new Dictionary<string, string>(),
                            new ReRouteBuilder()
                                .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                    .WithDownstreamPathTemplate("/someDownstreamPath")
                                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                                    .WithUpstreamPathTemplate(new UpstreamPathTemplate("/someUpstreamPath", 1, false, "/someUpstreamPath", new List<string>()))
                                    .Build())
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("/someUpstreamPath", 1, false, "/someUpstreamPath", new List<string>()))
                                .Build()
                )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly("/matchInUrlMatcher"))
                .BDDfy();
        }

        [Fact]
        public void should_return_route_if_upstream_path_and_upstream_template_are_the_same()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string> { "Get" })
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRoute(new Dictionary<string, string>(),
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_return_correct_route_for_http_verb_setting_all_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("someUpstreamPath"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                {
                    new ReRouteBuilder()
                        .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                            .WithDownstreamPathTemplate("someDownstreamPath")
                            .WithUpstreamHttpMethod(new List<string>())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath", new List<string>()))
                            .Build())
                        .WithUpstreamHttpMethod(new List<string>())
                        .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath", new List<string>()))
                        .Build()
                }, string.Empty, serviceProviderConfig
                    ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Post"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRoute(new Dictionary<string, string>(),
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Post" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Post" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("", 1, false, "someUpstreamPath", new List<string>()))
                            .Build()
                        )))
                .BDDfy();
        }

        [Fact]
        public void should_return_route_when_host_matches()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                    {
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRoute(
                        new Dictionary<string, string>(),
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .Build()
                    )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_return_route_when_upstreamhost_is_null()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                    {
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .Then(
                    x => x.ThenTheFollowingIsReturned(new DownstreamRoute(
                        new Dictionary<string, string>(),
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string> { "Get" })
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string> { "Get" })
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .Build()
                    )))
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_return_route_when_host_does_match_with_empty_upstream_http_method()
        {
            var serviceProviderConfig = new ServiceProviderConfigurationBuilder().Build();

            this.Given(x => x.GivenThereIsAnUpstreamUrlPath("matchInUrlMatcher/"))
                .And(x => GivenTheUpstreamHostIs("MATCH"))
                .And(x => x.GivenTheConfigurationIs(new List<ReRoute>
                    {
                        new ReRouteBuilder()
                            .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                                .WithDownstreamPathTemplate("someDownstreamPath")
                                .WithUpstreamHttpMethod(new List<string>())
                                .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                                .Build())
                            .WithUpstreamHttpMethod(new List<string>())
                            .WithUpstreamPathTemplate(new UpstreamPathTemplate("someUpstreamPath", 1, false, "someUpstreamPath", new List<string>()))
                            .WithUpstreamHost("MATCH")
                            .Build()
                    }, string.Empty, serviceProviderConfig
                ))
                .And(x => x.GivenTheUrlMatcherReturns(new OkResponse<DownstreamRoute>(new DownstreamRoute(new Dictionary<string, string>(),_reRoutesConfig[0]))))
                .And(x => x.GivenTheUpstreamHttpMethodIs("Get"))
                .When(x => x.WhenICallTheFinder())
                .And(x => x.ThenTheUrlMatcherIsCalledCorrectly(1, 0))
                .BDDfy();
        }

        private void GivenTheUpstreamHostIs(string upstreamHost)
        {
            _upstreamHost = upstreamHost;
        }

        private void GivenTheUpstreamHttpMethodIs(string upstreamHttpMethod)
        {
            _upstreamHttpMethod = upstreamHttpMethod;
        }

        private void ThenAnErrorResponseIsReturned()
        {
            _result.IsError.ShouldBeTrue();
        }

        private void ThenTheUrlMatcherIsCalledCorrectly()
        {
            _mockMatcher
                .Verify(x => x.Match(_fakeHttpRequest, _reRoutesConfig[0]), Times.Once);
            
        }

        private void ThenTheUrlMatcherIsCalledCorrectly(int times, int index = 0)
        {
            _mockMatcher
                .Verify(x => x.Match(It.IsAny<FakeHttpRequest>(), _reRoutesConfig[index]), Times.Exactly(times));
        }

        private void ThenTheUrlMatcherIsCalledCorrectly(string expectedUpstreamUrlPath)
        {
            _mockMatcher
                .Verify(x => x.Match(It.Is<FakeHttpRequest>(http => http.Path.Value == expectedUpstreamUrlPath), _reRoutesConfig[0]), Times.Once);
        }

        private void ThenTheUrlMatcherIsNotCalled()
        {
            _mockMatcher
                .Verify(x => x.Match(It.IsAny<FakeHttpRequest>(), _reRoutesConfig[0]), Times.Never);
        }

        private void GivenTheUrlMatcherReturns(Response<DownstreamRoute> match)
        {
            _match = match;
            _mockMatcher
                .Setup(x => x.Match(It.IsAny<HttpRequest>(), It.IsAny<ReRoute>()))
                .Returns(_match);
        }

        private void GivenTheConfigurationIs(List<ReRoute> reRoutesConfig, string adminPath, ServiceProviderConfiguration serviceProviderConfig)
        {
            _reRoutesConfig = reRoutesConfig;
            _config = new InternalConfiguration(_reRoutesConfig, adminPath, serviceProviderConfig, "", new LoadBalancerOptionsBuilder().Build(), "", new QoSOptionsBuilder().Build(), new HttpHandlerOptionsBuilder().Build());
        }

        private void GivenThereIsAnUpstreamUrlPath(string upstreamUrlPath)
        {
            _upstreamUrlPath = upstreamUrlPath.StartsWith("/") ? upstreamUrlPath : $"/{upstreamUrlPath}";
        }

        private void WhenICallTheFinder()
        {
            _result = _downstreamRouteFinder.Get(_fakeHttpRequest, _config);
        }

        private void ThenTheFollowingIsReturned(DownstreamRoute expected)
        {
            _result.Data.ReRoute.DownstreamReRoute[0].DownstreamPathTemplate.Value.ShouldBe(expected.ReRoute.DownstreamReRoute[0].DownstreamPathTemplate.Value);
            _result.Data.ReRoute.UpstreamTemplatePattern.Priority.ShouldBe(expected.ReRoute.UpstreamTemplatePattern.Priority);

            foreach (var kvp in _result.Data.UrlValues)
            {
                expected.UrlValues.ContainsKey(kvp.Key).ShouldBeTrue();
                expected.UrlValues[kvp.Key].ShouldMatch(kvp.Value);
            }

            _result.IsError.ShouldBeFalse();
        }
    }
}
