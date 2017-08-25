using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Piece : MonoBehaviour
{
    Transform[] sides;                  // Gold and Silver
    bool followMouse = false;           // true if player is moving piece
    float startTime;                    // for checking for quick taps
    Vector3 offset;
    bool _gold = true;                  // false for silver  
    Vector3 _home;                      // Location of the home tray
    bool _played = false;               // if this piece is used         
    Vector3 _previousPosition;          // Only used when placed location is bad
    bool _npcdrag = false;              // NPC is moving the piece 
    DragEffect drag;                    // Tilt the GameObject as it is being dragged
    bool _positionSet = false;
    bool _active = false;               // Active piece has z = -1 (on top of other pieces)
    bool _locked = false;               // true if we want to force a piece to be unmoveable (locked)

	// Use this for initialization
	void Awake ()
    {
        sides = new Transform[2];
        sides[0] = transform.FindChild("Gold");
        sides[1] = transform.FindChild("Silver");
        previousPosition = this.transform.position;
        drag = new DragEffect(transform.position, -25, 25, 2, 2);
	}
	
	// Used for following mouse and tilting
	void Update ()
    {             
        // Tilt this GameObject as it is being dragged either by a player or an NPC
        if(followMouse)
        {
            //print("Following mouse");
            if(!npcdrag)
            {
                //print("NPC NOT dragging");
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = -1.0f;
                transform.position = offset + mousePosition;
                transform.rotation = drag.Tilt(transform.position);
            }                               
        }        
        else
        {
            //print("Not following mouse");
            if(npcdrag)
            {
                //print("NPC IS dragging");
                if(this.GetComponent<LerpTest>().moving)
                {
                    //Vector3 p = transform.position;
                    //p.z = -1.0f;
                    //transform.position = p;

                    transform.rotation = drag.Tilt(transform.position);
                    
                    //print(transform.position);
                }
                    
                if(!this.GetComponent<LerpTest>().Moving())
                {
                    EndTiltLerp();
                }
            }
            else
            {
                //print("NPC is NOT dragging");
                transform.rotation = drag.ResetTilt();
               
                if (FinishedRotating())
                {
                    if(!positionSet)
                        SetPosition(transform.position);
                }
            }          
        }             
	}

    // Automatically move this piece home
    public void DragHomeByNPC(float delay)
    {
        npcdrag = true;
        //this.GetComponent<LerpTest>().alive = true;
        GetComponent<LerpTest>().ltype = LerpType.sin;
        this.GetComponent<LerpTest>().endPos = home;
        this.GetComponent<LerpTest>().delay = delay;
        positionSet = false;
    }

    // Automatically move thie piece to indicated position pos
    public void DragByNPC(Vector3 pos, float delay)
    {
        npcdrag = true;
        //this.GetComponent<LerpTest>().alive = true;
        GetComponent<LerpTest>().ltype = LerpType.linear;
        this.GetComponent<LerpTest>().endPos = pos;
        this.GetComponent<LerpTest>().delay = delay;
        positionSet = false;
    }

    // Check if Piece is rotated or not
    public bool FinishedRotating()
    {
        return transform.rotation.x == 0 && transform.rotation.y == 0;
    }

    // Check if Piece is home or not
    public bool AtHome()
    {
        //print("Piece = " +transform.position + "  " + home);
        return transform.position == home;
    }

    public void SetPosition(Vector3 position)
    {
        //print("Setting position");
        position.z = 0;
        transform.position = position;
        previousPosition = position;
        this.GetComponent<LerpTest>().ResetPosition();
        //GetComponent<LerpTest>().ltype = LerpType.linear;
    }

    // Set starting conditions of this piece
    public void SetStartPosition(Vector3 position)
    {
        //position.z = 0;
        SetPosition(position);    
        gold = true;
        this.GetComponent<LerpTest>().ResetPosition();
    }

    // Whenever a piece is being clicked/tapped
    private void OnMouseDown()
    {
        if(GameLogic.S.ValidPiecesToSelect(gameObject) && !locked)
        {
            //print("valid piece to move");
            followMouse = true;
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset.z = 0.0f;
            startTime = Time.time;
            drag.StartLerpTime();
            positionSet = false;
        }      
    }

    // When this piece is no longer being dragged around or tapped
    private void OnMouseUp()
    {
        if(followMouse)
        {
            Vector3 position = PlayField.S.GetClosestLocation(transform.position);
            followMouse = false;
            drag.ResetTiltValues();
            if(PlayField.S.LocationIsEmpty(position))
            {       
                //print("Location is empty");
                if(OtherPiece())              
                {
                    //print("flipping other piece");
                    Vector3 temp = position - previousPosition;
                    temp.z = 0.0f;
                    //print(temp.sqrMagnitude);
                    if (PlayField.S.LocationIsAdjacent(position, previousPosition))
                    {
                        //print("Location is adjacent");
                    
                    // (position != previousPosition && temp.sqrMagnitude < 3.0f)
                    //
                        GameLogic.S.flippedPiece = true;
                        Flip();                     
                        SetPosition(position);
                    }
                    else
                    {
                        Vector3 p = previousPosition;
                        SetPosition(p);
                    }
                }

                if(OwnPiece())
                {
                    Vector3 magVec = position - transform.position;
                    magVec.z = 0.0f;
                    if (magVec.sqrMagnitude < 0.5f)
                    {
                        GameLogic.S.movedPieceToBoard = true;
                        SetPosition(position);
                        PlayField.S.ActivateButtonsByTurn();
                    }
                    else
                    {
                        Vector3 p = previousPosition;
                        p.z = transform.position.z;
                        SetPosition(p);
                    }
                }
            }
            else
            {
                Vector3 p = previousPosition;
                p.z = transform.position.z;
                SetPosition(p);
            }
                       
            if(OwnPiece())
            {
                //print(Time.time - startTime);
                if (Time.time - startTime < 0.3f)
                    Flip();
            }          
        }       
    }

    // Check if this piece is player's own piece
    bool OwnPiece()
    {
        if ((GameLogic.S.sunTurn && gameObject.transform.name == "SUN") ||
            (!GameLogic.S.sunTurn && gameObject.transform.name == "MOON"))
        {
            return true;
        }
        return false;
    }

    // Check if this piece is other player's piece
    bool OtherPiece()
    {
        if ((GameLogic.S.sunTurn && gameObject.transform.name == "MOON") ||
            (!GameLogic.S.sunTurn && gameObject.transform.name == "SUN"))
        {
            return true;
        }
        return false;
    }

    // Flip a piece from gold to silver or silver to gold
    public void Flip()
    {
        //print("FLIP");
        gold = !gold;       
    }  

    // Change Children active states so either Gold or Silver is displayed
    void ChangeSortingOrder()
    {      
        if(gold)
        {
            sides[0].gameObject.SetActive(true);
            sides[1].gameObject.SetActive(false);              
        }
        else
        {
            sides[0].gameObject.SetActive(false);
            sides[1].gameObject.SetActive(true);
        }
        
    }

    public void StartTiltLerp()
    {
        drag.StartLerpTime();
    }

    public void EndTiltLerp()
    {
        npcdrag = false;
        drag.ResetTiltValues();
    }




    // Properties of this piece
    public bool gold
    {
        set
        {
            _gold = value;
            ChangeSortingOrder();
        }
        get { return _gold; }

    }
    
    public bool played
    {
        set { _played = value; }
        get { return _played; }
    }
  
    public Vector3 previousPosition
    {
        set { _previousPosition = value; }
        get { return _previousPosition; }
    }

    public bool npcdrag
    {
        set { _npcdrag = value; }
        get { return _npcdrag; }
    }

    public Vector3 home
    {
        set { _home = value; }
        get { return _home; }
    }

   
    public bool positionSet
    {
        set { _positionSet = value; }
        get { return _positionSet; }
    }

    public bool active
    {
        set { _active = value; }
        get { return _active; }
    }

    public bool locked
    {
        set { _locked = value; }
        get { return _locked; }
    }
}
