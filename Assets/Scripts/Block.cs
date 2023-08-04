using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector2 coords;

    public void move(Vector3 posShift, Vector2 coordShift)
    {
        transform.position += posShift;
        coords += coordShift;
    }

}
