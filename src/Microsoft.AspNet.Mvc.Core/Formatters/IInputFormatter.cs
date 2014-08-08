// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public interface IInputFormatter
    {
        /// <summary>
        /// Determines whether this <see cref="IInputFormatter"/> can de-serialize
        /// an object of the specified type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestContentType">The content type header for the request.</param>
        /// <returns>True if this <see cref="IInputFormatter"/> supports the passed in 
        /// <paramref name="requestContentType"/> and is able to de-serialize the request body.
        /// False otherwise.</returns>
        bool CanReadType(InputFormatterContext context, MediaTypeHeaderValue requestContentType);

        /// <summary>
        /// Called during deserialization to read an object from the request.
        /// </summary>
        Task<object> ReadAsync(InputFormatterContext context);
    }
}
