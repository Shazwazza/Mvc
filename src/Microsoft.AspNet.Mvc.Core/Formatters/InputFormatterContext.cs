// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class InputFormatterContext
    {
        public InputFormatterContext([NotNull] ActionContext actionContext,
                                     [NotNull] Type modelType, 
                                     [NotNull] ModelStateDictionary modelState)
        {
            ActionContext = actionContext;
            ModelType = modelType;
            ModelState = modelState;
        }

        public ActionContext ActionContext { get; private set; }

        public Type ModelType { get; private set; }

        public ModelStateDictionary ModelState { get; private set; }
    }
}
