namespace Fennec.Database.Graph;

public class Node
{
    public string Id { get; set; }
    public string Name { get; set; }

    public Node(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
