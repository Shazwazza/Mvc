// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class ReflectedActionInvoker : FilterActionInvoker
    {
        private readonly ReflectedActionDescriptor _descriptor;
        private readonly IControllerFactory _controllerFactory;
        public ReflectedActionInvoker([NotNull] ActionContext actionContext,
                                      [NotNull] IActionBindingContextProvider bindingContextProvider,
                                      [NotNull] INestedProviderManager<FilterProviderContext> filterProvider,
                                      [NotNull] IControllerFactory controllerFactory,
                                      [NotNull] ReflectedActionDescriptor descriptor)
            : base(actionContext, bindingContextProvider, filterProvider)
        {
            _descriptor = descriptor;
            _controllerFactory = controllerFactory;

            if (descriptor.MethodInfo == null)
            {
                throw new ArgumentException(
                    Resources.FormatPropertyOfTypeCannotBeNull("MethodInfo",
                                                               typeof(ReflectedActionDescriptor)),
                    "descriptor");
            }
        }

        public override Task InvokeAsync()
        {
            ActionContext.Controller = _controllerFactory.CreateController(ActionContext);
            return base.InvokeAsync();
        }

        protected override async Task<IActionResult> InvokeActionAsync(ActionExecutingContext actionExecutingContext)
        {
            var actionMethodInfo = _descriptor.MethodInfo;
            var actionReturnValue = await ReflectedActionExecutor.ExecuteAsync(
                actionMethodInfo,
                ActionContext.Controller,
                actionExecutingContext.ActionArguments);

            var actionResult = CreateActionResult(
                actionMethodInfo.ReturnType,
                actionReturnValue);
            return actionResult;
        }

        // Marking as internal for Unit Testing purposes.
        internal static IActionResult CreateActionResult([NotNull] Type declaredReturnType, object actionReturnValue)
        {
            // optimize common path
            var actionResult = actionReturnValue as IActionResult;
            if (actionResult != null)
            {
                return actionResult;
            }

            if (declaredReturnType == typeof(void) ||
                declaredReturnType == typeof(Task))
            {
                return new NoContentResult();
            }

            // Unwrap potential Task<T> types. 
            var actualReturnType = TypeHelper.GetTaskInnerTypeOrNull(declaredReturnType) ?? declaredReturnType;
            if (actionReturnValue == null && typeof(IActionResult).IsAssignableFrom(actualReturnType))
            {
                throw new InvalidOperationException(
                    Resources.FormatActionResult_ActionReturnValueCannotBeNull(actualReturnType));
            }

            return new ObjectResult(actionReturnValue)
                       {
                           DeclaredType = actualReturnType
                       };
        }
    }
}

        private async Task InvokeExceptionFilter()
        {
            var current = _cursor.GetNextFilter<IExceptionFilter, IAsyncExceptionFilter>();
            if (current.FilterAsync != null)
            {
                // Exception filters run "on the way out" - so the filter is run after the rest of the
                // pipeline.
                await InvokeExceptionFilter();

                Contract.Assert(_exceptionContext != null);
                if (_exceptionContext.Exception != null)
                {
                    // Exception filters only run when there's an exception - unsetting it will short-circuit
                    // other exception filters.
                    await current.FilterAsync.OnExceptionAsync(_exceptionContext);
                }
            }
            else if (current.Filter != null)
            {
                // Exception filters run "on the way out" - so the filter is run after the rest of the
                // pipeline.
                await InvokeExceptionFilter();

                Contract.Assert(_exceptionContext != null);
                if (_exceptionContext.Exception != null)
                {
                    // Exception filters only run when there's an exception - unsetting it will short-circuit
                    // other exception filters.
                    current.Filter.OnException(_exceptionContext);
                }
            }
            else
            {
                // We've reached the 'end' of the exception filter pipeline - this means that one stack frame has
                // been built for each exception. When we return from here, these frames will either:
                //
                // 1) Call the filter (if we have an exception)
                // 2) No-op (if we don't have an exception)
                Contract.Assert(_exceptionContext == null);
                _exceptionContext = new ExceptionContext(_actionContext, _filters);

                try
                {
                    await InvokeActionAuthorizationFilters();

                    Contract.Assert(_authorizationContext != null);
                    if (_authorizationContext.Result == null)
                    {
                        // Authorization passed, run authorization filters and the action
                        await InvokeActionMethodWithFilters();

                        // Action filters might 'return' an unahndled exception instead of throwing
                        Contract.Assert(_actionExecutedContext != null);
                        if (_actionExecutedContext.Exception != null && !_actionExecutedContext.ExceptionHandled)
                        {
                            _exceptionContext.Exception = _actionExecutedContext.Exception;
                            if (_actionExecutedContext.ExceptionDispatchInfo != null)
                            {
                                _exceptionContext.ExceptionDispatchInfo = _actionExecutedContext.ExceptionDispatchInfo;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    _exceptionContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                }
            }
        }

        private async Task InvokeActionAuthorizationFilters()
        {
            _cursor.SetStage(FilterStage.AuthorizationFilters);

            _authorizationContext = new AuthorizationContext(_actionContext, _filters);
            await InvokeAuthorizationFilter();
        }

        private async Task InvokeAuthorizationFilter()
        {
            // We should never get here if we already have a result.
            Contract.Assert(_authorizationContext != null);
            Contract.Assert(_authorizationContext.Result == null);

            var current = _cursor.GetNextFilter<IAuthorizationFilter, IAsyncAuthorizationFilter>();
            if (current.FilterAsync != null)
            {
                await current.FilterAsync.OnAuthorizationAsync(_authorizationContext);

                if (_authorizationContext.Result == null)
                {
                    // Only keep going if we don't have a result
                    await InvokeAuthorizationFilter();
                }
            }
            else if (current.Filter != null)
            {
                current.Filter.OnAuthorization(_authorizationContext);

                if (_authorizationContext.Result == null)
                {
                    // Only keep going if we don't have a result
                    await InvokeAuthorizationFilter();
                }
            }
            else
            {
                // We've run out of Authorization Filters - if we haven't short circuited by now then this
                // request is authorized.
            }
        }

        private async Task InvokeActionMethodWithFilters()
        {
            _cursor.SetStage(FilterStage.ActionFilters);

            var arguments = await GetActionArguments(_actionContext.ModelState);
            _actionExecutingContext = new ActionExecutingContext(_actionContext, _filters, arguments);

            await InvokeActionMethodFilter();
        }

        internal async Task<IDictionary<string, object>> GetActionArguments(ModelStateDictionary modelState)
        {
            var actionBindingContext = await _bindingProvider.GetActionBindingContextAsync(_actionContext);
            var parameters = _descriptor.Parameters;
            var metadataProvider = actionBindingContext.MetadataProvider;
            var parameterValues = new Dictionary<string, object>(parameters.Count, StringComparer.Ordinal);

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                if (parameter.BodyParameterInfo != null)
                {
                    var parameterType = parameter.BodyParameterInfo.ParameterType;
                    var modelMetadata = metadataProvider.GetMetadataForType(
                        modelAccessor: null,
                        modelType: parameterType);
                    var providerContext = new InputFormatterProviderContext(
                        actionBindingContext.ActionContext,
                        modelMetadata,
                        modelState);

                    var inputFormatter = actionBindingContext.InputFormatterProvider.GetInputFormatter(
                        providerContext);

                    var formatterContext = new InputFormatterContext(actionBindingContext.ActionContext,
                                                                     modelMetadata.ModelType,
                                                                     modelState);
                    parameterValues[parameter.Name] = await inputFormatter.ReadAsync(formatterContext);
                }
                else
                {
                    var parameterType = parameter.ParameterBindingInfo.ParameterType;
                    var modelMetadata = metadataProvider.GetMetadataForType(
                        modelAccessor: null,
                        modelType: parameterType);

                    var modelBindingContext = new ModelBindingContext
                    {
                        ModelName = parameter.Name,
                        ModelState = modelState,
                        ModelMetadata = modelMetadata,
                        ModelBinder = actionBindingContext.ModelBinder,
                        ValueProvider = actionBindingContext.ValueProvider,
                        ValidatorProviders = actionBindingContext.ValidatorProviders,
                        MetadataProvider = metadataProvider,
                        HttpContext = actionBindingContext.ActionContext.HttpContext,
                        FallbackToEmptyPrefix = true
                    };
                    if (await actionBindingContext.ModelBinder.BindModelAsync(modelBindingContext))
                    {
                        parameterValues[parameter.Name] = modelBindingContext.Model;
                    }
                }
            }

            return parameterValues;
        }

        private async Task<ActionExecutedContext> InvokeActionMethodFilter()
        {
            Contract.Assert(_actionExecutingContext != null);
            if (_actionExecutingContext.Result != null)
            {
                // If we get here, it means that an async filter set a result AND called next(). This is forbidden.
                var message = Resources.FormatAsyncActionFilter_InvalidShortCircuit(
                    typeof(IAsyncActionFilter).Name,
                    "Result",
                    typeof(ActionExecutingContext).Name,
                    typeof(ActionExecutionDelegate).Name);

                throw new InvalidOperationException(message);
            }

            var item = _cursor.GetNextFilter<IActionFilter, IAsyncActionFilter>();
            try
            {
                if (item.FilterAsync != null)
                {
                    await item.FilterAsync.OnActionExecutionAsync(_actionExecutingContext, InvokeActionMethodFilter);

                    if (_actionExecutedContext == null)
                    {
                        // If we get here then the filter didn't call 'next' indicating a short circuit
                        _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters)
                        {
                            Canceled = true,
                            Result = _actionExecutingContext.Result,
                        };
                    }
                }
                else if (item.Filter != null)
                {
                    item.Filter.OnActionExecuting(_actionExecutingContext);

                    if (_actionExecutingContext.Result != null)
                    {
                        // Short-circuited by setting a result.
                        _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters)
                        {
                            Canceled = true,
                            Result = _actionExecutingContext.Result,
                        };
                    }
                    else
                    {
                        item.Filter.OnActionExecuted(await InvokeActionMethodFilter());
                    }
                }
                else
                {
                    // All action filters have run, execute the action method.
                    _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters)
                    {
                        Result = await InvokeActionMethod()
                    };
                }
            }
            catch (Exception exception)
            {
                // Exceptions thrown by the action method OR filters bubble back up through ActionExcecutedContext.
                _actionExecutedContext = new ActionExecutedContext(_actionExecutingContext, _filters)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }
            return _actionExecutedContext;
        }

        private async Task<IActionResult> InvokeActionMethod()
        {
            _cursor.SetStage(FilterStage.ActionMethod);

            var actionMethodInfo = _descriptor.MethodInfo;
            var actionReturnValue = await ReflectedActionExecutor.ExecuteAsync(
                actionMethodInfo,
                _actionContext.Controller,
                _actionExecutingContext.ActionArguments);

            var actionResult = CreateActionResult(
                actionMethodInfo.ReturnType,
                actionReturnValue);
            return actionResult;
        }

        private async Task InvokeActionResultWithFilters(IActionResult result)
        {
            _cursor.SetStage(FilterStage.ResultFilters);

            _resultExecutingContext = new ResultExecutingContext(_actionContext, _filters, result);
            await InvokeActionResultFilter();

            Contract.Assert(_resultExecutingContext != null);
            if (_resultExecutedContext.Exception != null && !_resultExecutedContext.ExceptionHandled)
            {
                // There's an unhandled exception in filters
                if (_resultExecutedContext.ExceptionDispatchInfo != null)
                {
                    _resultExecutedContext.ExceptionDispatchInfo.Throw();
                }
                else
                {
                    throw _resultExecutedContext.Exception;
                }
            }
        }

        private async Task<ResultExecutedContext> InvokeActionResultFilter()
        {
            Contract.Assert(_resultExecutingContext != null);
            if (_resultExecutingContext.Cancel == true)
            {
                // If we get here, it means that an async filter set cancel == true AND called next().
                // This is forbidden.
                var message = Resources.FormatAsyncResultFilter_InvalidShortCircuit(
                    typeof(IAsyncResultFilter).Name,
                    "Cancel",
                    typeof(ResultExecutingContext).Name,
                    typeof(ResultExecutionDelegate).Name);

                throw new InvalidOperationException(message);
            }

            try
            {
                var item = _cursor.GetNextFilter<IResultFilter, IAsyncResultFilter>();
                if (item.FilterAsync != null)
                {
                    await item.FilterAsync.OnResultExecutionAsync(_resultExecutingContext, InvokeActionResultFilter);

                    if (_resultExecutedContext == null)
                    {
                        // Short-circuited by not calling next
                        _resultExecutedContext = new ResultExecutedContext(
                            _resultExecutingContext,
                            _filters,
                            _resultExecutingContext.Result)
                        {
                            Canceled = true,
                        };
                    }
                    else if (_resultExecutingContext.Cancel == true)
                    {
                        // Short-circuited by setting Cancel == true
                        _resultExecutedContext = new ResultExecutedContext(
                            _resultExecutingContext,
                            _filters,
                            _resultExecutingContext.Result)
                        {
                            Canceled = true,
                        };
                    }
                }
                else if (item.Filter != null)
                {
                    item.Filter.OnResultExecuting(_resultExecutingContext);

                    if (_resultExecutingContext.Cancel == true)
                    {
                        // Short-circuited by setting Cancel == true
                        _resultExecutedContext = new ResultExecutedContext(
                            _resultExecutingContext,
                            _filters,
                            _resultExecutingContext.Result)
                        {
                            Canceled = true,
                        };
                    }
                    else
                    {
                        item.Filter.OnResultExecuted(await InvokeActionResultFilter());
                    }
                }
                else
                {
                    await InvokeActionResult();

                    Contract.Assert(_resultExecutedContext == null);
                    _resultExecutedContext = new ResultExecutedContext(
                        _resultExecutingContext,
                        _filters,
                        _resultExecutingContext.Result);
                }
            }
            catch (Exception exception)
            {
                _resultExecutedContext = new ResultExecutedContext(
                    _resultExecutingContext,
                    _filters,
                    _resultExecutingContext.Result)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception)
                };
            }

            return _resultExecutedContext;
        }

        private async Task InvokeActionResult()
        {
            _cursor.SetStage(FilterStage.ActionResult);

            // The empty result is always flowed back as the 'executed' result
            if (_resultExecutingContext.Result == null)
            {
                _resultExecutingContext.Result = new EmptyResult();
            }

            await _resultExecutingContext.Result.ExecuteResultAsync(_resultExecutingContext);
        }

        private enum FilterStage
        {
            Undefined,
            ExceptionFilters,
            AuthorizationFilters,
            ActionFilters,
            ActionMethod,
            ResultFilters,
            ActionResult
        }

        /// <summary>
        /// A one-way cursor for filters.
        /// </summary>
        /// <remarks>
        /// This will iterate the filter collection once per-stage, and skip any filters that don't have
        /// the one of interfaces that applies to the current stage.
        ///
        /// Filters are always executed in the following order, but short circuiting plays a role.
        ///
        /// Indentation reflects nesting.
        ///
        /// 1. Exception Filters
        ///     2. Authorization Filters
        ///     3. Action Filters
        ///        Action
        ///
        /// 4. Result Filters
        ///    Result
        ///
        /// </remarks>
        private struct FilterCursor
        {
            private FilterStage _stage;
            private int _index;
            private readonly IFilter[] _filters;

            public FilterCursor(FilterStage stage, int index, IFilter[] filters)
            {
                _stage = stage;
                _index = index;
                _filters = filters;
            }

            public FilterCursor(IFilter[] filters)
            {
                _stage = FilterStage.Undefined;
                _index = 0;
                _filters = filters;
            }

            public void SetStage(FilterStage stage)
            {
                _stage = stage;
                _index = 0;
            }

            public FilterCursorItem<TFilter, TFilterAsync> GetNextFilter<TFilter, TFilterAsync>()
                where TFilter : class
                where TFilterAsync : class
            {
                while (_index < _filters.Length)
                {
                    var filter = _filters[_index] as TFilter;
                    var filterAsync = _filters[_index] as TFilterAsync;

                    _index += 1;

                    if (filter != null || filterAsync != null)
                    {
                        return new FilterCursorItem<TFilter, TFilterAsync>(_stage, _index, filter, filterAsync);
                    }
                }

                return default(FilterCursorItem<TFilter, TFilterAsync>);
            }

            public bool StillAt<TFilter, TFilterAsync>(FilterCursorItem<TFilter, TFilterAsync> current)
            {
                return current.Stage == _stage && current.Index == _index;
            }
        }

        private struct FilterCursorItem<TFilter, TFilterAsync>
        {
            public readonly FilterStage Stage;
            public readonly int Index;
            public readonly TFilter Filter;
            public readonly TFilterAsync FilterAsync;

            public FilterCursorItem(FilterStage stage, int index, TFilter filter, TFilterAsync filterAsync)
            {
                Stage = stage;
                Index = index;
                Filter = filter;
                FilterAsync = filterAsync;
            }
        }
    }
}
