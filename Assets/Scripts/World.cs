using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class World : MonoBehaviour {
    

    // GameObject parents
    private GameObject floor;
    private GameObject holes;
    private GameObject obstacles;
    private GameObject boxes;

    // GameObject prefabs
    private GameObject tileWhite;
    private GameObject tileGrey;
    private GameObject holePrefab;
    private GameObject obstaclePrefab;
    private GameObject playerPrefab;
    private GameObject boxPrefab;
    private GameObject player;

    // World variables
    private int gridSize = 10;
    public int holeNumber = 3;
    public int boxNumber = 3;
    public int obstacleNumber = 10;
    public int minLifespan = 15;
    public int maxLifespan = 20;
    public int totalHoles = 10;
    public int totalBoxes = 10;
    private int createdHoles = 0;
    private int createdBoxes = 0;

    // Position hashsets
    private HashSet<Vector3> holePositions;
    private HashSet<Vector3> obstaclePositions;
    private HashSet<Vector3> boxPositions;
    private List<GameObject> holeTargets;
    private List<GameObject> boxTargets;

    // World layout hashsets
    private Dictionary<string, Tile> worldLayout;
    private Dictionary<string, Tile> currentWorldLayout;

    // UI
    public Text scoreText;

    Vector3 position;

    Helper helper;

    void Start ()
    {
        // Load prefabs
        tileWhite = (GameObject)Resources.Load("Prefabs/TileWhite", typeof(GameObject));
        tileGrey = (GameObject)Resources.Load("Prefabs/TileGrey", typeof(GameObject));
        holePrefab = (GameObject)Resources.Load("Prefabs/Hole", typeof(GameObject));
        obstaclePrefab = (GameObject)Resources.Load("Prefabs/Obstacle", typeof(GameObject));
        boxPrefab = (GameObject)Resources.Load("Prefabs/Box", typeof(GameObject));
        playerPrefab = (GameObject)Resources.Load("Prefabs/Player", typeof(GameObject));

        // Find gameobjects to add the world objects that will be instantiated
        floor = transform.Find("Floor").gameObject;
        holes = transform.Find("Holes").gameObject;
        obstacles = transform.Find("Obstacles").gameObject;
        boxes = transform.Find("Boxes").gameObject;

        // Create hashsets for object positions
        holePositions = new HashSet<Vector3>(new PositionComparer());
        obstaclePositions = new HashSet<Vector3>(new PositionComparer());
        boxPositions = new HashSet<Vector3>(new PositionComparer());
        worldLayout = new Dictionary<string, Tile>();
        currentWorldLayout = new Dictionary<string, Tile>();
        holeTargets = new List<GameObject>();
        boxTargets = new List<GameObject>();

        // Create helper object
        helper = new Helper();

        // Create World
        Create();
    }
	
	// Update is called once per frame
	void Update () {
        if (holeTargets.Count < 0 && totalBoxes <= createdBoxes)
        {
            player.GetComponent<PlayerAgent>().End();
        }
    }


    // Set new size for the world
    public void SetSize(int size)
    {
        if (size > 0 && size <= 100)
        {
            this.gridSize = size;
            RectTransform rt = floor.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);

            this.NewWorld();
        }
    }

    /////////////////////////// GET FUNCTIONS ///////////////////////////

    public Dictionary<string, Tile> GetWorldLayout()
    {
        return currentWorldLayout;
    }

    public List<GameObject> GetTargets()
    {
        holeTargets.Clear();
        foreach (Transform obj in holes.transform)
        {
            holeTargets.Add(obj.gameObject);
        }
        return holeTargets;
    }

    public List<GameObject> GetBoxes()
    {
        boxTargets.Clear();
        foreach (Transform obj in boxes.transform)
        {
            boxTargets.Add(obj.gameObject);
        }
        return boxTargets;
    }

    public HashSet<Vector3> GetObstacles()
    {
        return obstaclePositions;
    }

    public int GetGridSize()
    {
        return this.gridSize;
    }

    public void SetText(Text text)
    {
        scoreText = text;
    }


    /////////////////////////// UPDATE LISTS FUNCTIONS ///////////////////////////

    public void UpdateWorld()
    {

        UpdateLists();
        if (totalHoles > createdHoles)
        {
            CreateHoles();

        }
        if (totalBoxes > createdBoxes)
        {
            CreateBoxes();
        }
    }

    public void UpdateLists()
    {
        currentWorldLayout = helper.CopyDict(worldLayout);

        boxTargets.Clear();
        boxPositions.Clear();
        holeTargets.Clear();
        holePositions.Clear();

        foreach (Transform obj in holes.transform)
        {
            currentWorldLayout[helper.ToHash(obj.position)].SetObstacle();
            holeTargets.Add(obj.gameObject);
            holePositions.Add(obj.position);
        }
        foreach (Transform obj in boxes.transform)
        {
            currentWorldLayout[helper.ToHash(obj.position)].SetObstacle();
            boxTargets.Add(obj.gameObject);
            boxPositions.Add(obj.position);
        }
    }

    /////////////////////////// CREATE WORLD FUNCTIONS ///////////////////////////

    // Create new world with the same world variables
    public void NewWorld()
    {
        // Destroy old world
        Clear();
        // Create World
        Create();
    }

    // Destroy all the objects of this world
    // Clear lists of objects
    void Clear()
    {
        // Destroy objects
        foreach (Transform child in boxes.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in holes.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in obstacles.transform)
        {
            Destroy(child.gameObject);
        }

        // Clear lists
        obstaclePositions.Clear();
        holePositions.Clear();
        boxPositions.Clear();
        holeTargets.Clear();
        boxTargets.Clear();
        currentWorldLayout = helper.CopyDict(worldLayout);
    }

    // Destroy old world and create the new world
    public void Create()
    {
        // Create floor
        CreateFloor();

        // Create Obstacles
        CreateObstacles();

        currentWorldLayout = helper.CopyDict(worldLayout);

        // Create Player
        CreatePlayer();

        // Create Holes
        CreateHoles();

        // Create Boxes
        CreateBoxes();

        // Setup agent
        player.GetComponent<PlayerAgent>().SetupAgent(this);
    }

    // Create the floor object
    void CreateFloor()
    {
        int x = 0;
        int z = 0;
        Quaternion rotation = new Quaternion();
        Vector3 position;
        Tile tile;
        for (int i = 0; i < gridSize; i++)
        {
            z = 0;
            for (int j = i; j < i + gridSize; j++)
            {
                position = new Vector3(x, 0, z);
                tile = new Tile(position);
                worldLayout[tile.GetHash()] = tile;
                if (j % 2 == 0)
                {
                    Instantiate(tileWhite, position, rotation, floor.transform);
                }
                else
                {
                    Instantiate(tileGrey, position, rotation, floor.transform);
                }
                z += 1;
            }
            x += 1;
        }
    }

    // Randomly add obstacles in the world
    void CreateObstacles()
    {
        int remaining = obstacleNumber;
        while (remaining > 0)
        {
            do
            {
                // Generate randomly a Vector3 position for the new obstacle
                position = new Vector3(Random.Range(0, gridSize), 0.5f, Random.Range(0, gridSize));
            } while ( obstaclePositions.Contains(position) );
            Instantiate(obstaclePrefab, position, new Quaternion(), obstacles.transform);
            obstaclePositions.Add(position);
            worldLayout[helper.ToHash(position)].SetObstacle();
            remaining--;
        }
    }

    // Randomly add a player to the world
    public void CreatePlayer()
    {
        do
        {
            // Generate randomly a Vector3 position for the new obstacle
            position = new Vector3(Random.Range(0, gridSize), 1f, Random.Range(0, gridSize));
        } while (IsNotOpen(position, obstaclePositions));
        if (player != null)
        {
            player.transform.position = position;
        }
        else
        {

            player = Instantiate(playerPrefab, position, new Quaternion(), transform);
            //player.AddComponent<PlayerAgentHeuristic>();
            player.GetComponent<PlayerAgent>().SetText(scoreText);
        }
    }

    // Randomly add holes in the world
    void CreateHoles()
    {
        int remaining = holeNumber;
        if (createdHoles > 0) remaining = 1;
        createdHoles += remaining;
        int lifespan;
        GameObject newHole;
        bool clear = false;
        while (remaining > 0)
        {
            lifespan = Random.Range(minLifespan, maxLifespan);
            do
            {
                // Generate randomly a Vector3 position for the new hole
                position = new Vector3(Random.Range(0, gridSize), 0.01f, Random.Range(0, gridSize));
                if (currentWorldLayout[helper.ToHash(position)].HasObstacle()) clear = false;
                else if (IsNextToObject(position, boxPositions)) clear = false;
                else if (IsNextToObject(position, obstaclePositions)) clear = false;
                else if (IsNextToObject(position, holePositions)) clear = false;
                else if (helper.ToHash(position) == helper.ToHash(player.transform.position)) clear = false;
                else if (IsNextToObject(position, player.transform.position)) clear = false;
                else clear = true;
            } while (!clear);
            newHole = Instantiate(holePrefab, position, new Quaternion(), holes.transform);
            newHole.GetComponent<LifeTime>().SetLifespan(lifespan);
            holePositions.Add(position);
            currentWorldLayout[helper.ToHash(position)].SetObstacle();
            holeTargets.Add(newHole);
            remaining--;
        }

    }

    // Randomly add boxes in the world
    void CreateBoxes()
    {
        int remaining = boxNumber;
        if (createdBoxes > 0) remaining = 1;
        createdBoxes += remaining;
        GameObject newBox;
        bool clear = false;
        while (remaining > 0)
        {
            do
            {
                // Generate randomly a Vector3 position for the new box
                position = new Vector3(Random.Range(1, gridSize - 1), 0.25f, Random.Range(1, gridSize - 1));
                if (currentWorldLayout[helper.ToHash(position)].HasObstacle()) clear = false;
                else if (IsNextToObject(position, boxPositions)) clear = false;
                else if (IsNextToObject(position, obstaclePositions)) clear = false;
                else if (IsNextToObject(position, holePositions)) clear = false;
                else if (helper.ToHash(position) == helper.ToHash(player.transform.position)) clear = false;
                else if (IsNextToObject(position, player.transform.position)) clear = false;
                else clear = true;
            } while (!clear) ;
            newBox = Instantiate(boxPrefab, position, new Quaternion(), boxes.transform);
            boxPositions.Add(position);
            currentWorldLayout[helper.ToHash(position)].SetObstacle();
            boxTargets.Add(newBox);
            remaining--;
        }
    }

    // Check if this position is next to any object in this group
    bool IsNextToObject(Vector3 worldObject, HashSet<Vector3> group)
    {
        foreach (Vector3 item in group)
        {
            if (IsNextToObject(worldObject, item)) return true;
        }
        return false;
    }

    // Check if this position is next to any object in this group
    bool IsNextToObject(Vector3 worldObject, Vector3 obj)
    {
        int dist;
        dist = helper.ManhattanDistance(obj, worldObject);
        if (dist <= 1) return true;
        if (dist == 2)
        {
            if (obj.x != worldObject.x && obj.z != worldObject.z) return true;
        }
        return false;
    }

    bool IsNotOpen(Vector3 worldObject, HashSet<Vector3> group)
    {
        if (group.Contains(worldObject)) return true;

        int dist;
        foreach (Vector3 item in group)
        {
            dist = helper.ManhattanDistance(item, worldObject);
            if (dist <= 1) return true;
            if (dist == 2)
            {
                if (item.x != worldObject.x && item.z != worldObject.z) return true;
            }
        }
        return false;
    }


}
