using System.Reflection;

var a = new {
    A = 100,
    B = 200,
    C = "Hello, world"
};

var names = a.GetType().GetProperties().Select(x => (x.Name, x.GetValue(a)));

Console.WriteLine(string.Join(" ", names));