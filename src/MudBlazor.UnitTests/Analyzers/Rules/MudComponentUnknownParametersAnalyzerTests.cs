// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MudBlazor.Analyzers;
using MudBlazor.UnitTests.Analyzers.Helpers;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Analyzers.Rules;

public class MudComponentUnknownParametersAnalyzerTests
{

    private const string Usings = """"
        namespace MudBlazor;
        using Microsoft.AspNetCore.Components;
        using System.Collections.Generic;
        public class MudComponentBase : ComponentBase 
        {
            [Parameter(CaptureUnmatchedValues = true)]
            public Dictionary<string, object?> UserAttributes { get; set; } = new Dictionary<string, object?>();
        }
        """";
    
    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            // .AddAnalyzerConfiguration("build_property.MudDebugAnalyzer", "true")
            .WithAnalyzer<MudComponentUnknownParametersAnalyzer>()
            .WithTargetFramework(TargetFramework.AspNetCore8_0);
    }
    
    
    [Theory]
    [TestCase("MudBadge", "Color")]
    public async Task ShouldNotGenerateDiagnosticsForLegalParameters(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    [Parameter]
    public string {{parameterName}} { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "Bottom")]
    [TestCase("MudBadge", "Left")]
    [TestCase("MudBadge", "Start")]
    public async Task ShouldGenerateDiagnosticForSimpleIllegalParameters(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    [Parameter]
    public string Param2 { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .ShouldReportDiagnostic(new DiagnosticResult()
            {
                Id = MudComponentUnknownParametersAnalyzer.DiagnosticId1,
                Message = $"Illegal Parameter '{parameterName}' detected on component '{componentName}' using 'V7IgnoreCase'",
                Locations = [
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length), 
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length)
                ]
            })
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudAutocomplete", "Direction")]
    [TestCase("MudAutocomplete", "OffsetX")]
    [TestCase("MudAutocomplete", "OffsetY")]
    public async Task ShouldGenerateDiagnosticForGenericComponentsIllegalParameters(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}}<T> : MudComponentBase
{
    [Parameter]
    public string Param2 { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}<string>>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .ShouldReportDiagnostic(new DiagnosticResult()
            {
                Id = MudComponentUnknownParametersAnalyzer.DiagnosticId1,
                Message = $"Illegal Parameter '{parameterName}' detected on component '{componentName}' using 'V7IgnoreCase'",
                Locations = [
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length), 
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length)
                ]
            })
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("UnCheckedColor")]
    [TestCase("Command")]
    [TestCase("CommandParameter")]
    [TestCase("IsEnabled")]
    public async Task ShouldGenerateDiagnostistForComponentBaseIllegalParameters(string parameterName)
    {
        // Due to the other components inheriting from MudComponentBase, we have it seperate.
        var componentName = "MudComponentBase";
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .ShouldReportDiagnostic(new DiagnosticResult()
            {
                Id = MudComponentUnknownParametersAnalyzer.DiagnosticId1,
                Message = $"Illegal Parameter '{parameterName}' detected on component '{componentName}' using 'V7IgnoreCase'",
                Locations = [
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length), 
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length)
                ]
            })
            .WithSourceCode(Usings + sourceCode)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "Bottom")]
    [TestCase("MudBadge", "Left")]
    [TestCase("MudBadge", "Start")]
    public async Task ShouldNotGenerateDiagnosticIfIllegalParameterIsPresent(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    // We add the parameter here but should still be a diagnostic since it is present in the illegal parameters set.
    [Parameter]
    public string {{parameterName}} { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "Bottom")]
    [TestCase("MudBadge", "Left")]
    [TestCase("MudBadge", "Start")]
    public async Task ShouldNotGenerateDiagnosticIfIllegalParametersAreDisabled(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    // We add the parameter here even though it's marked as illegal. This prevents MUD0002 from triggering in this case.
    [Parameter]
    public string {{parameterName}} { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .AddAnalyzerConfiguration("build_property.mudillegalparameters", "None") // Report none of the illegal parameters
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "NotAParam3")]
    [TestCase("MudBadge", "IAmUnknown")]
    public async Task ShouldGenerateDiagnosticForUnknownNonLowerCaseParameters(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    [Parameter]
    public string Color { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .ShouldReportDiagnostic(new DiagnosticResult()
            {
                Id = MudComponentUnknownParametersAnalyzer.DiagnosticId2,
                Message = $"Illegal Attribute '{parameterName}' detected on component '{componentName}' using pattern 'LowerCase'",
                Locations = [
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length), 
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length)
                ]
            })
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "aria-label")]
    [TestCase("MudBadge", "data-text")]
    public async Task ShouldNotGenerateDiagnosticForUnknownAiraOrDataParameters(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    [Parameter]
    public string Color { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "aria-label")]
    [TestCase("MudBadge", "data-text")]
    [TestCase("MudBadge", "NotAParam3")]
    [TestCase("MudBadge", "IAmUnknown")]
    [TestCase("MudBadge", "title")]
    [TestCase("MudBadge", "oninput")]
    public async Task ShouldGenerateDiagnosticForUnknownParametersWhenNoneAllowed(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    [Parameter]
    public string Color { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .AddAnalyzerConfiguration("build_property.mudallowedattributepattern", "None")
            .ShouldReportDiagnostic(new DiagnosticResult()
            {
                Id = MudComponentUnknownParametersAnalyzer.DiagnosticId2,
                Message = $"Illegal Attribute '{parameterName}' detected on component '{componentName}' using pattern 'None'",
                Locations = [
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length), 
                    new DiagnosticResultLocation("Test0.cs", 13, 9, 13, 46 + parameterName.Length)
                ]
            })
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
    
    [Theory]
    [TestCase("MudBadge", "aria-label")]
    [TestCase("MudBadge", "data-text")]
    [TestCase("MudBadge", "NotAParam3")]
    [TestCase("MudBadge", "IAmUnknown")]
    [TestCase("MudBadge", "title")]
    [TestCase("MudBadge", "oninput")]
    public async Task ShouldNotGenerateDiagnosticForUnknownParametersWhenAnyAllowed(string componentName, string parameterName)
    {
        var componentWithParameters = $$"""
public class {{componentName}} : MudComponentBase
{
    [Parameter]
    public string Color { get; set; }

    public string NotAParam3 { get; set; }
}
""";
        
        var sourceCode = $$"""
class TypeName : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenComponent<{{componentName}}>(0);
        __builder.AddAttribute(1, "{{parameterName}}", "test");
        __builder.CloseComponent();
    }
}
""";

        await CreateProjectBuilder()
            .AddAnalyzerConfiguration("build_property.mudallowedattributepattern", "Any")
            .WithSourceCode(Usings + sourceCode + componentWithParameters)
            .ValidateAsync();
    }
}
