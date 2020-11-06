using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEmitter : MonoBehaviour
{
    // Exposed Variables
    [SerializeField] private Transform startPoint = null; //These refer to two gameObjects that contain the "start" and the "end" of the laser. It's a more efficient way than calculating
    [SerializeField] private Transform endPoint = null; //it by hand with script...

    [SerializeField] private LayerMask laserMask = 0; //What layers adjust the laser.

    [SerializeField] private List<GameObject> laserChain = null; //The chain of objects through which the laser goes through.
    public GameObject GetLastCubeInChain()
    {
        for(int i = laserChain.Count - 1; i > -1; i--)
        {
            if(laserChain[i].GetComponent<RedirectionCube>() != null)
            {
                return laserChain[i];
            }
        }

        return null;
    }

    [SerializeField] private Color laserColor = Color.white;
    public Color GetColor()
    {
        return laserColor;
    }

    // Hidden Variables
    private LineRenderer lr; //The renderer that will display the laser.

    private GameObject previousObject, currentObject; //The last known hit object and the object that we currently hit, this is used in the Update.

    //
    // Mono Fuctions
    //
    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null)
        {
            Debug.Log("Laser Emitter failed to grab the LineRenderer!");
        }
        EnableLaser();
    }

    private void Update()
    {
        UpdateLaserChain();
    }

    //
    // Our Functions
    //

    // Initializes the laser.
    public void EnableLaser()
    {
        //start position should always be the actual position of the emitter

        if (lr != null)
        {
            lr.positionCount = 2; //Set the start, and end of the laser.
            if(startPoint != null)
            {
                lr.SetPosition(0, startPoint.position);
            }

            lr.startColor = laserColor;
            lr.endColor = laserColor;
            
        }
    }

    //Updates the Laser Chain
    private void UpdateLaserChain()
    {
        if(endPoint == null || startPoint == null)
        {
            return;
        }

        previousObject = currentObject;
        currentObject = null;

        RaycastHit hit;
        if(Physics.Linecast(startPoint.position, endPoint.position, out hit, laserMask))
        {
            currentObject = hit.collider.gameObject;
            lr.SetPosition(1, hit.point);
        }

        else
        {
            currentObject = null;
            lr.SetPosition(1, endPoint.position);
        }

        if(previousObject != currentObject)
        {
            Debug.Log("Re-evaluating the chain reaction");
            ResetChain();
        }
    }

    //Extends the Chain Reaction of this particular laser
    private void ExtendChainReaction(GameObject obj)
    {
        //the obj represents the object that we hit with the laser.

        RedirectionCube rc = obj.GetComponent<RedirectionCube>();
        if(rc != null)
        {
            //We can only pass through a cube ONCE, otherwise we risk triggering an infinite Chain Reaction.
            foreach(GameObject exObj in laserChain)
            {
                if(obj == exObj)
                {
                    //Chain reaction ends with this as we are in.
                    return;
                }
            }

            Debug.Log("Reflecting " + obj.name);
            laserChain.Add(obj);
            rc.AddLaserToBox(this);
            obj = rc.TriggerLineCast();
            if (obj != null)
            {
                //StartCoroutine(DelayChainReaction(obj));
                ExtendChainReaction(obj);
            }
        }

        
        LaserCatcher lc = obj.GetComponent<LaserCatcher>();  
        if (lc != null)
        {
            //If we are hitting the laser catcher, then it's safe to assume that our chain ends there, therefore we can grab the color of the last known box through here.
            laserChain.Add(obj);
            lc.AddLaserToCatcher(this);
            return;
        }
    }

    //WE DON'T USE THIS - But if we get stuck in an infinite loop, this helps in dealing with it.
    private IEnumerator DelayChainReaction(GameObject obj)
    {
        Debug.Log("YIELD!");

        yield return new WaitForSeconds(1.0f);

        Debug.Log("CONTINUE!");

        ExtendChainReaction(obj);
    }

    //Resets the Chain Reaction
    public void ResetChain()
    {
        for(int i = 0; i < laserChain.Count; i++)
        {
            RedirectionCube rc = laserChain[i].GetComponent<RedirectionCube>();
            if(rc != null)
            {
                rc.RemoveLaserFromTheBox(this);
            }

            LaserCatcher lc = laserChain[i].GetComponent<LaserCatcher>();
            if(lc != null)
            {
                lc.RemoveLaserFromCatcher(this);
            }
        }

        laserChain.Clear();

        RaycastHit hit;
        if (Physics.Linecast(startPoint.position, endPoint.position, out hit, laserMask))
        {
            ExtendChainReaction(hit.collider.gameObject);
        }
    }
}
