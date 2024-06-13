// Borrowed from https://github.com/meziantou/Meziantou.Analyzer
// Copyright (c) 2018 Gérald Barré
// License MIT

namespace MudBlazor.UnitTests.Analyzers.Helpers;

﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public sealed class DiagnosticResult
{
    private IReadOnlyList<DiagnosticResultLocation> _locations;

    public IReadOnlyList<DiagnosticResultLocation> Locations
    {
        get => _locations ??= [];
        set => _locations = value;
    }

    public DiagnosticSeverity? Severity { get; set; }

    public string Id { get; set; }

    public string Message { get; set; }

    public string Path => Locations.Count > 0 ? Locations[0].Path : "";

    public int Line => Locations.Count > 0 ? Locations[0].LineStart : -1;

    public int Column => Locations.Count > 0 ? Locations[0].ColumnStart : -1;
}
