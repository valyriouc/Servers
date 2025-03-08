namespace Servers.HttpServer;

public class HttpGenerator
{

    private static Dictionary<int, Func<HttpNode, bool>> _ruleSet = new()
    {
        { 0, (node) => node.Type == HttpNodeType.Version },
        { 1, (node) => node.Type == HttpNodeType.Status },
        { 2, (node) => node.Type == HttpNodeType.Header },
        { 3, (node) => node.Type == HttpNodeType.Body }
    };

    private static IEnumerable<HttpNode> AsValidatedNodes(IEnumerable<HttpNode> nodes)
    {
        int counter = 0;
        bool firstHeader = true;
        foreach (HttpNode node in nodes)
        {
            Func<HttpNode, bool> func = _ruleSet[counter];
            if (!func(node))
            {
                throw new HttpParserException("Invalid http node in response!");
            }
            
            if (node.Type == HttpNodeType.Header && firstHeader)
            {
                counter++;
            }
            else
            {
                continue;
            }

            counter++;
            yield return node;
        }
    }

    public static ReadOnlyMemory<byte> Generate(IEnumerable<HttpNode> nodes)
    {
        using IEnumerator<HttpNode> enumerator = AsValidatedNodes(nodes).GetEnumerator();
        MemoryStream stream = new();
        
        while (enumerator.MoveNext())
        {
            HttpNode current = enumerator.Current;   
            stream.Write(current.Value);    
        }
        
        return stream.ToArray();
    }
}