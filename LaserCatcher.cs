using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserCatcher : MonoBehaviour
{
    //Exposed Variables

    [SerializeField] private Color targetColor = Color.white; //The target color required for the object to activate
    [SerializeField] private List<LaserEmitter> recievedLasers;
    [SerializeField] private List<GameObject> recievedObjects;
    [SerializeField] private Color currentColor = Color.black; //The current value of the laser.

    [SerializeField] private Light indicatorLight = null; //The indicator light that we'll toggle on and off.
    [SerializeField] private Light targetLight = null; //The target light that we'll switch the color to for the laser.

    [Header("Events")]
    [SerializeField] private UnityEvent OnActivate;
    [SerializeField] private UnityEvent OnDeactivate;

    //Hidden Variables
    private bool isActive = false; //Is the object currently operating?

    // Unity Methods
    private void Start()
    {
        //Match the color of the target light to our target Color
        targetLight.color = targetColor;
    }

    // Custom Methods

    public void AddLaserToCatcher(LaserEmitter parentLaser)
    {
        foreach (LaserEmitter parent in recievedLasers)
        {
            if (parentLaser == parent)
            {
                Debug.Log("Found matching laser parent for " + gameObject.name);
                return;
            }
        }

        Debug.Log("No matching laser parent found for " + gameObject.name);

        //Add the laser to the parent list
        recievedLasers.Add(parentLaser);

        EvaluateCatcher();
    }

    public void RemoveLaserFromCatcher(LaserEmitter parentLaser)
    {
        for (int i = 0; i < recievedLasers.Count; i++)
        {
            if (recievedLasers[i] == parentLaser)
            {
                recievedLasers.RemoveAt(i);
                EvaluateCatcher();
                return;
            }
        }
    }

    private void EvaluateCatcher()
    {
        //We can grab the information we need from the parent lasers.
        //What we need is to know each laser's last known cube. The cube should usually be the second LAST object of each laser emitter.
        //So if we create a list of those cubes, and compare them to each other, ensuring that they match, we can evaluate that the laser comes from a single cube. Allowing us
        //to mix the colors.

        //Clear the recievedObjects
        recievedObjects.Clear();

        foreach (LaserEmitter parent in recievedLasers)
        {
            GameObject cube = parent.GetLastCubeInChain();
            if (cube != null)
            {
                recievedObjects.Add(cube);
            }
        }

        bool isMatching = EvaluateCubes();

        if (isMatching)
        {
            Color newColor = Color.black;

            RedirectionCube cube = recievedObjects[0].GetComponent<RedirectionCube>();
            if(cube != null)
            {
                Debug.Log("Retrieving the last cube's laser color, cube is " + cube.gameObject.name);
                newColor = cube.GetLaserColor();

                Debug.Log("= CUBE COLOR =");
                Debug.Log("R: " + newColor.r);
                Debug.Log("B: " + newColor.b);
                Debug.Log("G: " + newColor.g);
                Debug.Log("===");

                Debug.Log("= TARGET COLOR =");
                Debug.Log("R: " + targetColor.r);
                Debug.Log("B: " + targetColor.b);
                Debug.Log("G: " + targetColor.g);
                Debug.Log("===");

                if (newColor == targetColor)
                {
                    ActivateCatcher();
                }
                
                else
                {
                    DeactivateCatcher();
                }
            }
        }

        else
        {
            DeactivateCatcher();
        }
    }

    private bool EvaluateCubes()
    {
        //Evaluate if the cubes in the array are the same.
        if(recievedObjects.Count == 0)
        {
            //If there's none, automatically return false
            return false;
        }

        else if(recievedObjects.Count == 1)
        {
            //If there is one, automatically return truel
            return true;
        }

        else
        {
            bool isMatching = true;
            GameObject lastObject = recievedObjects[0];
            for(int i = 1; i < recievedObjects.Count; i++)
            {
                if(lastObject != recievedObjects[i])
                {
                    isMatching = false;
                }

                lastObject = recievedObjects[i];
            }

            return isMatching;
        }
    }

    private Color EvaluateColor()
    {
        float red = 0;
        float blue = 0;
        float green = 0;
        int count = 0;
        Color newColor;

        //Get the total amount of each emitter's colors.
        foreach (LaserEmitter parent in recievedLasers)
        {
            red = red + parent.GetColor().r;
            blue = blue + parent.GetColor().b;
            green = green + parent.GetColor().g;
            count++;
        }

        if (count > 0)
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

        currentColor = newColor;
        return newColor;
    }

    private void ActivateCatcher()
    {
        if (!isActive)
        {
            isActive = true;
            OnActivate.Invoke();
            if (indicatorLight != null)
            {
                indicatorLight.enabled = true;
            }
        }
    }

    private void DeactivateCatcher()
    {
        if (isActive)
        {
            isActive = false;
            OnDeactivate.Invoke();
            if (indicatorLight != null)
            {
                indicatorLight.enabled = false;
            }
        }
    }
}
