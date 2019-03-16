using System;
using Npgsql;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DynamicTables.Query {

    class Student {
        [Key]
        public string Id { set; get; }
        public string T1 { set; get; }
        public string T2 { set; get; }
    }

    class MyContext : DbContext {
        public MyContext(DbContextOptions options) : base(options) { }
        public DbSet<Student> Students { set; get; }
    }

    class Program {

        static async Task<int> InsertAsync(DbContextOptions options) {
            using var context = new MyContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Students.AddRange(new[] {
                new Student {
                    T1 = "ทั้งดูหนังดูซีรี่ส์",
                    T2 = "Tesla ประกาศแต่งตั้ง Zachary Kirkhorn"
                },
                new Student {
                    T1 = "ผู้พัฒนาเกม Stardew Valley",
                    T2 = "ประกาศแผนการอัพเกรดสมาร์ทโฟนรุ่นเก่าๆ"
                },
                new Student {
                    T1 = "ตามปีการเงินบริษัท 2019",
                    T2 = "แจ้งเหตุฉุกเฉิน"
                },
             });
            return await context.SaveChangesAsync();
        }

        static async Task<List<Student>> QueryAsync(DbContextOptions options) {
            using var context = new MyContext(options);
            return await context.Students.ToListAsync();
        }



        static async Task Main(string[] args) {
            var str = "Host=localhost;User Id=postgres;Password=1234;Database=FullTextSearch";

            var options = new DbContextOptionsBuilder()
                .UseNpgsql(str).Options;
            // .UseInMemoryDatabase("InMemory").Options;

            await InsertAsync(options);

            var connection = new NpgsqlConnection(str);
            var query = connection.Query<dynamic>(@"select * from ""Students"" limit 20");

            var format = Format.MarkDown;

            DynamicTable.From(query).Write(format);
        }
    }
}
