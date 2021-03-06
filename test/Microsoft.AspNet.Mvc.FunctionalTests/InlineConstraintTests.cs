﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InlineConstraints;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class InlineConstraintTests
    {
        private readonly IServiceProvider _provider;
        private readonly Action<IBuilder> _app = new Startup().Configure;
    
        public InlineConstraintTests()
        {
            _provider = TestHelper.CreateServices("InlineConstraintsWebSite");
            _provider = new ServiceCollection()
                         .AddScoped<ICommandLineArgumentBuilder, DefaultCommandLineArgumentBuilder>()
                         .BuildServiceProvider(_provider);
        }

        [Fact]
        public async Task RoutingToANonExistantArea_WithExistConstraint_RoutesToCorrectAction()
        {
            var svc = _provider.GetService<ICommandLineArgumentBuilder>();
            svc.AddArgument("--TemplateCollection:areaRoute:TemplateValue="+
                            "{area:exists}/{controller=Home}/{action=Index}");
            svc.AddArgument("--TemplateCollection:actionAsMethod:TemplateValue="+
                            "{controller=Home}/{action=Index}");

            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;

            // Act
            var result = await client.GetAsync("http://localhost/Users");
            Assert.Equal(200, result.StatusCode); 

            // Assert
            var returnValue = await result.ReadBodyAsStringAsync();
            Assert.Equal("Users.Index", returnValue);
        }

        [Fact]
        public async Task RoutingToANonExistantArea_WithoutExistConstraint_RoutesToIncorrectAction()
        {
            // Arrange
            var svc = _provider.GetService<ICommandLineArgumentBuilder>();
            svc.AddArgument("--TemplateCollection:areaRoute:TemplateValue="+
                            "{area}/{controller=Home}/{action=Index}");
            svc.AddArgument("--TemplateCollection:actionAsMethod:TemplateValue"+
                            "={controller=Home}/{action=Index}");

            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>
                           (async () => await client.GetAsync("http://localhost/Users"));

            Assert.Equal("The view 'Index' was not found." +
                         " The following locations were searched:\r\n/Areas/Users/Views/Home/Index.cshtml\r\n" +
                         "/Areas/Users/Views/Shared/Index.cshtml\r\n/Views/Shared/Index.cshtml.",
                         ex.Message);
        }
    }
}