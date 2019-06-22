using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    bool inside = false;

    void OnTriggerStay2D(Collider2D other)
    {
      if (other.tag == "Player")
      {
        inside = true;
      }
    }

    void OnTriggerExit2D(Collider2D other)
    {
      if (other.tag == "Player")
      {
        inside = false;
      }
    }

    public bool isInside()
    {
      return inside;
    }
}
