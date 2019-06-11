using System.Collections;
using UnityEngine;

public class LifeTime : MonoBehaviour {
    // LifeTime Variables
    public System.DateTime createdAt;
    private int lifespan = 10;

    void Start () {
        createdAt = System.DateTime.UtcNow;
        StartCoroutine("Tick");
    }
	
    public void SetLifespan(int span)
    {
        lifespan = span;
        //print("lifespan: " + span);
    }

    IEnumerator Tick()
    {
        for (; ; )
        {
            System.TimeSpan ts = System.DateTime.UtcNow - createdAt;
            if (lifespan < ts.Seconds)
            {
                //print("time to die");
                Destroy(gameObject);
            }
            //print(transform.name + "alive for: " + ts.Seconds);
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public int GetRemaining()
    {
        return (System.DateTime.UtcNow - createdAt).Seconds;
    }
}
