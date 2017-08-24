using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// The PlayField contains all of the information regarding the locations and
/// states of everything you see on the screen when playing the game.
/// This includes the board, the buttons, and the trays.  
/// </summary>
public class PlayField : MonoBehaviour
{
    //Variables to be set in Inspector
    public static PlayField S;
    public GameObject[] locations;
    public GameObject sunTray;
    public GameObject moonTray;
  
    int[] _board;           // Defines how pieces are placed on board at any time
    Vector3 _sunHome;
    Vector3 _moonHome;
    float adjacentDistance;

    private void Awake()
    {
        S = this;
        sunHome = sunTray.transform.position;
        moonHome = moonTray.transform.position;
        board = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        adjacentDistance = locations[1].transform.position.x - locations[0].transform.position.x;
    }

    void Start ()
    {
		
	}

    // What is the index this piece is on?
    public int ConvertLocationToIndex(GameObject piece)
    {
        for (int i = 0; i < locations.Length; i++)
        {
            if (piece.transform.position == locations[i].transform.position) { return i; }
        }
        return -1;
    }

    // Check to see if this position is actually empty
    public bool LocationIsEmpty(Vector3 position)
    {
        int[] s = EvaluateBoard();
        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i].transform.position == position)
            {
                if (s[i] != 0) { return false; }
            }
        }
        return true;
    }

    // Check if position is adjacent to previous position
    public bool LocationIsAdjacent(Vector3 position, Vector3 previous)
    {
        if(position.x == previous.x)
        {
            if(Mathf.Abs(position.y - previous.y) == adjacentDistance)
            {
                return true;
            }
        }

        if (position.y == previous.y)
        {
            if (Mathf.Abs(position.x - previous.x) == adjacentDistance)
            {
                return true;
            }
        }
        return false;
    }

    // Build the board array based on the pieces on the board
    public int[] EvaluateBoard()
    {
        GameObject[] suns = GameLogic.S.sunPieces;
        GameObject[] moons = GameLogic.S.moonPieces;
        int[] newboard = (int[])board.Clone();

        for (int i = 0; i < locations.Length; i++)
        {
            newboard[i] = 0;
            for (int s = 0; s < suns.Length; s++)
            {
                if (locations[i].transform.position == suns[s].transform.position)
                {
                    if (suns[s].GetComponent<Piece>().gold)
                        newboard[i] = 1;
                    else { newboard[i] = -1; }
                }
            }
            for (int m = 0; m < moons.Length; m++)
            {
                if (locations[i].transform.position == moons[m].transform.position)
                {
                    if (moons[m].GetComponent<Piece>().gold)
                        newboard[i] = 2;
                    else { newboard[i] = -2; }
                }
            }
        }
        board = newboard;
        return newboard;
    }

    // Return the closest location from released location
    public Vector3 GetClosestLocation(Vector3 released_location)
    {
        float[] distances = new float[locations.Length];
        float shortest_distance = 0.0f;
        int index_to_shortest = 0;
        for (int i = 0; i < locations.Length; i++)
        {
            Vector3 v = locations[i].transform.position - released_location;
            v.z = 0.0f;
            distances[i] = v.sqrMagnitude;
            if (i == 0) { shortest_distance = distances[i]; }
            else
            {
                if (distances[i] < shortest_distance)
                {
                    shortest_distance = distances[i];
                    index_to_shortest = i;
                }
            }
        }
        return locations[index_to_shortest].transform.position;
    }

    // Check if all pieces are on the Tray or not
    public bool AllPiecesHome(GameObject[] pieces)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (!pieces[i].GetComponent<Piece>().AtHome())
            {
                return false;
            }
        }
        return true;
    }

    // Activate either the Sun or Moon button based on current turn
    public void ActivateButtonsByTurn()
    {
        if (GameLogic.S.sunTurn) { ActivateButton("SunButton"); }
        else { ActivateButton("MoonButton"); }
    }

    // Activate either the Sun or Moon Button
    public void ActivateButton(string name)
    {
        GameObject button;
        button = GameObject.Find(name);
        button.GetComponent<Button>().interactable = true;
    }

    // Deactivate both the Sun and Moon Buttons
    public void DeactivateButtons()
    {
        GameObject button;
        button = GameObject.Find("SunButton");
        button.GetComponent<Button>().interactable = false;
        button = GameObject.Find("MoonButton");
        button.GetComponent<Button>().interactable = false;
    }


    // Properties 
    public Vector3 sunHome
    {
        set { _sunHome = value; }
        get { return _sunHome; }
    }

    public Vector3 moonHome
    {
        set { _moonHome = value; }
        get { return _moonHome; }
    }

    public int[] board
    {
        set { _board = value; }
        get { return _board; }
    }

   
}
