using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public abstract class PlayerAgent : MonoBehaviour
{

    // Target object transforms
    protected Transform holeTarget;
    protected Transform boxTarget;
    protected GameObject boxMark;

    protected int maxRewardValue = 20;

    // World object
    protected World world;

    // PathPlanning object
    protected PathPlanning plan;

    // Particle prefab for marking the targets
    protected GameObject targetMark;

    // Path of moves to complete goal
    protected List<Tile> path;

    // Current target index
    protected int currentTarget;

    // Helper object
    protected Helper helper;

    // Player object
    protected Player player;

    // Total reward of the agent
    protected float totalReward;

    // Flat reward for each completed target
    public int flatReward;

    // Distance of targets to calculate final reward
    protected int distance;

    protected enum STATE  {MOVING, PICKING_TARGET, END}
    protected STATE currentState;

    public enum STRATEGY { SAFE, RISKY }
    public STRATEGY strategy;

    protected Text scoreText;

    public void SetupAgent(World world)
    {
        this.world = world;
        PickTarget();
        
    }

    protected void PickTarget()
    {
        if (player) player.Stop();
        currentState = STATE.PICKING_TARGET;


        List<GameObject> holes = world.GetTargets();
        if (holes.Count < 1)
        {
            currentState = STATE.END;
            return;
        }
        // Pick box and hole targets
        Transform[] targets = Strategy(world.GetBoxes(), holes);
        while(targets == null && currentState == STATE.PICKING_TARGET)
        {
            world.UpdateLists();
            targets = Strategy(world.GetBoxes(), world.GetTargets());
        }
        boxTarget = targets[0];
        holeTarget = targets[1];

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

    protected void FollowPlan()
    {
        if (holeTarget == null || boxTarget == null)
        {
            currentState = STATE.PICKING_TARGET;
            PickTarget();
        }
        else if (Mathf.Abs(transform.position.x - path[currentTarget].GetPosition().x) > 0.1 || Mathf.Abs(transform.position.z - path[currentTarget].GetPosition().z) > 0.1)
        {
            player.Move(path[currentTarget].GetPosition());
        }
        else
        {
            currentTarget++;
            if (path == null)
            {
                player.Stop();
                scoreText.text = totalReward.ToString();
                world.UpdateWorld();
                PickTarget();
            }
            else if (currentTarget >= path.Count)
            {
                player.Stop();
                if (currentTarget <= path.Count && currentTarget >= 1) player.transform.position = path[currentTarget--].GetPosition();
                path = null;

                totalReward += distance + flatReward;
                scoreText.text = totalReward.ToString();
                world.UpdateWorld();
                PickTarget();
            }
        }
    }

    protected abstract Transform[] Strategy(List<GameObject> boxes, List<GameObject> holes);

    public void End()
    {
        currentState = STATE.END;
        player.Stop();
    }

    public void SetText(Text text)
    {
        scoreText = text;
        scoreText.text = totalReward.ToString();
    }
}
