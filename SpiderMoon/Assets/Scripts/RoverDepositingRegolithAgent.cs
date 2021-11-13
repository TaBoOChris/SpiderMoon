using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoverDepositingRegolithAgent : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform dome;

    public int step = 0;

    public float angle = 10;

    public Vector3 beginingPoint;
    public Vector3 destination;
    public float DistanceBetweenBeginingAndDome;

    // Start is called before the first frame update
    void Start()
    {
        DistanceBetweenBeginingAndDome = Vector3.Distance(beginingPoint, dome.position);

    }

    // Update is called once per frame
    void Update()
    {
        if (step == 0) goGetRegolith();
        if (step == 1) goToBeginingPoint();
        if (step == 2) goDeposeRegolith();
        if (step == 3) goToBeginingPoint();
        if (step == 4) CalculateNewBeginPoint();
        if (step > 4)
        {
            step = 0;
        }

    }

    void goDeposeRegolith()
    {
        destination = dome.position;
        agent.SetDestination(destination);

        if (Vector3.Distance(destination, transform.position) < 2f)
        {
            step++;            
        }
    }


    void goGetRegolith()
    {
        Vector3 destination = getNearestRegolithPosition();
        agent.SetDestination(destination);

        if(Vector3.Distance(destination, transform.position) < 2f)
        {
            step++;
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


    void goToBeginingPoint()
    {
        destination = beginingPoint;
        agent.SetDestination(destination);

        if (Vector3.Distance(destination, transform.position) < 2f)
        {
            step++;
        }

    }

    void CalculateNewBeginPoint()
    {
        Vector3 newVector = new Vector3(0, 2, 0);
        newVector.x = beginingPoint.x * Mathf.Cos(Mathf.Deg2Rad * angle) - beginingPoint.z * Mathf.Sin(Mathf.Deg2Rad * angle);
        newVector.z = beginingPoint.z * Mathf.Cos(Mathf.Deg2Rad * angle) + beginingPoint.x * Mathf.Sin(Mathf.Deg2Rad * angle);
        beginingPoint = newVector;
        step++;
    }
}
