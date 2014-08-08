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
            var request = context.HttpContext.Request;
            var formatters = _defaultFormatters;
            var contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            if (contentType == null)
            {
                // TODO: http exception?
                throw new InvalidOperationException("400: Bad Request");
            }

            foreach (var formatter in formatters)
            {
                var formatterMatched = formatter.SupportedMediaTypes
                                                .Any(supportedMediaType =>
                                                        supportedMediaType.IsSubsetOf(contentType));
                if (formatterMatched)
                {
                    return formatter;
                }
            }

            // TODO: Http exception
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                                              "415: Unsupported content type {0}",
                                                              contentType.RawValue));
        }
    }
}
