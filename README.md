## Dynamic Tables

[![NuGet](https://img.shields.io/nuget/v/wk.DynamicTables.svg)](https://www.nuget.org/packages/wk.DynamicTables)

Write collection of anonymous object out to console, Most of code grep from [ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables)

## Installation

```bash
dotnet add package wk.DynamicTables
```

## Usage

```csharp
using DynamicTables;

var data = new[] {
    new { A = 100, B = 200, C = 300 },
    new { A = 200, B = 300, C = 400 },
    new { A = 300, B = 500, C = 500 },
};

DynamicTable.From(data).Write();
```

```bash
 -------------------
 | A   | B   | C   |
 -------------------
 | 100 | 200 | 300 |
 -------------------
 | 200 | 300 | 400 |
 -------------------
 | 300 | 500 | 500 |
 -------------------
 ```