using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LerpType
{
    linear,
    sinEasing,
    sin
}

/// <summary>
/// Any Gameobject can move from point A to point B on its own.  This helps to make that movement smooth.
/// This is attached to that gameobject and anytime the startPos and endPos of that gameobject are not equal,
/// then that gameobject will lerp from startPos to endPos.
/// </summary>
public class LerpTest : MonoBehaviour
{
    Vector3 startPos;
    Vector3 _endPos;
    float _delay;
    float startTime;
    float initTime;
    float duration;
    bool _moving;
    [HideInInspector]
    public Vector3 test;  // used for testing end position changes
    //public bool alive = false;
    [HideInInspector]
    public LerpType ltype = LerpType.linear;
    

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
        //if(GameLogic.S.NPCTurn()) // I don't like having his here since I would want to attach this to any gameobject not necessarily related to whose turn it is.
        //{
            Lerp();
        //}
            	
	}

    void Lerp()
    {
        if(startPos != endPos)
        //if(transform.position != endPos)
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
                //print("u = " + u);
                u = LerpU(u, ltype);
                //print("  = \n" + u);
                transform.position = (1 - u) * startPos + u * endPos;
            }
        }       
    }

    // Adjust the u depending on the type of Lerping we are doing
    float LerpU(float u, LerpType lType)
    {
        float u2 = u;
        switch(lType)
        {
            case LerpType.linear:
                u2 = u;
                break;
            case LerpType.sinEasing:
                u2 = u - 0.4f * Mathf.Sin(2 * Mathf.PI * u);
                break;
            case LerpType.sin:
                u2 = 2.0f*Mathf.Sin(u);
                break;
        }
        return u2;
    }
    

    // Ensures that the start and end positions are the same (so no lerping)
    public void ResetPosition()
    {
        startPos = transform.position;
        endPos = transform.position;
    }

    public void CheckPositions()
    {
        //print(transform.position + "  " + endPos);
    }

    // May not be moving right now, but it will move 
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
            //_endPos.z = -1;
            //startPos.z = -1;      
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
