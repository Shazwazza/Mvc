// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public class TempInputFormatterProvider : IInputFormatterProvider
    {
        public IInputFormatter GetInputFormatter(InputFormatterProviderContext context)
        {
            var request = context.InputFormatterContext.ActionContext.HttpContext.Request;
            var formatters = context.InputFormatterContext.ActionContext.InputFormatters;
            var requestContentType = MediaTypeHeaderValue.Parse(request.ContentType);
            if (requestContentType == null)
            {
                // TODO: http exception?
                throw new InvalidOperationException("400: Bad Request");
            }

            foreach (var formatter in formatters)
            {
                var formatterMatched = formatter.CanReadType(context.InputFormatterContext, requestContentType);
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
