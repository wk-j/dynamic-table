#! "netcoreapp2.0"
#r "nuget: NetStandard.Library, 2.0.0"
#r "nuget: DynamicTables, 0.0.1"

using DynamicTables;

var data = new[] {
    new { A = 100, B = 200, C = 300 },
    new { A = 200, B = 300, C = 400 },
    new { A = 300, B = 500, C = 500 },
};

DynamicTable.From(data).Write();