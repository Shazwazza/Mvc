﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    public class TestSink
    {
        public TestSink(
            Func<WriteCoreContext, bool> writeEnabled = null, 
            Func<BeginScopeContext, bool> beginEnabled = null)
        {
            WriteEnabled = writeEnabled;
            BeginEnabled = beginEnabled;

            Scopes = new List<BeginScopeContext>();
            Writes = new List<WriteCoreContext>();
        }

        public Func<WriteCoreContext, bool> WriteEnabled { get; set; }

        public Func<BeginScopeContext, bool> BeginEnabled { get; set; }

        public List<BeginScopeContext> Scopes { get; set; }

        public List<WriteCoreContext> Writes { get; set; }

        public void Write(WriteCoreContext context)
        {
            if (WriteEnabled == null || WriteEnabled(context))
            {
                Writes.Add(context);
            }
        }

        public void Begin(BeginScopeContext context)
        {
            if (BeginEnabled == null || BeginEnabled(context))
            {
                Scopes.Add(context);
            }
        }

        public static bool EnableWithTypeName<T>(WriteCoreContext context)
        {
            return context.LoggerName.Equals(typeof(T).FullName);
        }

        public static bool EnableWithTypeName<T>(BeginScopeContext context)
        {
            return context.LoggerName.Equals(typeof(T).FullName);
        }
    }
}