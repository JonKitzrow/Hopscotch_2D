using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public Transform balanceBar, barTarget, powerTarget, directionArrow, footprint, balanceTotalBar, instructions, winUI;
    public SpriteRenderer miss1, miss2, miss3;
    public float powerRange, balanceRange, sinSpeed, feedbackDelay;
    float sinAngle, power, balanceTotal, balanceHop, timeNext;
    int nextBox, missed;
    Vector3 direction, lastPos;
    string state = "init";
    string lastState = "";
    public Box[] boxNum;

    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = false;
        directionArrow.gameObject.SetActive(false);
        powerTarget.gameObject.SetActive(false);
        balanceBar.gameObject.SetActive(false);
        instructions.gameObject.SetActive(true);
        winUI.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
          // show instructions at startup and wait for input
          case "init":
            if (Input.anyKey || validClick())
            {
              state = "aim";
              instructions.gameObject.SetActive(false);
            }
            break;

          // get rotation for hop
          case "aim":
            directionArrow.position = footprint.position;
            directionArrow.gameObject.SetActive(true);
            directionArrow.eulerAngles = new Vector3(0, 0, Mathf.Atan2(Input.mousePosition.y - Screen.height / 2, Input.mousePosition.x - Screen.width / 2) * Mathf.Rad2Deg);
            if (validClick())
            {
              direction = directionArrow.eulerAngles;
              directionArrow.gameObject.SetActive(false);
              startSin(180f, 0.5f);
              state = "power";
            }
            break;

          // get distance for hop
          case "power":
            power = Mathf.Sin(sinAngle * Mathf.Deg2Rad) * powerRange / 2 + powerRange / 2;
            powerTarget.position = footprint.position + new Vector3(Mathf.Cos(direction.z * Mathf.Deg2Rad), Mathf.Sin(direction.z * Mathf.Deg2Rad), 0) * power;
            powerTarget.gameObject.SetActive(true);
            if (validClick())
            {
              state = "powerFeedback";
              startSin(0, 0.5f);
              timeNext = Time.time + feedbackDelay;
            }
            break;

            case "powerFeedback":
              if (Time.time >= timeNext)
              {
                powerTarget.gameObject.SetActive(false);
                state = "balance";
              }
              break;

          // get balance for hop
          case "balance":
            balanceHop = Mathf.Sin(sinAngle * Mathf.Deg2Rad) * balanceRange;
            barTarget.localPosition = Vector3.right * balanceHop;
            balanceBar.position = footprint.position + Vector3.up * 2;
            balanceBar.gameObject.SetActive(true);
            if (validClick())
            {
              state = "balanceFeedback";
              timeNext = Time.time + feedbackDelay;
            }
            break;

          case "balanceFeedback":
            if (Time.time >= timeNext)
            {
              balanceBar.gameObject.SetActive(false);
              state = "hop";
            }
            break;

          // use hop values to update game values
          case "hop":
            lastPos = footprint.position;
            footprint.position = powerTarget.position;
            footprint.localEulerAngles = Vector3.forward * (direction.magnitude - 90f);
            balanceTotal += balanceHop * 1.4f;
            state = "feedbackWait";
            timeNext = Time.time + 0.1f;
            break;

          case "feedbackWait":
            if (Time.time >= timeNext)
            {
              state = "feedback";
            }
            break;

          // display any user feedback before looping back to "aim"
          case "feedback":
            if (boxNum[nextBox].isInside())
            {
              state = "feedbackFeedback";
              if (nextBox == 9)
              {
                state = "win";
              }
              nextBox++;
              timeNext = Time.time + feedbackDelay;
            }
            else
            {
              state = "miss";
            }
            break;

          case "feedbackFeedback":
            if (Time.time >= timeNext)
            {
              state = "aim";
            }
            break;

          case "miss":
            missed += 1;
            if (missed >= 3)
            {
              state = "lose";
              startSin(-90f, 0.5f);
            }
            else
            {
              state = "missFeedback";
            }

            if (missed == 1)
            {
              miss1.transform.localScale = new Vector3(2f, 2f, 0);
            }
            else if (missed == 2)
            {
              miss2.transform.localScale = new Vector3(2f, 2f, 0);
            }
            else if (missed == 3)
            {
              miss3.transform.localScale = new Vector3(2f, 2f, 0);
            }

            timeNext = Time.time + feedbackDelay;
            break;

          case "missFeedback":
            if (Time.time >= timeNext)
            {
              footprint.position = lastPos;
              state = "aim";
            }
            break;

          case "lose":
            state = "loseDelay";
            timeNext = Time.time + 0.5f;
            break;

          case "loseDelay":
            if (Time.time >= timeNext)
            {
              SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            break;

          case "win":
            winUI.gameObject.SetActive(true);
            if (validClick())
            {
              SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            break;
        }

        // cursor visibility
        if (Input.GetKeyDown(KeyCode.Escape))
        {
          Cursor.visible = true;
        }

        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
          Cursor.visible = false;
        }

        // update sine angle for oscillating values
        sinAngle += sinSpeed * 360f * Time.deltaTime;
        if (sinAngle >= 360f)
        {
          sinAngle -= 360f;
        }

        // update UI indicators
        balanceTotalBar.localPosition = Vector3.down * balanceTotal * 1.1f / balanceRange;

        if (missed >= 1)
        {
          miss1.color = new Color(1f, 1f, 1f, 1f);
        }
        if (missed >= 2)
        {
          miss2.color = new Color(1f, 1f, 1f, 1f);
        }
        if (missed >= 3)
        {
          miss3.color = new Color(1f, 1f, 1f, 1f);
        }

        if (miss1.transform.localScale.x > 1.5f)
        {
          miss1.transform.localScale -= new Vector3(1f, 1f, 0) * Time.deltaTime * 2;
        }
        else if (miss1.transform.localScale.x <= 1.5f)
        {
          miss1.transform.localScale = new Vector3(1.5f, 1.5f, 0);
        }

        if (miss2.transform.localScale.x > 1.5f)
        {
          miss2.transform.localScale -= new Vector3(1f, 1f, 0) * Time.deltaTime * 2;
        }
        else if (miss2.transform.localScale.x <= 1.5f)
        {
          miss2.transform.localScale = new Vector3(1.5f, 1.5f, 0);
        }

        if (miss3.transform.localScale.x > 1.5f)
        {
          miss3.transform.localScale -= new Vector3(1f, 1f, 0) * Time.deltaTime * 2;
        }
        else if (miss3.transform.localScale.x <= 1.5f)
        {
          miss3.transform.localScale = new Vector3(1.5f, 1.5f, 0);
        }

        if (Mathf.Abs(balanceTotal) > 4.3f)
        {
          state = "lose";
        }

        if (!string.Equals(state, lastState))
        {
          lastState = state;
          Debug.Log(state);
        }
    }

    public void startSin(float angle, float speed)
    {
      sinAngle = angle;
      sinSpeed = speed;
    }

    public bool validClick()
    {
      return Input.GetMouseButtonDown(0) && !Cursor.visible;
    }
}
