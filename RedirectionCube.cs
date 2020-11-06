using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RedirectionCube : MonoBehaviour
{
    // Exposed Variables

    [SerializeField] private Transform startPoint = null, endPoint = null;

    [SerializeField] private LayerMask laserMask = 0;

    [SerializeField] private bool isActive = false;

    [SerializeField] private Color laserColor = Color.black;
    public Color GetLaserColor()
    {
        return laserColor;
    }
    [SerializeField] private List<LaserEmitter> laserParents = null;

    [SerializeField] private LineRenderer laserHelper = null; //Helps with displaying where the laser will go.

    // Hidden Variables

    private LineRenderer lr = null; //main line renderer
    private GameObject previousObject, currentObject;
    public bool HasLaserParent()
    {
        return true;
    }

    //
    // Mono Fuctions
    //

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        if(lr != null)
        {
            lr.positionCount = 2;
        }

        if(laserHelper != null)
        {
            laserHelper.positionCount = 2;
        }

        DisableBox();
    }

    private void Update()
    {
        UpdateLaserHelper();

        if (isActive)
        {
            UpdateLaserState();
        }
    }

    //
    // Our Functions
    //

    public void AddLaserToBox(LaserEmitter parentLaser)
    {
        //In order to prevent a possible infinite loop, check if the parentLaser is already in our list

        //So appereantly this does not work like I thought as much.
        foreach(LaserEmitter parent in laserParents)
        {
            if(parentLaser == parent)
            {
                Debug.Log("Found matching laser parent for " + gameObject.name);
                return;
            }
        }

        Debug.Log("No matching laser parent found for " + gameObject.name);

        //Add the laser to the parent list
        laserParents.Add(parentLaser);

        CheckBoxState();
    }

    public void RemoveLaserFromTheBox(LaserEmitter parentLaser)
    {
        for(int i = 0; i < laserParents.Count; i++)
        {
            if(laserParents[i] == parentLaser)
            {
                laserParents.RemoveAt(i);
                CheckBoxState();
                return;
            }
        }
    }

    private void CheckBoxState()
    {
        if(laserParents.Count > 0)
        {
            EnableBox();
        }

        else
        {
            DisableBox();
        }
    }

    private void EnableBox()
    {
        if (isActive)
        {
            //Update the Color Here;
            UpdateLaserColor();
        }

        else
        {
            //For now let's just grab the first laser color.
            lr.enabled = true;
            UpdateLaserColor();
            isActive = true;
        }
    }

    private void DisableBox()
    {
        lr.enabled = false;
        isActive = false;
        //laserParent = null;
    }

    private void UpdateLaserColor()
    {
        float red = 0;
        float blue = 0;
        float green = 0;
        int count = 0;
        Color newColor;

        //Get the total amount of each emitter's colors.
        foreach (LaserEmitter parent in laserParents)
        {
            red = red + parent.GetColor().r;
            blue = blue + parent.GetColor().b;
            green = green + parent.GetColor().g;
            count++;
        }

        if(count > 0)
        {
            red = red / count;
            blue = blue / count;
            green = green / count;

            newColor = new Color(red, green, blue, 1.0f);
        }

        else
        {
            newColor = Color.black; //Default value.
        }

        laserColor = newColor;
        lr.startColor = laserColor;
        lr.endColor = laserColor;
    }

    private void UpdateLaserState()
    {
        lr.SetPosition(0, startPoint.position);

        previousObject = currentObject;
        currentObject = null;

        RaycastHit hit;
        if (Physics.Linecast(startPoint.position, endPoint.position, out hit, laserMask))
        {
            currentObject = hit.collider.gameObject;
            lr.SetPosition(1, hit.point);
            laserHelper.SetPosition(1, hit.point);

            if (previousObject != currentObject)
            {
                Debug.Log("Calling a new chain reaction because of " + gameObject.name);
                if(laserParents.Count > 0)
                {
                    List<LaserEmitter> les = new List<LaserEmitter>();

                    foreach (LaserEmitter parent in laserParents)
                    {
                        les.Add(parent);
                    }

                    foreach(LaserEmitter parent in les)
                    {
                        parent.ResetChain();
                    }
                }
            }
        }

        else
        {
            lr.SetPosition(1, endPoint.position);
        }
    }

    private void UpdateLaserHelper()
    {
        laserHelper.SetPosition(0, startPoint.position);
        RaycastHit hit;
        if (Physics.Linecast(startPoint.position, endPoint.position, out hit, laserMask))
        {
            laserHelper.SetPosition(1, hit.point);
        }

        else
        {
            laserHelper.SetPosition(1, endPoint.position);
        }

    }

    public GameObject TriggerLineCast()
    {
        GameObject gb = null;
        RaycastHit hit;
        if(Physics.Linecast(startPoint.position, endPoint.position, out hit, laserMask))
        {
            gb = hit.collider.gameObject;
        }

        return gb;
    }
}
