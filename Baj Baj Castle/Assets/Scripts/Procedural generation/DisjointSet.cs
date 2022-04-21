public class DisjointSet
{
    int[] parent;
    int[] rank;

    public DisjointSet(int size)
    {
        parent = new int[size];
        rank = new int[size];
    }

    public void MakeSet(int x)
    {
        parent[x] = x;
    }

    public int Find(int x)
    {
        while (parent[x] != x)
        {
            x = parent[x];
        }
        return x;
    }

    public void Union(int x, int y)
    {
        int xRoot = Find(x);
        int yRoot = Find(y);

        if (xRoot == yRoot) return;

        if (rank[xRoot] > rank[yRoot])
        {
            parent[yRoot] = xRoot;
        }
        else
        {
            parent[xRoot] = yRoot;
            if (rank[xRoot] == rank[yRoot])
            {
                rank[yRoot]++;
            }
        }
    }
}
