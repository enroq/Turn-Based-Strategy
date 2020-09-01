using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingHandler 
{
    public static int ParseVectorDistance(Vector2 delta, Vector2 gamma)
    {
        float xDistance = Mathf.Abs(delta.x - gamma.x);
        float yDistance = Mathf.Abs(delta.y - gamma.y);

        int i = (int)(xDistance + yDistance);

        return i;
    }

    public static GameBoardComponent DetermineNextNode
        (GameBoardComponent current, Vector2 destination)
    {   
        int currentLowestDistance = Int32.MaxValue;
        int currentLowestHeuristic = Int32.MaxValue;

        GameBoardComponent closestComponent = null;
        Vector2 origin = current.Vector;
       
        GameBoardComponent[] components = current.GetNeighbors();

        for (int i = 0; i < components.Length; i++)
        {
            if (!components[i].IsPassable) 
                break;

            int h = ParseVectorDistance(components[i].Vector, destination);
            int g = ParseVectorDistance(components[i].Vector, origin);
            int d = h + g;

            if (d < currentLowestDistance)
            {
                closestComponent = components[i];
                currentLowestDistance = d;
                currentLowestHeuristic = h;
            }

            else if (d == currentLowestDistance)
            {
                if (h < currentLowestHeuristic)
                {
                    closestComponent = components[i];
                    currentLowestDistance = d;
                    currentLowestHeuristic = h;
                }

                else if (h == currentLowestHeuristic)
                {
                    if (Utility.RandomBoolean())
                    {
                        closestComponent = components[i];
                        currentLowestDistance = d;
                        currentLowestHeuristic = h;
                    }
                }
            }
        }

        if (closestComponent != null) return closestComponent;
        else 
            return null;
    }
}
