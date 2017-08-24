﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{
    public static GameLogic S;  
    public GameObject sunPrefab;
    public GameObject moonPrefab;

    [HideInInspector]
    public GameObject[] sunPieces;
    [HideInInspector]
    public GameObject[] moonPieces; 
    
    // Variables that deal with the logic of the game flow   
    bool _sunTurn = true;                   // false if moon's turn
    bool _movedPieceToBoard = false;
    bool _flippedPiece = false;
    bool _finishedOwnMoves = false;
    bool _allPiecesMovedHome = false;       // true when all pieces are home
    int _numPlayers;                        //1 or 2 players only  
    GameState _state;                       //State of the game

    private void Awake()
    {
        S = this;
        if (PlayerPrefs.HasKey("players")){
            numPlayers = PlayerPrefs.GetInt("players");
        }             
    }

    // Create the Pieces (16) and set them on the board initially
    void Start()
    {
        if (numPlayers == 1) { NPCPlayer.S.alive = true; }
        sunPieces = new GameObject[8];
        moonPieces = new GameObject[8];
        
        for(int i=0; i<8; i++)
        {
            sunPieces[i] = Instantiate(sunPrefab) as GameObject;
            moonPieces[i] = Instantiate(moonPrefab) as GameObject;    
            sunPieces[i].transform.name = "SUN";
            moonPieces[i].transform.name = "MOON";       
        }
        
        // Set initial positions by placing each piece on the board.  Moon on top.
        for (int i = 0; i < 8; i++)
        {
            moonPieces[i].GetComponent<Piece>().SetStartPosition(PlayField.S.locations[i].transform.position);
            moonPieces[i].GetComponent<Piece>().home = PlayField.S.moonHome;
            sunPieces[i].GetComponent<Piece>().SetStartPosition(PlayField.S.locations[i + 8].transform.position);
            sunPieces[i].GetComponent<Piece>().home = PlayField.S.sunHome;
        }
        
        state = new GameState(PlayField.S.board, 0);
        SetupGame();       
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(!allPiecesMovedHome)
        {         
            if(PlayField.S.AllPiecesHome(sunPieces))
            {
                if(PlayField.S.AllPiecesHome(moonPieces))
                {
                    allPiecesMovedHome = true;
                    print("All pieces are home");
                }
            }          
        }     
	}

    // The NPC is ever only Moon and when there is only 1 player
    public bool NPCTurn()
    {
        return (!sunTurn && numPlayers == 1) || !allPiecesMovedHome;
    }
    
    // Check whether the selected piece is moveable (valid) for the player
    public bool ValidPiecesToSelect(GameObject piece)
    {
        if(sunTurn)
        {
            if (!state.FlippableMoons()) { flippedPiece = true; }
            if(piece.transform.name == "SUN")
            {
                if(flippedPiece)
                {
                    if(!piece.GetComponent<Piece>().played)
                    {
                        if (movedPieceToBoard)
                        {
                            if(piece.GetComponent<Piece>().AtHome())
                                return false;
                        }        
                    }
                    else { return false; }
                    
                }
                else { return false; }
            }
            else  //Moon piece
            {
                if(piece.GetComponent<Piece>().played)
                {
                    int pieceIndex = PlayField.S.ConvertLocationToIndex(piece);
                    if(state.FlippablePiece(pieceIndex))
                    {
                        if (flippedPiece)
                        {
                            return false;
                        }
                    }
                    else { return false; }
                }
                else { return false; }
            }
        }
        else //Moon's turn
        {
            if (!state.FlippableSuns()) { flippedPiece = true; }
            if (piece.transform.name == "MOON")
            {
                if (flippedPiece)
                {
                    if (!piece.GetComponent<Piece>().played)
                    {
                        if (movedPieceToBoard)
                        {
                            if (piece.GetComponent<Piece>().AtHome())
                                return false;
                        }
                    }
                    else { return false; }
                }
                else { return false; }
            }
            else  //Sun piece
            {
                if (piece.GetComponent<Piece>().played)
                {
                    int pieceIndex = PlayField.S.ConvertLocationToIndex(piece);
                    if (state.FlippablePiece(pieceIndex))
                    {
                        if (flippedPiece)
                        {
                            return false;
                        }
                    }
                    else { return false; }
                }
                else { return false; }
            }
        }
        return true;
    }
  
    // Set all pieces on the board as being played  
    public void SetPiecesAsPlayed()
    {
        for(int i=0; i<moonPieces.Length; i++)
        {
            //moonPieces[i].GetComponent<Piece>().SetPosition(moonPieces[i].transform.position);
            //sunPieces[i].GetComponent<Piece>().SetPosition(sunPieces[i].transform.position);
        
            if(!moonPieces[i].GetComponent<Piece>().AtHome())
                moonPieces[i].GetComponent<Piece>().played = true;
            if(!sunPieces[i].GetComponent<Piece>().AtHome())    
                sunPieces[i].GetComponent<Piece>().played = true;
        }
    }

    public void NextPlayer()
    {    
        if (NPCPlayer.S.alive) { NPCPlayersTurn(); }
        else { NextPlayersTurn(); }            
    }

    public void NPCPlayersTurn()
    {
        NextPlayersTurn();
        //print("NPC");
        //PrintArray(state.state); 
        NPCPlayer.S.GetNextBestStateNPC(state.state);
    }

    public void NextPlayersTurn()
    {
        sunTurn = !sunTurn;
        movedPieceToBoard = false;
        flippedPiece = false;
        finishedOwnMoves = false;
        int[] b = PlayField.S.EvaluateBoard();
        state = new GameState(b, 0);
        SetPiecesAsPlayed();
        PlayField.S.DeactivateButtons();
     
        if(state.end)
        {
            int[] winners = state.CheckWin();
            if(winners[0] > winners[1]) { print("Sun Wins"); }
            else if(winners[0] < winners[1]) { print("Moon Wins"); }
            else { print("Draw"); }
            //SceneManager.LoadScene("testing"); 
        }
        //print(sunTurn);
    }

    // Initially drag the pieces to their homes
    public void SetupGame()
    {
        PlayField.S.DeactivateButtons();
        
        for (int i=0; i<sunPieces.Length; i++)
        {
            sunPieces[i].GetComponent<Piece>().DragHomeByNPC(i*.2f);          
        }
        for (int i = 0; i < moonPieces.Length; i++)
        {
            moonPieces[i].GetComponent<Piece>().DragHomeByNPC(i*.2f);          
        }         
    }

    // For testing if we want to know an array's contents
    /*
    public void PrintTurn()
    {
        int[] board = PlayField.S.EvaluateBoard();
        GameState s = new GameState(board, 0);
    }
    */
    public void ReportState()
    {
        int[] p = PlayField.S.EvaluateBoard();
        PrintArray(p);
    }
    
    public void PrintArray(int[] A)
    {
        string s = "[";
        for (int i = 0; i < A.Length; i++)
        {
            s += "," + A[i];
        }
        s += "]";
        print(s);
    }

    // Properties
    public GameState state
    {
        set { _state = value; }
        get { return _state; }
    }

    public bool sunTurn
    {
        set { _sunTurn = value; }
        get { return _sunTurn; }
    }

    public bool movedPieceToBoard
    {
        set { _movedPieceToBoard = value; }
        get { return _movedPieceToBoard; }
    }

    public bool flippedPiece
    {
        set { _flippedPiece = value; }
        get { return _flippedPiece; }
    }

    public bool finishedOwnMoves
    {
        set { _finishedOwnMoves = value; }
        get { return _finishedOwnMoves; }
    }

    public bool allPiecesMovedHome
    {
        set { _allPiecesMovedHome = value; }
        get { return _allPiecesMovedHome; }
    }

    public int numPlayers
    {
        set { _numPlayers = value; }
        get { return _numPlayers; }
    }
}

