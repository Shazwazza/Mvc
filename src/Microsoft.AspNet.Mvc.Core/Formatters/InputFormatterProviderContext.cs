// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class InputFormatterProviderContext
    {
        public InputFormatterProviderContext([NotNull] InputFormatterContext inputFormatterContext)
        {
            InputFormatterContext = inputFormatterContext;
        }

        public InputFormatterContext InputFormatterContext { get; private set; }
    }
}
