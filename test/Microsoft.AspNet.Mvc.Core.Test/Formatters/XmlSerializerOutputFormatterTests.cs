﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class XmlSerializerOutputFormatterTests
    {
        public class DummyClass
        {
            public int SampleInt { get; set; }
        }

        public class TestLevelOne
        {
            public int SampleInt { get; set; }
            public string sampleString;
        }

        public class TestLevelTwo
        {
            public string SampleString { get; set; }
            public TestLevelOne TestOne { get; set; }
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterWritesSimpleTypes()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: false);
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SampleInt>10</SampleInt></DummyClass>",
                new StreamReader(outputFormatterContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterWritesComplexTypes()
        {
            // Arrange
            var sampleInput = new TestLevelTwo
            {
                SampleString = "TestString",
                TestOne = new TestLevelOne
                {
                    SampleInt = 10,
                    sampleString = "TestLevelOne string"
                }
            };
            var formatter = new XmlSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: false);
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<TestLevelTwo xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                            "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SampleString>TestString</SampleString>" +
                            "<TestOne><sampleString>TestLevelOne string</sampleString>" +
                            "<SampleInt>10</SampleInt></TestOne></TestLevelTwo>",
                new StreamReader(outputFormatterContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterWritesOnModifiedWriterSettings()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());
            var formatter = new XmlSerializerOutputFormatter(
                new System.Xml.XmlWriterSettings
                {
                    OmitXmlDeclaration = false,
                    CloseOutput = false
                },
                indent: false);

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                            "<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                            "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SampleInt>10</SampleInt></DummyClass>",
                        new StreamReader(outputFormatterContext.HttpContext.Response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterWritesUTF16Output()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType(), "application/xml; charset=utf-16");
            var formatter = new XmlSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: false);
            formatter.WriterSettings.OmitXmlDeclaration = false;

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                            "<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                            "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SampleInt>10</SampleInt></DummyClass>",
                        new StreamReader(outputFormatterContext.HttpContext.Response.Body,
                                Encodings.UTF16EncodingLittleEndian).ReadToEnd());
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterWritesIndentedOutput()
        {
            // Arrange
            var sampleInput = new DummyClass { SampleInt = 10 };
            var formatter = new XmlSerializerOutputFormatter(
                XmlSerializerOutputFormatter.GetDefaultXmlWriterSettings(),
                indent: true);
            var outputFormatterContext = GetOutputFormatterContext(sampleInput, sampleInput.GetType());

            // Act
            await formatter.WriteAsync(outputFormatterContext, CancellationToken.None);

            // Assert
            Assert.NotNull(outputFormatterContext.HttpContext.Response.Body);
            outputFormatterContext.HttpContext.Response.Body.Position = 0;
            var outputString = new StreamReader(outputFormatterContext.HttpContext.Response.Body,
                Encoding.UTF8).ReadToEnd();
            Assert.Equal("<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <SampleInt>10</SampleInt>\r\n</DummyClass>",
                outputString);
        }

        private OutputFormatterContext GetOutputFormatterContext(object outputValue, Type outputType,
                                                        string contentType = "application/xml; charset=utf-8")
        {
            return new OutputFormatterContext
            {
                ObjectResult = new ObjectResult(outputValue),
                DeclaredType = outputType,
                HttpContext = GetHttpContext(contentType)
            };
        }

        private static HttpContext GetHttpContext(string contentType)
        {
            var response = new Mock<HttpResponse>();
            var headers = new Mock<IHeaderDictionary>();
            response.Setup(r => r.ContentType).Returns(contentType);
            response.SetupGet(r => r.Headers).Returns(headers.Object);
            response.SetupGet(f => f.Body).Returns(new MemoryStream());
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Response).Returns(response.Object);
            return httpContext.Object;
        }
    }
}