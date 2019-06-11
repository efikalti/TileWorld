using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerAgentHeuristic : PlayerAgent {

    void Start()
    {
        // Setup reward params
        totalReward = 0;
        currentTarget = 0;

        StartCoroutine("UpdatePlayer");
    }

    void Awake()
    {
        player = GetComponent<Player>();
        currentState = STATE.PICKING_TARGET;
    }


    IEnumerator UpdatePlayer()
    {
        for (; ; )
        {
            if (currentState == STATE.MOVING)
            {
                if (holeTarget == null || boxTarget == null)
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
                    Vector3 pos = path[currentTarget].GetPosition();
                    pos.y = transform.position.y;
                    player.transform.position = pos;
                    scoreText.text = totalReward.ToString();
                    world.UpdateWorld();
                    PickTarget();

                }
                else
                {
                    FollowPlan();
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }


    void Update()
    {
    }
    
    override
    protected Transform[] Strategy(List<GameObject> boxes, List<GameObject> holes)
    {

        Transform[] targets = new Transform[2];
        int selectedBox=0;
        int selectedHole=0;
        int dist;

        helper = new Helper();

        if (holes[selectedHole] == null)
        {
            return null;
        }
        int selectedDist = helper.ManhattanDistance(boxes[selectedBox].transform.position, holes[selectedHole].transform.position);

        for (int i=0; i<boxes.Count; i++)
        {
            if (holes.Count <= 0)
            {
                return null;
            }
            for (int j=0; j<holes.Count; j++)
            {
                if (holes[j] == null)
                {
                    return null;
                }
                dist = helper.ManhattanDistance(boxes[i].transform.position, holes[j].transform.position);
                
                if (strategy == STRATEGY.SAFE)
                {
                    if (dist < selectedDist)
                    {
                        selectedBox = i;
                        selectedHole = j;
                        selectedDist = dist;
                    }
                }
                else if (strategy == STRATEGY.RISKY)
                {
                    if (dist > selectedDist)
                    {
                        selectedBox = i;
                        selectedHole = j;
                        selectedDist = dist;
                    }
                }
            }
        }

        distance = selectedDist;

        targets[0] = boxes[selectedBox].transform;
        targets[1] = holes[selectedHole].transform;

        return targets;
    }
}
