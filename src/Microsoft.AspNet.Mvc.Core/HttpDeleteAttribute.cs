// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Identifies an action that only supports the HTTP DELETE method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpDeleteAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        private static readonly IEnumerable<string> _supportedMethods = new string[] { "DELETE" };

        /// <summary>
        /// Creates a new <see cref="HttpDeleteAttribute"/>.
        /// </summary>
        public HttpDeleteAttribute()
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpDeleteAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpDeleteAttribute(string template)
        {
            Template = template;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods
        {
            get { return _supportedMethods; }
        }

        /// <inheritdoc />
        public string Template { get; private set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public int? Order { get; set; }

        /// <inheritdoc />
        public virtual IDictionary<string, IRouteConstraint> Constraints
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> Defaults
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> DataTokens
        {
            get { return null; }
        }
    }
}