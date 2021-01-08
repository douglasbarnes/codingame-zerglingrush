using System;
using System.Collections.Generic;

class StateMap
{
    // Each tile on the grid has a state
    // The problem describes four states: "." "#", "B", and "z", but we add "x" to denote an inaccessible "."
    // and "?" to denote a tile that has not yet been evaluated by our algorithm
    public enum State
    {
        Unknown=0,
        Accessible=1,
        Wall=2,
        Building=4,
        Inaccessible=8,
        Zerg=16
    }

    // Dimensions of the map. Not the same as the input to the program
    public int W;
    public int H;

    // We take the origin to be the top left corner of the map
    public State[,] innerMap;
    public StateMap(int w, int h)
    {

        // We add two on because unlike in the problem description, we surround the map that is input with "."s. 
        // This has the effect of telling our algorithm that zerglings can enter from all sides(Note 1)
        W=w+2;
        H=h+2;

        innerMap = new State[W,H];

        // As above, set perimeter to "accessible". By default, everything is already marked as "?",
        // because the enum value of "?" is 0.
        for(int i=0;i<W;i++)
        {
            innerMap[i,0] = State.Accessible;
            innerMap[i,H-1] = State.Accessible;
        }

        for(int i=0;i<H;i++)
        {
            innerMap[W-1,i] = State.Accessible;
            innerMap[0,i] = State.Accessible;
        }

    }

    private char get_char(int x, int y, bool answer=false) 
    {
        // Convert a State into its character representation

        
        switch(innerMap[x,y])
        {
            case State.Accessible:
                return '.';
            case State.Wall:
                return '#';
            case State.Inaccessible:
                // If we answering, don't show "innaccessible"s, output them as "."s
                if(answer)
                    return '.';
                else
                    return 'x';
            case State.Building:
                return 'B';
            case State.Unknown:
                return '?';
            case State.Zerg:
                return 'z';
        }

        // Something has gone wrong if the above did not find the character
        throw new Exception();
    }

    public void Visualise(bool answer=false)
    {
        // Print out the graphical representation of the board, as given in the problem description.
        // If answer is true, our custom characters(for debugging purposes) "x" are not output.

        // Iterate y coords
        for(int i=0;i<H;i++)
        {
            // String that will be output
            string currentLine = "";

            // Iterate x coords and add them onto output string
            for(int j = 0;j<W;j++)
            {
                currentLine += get_char(j,i, answer);
            }

            // If we are answering, we also need to make sure that we do not output the extra "." padding we added around the map
            // Also, answers are output to stdout, debug messages to stderr.
            if(answer)
            {
                // If not y=0 or y=H 
                if(i > 0 && i < H-1)
                {
                    // Write x=1 to x=W-1
                    Console.WriteLine(currentLine.Substring(1,W-2));
                }
            }
            else
            {
                Console.Error.WriteLine(currentLine);
            }
        }
    }

    public State CheckAccessible(int x, int y)
    {
        // An algorithm that evaluates the state of an unknown "?" based on its surroundings

        // Don't use on perimeter

        // If adjacent to any accessible points, a zerg can get there
        if(innerMap[x,y-1] == State.Accessible
            || innerMap[x,y+1] == State.Accessible
            || innerMap[x+1,y] == State.Accessible
            || innerMap[x-1,y] == State.Accessible)
        {
            return State.Accessible;
        }

        // If adjacent to any unknowns, a zerg may or may not be able to get there
        // The scan should be repeated using Sweep()
        if(innerMap[x,y-1] == State.Unknown
            || innerMap[x,y+1] == State.Unknown
            || innerMap[x+1,y] == State.Unknown
            || innerMap[x-1,y] == State.Unknown)
            {
                return State.Unknown;
            }

        // Otherwise we cannot get there
        return State.Inaccessible;
    }
    

    public bool AddEntry(int x,int y,char input)
    {
        // Set a value in the innerMap. Essentially the reverse of get_char()
        // If the entry is ambiguous, return false. This only occurs if we add a "." but we cannot determine
        // whether we can access it (stays at ".") or cannot (becomes "x")

        if(input == 'B')
        {
            innerMap[x,y] = State.Building;
        }
        else if(input == '#')
        {
            innerMap[x,y] = State.Wall;
        }
        else if(input == 'z')
        {
            innerMap[x,y] = State.Zerg;
        }
        else
        {
            State state = CheckAccessible(x,y);

            if(state == State.Accessible) 
            {
                innerMap[x,y] = State.Accessible;
            }
            else if(state == State.Unknown)
            {
                innerMap[x,y] = State.Unknown;
                return false;
            }
            else
            {
                innerMap[x,y] = State.Inaccessible;
            }
        }
        return true;
    }
}

class Solution
{
    static StateMap map;

