// Borrowed from https://github.com/meziantou/Meziantou.Analyzer
// Copyright (c) 2018 Gérald Barré
// License MIT

// ReSharper disable InconsistentNaming
namespace MudBlazor.UnitTests.Analyzers.Helpers;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute")]
public enum TargetFramework
{
    NetStandard2_0,
    NetStandard2_1,
    Net4_8,
    Net5_0,
    Net6_0,
    Net7_0,
    Net8_0,
    Net9_0,
    NetLatest = Net9_0,
    AspNetCore5_0,
    AspNetCore6_0,
    AspNetCore7_0,
    AspNetCore8_0,
    WindowsDesktop5_0,
}
