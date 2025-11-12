using Plexer;

class Program
{
  static void Main()
  {
    var left = new Symbol("left", "<");
    var right = new Symbol("right", ">");

    var dir = left.Or(right).WithName("dir");
    var otherDir = Node.InOrder([left, right]).WithName("otherDir");
    var both = otherDir.Optional().Or(dir).WithName("as").Or(left).WithName("both");

    Console.WriteLine(left.ToString());
    Console.WriteLine(right.ToString());
    Console.WriteLine(dir.ToString());
    Console.WriteLine(otherDir.ToString());
    Console.WriteLine(both.ToString());
  }
}
