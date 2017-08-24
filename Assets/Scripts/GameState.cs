using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameState// : MonoBehaviour
{   
    public List<GameState> children;
    public GameState bestChild = null;
    public int[] state;
    int _level;
    int _score;
    bool _end;
    //int numFlips;
    public int winScore = 100;
    public int loseScore = -100;
    //bool invalid;
    //bool randomPaths; 
    int emptyVal;
    int[] sun;
    int[] moon;
    List<int[]> LINES;

    public GameState(int[] s, int lvl)
    {
        state = s;
        level = lvl;
        children = new List<GameState>();
        sun = new int[] { 1, -1 };
        moon = new int[] { 2, -2 };
        emptyVal = 0;
        _score = 0;
        _end = false;
        //numFlips = 0;
        //invalid = false;
        //randomPaths = false;
        SetupWinningLines();
        CheckEndState();
      
    }

    //This is an end state if there are no empty spaces or either sun or moon wins
    void CheckEndState ()
    {
        //Get the indices to all of the empty spaces
        List<int> indices = EmptySpaces();
        if(indices.Count == 0) { end = true; }
        else
        {
            //Get the score for this state
            int[] winners = CheckWin();
            GetScore(winners);
            if(winners[0] > 0 || winners[1] > 0) { end = true; }
        }
    }

    //Returns a List of indices that represent the empty spaces on the board
    public List<int> EmptySpaces()
    {
        List<int> indices = new List<int>();
        for(int i=0; i<state.Length; i++)
        {
            if(state[i] == emptyVal) { indices.Add(i); }
        }
        return indices;
    }

    //Check this state and see how many winning lines sun and moon have if any
    public int[] CheckWin()
    {
        int[] winners = new int[2] { 0, 0 };
        for(int i=0; i<LINES.Count; i++)
        {
            for(int s=0; s<sun.Length; s++)
            {
                if(AllPiecesMatch(LINES[i], sun[s]))
                {
                    bool locked = CheckLockedPieces(LINES[i]);
                    if (locked) { winners[0] += 1; }
                }
                
            }
            for(int m=0; m<moon.Length; m++)
            {
                if (AllPiecesMatch(LINES[i], moon[m]))
                {
                    bool locked = CheckLockedPieces(LINES[i]);
                    if (locked) { winners[1] += 1; }
                }
                
            }
        }
        return winners;
    }

    // Check if the indices are locked (adjacent values do not contain 0)
    bool CheckLockedPieces(int[] indices)
    {
        List<int> adjacents = GetAdjacentIndices(indices);
        for(int i=0; i<adjacents.Count; i++)
        {
            if(state[adjacents[i]] == 0) { return false; }
        }
        return true;  
    }

    // Check if all values in indices match
    bool AllPiecesMatch(int[] indices, int val)
    {
        if(state[indices[0]] == state[indices[1]] &&
            state[indices[0]] == state[indices[2]] &&
            state[indices[0]] == val)
        {
            return true;
        }
        return false;
    }

    //Check if two values are on the same row or not
    bool OnSameRow(int val1, int val2)
    {
        if(val1/4 == val2 / 4) { return true; }
        return false;
    }

    //Given an array of indices, return a List of adjacent indices
    List<int> GetAdjacentIndices(int index)
    {
        List<int> adjacents = new List<int>();
        if (index - 4 >= 0) { adjacents.Add(index - 4); }
        if (index + 4 < state.Length) { adjacents.Add(index + 4); }
        if (OnSameRow(index + 1, index)) { adjacents.Add(index + 1); }
        if (OnSameRow(index - 1, index) && index - 1 >= 0) { adjacents.Add(index - 1); }
        return adjacents;
    }

    List<int> GetAdjacentIndices(int[] indices)
    {
        List<int> adjacents = new List<int>(); 
        for (int i = 0; i < indices.Length; i++)
        {             
            adjacents.AddRange(GetAdjacentIndices(indices[i]));   
        }
       
        HashSet<int> temp = new HashSet<int>();
        for (int i = 0; i < adjacents.Count; i++)
            temp.Add(adjacents[i]);
        List<int> temp2 = temp.ToList();
        for(int i=0; i<indices.Length; i++)
        {
            if(temp2.Contains(indices[i]))
            {
                temp2.Remove(indices[i]);
            }
        }             
        return temp2;         
    }

    // Return a List of empty indices adjacent to index
    List<int> GetAdjacentEmpty(int index)
    {
        List<int> result = new List<int>();
        List<int> adjacent = GetAdjacentIndices(index);
        List<int> empty = EmptySpaces();
        for(int i=0; i<empty.Count; i++)
        {
            if(adjacent.Contains(empty[i]))
            {
                result.Add(empty[i]);
            }
        }
        return result;
    }

    // Returns true if index has any empty adjacent neighbors
    bool AnyAdjacentEmpty(int index)
    {
        List<int> values = GetAdjacentEmpty(index);
        if(values.Count > 0) { return true; }  
        return false;
    }

    //Returns true if Sun player is the root, false if Moon is root
    public bool SunIsRoot(int level = 0)
    {
        List<int> indices = EmptySpaces();
        if(IsEven(indices.Count - level)) { return true; }
        return false;
    }

    //Sun is the parent if even number of empty spaces on board
    bool SunIsParent()
    {
        List<int> indices = EmptySpaces();
        if (IsEven(indices.Count)) { return true; }
        return false;
    }

    //Returns true if value is even, false if odd
    bool IsEven(int value)
    {
        if(value % 2 == 0) { return true; }
        return false;
    }

    // true if there are moons to flip
    public bool FlippableMoons()
    {
        List<int> tempIndices = GetPlayerIndices(moon);
        List<int> moonIndices = GetFlippableIndices(tempIndices);
        if(moonIndices.Count > 0) { return true; }
        return false;
    }

    // true if there are suns to flip
    public bool FlippableSuns()
    {
        List<int> tempIndices = GetPlayerIndices(sun);
        List<int> sunIndices = GetFlippableIndices(tempIndices);
        if (sunIndices.Count > 0) { return true; }
        return false;
    }

    public bool FlippablePiece(int index)
    {
        if(GetFlippableIndices(index).Count > 0)
            return true;
        return false;
    }

    // Populate the children List with next GameStates
    public void PopulateChildren()
    {
        if(SunIsParent())
        {
            List<int> tempIndices = GetPlayerIndices(moon);
            List<int> moonIndices = GetFlippableIndices(tempIndices);
            CreateChildren(sun, moonIndices);
        }
        else
        {
            List<int> tempIndices = GetPlayerIndices(sun);
            List<int> sunIndices = GetFlippableIndices(tempIndices);
            CreateChildren(moon, sunIndices);
        }

        PruneChildren();
    }

    // Create all possible child states for this parent state
    void CreateChildren(int[] parent, List<int> indices)
    {
        if(indices.Count > 0)
        {
            for(int i=0; i<indices.Count; i++)
            {
                List<int> flipindices = GetAdjacentEmpty(indices[i]);
                for(int f=0; f<flipindices.Count; f++)
                {
                    List<int> empty = EmptySpaces();
                    empty.Remove(flipindices[f]);
                    empty.Add(indices[i]);
                    empty.Sort();
                    for(int e=0; e<empty.Count; e++)
                    {
                        for(int side=0; side<parent.Length; side++)
                        {
                            int[] newState = (int[])state.Clone();                  
                            newState[flipindices[f]] = newState[indices[i]] * -1;
                            newState[indices[i]] = 0;
                            newState[empty[e]] = parent[side];

                            GameState s = new GameState(newState, level+1);
                            children.Add(s);
                        }
                    }
                }
            }
        }
        else // No flippable pieces, so just place own piece
        {
            List<int> empty = EmptySpaces();
            for (int e = 0; e < empty.Count; e++)
            {
                for (int side = 0; side < parent.Length; side++)
                {
                    int[] newState = (int[])state.Clone();
                    newState[empty[e]] = parent[side];
                    GameState s = new GameState(newState, level+1);
                    children.Add(s);
                }
            }
        }
    }

    // Get all the indices where values is located
    List<int> GetPlayerIndices(int[] values)
    {
        List<int> indices = new List<int>();
        for(int i=0; i<state.Length; i++)
        {
            for(int v=0; v<values.Length; v++)
            {
                if(state[i] == values[v])
                {
                    indices.Add(i);
                }
            }
        }
        return indices;
    }

    // Return a subset of indices which have empty adjacent spaces
    List<int> GetFlippableIndices(List<int> indices)
    {
        List<int> flippables = new List<int>();
        for(int i=0; i<indices.Count; i++)
        {
            if (AnyAdjacentEmpty(indices[i]))
            {
                flippables.Add(indices[i]);
            }
        }
        return flippables;
    }

    List<int> GetFlippableIndices(int index)
    {
        List<int> flippables = new List<int>();
        if(AnyAdjacentEmpty(index))
        {
            flippables.Add(index);
        }
        return flippables;
    }

    //These should include getters and setters and be public
    public void GetScore(int[] winners)
    {
        //int[] winners = CheckWin();
        if (winners[0] > winners[1]) //Sun has more wins
        {
            if (SunIsRoot(level)) { score = winScore; }
            else { score = loseScore; }
        }
        else if (winners[0] < winners[1])  //Moon has more wins
        {
            if (SunIsRoot(level)) { score = loseScore; }
            else { score = winScore; }
        }
    }

    // Get the best score from all the children
    public void GetScoreFromChildren()
    {
        List<int> levels = new List<int>();
        int[] scores = new int[children.Count];
        for (int i = 0; i < scores.Length; i++)
            scores[i] = children[i].score;
       
        //List<int> empties = EmptySpaces();
     
        score = scores.Max();

        int score_num = 0;
        for (int i=0; i<scores.Length; i++)
        {
            if(scores[i] == score) { score_num++; }
        }
   
        if(score_num > 1) //More than 1 child has the best score
        {
            List<GameState> kids = FilterChildrenByScore(score);
            levels = GetBestChildLevel(kids);
            int min_level = levels.Min();
            int level_num = 0;
            for(int i=0; i<levels.Count; i++)
            {
                if(levels[i] == min_level) { level_num++; }
            }

            if(level_num > 1) // More than one best child has the lowest level
            {
                List<int> indices = new List<int>();
                for(int i=0; i<levels.Count; i++)
                {
                    if(levels[i] == min_level)
                    {
                        indices.Add(i);
                    }
                }
              
                int index = Random.Range(0, indices.Count);
                bestChild = children[indices[index]];
            }
            else // Only one best child has the lowest level
            {               
                for (int i = 0; i < kids.Count; i++)
                {
                    if (kids[i].level == min_level)
                    {
                        bestChild = children[i];
                        break;
                    }
                }               
            }
        }
        else // Only one child has the best score
        {
            for(int i=0; i<scores.Length; i++)
            {
                if(scores[i] == score)
                {
                    bestChild = children[i];
                    break;
                }
            }       
        }         
    }

    List<int> GetBestChildLevel(List<GameState> kids)
    {
        List<int> levels = new List<int>();
        for(int i=0; i<kids.Count; i++)
        {
            if(kids[i].bestChild != null)
            {
                levels.Add(kids[i].bestChild.level);
            }
            else { levels.Add(kids[i].level); }
        }
        return levels;
    }

    // Get only the children with the best score
    List<GameState> FilterChildrenByScore(int score)
    {
        List<GameState> kids = new List<GameState>();
        for(int i=0; i<children.Count; i++)
        {
            if(children[i].score == score) { kids.Add(children[i]); }
        }
        return kids;
    }

    // Optimization methods

    void PruneChildren()
    {
        List<GameState> temp = new List<GameState>();
        for(int i=0; i<children.Count; i++)
        {
            if(!InvalidState())
            {
                temp.Add(children[i]);
            }
        }
        if(temp.Count > 0) { children = temp; }

        for(int i=0; i<children.Count; i++)
        {
            if(children[i].WinStateRoot() || children[i].WinStateOther())
            {
                List<GameState> onlyChild = new List<GameState>();
                onlyChild.Add(children[i]);
                children = onlyChild;
                break;
            }
        }

    }

    // Checks if this state is invalid or not
    bool InvalidState()
    {
        if((score == winScore && IsEven(level)) || (score == loseScore && !IsEven(level)))
        {
            return true;
        }
        return false;
    }

    bool LoseStateRoot()
    {
        return end && score == loseScore && !IsEven(level);
    }

    bool WinStateRoot()
    {
        return end && score == winScore && !IsEven(level);
    }

    bool WinStateOther()
    {
        return end && score == loseScore && IsEven(level);
    }

    bool LoseStateOther()
    {
        return end && score == winScore && IsEven(level);
    }

    public int SetMaxLevel(int maxLevel)
    {
        if(WinStateRoot())
        {
            return level - 2;
        }
        return maxLevel;
    }

    public void FirstMove()
    {
        int[] choices = new int[] { 0, 1, 6, 7, 24, 25, 30, 31 };
        List<int> empty = new List<int>();
        if(empty.Count == 16)
        {
            int index = Random.Range(0, choices.Length);
            List<GameState> onlyChild = new List<GameState>();
            onlyChild.Add(children[index]);
            children = onlyChild;
        }
    }

    public void SecondMove()
    {
        List<int> empty = new List<int>();
        if (empty.Count == 15)
        {
            int index = Random.Range(0, 2);
            List<GameState> onlyChild = new List<GameState>();
            onlyChild.Add(children[index]);
            children = onlyChild;
        }
    }

    public void ChooseRandomChild()
    {
        if(children.Count > 0)
        {
            int index = Random.Range(0, children.Count);
            List<GameState> onlyChild = new List<GameState>();
            onlyChild.Add(children[index]);
            children = onlyChild;
        }
    }

    // Determine how the two states differ
    // Result can be one of two things
    //result: [flipFrom, flipTo, place, side]
    //result: [place, side]
    public List<int> DiffState(int[] otherState)
    {
        List<int> result = new List<int>();
        List<int> indices = new List<int>();
        List<int> oldvalues = new List<int>();
        List<int> newvalues = new List<int>();
        for (int i = 0; i < otherState.Length; i++)
        {
            if (otherState[i] != state[i])
            {
                indices.Add(i);
                oldvalues.Add(otherState[i]);
                newvalues.Add(state[i]);
            }
        }

        if (oldvalues.Count > 1)  // A piece was flipped
        {
            for (int i = 0; i < oldvalues.Count; i++)
            {
                if (oldvalues[i] != 0)
                {
                    if (newvalues.Contains(oldvalues[i] * -1))
                    {
                        result.Add(indices[i]);
                        result.Add(indices[newvalues.IndexOf(oldvalues[i] * -1)]);                      
                    }
                }
            }
                
            List<int> test = new List<int>();
            for(int i=0; i<indices.Count; i++)
            {
                if(!result.Contains(indices[i]))
                {
                    test.Add(indices[i]);
                }
            }
            if(test.Count == 0) { result.Add(result[0]); }
            else { result.Add(test[0]); }
           
            if(state[result[2]] > 0) { result.Add(1); }
            else { result.Add(0); }

        }
        else // No pieces flipped
        {
            result.Add(indices[0]);
            if(state[result[0]] > 0) { result.Add(1); }
            else { result.Add(0); }
        }

        return result;
    }

    public int score
    {
        get { return _score; }
        set { _score = value; }
    }

    public int level
    {
        get { return _level; }
        set { _level = value; }
    }

    public bool end
    {
        get { return _end; }
        set { _end = value; }
    }

    void SetupWinningLines()
    {
        LINES = new List<int[]>();
        LINES.Add(new int[] { 0, 1, 2 });
        LINES.Add(new int[] { 1, 2, 3 });
        LINES.Add(new int[] { 4, 5, 6 });
        LINES.Add(new int[] { 5, 6, 7 });
        LINES.Add(new int[] { 8, 9, 10 });
        LINES.Add(new int[] { 9, 10, 11 });
        LINES.Add(new int[] { 12, 13, 14 });
        LINES.Add(new int[] { 13, 14, 15 });
        LINES.Add(new int[] { 0, 4, 8 });
        LINES.Add(new int[] { 4, 8, 12 });
        LINES.Add(new int[] { 1, 5, 9 });
        LINES.Add(new int[] { 5, 9, 13 });
        LINES.Add(new int[] { 2, 6, 10 });
        LINES.Add(new int[] { 6, 10, 14 });
        LINES.Add(new int[] { 3, 7, 11 });
        LINES.Add(new int[] { 7, 11, 15 });
        LINES.Add(new int[] { 1, 6, 11 });
        LINES.Add(new int[] { 0, 5, 10 });
        LINES.Add(new int[] { 5, 10, 15 });
        LINES.Add(new int[] { 4, 9, 14 });
        LINES.Add(new int[] { 2, 5, 8 });
        LINES.Add(new int[] { 3, 6, 9 });
        LINES.Add(new int[] { 6, 9, 12 });
        LINES.Add(new int[] { 7, 10, 13 });
    }

    public string PrintArray(int[] A)
    {
        string s = "[";
        for(int i=0; i<A.Length; i++)
        {
            s += ","+A[i];
        }
        s += "]";
        return s;
    }

    public string PrintState()
    {
        string s = "[";
        //print(state.Length);
        for (int i = 0; i < state.Length; i++)
        {
            s += "," + state[i];
        }
        s += "]";
        return s;
        
    }

    public string PrintChild(int i)
    {
        return PrintArray(children[i].state);
      
    }

    public string PrintChildScores()
    {
        string s = "Scores: [";
        for(int i=0; i<children.Count; i++)
        {
            s += ","+children[i].score;
        }
        s += "]";
        return s;
    }

    public string PrintBestChild()
    {
        return PrintArray(bestChild.state);
    }
}
