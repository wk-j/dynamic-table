using System;
using Npgsql;
using Dapper;

namespace DynamicTables.Query {
    class Program {
        static void Main(string[] args) {
            var str = "Host=localhost;User Id=postgres;Password=1234;Database=FullTextSearch";
            var connection = new NpgsqlConnection(str);
            var query = connection.Query<dynamic>(@"select ""Id"", ""Name"" from ""Students"" limit 20");
            DynamicTable.From(query).Write();
        }
    }
}
