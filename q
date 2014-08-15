[1mdiff --git a/Mvc.sln b/Mvc.sln[m
[1mindex 6572638..0a7055d 100644[m
[1m--- a/Mvc.sln[m
[1m+++ b/Mvc.sln[m
[36m@@ -1,7 +1,6 @@[m
 ﻿[m
 Microsoft Visual Studio Solution File, Format Version 12.00[m
 # Visual Studio 14[m
[31m-VisualStudioVersion = 14.0.21902.1[m
 VisualStudioVersion = 14.0.22013.1[m
 MinimumVisualStudioVersion = 10.0.40219.1[m
 Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "samples", "samples", "{DAAE4C74-D06F-4874-A166-33305D2643CE}"[m
[36m@@ -68,11 +67,11 @@[m [mProject("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Solution Items", "Solution[m
 	ProjectSection(SolutionItems) = preProject[m
 		global.json = global.json[m
 	EndProjectSection[m
[31m-Project("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}") = "ConnegWebsite", "test\WebSites\ConnegWebSite\ConnegWebsite.kproj", "{C6E5AFFA-890A-448F-8DE3-878B1D3C9FC7}"[m
[31m-Project("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}") = "AntiForgeryWebSite", "test\WebSites\AntiForgeryWebSite\AntiForgeryWebSite.kproj", "{A353B17E-A940-4CE8-8BF9-179E24A9041F}"[m
 EndProject[m
 Project("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}") = "ConnegWebsite", "test\WebSites\ConnegWebSite\ConnegWebsite.kproj", "{C6E5AFFA-890A-448F-8DE3-878B1D3C9FC7}"[m
 EndProject[m
[32m+[m[32mProject("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}") = "AntiForgeryWebSite", "test\WebSites\AntiForgeryWebSite\AntiForgeryWebSite.kproj", "{A353B17E-A940-4CE8-8BF9-179E24A9041F}"[m
[32m+[m[32mEndProject[m
 Global[m
 	GlobalSection(SolutionConfigurationPlatforms) = preSolution[m
 		Debug|Any CPU = Debug|Any CPU[m
[36m@@ -363,16 +362,6 @@[m [mGlobal[m
 		{A353B17E-A940-4CE8-8BF9-179E24A9041F}.Release|Mixed Platforms.ActiveCfg = Release|Any CPU[m
 		{A353B17E-A940-4CE8-8BF9-179E24A9041F}.Release|Mixed Platforms.Build.0 = Release|Any CPU[m
 		{A353B17E-A940-4CE8-8BF9-179E24A9041F}.Release|x86.ActiveCfg = Release|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Debug|Any CPU.ActiveCfg = Debug|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Debug|Any CPU.Build.0 = Debug|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Debug|Mixed Platforms.ActiveCfg = Debug|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Debug|Mixed Platforms.Build.0 = Debug|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Debug|x86.ActiveCfg = Debug|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Release|Any CPU.ActiveCfg = Release|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Release|Any CPU.Build.0 = Release|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Release|Mixed Platforms.ActiveCfg = Release|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Release|Mixed Platforms.Build.0 = Release|Any CPU[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254}.Release|x86.ActiveCfg = Release|Any CPU[m
 	EndGlobalSection[m
 	GlobalSection(SolutionProperties) = preSolution[m
 		HideSolutionNode = FALSE[m
[36m@@ -407,6 +396,5 @@[m [mGlobal[m
 		{EE1AB716-F102-4CA3-AD2C-214A44B459A0} = {16703B76-C9F7-4C75-AE6C-53D92E308E3C}[m
 		{C6E5AFFA-890A-448F-8DE3-878B1D3C9FC7} = {16703B76-C9F7-4C75-AE6C-53D92E308E3C}[m
 		{A353B17E-A940-4CE8-8BF9-179E24A9041F} = {16703B76-C9F7-4C75-AE6C-53D92E308E3C}[m
[31m-		{5F945B82-FE5F-425C-956C-8BC2F2020254} = {3BA657BF-28B1-42DA-B5B0-1C4601FCF7B1}[m
 	EndGlobalSection[m
 EndGlobal[m
[1mdiff --git a/src/Microsoft.AspNet.Mvc.ModelBinding/Formatters/JsonInputFormatter.cs b/src/Microsoft.AspNet.Mvc.ModelBinding/Formatters/JsonInputFormatter.cs[m
[1mindex dd119df..b84cd89 100644[m
[1m--- a/src/Microsoft.AspNet.Mvc.ModelBinding/Formatters/JsonInputFormatter.cs[m
[1m+++ b/src/Microsoft.AspNet.Mvc.ModelBinding/Formatters/JsonInputFormatter.cs[m
[36m@@ -74,7 +74,8 @@[m [mpublic JsonSerializerSettings SerializerSettings[m
                 return;[m
             }[m
 [m
[31m-            var requestContentType = MediaTypeHeaderValue.Parse(request.ContentType);[m
[32m+[m[32m            MediaTypeHeaderValue requestContentType = null;[m
[32m+[m[32m            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);[m
 [m
             // Get the character encoding for the content[m
             // Never non-null since SelectCharacterEncoding() throws in error / not found scenarios[m
[1mdiff --git a/src/Microsoft.AspNet.Mvc.ModelBinding/ValueProviders/FormValueProviderFactory.cs b/src/Microsoft.AspNet.Mvc.ModelBinding/ValueProviders/FormValueProviderFactory.cs[m
[1mindex c56ae80..3957fa1 100644[m
[1m--- a/src/Microsoft.AspNet.Mvc.ModelBinding/ValueProviders/FormValueProviderFactory.cs[m
[1m+++ b/src/Microsoft.AspNet.Mvc.ModelBinding/ValueProviders/FormValueProviderFactory.cs[m
[36m@@ -27,8 +27,8 @@[m [mpublic IValueProvider GetValueProvider([NotNull] ValueProviderFactoryContext con[m
 [m
         private bool IsSupportedContentType(HttpRequest request)[m
         {[m
[31m-            var requestContentType = MediaTypeHeaderValue.Parse(request.ContentType);[m
[31m-            return requestContentType != null &&[m
[32m+[m[32m            MediaTypeHeaderValue requestContentType = null;[m
[32m+[m[32m            return MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType) &&[m
                     _formEncodedContentType.IsSubsetOf(requestContentType);[m
         }[m
 [m
[1mdiff --git a/src/Microsoft.AspNet.Mvc/MvcServices.cs b/src/Microsoft.AspNet.Mvc/MvcServices.cs[m
[1mindex a0b9485..2ad09b4 100644[m
[1m--- a/src/Microsoft.AspNet.Mvc/MvcServices.cs[m
[1m+++ b/src/Microsoft.AspNet.Mvc/MvcServices.cs[m
[36m@@ -64,10 +64,8 @@[m [mpublic static IEnumerable<IServiceDescriptor> GetDefaultServices(IConfiguration[m
             yield return describe.Transient<IModelMetadataProvider, DataAnnotationsModelMetadataProvider>();[m
             yield return describe.Scoped<IActionBindingContextProvider, DefaultActionBindingContextProvider>();[m
 [m
[31m-            yield return describe.Transient<IInputFormatter, JsonInputFormatter>();[m
[31m-            yield return describe.Transient<IInputFormatter, XmlSerializerInputFormatter>();[m
[31m-            yield return describe.Transient<IInputFormatter, XmlDataContractSerializerInputFormatter>();[m
             yield return describe.Transient<IInputFormatterProvider, TempInputFormatterProvider>();[m
[32m+[m[32m            yield return describe.Transient<IInputFormattersProvider, DefaultInputFormattersProvider>();[m
 [m
             yield return describe.Transient<IModelBinderProvider, DefaultModelBindersProvider>();[m
             yield return describe.Scoped<ICompositeModelBinder, CompositeModelBinder>();[m
[1mdiff --git a/test/Microsoft.AspNet.Mvc.Test/MvcOptionSetupTest.cs b/test/Microsoft.AspNet.Mvc.Test/MvcOptionSetupTest.cs[m
[1mindex 591e47b..32d3baa 100644[m
[1m--- a/test/Microsoft.AspNet.Mvc.Test/MvcOptionSetupTest.cs[m
[1m+++ b/test/Microsoft.AspNet.Mvc.Test/MvcOptionSetupTest.cs[m
[36m@@ -90,9 +90,9 @@[m [mpublic void Setup_SetsUpInputFormatters()[m
 [m
             // Assert[m
             Assert.Equal(3, mvcOptions.InputFormatters.Count);[m
[31m-            Assert.IsType<JsonInputFormatter>(mvcOptions.InputFormatters[0]);[m
[31m-            Assert.IsType<XmlSerializerInputFormatter>(mvcOptions.InputFormatters[1]);[m
[31m-            Assert.IsType<XmlDataContractSerializerInputFormatter>(mvcOptions.InputFormatters[2]);[m
[32m+[m[32m            Assert.IsType<JsonInputFormatter>(mvcOptions.InputFormatters[0].Instance);[m
[32m+[m[32m            Assert.IsType<XmlSerializerInputFormatter>(mvcOptions.InputFormatters[1].Instance);[m
[32m+[m[32m            Assert.IsType<XmlDataContractSerializerInputFormatter>(mvcOptions.InputFormatters[2].Instance);[m
         }[m
     }[m
 }[m
\ No newline at end of file[m
