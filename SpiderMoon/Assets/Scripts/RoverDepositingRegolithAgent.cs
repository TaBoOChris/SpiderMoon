using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoverDepositingRegolithAgent : MonoBehaviour
{
    public NavMeshAgent agent;      // gère les deplacement de l'IA
        
    public Transform dome;          // position du Dome principal

    public int regoltihAmmo = 400;      // Munition de Regolith, si tombe à zéro faut aller en chercher

    public enum Action { GetRegolith, MoveToDome, LookingForNextPoint, Building };      // Ce que l'araignee est cense faire
    public Action action;

    public float speed = 3.5f;      // vitesse de l'araignee

    public Vector3 destination;     // Sa destination, la ou elle va

    public float maxDistanceFromDome = 35;      // La distance max pour construire, au dela, elle s'arrete

    public float minDistanceBetweenTwoPoint = 10.0f;    // Distance min entre deux points(foyer) a relier
    public float maxDistanceBetweenTwoPoint = 20.0f;    // Distance max entre deux points(foyer) a relier

    public float domeRadius = 15.0f;    // la taille du dome


    // Building with Pooler --- Affichage des cube de construction
    public float buildingDelay = 1; 
    public float lastBuildingTime = 0;
    ObjectPooler objectPooler;


    bool movingToFoyer = false; // POur savoir si l'on va vers ou foyer ou non

    // Start is called before the first frame update
    void Start()
    {
        objectPooler = ObjectPooler.Instance;   // On recupere notre piscine de cube pour les faire spawner plus tard

        action = Action.MoveToDome;     // première action

        agent.speed = speed;            // on set la vitesse
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
        destination = dome.position;                // On defini la destination comme la position du dome
        agent.SetDestination(destination);          // On y va

        if (Vector3.Distance(destination, transform.position) < domeRadius)     // SI on est assez proche
        {
            transform.LookAt(-transform.forward + transform.position);      // On retourne la spider
            action = Action.LookingForNextPoint;                            // On cherche notre prochaine destination

        }
    }


    void goGetRegolith()
    {
        Vector3 destination = getNearestRegolithPosition();     // On recupère la regolith la plus proche
        agent.SetDestination(destination);                      // On y va

        if(Vector3.Distance(destination, transform.position) < 2f)  // Si on est asser proche
        {
            regoltihAmmo = 100;             // On recharche la spider
            action = Action.MoveToDome;     // On repars au dome
        }
    }

    // Trouver la regolith la plus proche
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

    // Lorsque on a fini de se deplacer et qu'on cherche le prochain point
    void SetNextBuildingPoint()
    {
        // Si on se dirigeait vers un foyer, pas besoin d'en poser un nouveau
        if(!movingToFoyer)
            objectPooler.SpawnFromPool("Foyer", transform.position, Quaternion.identity);


        // On essayer d'abord de trouver des foyer proche
        GameObject[] foyers = GameObject.FindGameObjectsWithTag("Foyer");   // On les recupere tous
        List<GameObject> potentialNextFoyers = new List<GameObject>();      // on prepare une liste pour notre premier filtrage
        foreach (GameObject foyer in foyers)
        {
            // On filtre par distance, il faut qu'il soit plus eloigne du dome que nous, pas trop porche de nous, mais pas trop loin
            float distanceFoyerRover =  Vector3.Distance(foyer.transform.position, transform.position); 
            float distanceFoyerDome  =  Vector3.Distance(foyer.transform.position, dome.position);
            float distancePlayerDome =  Vector3.Distance(dome.position, transform.position);
            if (distanceFoyerRover < 20 && distanceFoyerRover > 5 && distanceFoyerDome - 5  > distancePlayerDome  )
            {
                potentialNextFoyers.Add(foyer); 
            }
        }
            
        int size = potentialNextFoyers.Count;       // nombre de foyer garde
        bool chance = (int)Random.Range(0, 5) < 3 ;     // Proba de finalement ne pas aller vers un foyer

        if (size > 0  && chance)
        {
            destination = potentialNextFoyers[(int)Random.Range(0, size)].transform.position;       // On va vers le foyer (selectionne random)

            movingToFoyer = true;
            action = Action.Building;
            return;
        }


        // no foyer, let's calculate next point
        movingToFoyer = false;

        float angle = Random.Range(90, -90);    // angle devant nous ( pas de marche arriere)
        float d = Random.Range(minDistanceBetweenTwoPoint,maxDistanceBetweenTwoPoint);  // Distance a parcourir

        Vector3 vector = Quaternion.Euler(0, angle, 0) * transform.forward;  // Calcule de notre vecteur direction
        Vector3 nexBuildingPoint = vector * d;

        nexBuildingPoint += transform.position;

        destination = nexBuildingPoint;     // On set la destination


        action = Action.Building;       // On construit
    }


    void Build()
    {

        agent.SetDestination(destination);  // ON va a notre destination

        // On pose des cube de contruction 
        if (Time.time - lastBuildingTime > buildingDelay/speed)
        {
            lastBuildingTime = Time.time;

            objectPooler.SpawnFromPool("Cube", transform.position, Quaternion.identity);
            
            
            regoltihAmmo--;
            if(regoltihAmmo <= 0)   // SI plus de munition
            {
                action = Action.GetRegolith;    // On va en chercher
            }
        }

        // si il est arrivé à destination
        if (Vector3.Distance(destination, transform.position) < 1f)
        {   
            // Si il est trop loin du dome
            if(Vector3.Distance(transform.position, dome.position) > maxDistanceFromDome)   // la spider est trop loin
            {   
                if (regoltihAmmo < 30)              // Il verifie s'il a besoin de munition
                    action = Action.GetRegolith;
                else
                    action = Action.MoveToDome;
                
            }
            else
            {
                // Il part chercher un nouveau point de destination
                action = Action.LookingForNextPoint;
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        // S'il est en collision avec une autre araignee, il l'a pousse
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
        
        // s'il fonce sur le dome et va le percuter pendant la construction, sil s'arrete de repars de zéro
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
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(transform.position, GetComponent<SphereCollider>().radius);
    }

}
