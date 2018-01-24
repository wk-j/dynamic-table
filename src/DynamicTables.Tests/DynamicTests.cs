using System;
using Xunit;

namespace DynamicTables.Tests {

    class C {
        public int A { set; get; } = 100;
        public int B { set; get; } = 200;
        public string E { set; get; } = "Hello, world!";
    }
    public class DynamicTests {
        [Fact]
        public void DynamicTest() {
            var data = new[] {
                new { A = 100, B = 200},
                new { A = 100, B = 200},
                new { A = 100, B = 200},
            };
            DynamicTable.From<dynamic>(data).Write();
        }

        [Fact]
        public void ClassTest() {
            var data = new[] {
                new C(),
                new C(),
                new C(),
                new C(),
                new C(),
            };
            DynamicTable.From(data).Write();
        }
    }
}
