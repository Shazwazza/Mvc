// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultInputFormatterCollectionProvider : IInputFormatterCollectionProvider
    {
        private readonly IList<IInputFormatter> _defaultFormatters; 

        public DefaultInputFormatterCollectionProvider(IOptionsAccessor<MvcOptions> optionsAccesor)
        {
            _defaultFormatters = optionsAccesor.Options.InputFormatters;
        }

        public virtual IList<IInputFormatter> GetInputFormatters(ActionContext actionContext)
        {
            return _defaultFormatters;
        }
    }
}
