using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper {

    public Helper()
    {
    }

    // Return hash using only x and z coords
    public string ToHash(Vector3 point)
    {
        return Mathf.RoundToInt(point.x) + "," + Mathf.RoundToInt(point.z);
    }

    // Calculates and return the manhattan distance between two objects
    public int ManhattanDistance(Vector3 objectA, Vector3 objectB)
    {
        return Mathf.FloorToInt(Mathf.Abs(objectA.x - objectB.x) + Mathf.Abs(objectA.z - objectB.z));
    }


    public Dictionary<string, Tile> CopyDict(Dictionary<string, Tile> oldDict)
    {
        Dictionary<string, Tile> newDict = new Dictionary<string, Tile>();
        Tile tile;
        foreach (string key in oldDict.Keys)
        {
            tile = new Tile(oldDict[key].GetPosition());
            if (oldDict[key].HasObstacle()) tile.SetObstacle();
            newDict.Add(key, tile);
        }
        return newDict;
    }
}
