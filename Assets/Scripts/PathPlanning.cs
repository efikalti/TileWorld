using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlanning {

    int Gridsize;
    Dictionary<string, Tile> World;
    Helper helper = new Helper();

    public List<Tile> PlanRoute(Vector3 position, Vector3 boxTarget, Vector3 holeTarget, Dictionary<string, Tile> w, int gridSize)
    {
        World = helper.CopyDict(w);
        Gridsize = gridSize;
        List<Tile> route = new List<Tile>();
        List<Tile> routeToPush = new List<Tile>();

        List<Tile> boxToHole = AStar(boxTarget, holeTarget, true);
        if (boxToHole == null) return null;

        Vector3 previous = position;
        Vector3 current = boxTarget;
        Vector3 next;
        Tile pushPosition;
        bool goToPushPosition = true;
        int index = 0;
        while(index < boxToHole.Count)
        {
            next = boxToHole[index].GetPosition();
            goToPushPosition = WillTurn(previous, current, next);

            // Check if the player needs to move to a different position to push the box along the path
            if (goToPushPosition)
            {
                // Calculate push position
                pushPosition = Opposite(current, next);
                if (pushPosition == null)
                {
                    Debug.Log("current: " + current.ToString());
                    Debug.Log("next: " + next.ToString());
                    Debug.Log("null push position");
                    return null;
                }

                routeToPush = AStar(previous, pushPosition.GetPosition(), false);
                if (routeToPush == null)
                {
                    Debug.Log("null routeToPush");
                    return null;
                }
                foreach(Tile step in routeToPush)
                {
                    route.Add(step);
                }
                previous = pushPosition.GetPosition();
            }
            else
            {

                route.Add(World[helper.ToHash(current)]);
                // Update world
                World[helper.ToHash(current)].RemoveObstacle();
                World[helper.ToHash(next)].SetObstacle();

                index++;
                previous = current;
                current = next;
            }
        }
        return route;
    }

    public List<Tile> AStar(Vector3 position, Vector3 target, bool accountTurns)
    {
        List<Tile> children;

        string start = helper.ToHash(position);
        string end = helper.ToHash(target);
        Vector3 previous = position;

        // Tiles to be explored
        List<int> distances = new List<int>();
        List<TreeNode<Tile>> frontier = new List<TreeNode<Tile>>();
       
        // Tiles that have been explored
        HashSet<Tile> explored = new HashSet<Tile>(new TileComparer());
        // Tree containing the nodes
        TreeNode<Tile> tree = new TreeNode<Tile>(World[start]);
        TreeNode<Tile> current = tree;


        // Add start as the root of the tree
        frontier.Add(current);
        distances.Add(helper.ManhattanDistance( current.Data.GetPosition(), target ));

        int index = 0;
        while (frontier.Count > 0)
        {
            // Select new node to explore based on the min calculated distance
            current = frontier[MinDistance(distances)];

            if (current.Data.Equals(target))
            {
                return GetPath(current);
            }
            
            // Get children for current node
            children = GetConnected(current.Data.GetPosition());

            // Check if they are already in explored or they contain obstacle
            index = 0;
            while (index < children.Count)
            {
                if (explored.Contains(children[index]))
                    children.Remove(children[index]);
                else if (children[index].HasObstacle() && children[index].GetHash() != end)
                    children.Remove(children[index]);
                else if (accountTurns)
                {
                    if (WillTurn(previous, current.Data.GetPosition(), children[index].GetPosition()))
                    {
                        if (HasObstacleOpposite(current.Data.GetPosition(), children[index].GetPosition()))
                        {
                            children.Remove(children[index]);
                        }
                        else
                        {
                            index++;
                        }
                    }
                    else
                    {
                        index++;
                    }
                }
                else
                    index++;

            }

            // Add them as leaf in the tree
            foreach (Tile child in children)
            {
                // Add children node to frontier
                TreeNode<Tile> ch = current.AddChild(child);
                frontier.Add(ch);
                distances.Add(helper.ManhattanDistance(ch.Data.GetPosition(), target));
            }
            // Add current node to explored
            explored.Add(current.Data);

            // Remove current node from frontier
            int remove_index = frontier.IndexOf(current);
            frontier.Remove(current);
            distances.Remove(distances[remove_index]);
            previous = current.Data.GetPosition();

        }

        return null;

    }

    private List<Tile> GetConnected(Vector3 position)
    {
        List<Tile> list = new List<Tile>();
        Tile temp;

        // One tile up
        if (position.x < Gridsize - 1)
        {
            temp = World[helper.ToHash(new Vector3(position.x + 1, 0, position.z))];
            list.Add(temp);
        }

        // One tile down
        if (position.x > 0)
        {
            temp = World[helper.ToHash(new Vector3(position.x - 1, 0, position.z))];
            list.Add(temp);
        }

        // One tile left
        if (position.z < Gridsize - 1)
        {
            temp = World[helper.ToHash(new Vector3(position.x, 0, position.z + 1))];
            list.Add(temp);
        }

        // One tile right
        if (position.z > 0)
        {
            temp = World[helper.ToHash(new Vector3(position.x, 0, position.z - 1))];
            list.Add(temp);
        }

        return list;
    }

    private List<Tile> GetPath(TreeNode<Tile> end)
    {
        List<Tile> path = new List<Tile>();
        Tile tile;

        do
        {
            tile = end.Data;
            path.Add(tile);
            end = end.Parent;
            if (end == null) return null;

        } while (end.Parent != null);

        
        path.Reverse();
        return path;
    }

    private int MinDistance(List<int> distances)
    {
        if (distances.Count < 1)
        {
            return -1;
        }

        int min_index = 0;
        int min = distances[0];
        for (int i=1; i<distances.Count; i++)
        {
            if (distances[i] < min)
            {
                min = distances[i];
                min_index = i;
            }
        }
        return min_index;
    }

    private Tile Opposite(Vector3 current, Vector3 next)
    {
        if (current.x >= 0 && current.x < Gridsize && current.z >= 0 && current.z < Gridsize)
        {
            if (current.x < next.x) // Going left
            {
                if (current.x - 1 < 0) return null;
                return World[helper.ToHash(new Vector3(current.x - 1, 0, current.z))];
            }
            if (current.x > next.x) // Going right
            {
                if (current.x + 1 > Gridsize - 1) return null;
                return World[helper.ToHash(new Vector3(current.x + 1, 0, current.z))];
            }
            if (current.z < next.z) // Going up
            {
                if (current.z - 1 < 0) return null;
                return World[helper.ToHash(new Vector3(current.x, 0, current.z - 1))];
            }
            if (current.z > next.z) // Going down
            {
                if (current.z + 1 > Gridsize - 1) return null;
                return World[helper.ToHash(new Vector3(current.x, 0, current.z + 1))];
            }
        }
        
        return null;
    }

    private bool HasObstacleOpposite(Vector3 current, Vector3 next)
    {

        Tile opposite = Opposite(current, next);
        if (opposite == null)
        {
            return true;
        }
        return World[helper.ToHash(opposite.GetPosition())].HasObstacle();   
    }

    private bool WillTurn(Vector3 previous, Vector3 current, Vector3 next)
    {
        if (previous.x == current.x && current.x == next.x)
        {
            return false;
        }
        if (previous.z == current.z && current.z == next.z)
        {
            return false;
        }

        return true;
    }
}
