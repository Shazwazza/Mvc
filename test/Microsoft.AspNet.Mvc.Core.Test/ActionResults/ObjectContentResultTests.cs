﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test.ActionResults
{
    public class ObjectResultTests
    {


        public static IEnumerable<object[]> ContentTypes
        {
            get
            {
                var contentTypes = new string[]
                        {
                            "text/plain",
                            "text/xml",
                            "application/json",
                        };

                // Empty accept header, should select based on contentTypes.
                yield return new object[] { contentTypes, "", "application/json" };
                
                // null accept header, should select based on contentTypes.
                yield return new object[] { contentTypes, null, "application/json" };

                // No accept Header match with given contentype collection. 
                // Should select based on if any formatter supported any content type.
                yield return new object[] { contentTypes, "text/custom", "application/json"};

                // Accept Header matches but no formatter supports the accept header.
                // Should select based on if any formatter supported any user provided content type.
                yield return new object[] { contentTypes, "text/xml", "application/json" };

                // Filtets out Accept headers with 0 quality and selects the one with highest quality.
                yield return new object[]
                        {
                            contentTypes,
                            "text/plain;q=0.3, text/json;q=0, text/cusotm;q=0.0, application/json;q=0.4",
                            "application/json"
                        };
            }
        }

        [Theory]
        [MemberData("ContentTypes")]
        public async Task ObjectResult_WithMultipleContentTypesAndAcceptHeaders_PerformsContentNegotiation(
            IEnumerable<string> contentTypes, string acceptHeader, string expectedHeader)
        {
            // Arrange
            var expectedContentType = expectedHeader;
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = new Mock<HttpResponse>();
            var tempContentType = string.Empty;
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object, acceptHeader);
            var result = new ObjectResult(input);
            
            // Set the content type property explicitly. 
            result.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType)).ToList();
            result.Formatters = new List<OutputFormatter>
                                            {
                                                new TestOutputFormatter(),
                                                new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), true),
                                            };

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // should always select the Json Output formatter even though it is second in the list.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public void ObjectResult_Create_CallsContentResult_InitializesValue()
        {
            // Arrange
            var input = "testInput";
            var actionContext = CreateMockActionContext();

            // Act
            var result = new ObjectResult(input);

            // Assert
            Assert.Equal(input, result.Value);
        }


        [Fact]
        public async Task ObjectResult_WithSingleContentType_TheGivenContentTypeIsSelected()
        {
            // Arrange
            var expectedContentType = "application/json";
            var input = "testInput";
            var httpResponse = GetMockHttpResponse();
            var actionContext = CreateMockActionContext(httpResponse.Object);
            
            // Set the content type property explicitly to a single value. 
            var result = new ObjectResult(input);
            result.ContentTypes = new List<MediaTypeHeaderValue>();
            result.ContentTypes.Add(MediaTypeHeaderValue.Parse(expectedContentType));

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_MultipleContentTypes_PicksFirstFormatterWhichSupportsAnyOfTheContentTypes()
        {
            // Arrange
            var expectedContentType = "application/json";
            var input = "testInput";
            var httpResponse = GetMockHttpResponse();
            var actionContext = CreateMockActionContext(httpResponse.Object, requestAcceptHeader: null);
            var result = new ObjectResult(input);

            // It should not select TestOutputFormatter, 
            // This is because it should accept the first formatter which supports any of the two contentTypes.
            var contentTypes = new[] { "application/custom", "application/json" };

            // Set the content type and the formatters property explicitly. 
            result.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType))
                                              .ToList();
            result.Formatters = new List<OutputFormatter>
                                    {
                                        new TestOutputFormatter(),
                                        new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), true),
                                    };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom. 
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_MultipleFormattersSupportingTheSameContentType_SelectsTheFirstFormatterInList()
        {
            // Arrange
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = GetMockHttpResponse();
            var actionContext = CreateMockActionContext(httpResponse.Object, requestAcceptHeader: null);
            var result = new ObjectResult(input);

            // It should select the mock formatter as that is the first one in the list.
            var contentTypes = new[] { "application/json", "text/custom" };
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse("text/custom");

            // Get a  mock formatter which supports everything.
            var mockFormatter = GetMockFormatter();

            result.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType)).ToList();
            result.Formatters = new List<OutputFormatter>
                                        {
                                            mockFormatter.Object,
                                            new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), true),
                                            new TestOutputFormatter()
                                        };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Verify that mock formatter was chosen.
            mockFormatter.Verify(o => o.WriteAsync(It.IsAny<OutputFormatterContext>(), CancellationToken.None));
        }

        [Fact]
        public async Task ObjectResult_NoContentTypeSetWithAcceptHeaders_PicksFormatterOnAcceptHeaders()
        {
            // Arrange
            var expectedContentType = "application/json";
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = GetMockHttpResponse();
            var actionContext = 
                CreateMockActionContext(httpResponse.Object,
                                        requestAcceptHeader: "text/custom;q=0.1,application/json;q=0.9",
                                        requestContentType: "application/custom");
            var result = new ObjectResult(input);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<OutputFormatter>
                                    {
                                        new TestOutputFormatter(),
                                        new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), true),
                                    };

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom. i.e the formatter is not TestOutputFormatter.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_NoContentTypeSetWithNoAcceptHeaders_PicksFormatterOnRequestContentType()
        {
            // Arrange
            var stream = new MemoryStream();
            var expectedContentType = "application/json";
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object,
                                                        requestAcceptHeader: null,
                                                        requestContentType: "application/json");
            var input = "testInput";
            var result = new ObjectResult(input);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<OutputFormatter>
                                    {
                                        new TestOutputFormatter(),
                                        new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), true),
                                    };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom. 
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        // TODO: Disabling since this scenario is no longer dealt with in object result. 
        // Re-enable once we do. 
        //[Fact]
        public async Task ObjectResult_Execute_CallsContentResult_SetsContent()
        {
            // Arrange
            var expectedContentType = "text/plain";
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = new Mock<HttpResponse>();
            var tempContentType = string.Empty;
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object);

            // Act
            var result = new ObjectResult(input);
            await result.ExecuteResultAsync(actionContext);

            // Assert
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
            // The following verifies the correct Content was written to Body
            Assert.Equal(input.Length, httpResponse.Object.Body.Length);
        }

        [Fact]
        public async Task ObjectResult_Execute_CallsJsonResult_SetsContent()
        {
            // Arrange
            var expectedContentType = "application/json;charset=utf-8";
            var nonStringValue = new { x1 = 10, y1 = "Hello" };
            var httpResponse = Mock.Of<HttpResponse>();
            httpResponse.Body = new MemoryStream();
            var actionContext = CreateMockActionContext(httpResponse);
            var tempStream = new MemoryStream();
            var tempHttpContext = new Mock<HttpContext>();
            var tempHttpResponse = new Mock<HttpResponse>();

            tempHttpResponse.SetupGet(o => o.Body).Returns(tempStream);
            tempHttpResponse.SetupProperty<string>(o => o.ContentType);
            tempHttpContext.SetupGet(o => o.Response).Returns(tempHttpResponse.Object);
            tempHttpContext.SetupGet(o => o.Request.AcceptCharset).Returns(string.Empty);
            var formatterContext = new OutputFormatterContext()
                                    {
                                        HttpContext = tempHttpContext.Object,
                                        ObjectResult = new ObjectResult(nonStringValue),
                                        DeclaredType = nonStringValue.GetType()
                                    };
            var formatter = new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(), true);
            formatter.SetResponseContentHeaders(formatterContext);
            await formatter.WriteAsync(formatterContext, CancellationToken.None);

            // Act
            var result = new ObjectResult(nonStringValue);
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expectedContentType, httpResponse.ContentType);
            Assert.Equal(tempStream.ToArray(), ((MemoryStream)actionContext.HttpContext.Response.Body).ToArray());
        }

        private static ActionContext CreateMockActionContext(HttpResponse response = null,
                                                             string requestAcceptHeader = "application/*",
                                                             string requestContentType = "application/json",
                                                             string requestAcceptCharsetHeader = "")
        {
            var httpContext = new Mock<HttpContext>();
            if (response != null)
            {
                httpContext.Setup(o => o.Response).Returns(response);
            }

            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var request = new Mock<HttpRequest>();
            request.SetupGet(r => r.AcceptCharset).Returns(requestAcceptCharsetHeader);
            request.SetupGet(r => r.Accept).Returns(requestAcceptHeader);
            request.SetupGet(r => r.ContentType).Returns(requestContentType);
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));

            httpContext.Setup(o => o.Request).Returns(request.Object);
            httpContext.Setup(o => o.RequestServices).Returns(GetServiceProvider());
            
            return new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
        }

        private static Mock<HttpResponse> GetMockHttpResponse()
        {
            var stream = new MemoryStream();
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);
            return httpResponse; 
        }

        private static Mock<TestOutputFormatter> GetMockFormatter()
        {
            var mockFormatter = new Mock<TestOutputFormatter>();
            mockFormatter.Setup(o => o.CanWriteResult(It.IsAny<OutputFormatterContext>(),
                                                      It.IsAny<MediaTypeHeaderValue>()))
                         .Returns(true);

            mockFormatter.Setup(o => o.WriteAsync(It.IsAny<OutputFormatterContext>(), CancellationToken.None))
                         .Returns(Task.FromResult<bool>(true))
                         .Verifiable();
            return mockFormatter;
        }

        private static IServiceProvider GetServiceProvider()
        {
            var optionsSetup = new MvcOptionsSetup();
            var options = new MvcOptions();
            optionsSetup.Setup(options);
            var optionsAccessor = new Mock<IOptionsAccessor<MvcOptions>>();
            optionsAccessor.SetupGet(o => o.Options).Returns(options);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInstance<IOptionsAccessor<MvcOptions>>(optionsAccessor.Object);
            return serviceCollection.BuildServiceProvider();
        }

        public class TestOutputFormatter : OutputFormatter
        {
            public TestOutputFormatter()
            {
                SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/custom"));
            }

            public override Task WriteAsync(OutputFormatterContext context, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}