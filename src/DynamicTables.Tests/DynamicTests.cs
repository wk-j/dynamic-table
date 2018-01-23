using System;
using Xunit;

namespace DynamicTables.Tests {
    public class DynamicTests {
        [Fact]
        public void Test1() {
            var data = new[] {
                new { A = 100, B = 200},
                new { A = 100, B = 200},
                new { A = 100, B = 200},
            };
            DynamicTable.From<dynamic>(data).Write(Format.Alternative);
        }
    }
}
