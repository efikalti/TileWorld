using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MLAgents;

[RequireComponent(typeof(Player))]
public class PlayerAgentDL : PlayerAgent {
    

    int[] distances;

    void Start()
    {
        // Setup reward params
        totalReward = 0;
        flatReward = 2;

        player = GetComponent<Player>();

        StartCoroutine("UpdatePlayer");

        helper = new Helper();
    }

    void Awake()
    {
        strategy = STRATEGY.DL;
        currentState = STATE.PICKING_TARGET;
    }

    void Update()
    {
    }

    public new void SetupAgent(World world)
    {
        this.world = world;
        RequestDecision();
    }

    public override void AgentReset()
    {
        // Reset Agent
        player.Stop();
        world.NewWorld();
    }

    public override void CollectObservations()
    {
        distances = DistBoxFromHole(world.GetBoxes(), world.GetTargets());
        foreach(int dist in distances) AddVectorObs(dist);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        currentState = STATE.PICKING_TARGET;
        int box = Mathf.FloorToInt(vectorAction[0]);
        int hole = Mathf.FloorToInt(vectorAction[1]);

        int reward = CalculateReward(box, hole, world.GetTargets());


        AddReward(reward);
        SetTargets(box, hole);


        Done();
    }

    public void SetTargets(int box, int hole)
    {
        // Set box and hole targets
        boxTarget = world.GetBoxes()[box].transform;
        holeTarget = world.GetTargets()[hole].transform;

        // PathPlanning
        plan = new PathPlanning();
        Vector3 position = new Vector3(Mathf.RoundToInt(transform.position.x), transform.position.y, Mathf.RoundToInt(transform.position.z));
        path = plan.PlanRoute(position, boxTarget.position, holeTarget.position, world.GetWorldLayout(), world.GetGridSize());

        if (path == null)
        {
            print("New world");
            print("null path");
            print("position: " + position);
            print("box: " + boxTarget.position);
            print("hole: " + holeTarget.position);
            world.NewWorld();
        }
        else
        {
            currentTarget = 0;
            targetMark = (GameObject)Resources.Load("Prefabs/TargetMark", typeof(GameObject));
            boxMark = Instantiate(targetMark, boxTarget.position, new Quaternion(), boxTarget);
            Instantiate(targetMark, holeTarget.position, new Quaternion(), holeTarget);
            currentState = STATE.MOVING;
        }
    }

    int CalculateReward(int box, int hole, List<GameObject> holes)
    {
        int[] rewards = new int[9];

        if (holes.Count <= hole) return -1;

        int reward = holes[hole].GetComponent<LifeTime>().GetRemaining() - distances[(box * 3) + hole];

        if (reward < -1) reward = -1;
        if (reward > 1) reward = 1;

        return reward;
    }

    IEnumerator UpdatePlayer()
    {
        for (; ; )
        {
            if (currentState == STATE.MOVING)
            {
                if (holeTarget == null)
                {
                    currentState = STATE.PICKING_TARGET;
                    player.Stop();
                    if (path.Count - currentTarget > 1)
                    {
                        totalReward -= distance + flatReward;
                        Destroy(boxMark.gameObject);
                    }
                    else
                    {
                        totalReward += distance + flatReward;
                    }
                    Vector3 pos = path[currentTarget--].GetPosition();
                    pos.y = transform.position.y;
                    player.transform.position = pos;
                    scoreText.text = totalReward.ToString();
                    world.UpdateWorld();
                    RequestDecision();

                }
                else
                {
                    FollowPlan();
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private int[] DistBoxFromHole(List<GameObject> boxes, List<GameObject> holes)
    {
        int[] distances = new int[9];

        for (int i = 0; i < 3; i++)
        {
            for(int j=0; j < 3; j++)
            {
                if (holes.Count < j) distances[(i * 3) + j] = int.MaxValue;
                else distances[(i * 3) + j] = helper.ManhattanDistance(boxes[i].transform.position, holes[j].transform.position);
            }
        }

        return distances;
    }

    override
    protected Transform[] Strategy(List<GameObject> boxes, List<GameObject> holes)
    {
        return null;
    }
}

