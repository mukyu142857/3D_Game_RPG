using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    public NavMeshAgent enemyAgent;
    public enum EnemyState
    {
        NormalState,
        FightingState,
        MovingState,
        RestingState
    }
    private EnemyState state = EnemyState.NormalState;
    private EnemyState childState = EnemyState.RestingState;
    private float restTime = 2f;
    private float restTimer = 0f;
    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == EnemyState.NormalState)
        {
            if(childState == EnemyState.RestingState)
            {
                restTimer += Time.deltaTime;
                if (restTimer>restTime)
                {
                    Vector3 randomPosition = FindRandomposition();
                    enemyAgent.SetDestination(randomPosition);
                    childState = EnemyState.MovingState;
                }         
            }
            else if(childState == EnemyState.MovingState)
            {
                if (enemyAgent.remainingDistance <= 0)
                {
                    restTimer = 0;
                    childState = EnemyState.RestingState;
                }
            }
        }
        else
        {

        }
    }
    Vector3 FindRandomposition()
    {
        Vector3 randomDir = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
        return transform.position + randomDir.normalized * Random.Range(2, 5);
    }
}
