using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoverDepositingRegolithAgent : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform dome;

    public bool walkPointSet = true;
    public bool hasregolith = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasregolith) goGetRegolith();
        if (hasregolith) goDeposeRegolith();

    }

    void goDeposeRegolith()
    {
        
        agent.SetDestination(dome.position);

        if (Vector3.Distance(dome.position, transform.position) < 5f)
        {
            hasregolith = false;
        }
    }


    void goGetRegolith()
    {
        Vector3 regolithRefillPosition = getNearestRegolithPosition();
        agent.SetDestination(regolithRefillPosition);

        if(Vector3.Distance(regolithRefillPosition, transform.position) < 5f)
        {
            hasregolith = true;
        }
    }


    Vector3 getNearestRegolithPosition()
    {
        RegolithRefill[] allRegolithRefill = FindObjectsOfType<RegolithRefill>();

        Vector3 nearestRegolithPosition = new Vector3(0,0,0);

        float minimumDistance = Mathf.Infinity;

        foreach (RegolithRefill regolithRefill in allRegolithRefill)
        {
            float distance = Vector3.Distance(transform.position, regolithRefill.transform.position);
            if (distance < minimumDistance)
            {
                minimumDistance = distance;
                nearestRegolithPosition = regolithRefill.transform.position;
            }
        }

        return nearestRegolithPosition;
    }
}
