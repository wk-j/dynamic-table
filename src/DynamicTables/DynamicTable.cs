using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicTables {
    public class DynamicTable {
        public IList<object> Columns { get; set; }
        public IList<object[]> Rows { get; protected set; }
        public ConsoleTableOptions Options { get; protected set; }

        public DynamicTable(params string[] columns)
            : this(new ConsoleTableOptions { Columns = new List<string>(columns) }) {
        }

        public DynamicTable(ConsoleTableOptions options) {
            Options = options ?? throw new ArgumentNullException("options");
            Rows = new List<object[]>();
            Columns = new List<object>(options.Columns);
        }

        public DynamicTable AddColumn(IEnumerable<string> names) {
            foreach (var name in names)
                Columns.Add(name);
            return this;
        }

        public DynamicTable AddRow(params object[] values) {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (!Columns.Any())
                throw new Exception("Please set the columns first");

            if (Columns.Count != values.Length)
                throw new Exception(
                    $"The number columns in the row ({Columns.Count}) does not match the values ({values.Length})");

            Rows.Add(values);
            return this;
        }

        private static IEnumerable<string> GetColumn(object o) =>
            o.GetType().GetProperties().Select(x => x.Name);

        private static DynamicTable FromDynamic<T>(IEnumerable<T> values) {
            var type = typeof(T);
            var results = values.Select(value => value.GetType().GetProperties().Select(x => (x.Name, x.GetValue(value))));
            var first = results.FirstOrDefault();
            if (first.Count() != 0) {
                var table = new DynamicTable();
                if (first != null) {
                    table.AddColumn(first.Select(x => x.Item1));
                }
                foreach (var item in results) {
                    table.AddRow(item.Select(x => x.Item2).ToArray());
                }
                return table;
            } else {
                // dynamic query
                var table = new DynamicTable();
                var dicts = values.Select(x => {
                    var dict = (IDictionary<String, Object>)x;
                    return dict;
                });

                var fs = dicts.FirstOrDefault();
                if (fs != null) {
                    table.AddColumn(fs.Keys);
                }
                foreach (var dict in dicts) {
                    table.AddRow(dict.Values.ToArray());
                }
                return table;
            }
        }

        private static DynamicTable FromDictionary<T>(IEnumerable<T> values) {
            var table = new DynamicTable();
            var dicts = values.Select(x => (IDictionary<string, string>)x);
            var fs = dicts.FirstOrDefault();
            if (fs != null) {
                table.AddColumn(fs.Keys);
            }
            foreach (var dict in dicts) {
                table.AddRow(dict.Values.ToArray());
            }
            return table;
        }

        private static DynamicTable FromObject<T>(IEnumerable<T> values, IEnumerable<string> columns) {
            var table = new DynamicTable();
            table.AddColumn(columns);

            var results = values.Select(value => columns.Select(column => GetColumnValue<T>(value, column)));
            foreach (var propertyValues in results) {
                table.AddRow(propertyValues.ToArray());
            }
            return table;
        }

        public static DynamicTable From<T>(IEnumerable<T> values) {
            var columns = GetColumns<T>();
            var isDynmaic = columns.Count() == 0;
            if (typeof(T).Name == "Dictionary`2") {
                return FromDictionary<T>(values);
            } else if (isDynmaic) {
                return FromDynamic<T>(values);
            } else {
                return FromObject<T>(values, columns);
            }
        }


        public override string ToString() {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // create the string format with padding
            var format = Enumerable.Range(0, Columns.Count)
                .Select(i => " | {" + i + ",-" + columnLengths[i] + "}")
                .Aggregate((s, a) => s + a) + " |";

            string f(object[] obj) {
                return Enumerable.Range(0, Columns.Count)
                    .Select(i => {
                        var floating = obj[i].ToString().Where(FloatingCharacter.Glyph.IgnoreCharacters.Contains).Count();
                        var k = " | {" + i + ",-" + (columnLengths[i] + floating) + "}";
                        return k;
                    })
                    .Aggregate((s, a) => s + a) + " |";
            }

            // find the longest formatted line
            var maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
            var columnHeaders = string.Format(format, Columns.ToArray());

            // longest line is greater of formatted columnHeader and longest row
            var longestLine = Math.Max(maxRowLength, columnHeaders.Length);

            // add each row
            //var results = Rows.Select((row, i) => string.Format(format, row)).ToList();
            var results = Rows.Select((row, i) => string.Format(f(row), row)).ToList();

            // create the divider
            var divider = " " + string.Join("", Enumerable.Repeat("-", longestLine - 1)) + " ";

            builder.AppendLine(divider);
            builder.AppendLine(columnHeaders);

            foreach (var row in results) {
                builder.AppendLine(divider);
                builder.AppendLine(row);
            }

            builder.AppendLine(divider);

            if (Options.EnableCount) {
                builder.AppendLine("");
                builder.AppendFormat(" Count: {0}", Rows.Count);
            }

            return builder.ToString();
        }

        public string ToMarkDownString() {
            return ToMarkDownString('|');
        }

        private string ToMarkDownString(char delimiter) {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // create the string format with padding
            var format = Format(columnLengths, delimiter);

            // find the longest formatted line
            var columnHeaders = string.Format(format, Columns.ToArray());

            // add each row
            var results = Rows.Select(row => {
                var f = F(columnLengths, row, delimiter);
                return string.Format(f, row);
            }).ToList();

            // create the divider
            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");

            builder.AppendLine(columnHeaders);
            builder.AppendLine(divider);
            results.ForEach(row => builder.AppendLine(row));

            return builder.ToString();
        }

        public string ToMinimalString() {
            return ToMarkDownString(char.MinValue);
        }

        public string ToStringAlternative() {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // create the string format with padding
            var format = Format(columnLengths);

            string f(object[] obj) {
                return Enumerable.Range(0, Columns.Count)
                    .Select(i => {
                        var floating = obj[i].ToString().Where(FloatingCharacter.Glyph.IgnoreCharacters.Contains).Count();
                        var k = "| {" + i + ",-" + (columnLengths[i] + floating) + "} ";
                        return k;
                    })
                    .Aggregate((s, a) => s + a) + "|";
            }

            // find the longest formatted line
            var columnHeaders = string.Format(format, Columns.ToArray());

            // add each row
            var results = Rows.Select(row => string.Format(f(row), row)).ToList();

            // create the divider
            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");
            var dividerPlus = divider.Replace("|", "+");

            builder.AppendLine(dividerPlus);
            builder.AppendLine(columnHeaders);

            foreach (var row in results) {
                builder.AppendLine(dividerPlus);
                builder.AppendLine(row);
            }
            builder.AppendLine(dividerPlus);

            return builder.ToString();
        }

        private string Format(List<int> columnLengths, char delimiter = '|') {
            var delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
            var format = (Enumerable.Range(0, Columns.Count)
                .Select(i => " " + delimiterStr + " {" + i + ",-" + columnLengths[i] + "}")
                .Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();
            return format;
        }

        private string F(List<int> columnLengths, object[] obj, char delimiter = '|') {
            var floating = FloatingCharacter.Glyph.IgnoreCharacters;
            var delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
            var format = (Enumerable.Range(0, Columns.Count)
                .Select(i => {
                    var count = obj[i]?.ToString().Where(floating.Contains).Count() ?? 0;
                    var a = " " + delimiterStr + " {" + i + ",-" + (columnLengths[i] + count) + "}";
                    return a;
                })
                .Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();
            return format;
        }

        private List<int> ColumnLengths() {
            var floats = FloatingCharacter.Glyph.IgnoreCharacters;
            var columnLengths = Columns
                .Select((t, i) => Rows.Select(x => x[i])
                    .Union(Columns)
                    .Where(x => x != null)
                    .Select(x => {
                        var value = x.ToString();
                        var floatCount = value.Where(k => floats.Contains(k)).Count();
                        var length = value.Length;
                        //Console.WriteLine($"{length} {floatCount} {value}");
                        return length - floatCount;
                        // return length;
                    })
                    .Max())
                .ToList();
            return columnLengths;
        }

        public void Write(Format format = DynamicTables.Format.Default) {
            switch (format) {
                case DynamicTables.Format.Default:
                    Console.WriteLine(ToString());
                    break;
                case DynamicTables.Format.MarkDown:
                    Console.WriteLine(ToMarkDownString());
                    break;
                case DynamicTables.Format.Alternative:
                    Console.WriteLine(ToStringAlternative());
                    break;
                case DynamicTables.Format.Minimal:
                    Console.WriteLine(ToMinimalString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static IEnumerable<string> GetColumns<T>() {
            return typeof(T).GetProperties().Select(x => x.Name).ToArray();
        }

        private static object GetColumnValue<T>(object target, string column) {
            return typeof(T).GetProperty(column).GetValue(target, null);
        }

        private static bool CheckIfAnonymousType(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}