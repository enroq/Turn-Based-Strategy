using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Utility
{
    static System.Random m_Random = new System.Random();
    public static bool RandomBoolean()
    {
        return m_Random.Next(0, 2) == 0;
    }

    static int[] m_RightAngles = new int[] { 0, 90, 180, 270, 360, -90 };
    public static Vector3 GetRotationToNearestRightAngle(Vector3 rot)
    {
        Vector3 deltaRot = rot;

        int currentClosetAngle = 0, currentClosetDistance = 360;

        for(int i = 0; i < m_RightAngles.Length; i++)
        {
            int tempDistance = (int)(Math.Abs(rot.y - m_RightAngles[i]));

            if (tempDistance < currentClosetDistance)
            {
                currentClosetAngle = m_RightAngles[i];
                currentClosetDistance = tempDistance;
            }
        }

        deltaRot.y = currentClosetAngle; return deltaRot;
    }
}
