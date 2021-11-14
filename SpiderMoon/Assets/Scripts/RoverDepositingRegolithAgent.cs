using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoverDepositingRegolithAgent : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform dome;

    public int regoltihAmmo = 400;

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


    bool movingToFoyer = false;

    // Start is called before the first frame update
    void Start()
    {
        objectPooler = ObjectPooler.Instance;

        action = Action.MoveToDome;

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
        if(!movingToFoyer)
            objectPooler.SpawnFromPool("Foyer", transform.position, Quaternion.identity);


        // try to find a foyer cloth 
        GameObject[] foyers = GameObject.FindGameObjectsWithTag("Foyer");
        Debug.Log("Il y a " + foyers.Length + "Foyer");
        List<GameObject> potentialNextFoyers = new List<GameObject>();
        foreach (GameObject foyer in foyers)
        {
            float distanceFoyerRover =  Vector3.Distance(foyer.transform.position, transform.position);
            float distanceFoyerDome  =  Vector3.Distance(foyer.transform.position, dome.position);
            float distancePlayerDome =  Vector3.Distance(dome.position, transform.position);
            if (distanceFoyerRover < 20 && distanceFoyerRover > 5 && distanceFoyerDome - 5  > distancePlayerDome  )
            {
                potentialNextFoyers.Add(foyer); 
            }
        }

        int size = potentialNextFoyers.Count;
        bool chance = (int)Random.Range(0, 5) < 3 ;

        if (size > 0  && chance)
        {
            destination = potentialNextFoyers[(int)Random.Range(0, size)].transform.position;

            movingToFoyer = true;
            action = Action.Building;
            return;
        }


        // no foyer, let's calculate next point
        movingToFoyer = false;

        float angle = Random.Range(90, -90);
        float d = Random.Range(minDistanceBetweenTwoPoint,maxDistanceBetweenTwoPoint);

        Vector3 vector = Quaternion.Euler(0, angle, 0) * transform.forward;
        Vector3 nexBuildingPoint = vector * d;

        nexBuildingPoint += transform.position;

        destination = nexBuildingPoint;


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

        // si il est arrivé à destination
        if (Vector3.Distance(destination, transform.position) < 1f)
        {
            if(Vector3.Distance(transform.position, dome.position) > maxDistanceFromDome)   // la spider est trop loin
            {
                if (regoltihAmmo < 30)
                    action = Action.GetRegolith;
                else
                    action = Action.MoveToDome;
                
            }
            else
            {

                action = Action.LookingForNextPoint;
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Rover"))
        {
            Debug.Log("Move Rover");
            Transform otherEntity = other.transform;
            Transform thisEntity = transform;
            //transform.position = (thisEntity.position - otherEntity.position).normalized * 16.0f + otherEntity.position;
            otherEntity.GetComponent<Rigidbody>().AddForce((otherEntity.position - thisEntity.position).normalized *5f);
        }

    }

    

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);

        if (other.CompareTag("Dome") && action == Action.Building)
        {
            Vector3 vector = dome.position - transform.position;
            if (Vector3.Angle(vector, transform.forward) < 90)
            {
                Debug.Log("BackToDome");
                action = Action.MoveToDome;

            }
        }


        
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, GetComponent<SphereCollider>().radius);
    }

}
