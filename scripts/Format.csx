
var data = new[] {
    ("ทุกคนอาจจะสงสัยว่าเบอร์เดียว",  40 + 5),
    ("ทุกคนอาจจะสงสัยว่าเบอร์เดียวที่", 40 + 7),
    ("ทั้งดูหนังดูซีรี่ส์ศ์สู้"  , 40 + 12),
    ("ทีที่"  , 40 + 3),
    ("Hi"  , 40),
};

foreach (var (text, l) in data) {
    var format = string.Format("|{0,-" + l + "}|", text);
    Console.WriteLine(format);
}

Console.WriteLine("สู้".Length == 3);
Console.WriteLine("สู".Length == 2);