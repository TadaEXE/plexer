namespace Plexer
{
  public interface Connectable
  {
    public static abstract Collection OneOf(List<Node> nodes);
    public Collection Or(Node node);
    public Collection Or(List<Node> nodes);

    public static abstract Chain InOrder(List<Node> nodes);
    public Chain Then(Node node);
    public Chain Then(List<Node> orderedNodes);
  }

  public class Node : Connectable
  {
    public string? Name { get; set; }
    public bool IsNamed => Name is not null;
    public NodeOption Option { get; set; } = NodeOption.ExactlyOne;
    public IDataContainer? AttachedData { get; set; } = null;
    public bool HasDataAttached => AttachedData is not null;

    public Node() { }

    public Node(Node node)
    {
      Name = node.Name;
      Option = node.Option;
      AttachedData = node.AttachedData;
    }

    public Node(string name) => Name = name;

    public void AttachDataContainer<T>()
      where T : IParsable<T> => AttachedData = new DataContainer<T>();

    public virtual Node WithName(string name)
    {
      var tmp = new Node(this);
      tmp.Name = name;
      return tmp;
    }

    public virtual Node WithOption(NodeOption option)
    {
      var tmp = new Node(this);
      tmp.Option = option;
      return tmp;
    }

    public virtual List<Node> Unpack()
    {
      return [this];
    }

    public virtual string ToString(bool inner = false)
    {
      return Name ?? "unnamed";
    }

    public static Collection OneOf(List<Node> nodes)
    {
      return new Collection(nodes);
    }

    public Collection Or(Node node)
    {
      return new Collection([this, node]);
    }

    public Collection Or(List<Node> nodes)
    {
      return new Collection(new List<Node>(((List<Node>)[this]).Concat(nodes)));
    }

    public static Chain InOrder(List<Node> nodes)
    {
      return new Chain(nodes);
    }

    public Chain Then(Node node)
    {
      return new Chain([this, node]);
    }

    public Chain Then(List<Node> orderedNodes)
    {
      return new Chain(
        new List<Node>(((List<Node>)[this]).Concat(orderedNodes))
      );
    }
  }

  public class Symbol : Node
  {
    public string Chars { get; }

    public override string ToString(bool inner = false)
    {
      return (base.Name ?? "") + (inner ? "" : $": '{Chars}'");
    }

    public override Symbol WithOption(NodeOption option)
    {
      var tmp = new Symbol(this);
      tmp.Option = option;
      return tmp;
    }

    public Symbol(Symbol symbol)
      : base(symbol) => Chars = symbol.Chars;

    public Symbol(string chars)
      : base() => Chars = chars;

    public Symbol(string name, string chars)
      : base(name) => Chars = chars;
  }

  public class Pattern : Node
  {
    public string RegEx { get; }

    public override string ToString(bool inner = false)
    {
      return (base.Name ?? "") + (inner ? "" : $": \\{RegEx}\\");
    }

    public override Pattern WithOption(NodeOption option)
    {
      var tmp = new Pattern(this);
      tmp.Option = option;
      return tmp;
    }

    public Pattern(Pattern pattern)
      : base(pattern) => RegEx = pattern.RegEx;

    public Pattern(string regEx)
      : base() => RegEx = regEx;

    public Pattern(string name, string regEx)
      : base(name) => RegEx = regEx;
  }

  public class Chain : Node
  {
    public List<Node> Nodes = new List<Node>();

    public override Chain WithName(string name)
    {
      var tmp = new Chain(this);
      tmp.Name = name;
      return tmp;
    }

    public override Chain WithOption(NodeOption option)
    {
      var tmp = new Chain(this);
      tmp.Option = option;
      return tmp;
    }

    public override List<Node> Unpack()
    {
      return Nodes
        .Select(n => n.Unpack())
        .Aggregate((a, b) => a.Concat(b).ToList());
    }

    public override string ToString(bool inner = false)
    {
      string res = inner ? "(" : base.ToString() + ": ";
      res += string.Join(" ", Nodes.Select(n => n.ToString(true)));
      return $"{res}{(inner ? ")" : "")}{NodeOptions.ToString(Option)}{(inner ? "" : ";")}";
    }

    public Chain()
      : base() { }

    public Chain(Chain chain)
      : base(chain) => Nodes = chain.Nodes;

    public Chain(string name)
      : base(name) { }

    public Chain(List<Node> nodes)
      : base() => Nodes = nodes;

    public Chain(string name, List<Node> nodes)
      : base(name) => Nodes = nodes;
  }

  public class Collection : Node
  {
    public List<Node> Nodes = new List<Node>();

    public override Collection WithName(string name)
    {
      var tmp = new Collection(this);
      tmp.Name = name;
      return tmp;
    }

    public override Collection WithOption(NodeOption option)
    {
      var tmp = new Collection(this);
      tmp.Option = option;
      return tmp;
    }

    public override List<Node> Unpack()
    {
      return Nodes
        .Select(n => n.Unpack())
        .Aggregate((a, b) => a.Concat(b).ToList());
    }

    public override string ToString(bool inner = false)
    {
      string res = inner ? "(" : base.ToString() + ": ";
      res += string.Join(" | ", Nodes.Select(n => n.ToString(true)));
      return $"{res}{(inner ? ")" : "")}{NodeOptions.ToString(Option)}{(inner ? "" : ";")}";
    }

    public Collection()
      : base() { }

    public Collection(Collection collection)
      : base(collection) => Nodes = collection.Nodes;

    public Collection(string name)
      : base(name) { }

    public Collection(List<Node> nodes)
      : base() => Nodes = nodes;

    public Collection(string name, List<Node> nodes)
      : base(name) => Nodes = nodes;
  }
}
