using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComparer : IEqualityComparer<Tile>
{
    public bool Equals(Tile tileA, Tile tileB)
    {
        return tileA.GetHashCode() == tileB.GetHashCode();
    }
    public int GetHashCode(Tile tile)
    {
        return tile.GetHash().GetHashCode();
    }
}