    static bool Sweep()
    {
        // There are two conditions determining whether we need to Sweep again, or if algorithm has completed(the map is evaluated)
        // (C1) There are still unknowns in the map
        // (C2) Entries in the map have changed since the last iteration
        // Sweep needs to be repeated if and only if both C1 and C2 are true
        // C1 should make sense, if there are unknowns, the map may be ambiguous, sweeping again may clear them.
        // C2 is not as obvious. If we repeat twice and nothing changes, sweeping once more will not change anything either.
        // so if there are no new paths discovered by sweeping, then the rest of the unknowns must be inaccessible.
        // TODO: This function sweeps down only, but alternating between sweeping up and down could be much more efficient
        
        // We start by assuming C1 and C2 are not met

        // If we still have unknowns in the map, we need to sweep again
        bool C1 = false;

        // However. if we sweep twice and nothing changes, we are done, the unknowns are inaccessible
        bool C2 = false;

        // Iterate through y coords
        for(int y=0; y<map.H;y++)
        {
            // Through x coords
            for(int x=0; x<map.W;x++)
            {
                // If it is an unknown "?", see if we can evaluate it.
                if(map.innerMap[x,y] == StateMap.State.Unknown)
                {
                    StateMap.State result = map.CheckAccessible(x,y);

                    // If the result is unknown, there may be more ambiguities, C1 is not met. If it isn't unknown, we leave the "Again" variable unchanged.
                    C1 |= result == StateMap.State.Unknown;

                    // Similarly, if the result is not unknown, something has changed therefore C2 is met.
                    C2 |= result != StateMap.State.Unknown;

                    map.innerMap[x,y] = result;
                }
            }
        }

        // If we didn't do anything in a sweep, we need to change all the unknowns into inaccessibles 
        if(!C2)
        {
            for(int y=0; y<map.H;y++)
            {
                for(int x=0; x<map.W;x++)
                {
                    if(map.innerMap[x,y] == StateMap.State.Unknown)
                    {
                        map.innerMap[x,y] = StateMap.State.Inaccessible;
                    }
                }
            }
        }

        return C1 && C2; 
    }

    
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int W = int.Parse(inputs[0]);
        int H = int.Parse(inputs[1]);

        // List of building coordinates for zerg to attack
        List<(int, int)> buildings = new List<(int, int)>();

        map = new StateMap(W,H);

        // True if we need extra scans. We may have solved it entirely after the first "sweep".
        // We don't use the sweep function here because there are some setup steps required
        bool SweepsRequired = false;

        for (int i = 0; i < H; i++)
        {
            string ROW = Console.ReadLine();

            // Read characters in and parse them into the map.
            for(int j=0;j<ROW.Length;j++)
            {                
                // Add buildings here, this saves us looping again later
                if(ROW[j] == 'B')
                {
                    buildings.Add((j+1,i+1));
                }

                // AddEntry returns false if when the entry was added, it is still unknown whether it is reachable or not, so we need to sweep again.
                SweepsRequired |= map.AddEntry(j+1,i+1,ROW[j]);
            }
        }
        
        // Before sweeps
        // map.Visualise();
        while(SweepsRequired)
        {
            SweepsRequired=Sweep();
        }
        
        // Place zerg around buildings
        Attack(buildings);

        // Print to debug and to answer
        map.Visualise();
        map.Visualise(true);        
    }

    static void Attack(List<(int, int)> buildings)
    {
        // A function best left unread, but ties together everything we've done. 
        // Essentially, we check if the surrounding coordinates of a B are accessible. If they are, we put a zergling there.
        
        foreach((int,int) coord in buildings)
        {
            // 123
            // 4B5
            // 678

            // 1
            if(map.innerMap[coord.Item1 -1, coord.Item2 -1] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1 -1, coord.Item2-1, 'z');
            }
            // 2
            if(map.innerMap[coord.Item1, coord.Item2 -1] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1, coord.Item2 -1, 'z');
            }
            // 3
            if(map.innerMap[coord.Item1 +1, coord.Item2 -1] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1 +1, coord.Item2-1, 'z');
            }
            // 4
            if(map.innerMap[coord.Item1 -1, coord.Item2] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1 -1, coord.Item2, 'z');
            }
            // 5
            if(map.innerMap[coord.Item1 +1, coord.Item2] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1 +1, coord.Item2, 'z');
            }
            // 6
            if(map.innerMap[coord.Item1 -1, coord.Item2 +1] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1 -1, coord.Item2+1, 'z');
            }
            // 7
            if(map.innerMap[coord.Item1, coord.Item2 +1] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1, coord.Item2 +1, 'z');
            }
            // 8
            if(map.innerMap[coord.Item1 +1, coord.Item2 +1] == StateMap.State.Accessible)
            {
                map.AddEntry(coord.Item1 +1, coord.Item2 +1, 'z');
            }            
        }
    }
}
