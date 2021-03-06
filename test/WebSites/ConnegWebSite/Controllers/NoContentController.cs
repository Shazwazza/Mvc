﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace ConnegWebsite
{
    public class NoContentController : Controller
    {
        public Task<string> ReturnTaskOfString_NullValue()
        {
            return Task.FromResult<string>(null);
        }

        public Task<object> ReturnTaskOfObject_NullValue()
        {
            return Task.FromResult<object>(null);
        }

        public string ReturnString_NullValue()
        {
            return null;
        }

        public object ReturnObject_NullValue()
        {
            return null;
        }
    }
}