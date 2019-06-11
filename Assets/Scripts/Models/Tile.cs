using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {

    private readonly string hash;
    private readonly Vector3 position;
    private bool obstacle;
    private bool box;

    public Tile(Vector3 position)
    {
        this.position = position;
        this.hash = ToHash(position);
        this.obstacle = false;
        this.box = false;
    }

    public void SetObstacle()
    {
        this.obstacle = true;
    }

    public void RemoveObstacle()
    {
        this.obstacle = false;
    }

    public bool HasObstacle()
    {
        return obstacle;
    }

    public void SetBox()
    {
        this.box = true;
    }

    public bool HasBox()
    {
        return box;
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public string GetHash()
    {
        return hash;
    }

    public bool Equals(Tile tile)
    {
        if (tile.position.x == this.position.x && tile.position.z == this.position.z)
        {
            return true;
        }
        return false;
    }

    public bool Equals(Vector3 position)
    {
        if (position.x == this.position.x && position.z == this.position.z)
        {
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return "Tile: " + position.x + ", " + position.z;
    }

    private string ToHash(Vector3 point)
    {
        return point.x + "," + point.z;
    }
}
