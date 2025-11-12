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
    public bool IsOptional { get; set; } = false;

    public Node() { }

    public Node(string name) => Name = name;

    public virtual Node WithName(string name)
    {
      return new Node(name);
    }

    public virtual Node Optional()
    {
      if (Name is null)
      {
        var unnamed = new Node();
        unnamed.IsOptional = true;
        return unnamed;
      }
      var named = new Node(Name);
      named.IsOptional = true;
      return named;
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

    public override Symbol Optional()
    {
      if (base.Name is null)
      {
        var unnamed = new Symbol(Chars);
        unnamed.IsOptional = true;
        return unnamed;
      }
      var named = new Symbol(base.Name, Chars);
      named.IsOptional = true;
      return named;
    }

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

    public override Pattern Optional()
    {
      if (base.Name is null)
      {
        var unnamed = new Pattern(RegEx);
        unnamed.IsOptional = true;
        return unnamed;
      }
      var named = new Pattern(base.Name, RegEx);
      named.IsOptional = true;
      return named;
    }

    public Pattern(string regEx)
      : base() => RegEx = regEx;

    public Pattern(string name, string regEx)
      : base(name) => RegEx = regEx;
  }

  public class Group : Node
  {
    public List<Node> Nodes = new List<Node>();
  }

  public class Chain : Node
  {
    public List<Node> Nodes = new List<Node>();

    public override Chain WithName(string name)
    {
      return new Chain(name, Nodes);
    }

    public override Chain Optional()
    {
      if (base.Name is null)
      {
        var unnamed = new Chain(Nodes);
        unnamed.IsOptional = true;
        return unnamed;
      }
      var named = new Chain(base.Name, Nodes);
      named.IsOptional = true;
      return named;
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
      return $"{res}{(inner ? ")" : "")}{(IsOptional ? "?" : "")}{(inner ? "" : ";")}";
    }

    public Chain()
      : base() { }

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
      return new Collection(name, Nodes);
    }

    public override Collection Optional()
    {
      if (base.Name is null)
      {
        var unnamed = new Collection(Nodes);
        unnamed.IsOptional = true;
        return unnamed;
      }
      var named = new Collection(base.Name, Nodes);
      named.IsOptional = true;
      return named;
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
      return $"{res}{(inner ? ")" : "")}{(IsOptional ? "?" : "")}{(inner ? "" : ";")}";
    }

    public Collection()
      : base() { }

    public Collection(string name)
      : base(name) { }

    public Collection(List<Node> nodes)
      : base() => Nodes = nodes;

    public Collection(string name, List<Node> nodes)
      : base(name) => Nodes = nodes;
  }
}
