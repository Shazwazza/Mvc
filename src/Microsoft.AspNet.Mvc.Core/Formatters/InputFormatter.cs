// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public abstract class InputFormatter : IInputFormatter
    {
        /// <inheritdoc />
        public IList<MediaTypeHeaderValue> SupportedMediaTypes { get; private set; }

        /// <inheritdoc />
        public IList<Encoding> SupportedEncodings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFormatter"/> class.
        /// </summary>
        protected InputFormatter()
        {
            SupportedEncodings = new List<Encoding>();
            SupportedMediaTypes = new List<MediaTypeHeaderValue>();
        }
        
        /// <inheritdoc />
        public bool CanReadType(InputFormatterContext context, MediaTypeHeaderValue requestContentType)
        {
            // If the request content type is not set, the formatter is free to choose itself.
            if (requestContentType == null)
            {
                return true;
            }
            else
            {
                // Since requestContentType Type is going to be more specific check if requestContentType is a subset
                // of the supportedMediaType.
                return SupportedMediaTypes
                                .Any(supportedMediaType => requestContentType.IsSubsetOf(supportedMediaType));
            }
        }

        /// <inheritdoc />
        public abstract Task<object> ReadAsync(InputFormatterContext context);
    }
}
