using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpTest : MonoBehaviour
{
    Vector3 startPos;
    Vector3 _endPos;
    float _delay;
    float startTime;
    float initTime;
    float duration;
    bool _moving;
    public Vector3 test;  // used for testing end position changes
    //public bool alive = false;

	// Use this for initialization
	void Awake ()
    {
        ResetPosition();     
        delay = 0.5f;
        duration = 1;
        moving = false;
        initTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Need to turn on when NPC is moving a piece and off when user is moving
        if(GameLogic.S.NPCTurn())
        {
            Lerp();
        }
            	
	}

    void Lerp()
    {
        if(transform.position != endPos)
        {
            if(!moving)
            {
                if ((Time.time - initTime) >= delay)
                {
                    startTime = Time.time;
                    moving = true;
                }
            }        
            else
            {
                float u = (Time.time - startTime) / duration;
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                    startPos = endPos;
                }
                transform.position = (1 - u) * startPos + u * endPos;
            }
        }       
    }

    public void ResetPosition()
    {
        startPos = transform.position;
        endPos = transform.position;
    }

    public void CheckPositions()
    {
        //print(transform.position + "  " + endPos);
    }

    public bool Moving()
    {
        if(transform.position != endPos) { return true; }
        return false;
    }

    // When an end position is selected, the lerp timer restarts
    public Vector3 endPos
    {
        set
        {        
            _endPos = value;           
            initTime = Time.time;
        }
        get { return _endPos; }
    }

    public bool moving
    {
        set { _moving = value; }
        get { return _moving; }
    }

    public float delay
    {
        set { _delay = value; }
        get { return _delay; }
    }
}
