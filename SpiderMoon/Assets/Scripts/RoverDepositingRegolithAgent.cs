using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoverDepositingRegolithAgent : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform dome;

    public int regoltihAmmo = 0;

    public enum Action { GetRegolith, MoveToDome, LookingForNextPoint, Building };
    public Action action;

    public float speed = 3.5f;

    public Vector3 destination;

    public float maxDistanceFromDome = 35;

    public float minDistanceBetweenTwoPoint = 10.0f;
    public float maxDistanceBetweenTwoPoint = 20.0f;

    public float domeRadius = 15.0f;


    // Building with Pooler
    public float buildingDelay = 1;
    public float lastBuildingTime = 0;
    ObjectPooler objectPooler;




    // Start is called before the first frame update
    void Start()
    {
        objectPooler = ObjectPooler.Instance;

        action = Action.GetRegolith;

        agent.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (action == Action.GetRegolith) goGetRegolith();

        if (action == Action.MoveToDome) goToDome();

        if (action == Action.LookingForNextPoint) SetNextBuildingPoint();

        if (action == Action.Building) Build();





    }

    void goToDome()
    {
        destination = dome.position;
        agent.SetDestination(destination);

        if (Vector3.Distance(destination, transform.position) < domeRadius)
        {
            transform.LookAt(-transform.forward + transform.position);
            action = Action.LookingForNextPoint;

        }
    }


    void goGetRegolith()
    {
        Vector3 destination = getNearestRegolithPosition();
        agent.SetDestination(destination);

        if(Vector3.Distance(destination, transform.position) < 2f)
        {
            regoltihAmmo = 100;
            action = Action.MoveToDome;
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


    void SetNextBuildingPoint()
    {
        float angle = Random.Range(90, -90);
        float d = Random.Range(minDistanceBetweenTwoPoint,maxDistanceBetweenTwoPoint);

        Vector3 vector = Quaternion.Euler(0, angle, 0) * transform.forward;
        Vector3 nexBuildingPoint = vector * d;

        nexBuildingPoint += transform.position;

        destination = nexBuildingPoint;

        Debug.Log(destination);

        action = Action.Building;
    }


    void Build()
    {

        agent.SetDestination(destination);

        if (Time.time - lastBuildingTime > buildingDelay/speed)
        {
            lastBuildingTime = Time.time;
            objectPooler.SpawnFromPool("Cube", transform.position, Quaternion.identity);
            
            
            regoltihAmmo--;
            if(regoltihAmmo <= 0)
            {
                action = Action.GetRegolith;
            }
        }

        if (Vector3.Distance(destination, transform.position) < 2f)
        {
            if(Vector3.Distance(transform.position, dome.position) > maxDistanceFromDome)
            {

                action = Action.MoveToDome;
            }
            else
            {

                action = Action.LookingForNextPoint;
            }
        }
    }

}
