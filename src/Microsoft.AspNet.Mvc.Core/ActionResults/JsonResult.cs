// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    public class JsonResult : ObjectResult
    {
        private static readonly IList<MediaTypeHeaderValue> _defaultSupportedContentTypes = 
                                                                new List<MediaTypeHeaderValue>()
                                                                {
                                                                    MediaTypeHeaderValue.Parse("application/json"),
                                                                    MediaTypeHeaderValue.Parse("text/json"),
                                                                };
        private IOutputFormatter _defaultFormatter;

        public JsonResult(object data) :
            this(data, null)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="JsonResult"/> class.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="defaultFormatter">If no matching formatter is found, 
        /// the response is written to using defaultFormatter.</param>
        /// <remarks>
        /// The default formatter must be able to handle either application/json
        /// or text/json.
        /// </remarks>
        public JsonResult(object data, IOutputFormatter defaultFormatter)
                : base(data)
        {
            _defaultFormatter = defaultFormatter;
        }

        public IOutputFormatter DefaultFormatter
        {
            get
            {
                if(_defaultFormatter == null)
                {
                    _defaultFormatter = new JsonOutputFormatter(JsonOutputFormatter.CreateDefaultSettings(),
                                                                    indent: false);
                }

                return _defaultFormatter;
            }
        }

        public override async Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            // Set the content type explicitly to application/json and text/json.
            if (ContentTypes == null || ContentTypes.Count == 0)
            {
                ContentTypes = _defaultSupportedContentTypes;
            }

            await base.ExecuteResultAsync(context);
        }

        public override IOutputFormatter SelectFormatter(OutputFormatterContext formatterContext,
                                                         IEnumerable<IOutputFormatter> formatters)
        {
            return base.SelectFormatter(formatterContext, formatters) ?? DefaultFormatter;
        }
    }
}
