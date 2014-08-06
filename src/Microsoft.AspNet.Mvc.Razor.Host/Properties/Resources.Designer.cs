// <auto-generated />
namespace Microsoft.AspNet.Mvc.Razor.Host
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Resources
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.AspNet.Mvc.Razor.Host.Resources", typeof(Resources).GetTypeInfo().Assembly);

        /// <summary>
        /// Argument cannot be null or empty.
        /// </summary>
        internal static string ArgumentCannotBeNullOrEmpty
        {
            get { return GetString("ArgumentCannotBeNullOrEmpty"); }
        }

        /// <summary>
        /// Argument cannot be null or empty.
        /// </summary>
        internal static string FormatArgumentCannotBeNullOrEmpty()
        {
            return GetString("ArgumentCannotBeNullOrEmpty");
        }

        /// <summary>
        /// The 'inherits' keyword is not allowed when a '{0}' keyword is used.
        /// </summary>
        internal static string MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword
        {
            get { return GetString("MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword"); }
        }

        /// <summary>
        /// The 'inherits' keyword is not allowed when a '{0}' keyword is used.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_CannotHaveModelAndInheritsKeyword(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword"), p0);
        }

        /// <summary>
        /// The 'inherits' keyword is not allowed when a '{0}' keyword is used.
        /// </summary>
        internal static string MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword1
        {
            get { return GetString("MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword1"); }
        }

        /// <summary>
        /// The 'inherits' keyword is not allowed when a '{0}' keyword is used.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_CannotHaveModelAndInheritsKeyword1(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword1"), p0);
        }

        /// <summary>
        /// A property name must be specified when using the '{0}' statement. Format for a '{0}' statement is '@{0} &lt;Type Name&gt; &lt;Property Name&gt;'.
        /// </summary>
        internal static string MvcRazorCodeParser_InjectDirectivePropertyNameRequired
        {
            get { return GetString("MvcRazorCodeParser_InjectDirectivePropertyNameRequired"); }
        }

        /// <summary>
        /// A property name must be specified when using the '{0}' statement. Format for a '{0}' statement is '@{0} &lt;Type Name&gt; &lt;Property Name&gt;'.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_InjectDirectivePropertyNameRequired(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_InjectDirectivePropertyNameRequired"), p0);
        }

        /// <summary>
        /// The '{0}' keyword must be followed by a type name on the same line.
        /// </summary>
        internal static string MvcRazorCodeParser_KeywordMustBeFollowedByTypeName
        {
            get { return GetString("MvcRazorCodeParser_KeywordMustBeFollowedByTypeName"); }
        }

        /// <summary>
        /// The '{0}' keyword must be followed by a type name on the same line.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_KeywordMustBeFollowedByTypeName(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_KeywordMustBeFollowedByTypeName"), p0);
        }

        /// <summary>
        /// The '{0}' keyword must be followed by a type name on the same line.
        /// </summary>
        internal static string MvcRazorCodeParser_ModelKeywordMustBeFollowedByTypeName
        {
            get { return GetString("MvcRazorCodeParser_ModelKeywordMustBeFollowedByTypeName"); }
        }

        /// <summary>
        /// The '{0}' keyword must be followed by a type name on the same line.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_ModelKeywordMustBeFollowedByTypeName(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_ModelKeywordMustBeFollowedByTypeName"), p0);
        }

        /// <summary>
        /// Only one '{0}' statement is allowed in a file.
        /// </summary>
        internal static string MvcRazorCodeParser_OnlyOneModelStatementIsAllowed
        {
            get { return GetString("MvcRazorCodeParser_OnlyOneModelStatementIsAllowed"); }
        }

        /// <summary>
        /// Only one '{0}' statement is allowed in a file.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_OnlyOneModelStatementIsAllowed(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_OnlyOneModelStatementIsAllowed"), p0);
        }

        /// <summary>
        /// Only one '{0}' statement is allowed in a file.
        /// </summary>
        internal static string MvcRazorCodeParser_OnlyOneModelStatementIsAllowed1
        {
            get { return GetString("MvcRazorCodeParser_OnlyOneModelStatementIsAllowed1"); }
        }

        /// <summary>
        /// Only one '{0}' statement is allowed in a file.
        /// </summary>
        internal static string FormatMvcRazorCodeParser_OnlyOneModelStatementIsAllowed1(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MvcRazorCodeParser_OnlyOneModelStatementIsAllowed1"), p0);
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            System.Diagnostics.Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
