using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Everything we need to simulate a player goes here.  That includes moving
/// pieces around the board and using Minimax or some other algorithm to 
/// determine a next best state.
/// </summary>
public class NPCPlayer : MonoBehaviour
{
    public static NPCPlayer S;
    // Variables that are used for Minimax
    int num;
    int maxLevel;
    bool _alive = false;

    GameObject flippingPiece;
    GameObject movingPiece;

    bool piecesChosen = false;

    private void Awake()
    {
        S = this;
        num = 0; //Counter for minimax
        maxLevel = 16;
    }

    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(piecesChosen)
        {
            if (!flippingPiece.GetComponent<LerpTest>().Moving())
            {
                if (!movingPiece.GetComponent<LerpTest>().Moving())
                {
                    GameLogic.S.NextPlayersTurn();
                    print("Finished moving NPC pieces");
                    piecesChosen = false;
                }
            }
        }
		
	}

    // Using the current state, find the next best state
    public void GetNextBestStateNPC(int[] oldState)
    {
        GameState state = new GameState(oldState, 0);
        StartMonteCarlo(state);
        PlayField.S.board = oldState;
        GameLogic.S.PrintArray(PlayField.S.board);
        GameLogic.S.PrintArray(state.bestChild.state);
        DisplayNewStateNPC(state.bestChild);
        PlayField.S.DeactivateButtons();
        //PlayField.S.ActivateButtonsByTurn();
        //GameLogic.S.NextPlayersTurn();
    }

    void DisplayNewStateNPC(GameState newState)
    {
        GameObject[] suns = GameLogic.S.sunPieces;
        GameObject[] moons = GameLogic.S.moonPieces;
        //Can result in length of 2 or 4
        List<int> result = newState.DiffState(PlayField.S.board);
        if (result.Count == 2)
        {
           
            List<int> empties = newState.EmptySpaces();
            if (empties.Count % 2 == 0)//Moon placed a piece
            {
                PlacePieceNPC(moons, PlayField.S.moonHome, result[0], result[1]);
            }
            else//Sun placed a piece
            {
                PlacePieceNPC(suns, PlayField.S.sunHome, result[0], result[1]);
            }
        }
        else if (result.Count == 4)
        {
           
            List<int> empties = newState.EmptySpaces();
            if (empties.Count % 2 == 0)//Moon placed a piece
            {
                //print("Flip sun, place moon: ");
                GameLogic.S.PrintArray(result.ToArray());
                FlipPieceNPC(suns, result[0], result[1]);
                PlacePieceNPC(moons, PlayField.S.moonHome, result[2], result[3]);
            }
            else//Sun placed a piece
            {
                FlipPieceNPC(moons, result[0], result[1]);
                PlacePieceNPC(suns, PlayField.S.sunHome, result[2], result[3]);
            }
        }
        piecesChosen = true;
        //GameLogic.S.flippedPiece = true;
        //GameLogic.S.movedPieceToBoard = true;
    }

    // The NPC flips opponents piece to a new location
    void FlipPieceNPC(GameObject[] pieces, int iFrom, int iTo)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].transform.position == PlayField.S.locations[iFrom].transform.position)
            {
                flippingPiece = pieces[i];
                print("Home " + pieces[i].GetComponent<Piece>().home);
                print("From: " + pieces[i].transform.position);
                print("To: " + PlayField.S.locations[iTo].transform.position);
                pieces[i].GetComponent<Piece>().Flip();
                pieces[i].GetComponent<Piece>().DragByNPC(PlayField.S.locations[iTo].transform.position, 0);
            }
        }
    }

    // The NPC moves own piece to a new location
    void PlacePieceNPC(GameObject[] pieces, Vector3 home, int index, int side)
    {
        int i;
        for (i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].transform.position == home)
            {
                movingPiece = pieces[i];
                if (side == 0)
                    pieces[i].GetComponent<Piece>().gold = false;
                else
                    pieces[i].GetComponent<Piece>().gold = true;

                pieces[i].GetComponent<Piece>().DragByNPC(PlayField.S.locations[index].transform.position, 2);
                break;
            }
        }
    }

    // The minimax algorithm
    void Minimax(GameState parent)
    {
        num++;
        if (parent.end)
        {
            int[] winners = parent.CheckWin();
            parent.GetScore(winners);
            maxLevel = parent.SetMaxLevel(maxLevel);
        }
        else
        {
            if (parent.level < maxLevel)
            {
                parent.PopulateChildren();
            }

            if (parent.children.Count > 0)
            {
                if (parent.children.Count == 1) { parent.end = true; }
                for (int i = 0; i < parent.children.Count; i++)
                {
                    if (num > 100000) { parent.children[i].end = true; }
                    Minimax(parent.children[i]);
                }
                parent.GetScoreFromChildren();
            }
        }
    }

    void StartMonteCarlo(GameState parent)
    {
        List<int> emptySpots = parent.EmptySpaces();
        int num = 16 - emptySpots.Count + 10;
        //int num = (int)Mathf.Pow(2, 16 - emptySpots.Count - 1) + 10;
        //print("Number of Monte Carlo iterations = " + num);

        parent.PopulateChildren();
        int[] numWins = new int[parent.children.Count];
        int[] totalWins = new int[parent.children.Count];
        for (int n = 0; n < num; n++)
        {
            for (int i = 0; i < numWins.Length; i++)
            {
                numWins[i] = MonteCarlo(parent.children[i]);
                totalWins[i] += numWins[i];
            }

        }

        int bestScore = totalWins.Max();
        for (int i = 0; i < parent.children.Count; i++)
        {
            if (totalWins[i] == bestScore)
            {
                parent.bestChild = parent.children[i];
            }
        }
    }

    // Monte Carlo Tree Search (Recursive)
    int MonteCarlo(GameState parent)
    {
        if (parent.end)
        {
            int[] winners = parent.CheckWin();
            parent.GetScore(winners);
            if (parent.score == parent.winScore)
            {
                return 1;
            }
            return 0;
        }
        else
        {
            parent.PopulateChildren();
            parent.ChooseRandomChild();
            return MonteCarlo(parent.children[0]);
        }
    }

    public bool alive
    {
        set { _alive = value; }
        get { return _alive; }
    }
}
