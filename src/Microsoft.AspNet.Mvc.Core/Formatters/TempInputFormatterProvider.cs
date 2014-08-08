// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class TempInputFormatterProvider : IInputFormatterProvider
    {
        private IList<IInputFormatter> _defaultFormatters;

        public TempInputFormatterProvider([NotNull] IOptionsAccessor<MvcOptions> optionsAccessor)
        {
            _defaultFormatters = optionsAccessor.Options.InputFormatters;
        }

        public IInputFormatter GetInputFormatter(InputFormatterProviderContext context)
        {
            var request = context.ActionContext.HttpContext.Request;
            var formatters = _defaultFormatters;
            var requestContentType = MediaTypeHeaderValue.Parse(request.ContentType);
            if (requestContentType == null)
            {
                // TODO: http exception?
                throw new InvalidOperationException("400: Bad Request");
            }

            var formatterContext = new InputFormatterContext(context.ActionContext,
                                                             context.Metadata.ModelType,
                                                             context.ModelState);
            foreach (var formatter in formatters)
            {
                var formatterMatched = formatter.CanReadType(formatterContext, requestContentType);
                if (formatterMatched)
                {
                    return formatter;
                }
            }

            // TODO: Http exception
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                                              "415: Unsupported content type {0}",
                                                              requestContentType.RawValue));
        }
    }
}
