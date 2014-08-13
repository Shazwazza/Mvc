﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class ViewStartProviderTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetViewStartLocations_ReturnsEmptySequenceIfViewPathIsEmpty(string viewPath)
        {
            // Arrange
            var appPath = @"x:\test";
            var mvcOptions = new MvcOptions();
            var provider = new ViewStartProvider(GetAppEnv(appPath),
                                                 Mock.Of<IRazorPageFactory>(),
                                                 GetOptionsAccessor(mvcOptions));

            // Act
            var result = provider.GetViewStartLocations(viewPath);

            // Assert
            Assert.Empty(result);
        }

        public static IEnumerable<object[]> GetViewStartLocations_ReturnsPotentialViewStartLocationsData
        {
            get
            {
                yield return new object[]
                {
                    @"x:\test\myapp",
                    "/Views/Home/View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\Views\Home\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\_ViewStart.cshtml",
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };

                yield return new object[]
                {
                    @"x:\test\myapp",
                    "Views/Home/View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\Views\Home\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\_ViewStart.cshtml",
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };

                yield return new object[]
                {
                    @"x:\test\myapp\",
                    "Views/Home/View.cshtml",
                    new[]
                    {
                        @"x:\test\myapp\Views\Home\_ViewStart.cshtml",
                        @"x:\test\myapp\Views\_ViewStart.cshtml",
                        @"x:\test\myapp\_ViewStart.cshtml",
                    }
                };
            }
        }

        [Theory]
        [MemberData("GetViewStartLocations_ReturnsPotentialViewStartLocationsData")]
        public void GetViewStartLocations_ReturnsPotentialViewStartLocations(string appPath,
                                                                             string viewPath,
                                                                             IEnumerable<string> expected)
        {
            // Arrange
            var mvcOptions = new MvcOptions();
            var provider = new ViewStartProvider(GetAppEnv(appPath),
                                                 Mock.Of<IRazorPageFactory>(),
                                                 GetOptionsAccessor(mvcOptions));

            // Act
            var result = provider.GetViewStartLocations(viewPath);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetViewStartLocations_ReturnsPotentialViewStartLocationsWithCorrectExtension()
        {
            // Arrange
            var expected = new[]
            {
                @"z:\mvcapp\views\home\_ViewStart.rzr",
                @"z:\mvcapp\views\_ViewStart.rzr",
                @"z:\mvcapp\_ViewStart.rzr",
            };
            var mvcOptions = new MvcOptions();
            mvcOptions.ViewEngineOptions.ViewExtension = ".rzr";
            var provider = new ViewStartProvider(GetAppEnv(@"z:\mvcapp\"),
                                                 Mock.Of<IRazorPageFactory>(),
                                                 GetOptionsAccessor(mvcOptions));

            // Act
            var result = provider.GetViewStartLocations(@"views\home\index.rzr");

            // Assert
            Assert.Equal(expected, result);
        }

        private static IApplicationEnvironment GetAppEnv(string appPath)
        {
            var appEnv = new Mock<IApplicationEnvironment>();
            appEnv.Setup(p => p.ApplicationBasePath)
                  .Returns(appPath);
            return appEnv.Object;
        }

        private static IOptionsAccessor<MvcOptions> GetOptionsAccessor(MvcOptions options)
        {
            var accessor = new Mock<IOptionsAccessor<MvcOptions>>();
            accessor.SetupGet(a => a.Options)
                    .Returns(options);

            return accessor.Object;
        }
    }
}