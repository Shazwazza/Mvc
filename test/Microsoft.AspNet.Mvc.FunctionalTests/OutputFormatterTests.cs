﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    /// <summary>
    /// Summary description for OutputFormatterTests
    /// </summary>
    public class OutputFormatterTests
    {
        private readonly IServiceProvider _services;
        private readonly Action<IBuilder> _app = new FormatterWebSite.Startup().Configure;

        public OutputFormatterTests()
        {
            _services = TestHelper.CreateServices("FormatterWebSite");
        }

        [Fact]
        public async Task OutputFormatterIsCalled()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;
            var headers = new Dictionary<string, string[]>();
            headers.Add("Accept", new string[] { "application/xml;charset=utf-8" });

            // Act
            var response = await client.SendAsync("POST", "http://localhost/Home/GetDummyClass?sampleInput=10", headers, null, null);

            //Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite\">" +
                "<SampleInt>10</SampleInt></DummyClass>",
                new StreamReader(response.Body, Encoding.UTF8).ReadToEnd());
        }
    }
}