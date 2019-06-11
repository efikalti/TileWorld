using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionComparer : IEqualityComparer<Vector3>
{
    public bool Equals(Vector3 positionA, Vector3 positionB)
    {
        if (positionA.x == positionB.x && positionA.z == positionB.z) { return true; }
        return false;
    }
    public int GetHashCode(Vector3 position)
    {
        string code = position.x + "," + position.z;
        return code.GetHashCode();
    }
}
