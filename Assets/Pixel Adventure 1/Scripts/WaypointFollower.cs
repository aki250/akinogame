using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;    //在Inspector存储路径点数组
    private int currentWaypointIndex = 0;   

    [SerializeField] private float speed = 2f;
    [SerializeField] private AudioSource collectSoundeffect; 

    void Update()
    {
        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < .1f)
        {
            currentWaypointIndex ++;
            if(currentWaypointIndex >= waypoints.Length)    //到达后返回
            {
                currentWaypointIndex = 0;       
            }
        }
        
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, Time.deltaTime * speed);
    }
}
