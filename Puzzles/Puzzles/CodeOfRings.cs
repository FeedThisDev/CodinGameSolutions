using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;

public struct Gamestate
{
    public int TotalStringLength { get; set; }
    public string TotalString { get; set; }
    public char[] CurrentRunes { get; set; }
    public int PlayerPosition { get; set; }    
}

public class Node
{
    public Gamestate State { get; set; }

    public Node[] Childrean { get; set; }

    public Node Parent { get; set; }
}

class Player
{
    static int PermCounter = 0;
    static char[] runeValues = " ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    #region MagicStrings
    public static string[] goRight = {
    "",
    ">",
    ">>",
    ">>>",
    ">>>>",
    ">>>>>",
    ">>>>>>",
    ">>>>>>>",
    ">>>>>>>>",
    ">>>>>>>>>",
    ">>>>>>>>>>",
    ">>>>>>>>>>>",
    ">>>>>>>>>>>>",
    ">>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>>>>>>",
    ">>>>>>>>>>>>>>>>>>>>>>",
    };

    public static string[] add = {
    "",
    "+",
    "++",
    "+++",
    "++++",
    "+++++",
    "++++++",
    "+++++++",
    "++++++++",
    "+++++++++",
    "++++++++++",
    "+++++++++++",
    "++++++++++++",
    "+++++++++++++",
    "++++++++++++++",
    "+++++++++++++++",
    "++++++++++++++++",
    "+++++++++++++++++",
    "++++++++++++++++++",
    "+++++++++++++++++++",
    "++++++++++++++++++++",
    "+++++++++++++++++++++",
    "++++++++++++++++++++++",
    };

    public static string[] substract = {
    "",
    "-",
    "--",
    "---",
    "----",
    "-----",
    "------",
    "-------",
    "--------",
    "---------",
    "----------",
    "-----------",
    "------------",
    "-------------",
    "--------------",
    "---------------",
    "----------------",
    "-----------------",
    "------------------",
    "-------------------",
    "--------------------",
    "---------------------",
    "----------------------",
    };

    public static string[] goLeft = {
    "",
    "<",
    "<<",
    "<<<",
    "<<<<",
    "<<<<<",
    "<<<<<<",
    "<<<<<<<",
    "<<<<<<<<",
    "<<<<<<<<<",
    "<<<<<<<<<<",
    "<<<<<<<<<<<",
    "<<<<<<<<<<<<",
    "<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<<<<<<",
    "<<<<<<<<<<<<<<<<<<<<<<",
    };

    #endregion

    const int MaxTake = 6;
    const int TreeDeph = 5;
    static string MagicPhrase = "THREE RINGS FOR THE ELVEN KINGS UNDER THE SKY SEVEN FOR THE DWARF LORDS IN THEIR HALLS OF STONE NINE FOR MORTAL MEN DOOMED TO DIE ONE FOR THE DARK LORD ON HIS DARK THRONEIN THE LAND OF MORDOR WHERE THE SHADOWS LIE ONE RING TO RULE THEM ALL ONE RING TO FIND THEM ONE RING TO BRING THEM ALL AND IN THE DARKNESS BIND THEM IN THE LAND OF MORDOR WHERE THE SHADOWS LIE";
    //public static string MagicPhrase = Console.ReadLine(); // "UMNE TALMAR RAHTAINE NIXENEN UMIR";
    static void Main(string[] args)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        StringBuilder resultBuilder = new StringBuilder();
        char[] runes = new char[30];
        for (int i = 0; i < 30; i++)
            runes[i] = ' ';


        Gamestate beginning = new Gamestate()
        {
            CurrentRunes = runes,
            TotalStringLength = 0,
            PlayerPosition = 0,
        };

        Node rootNode = new Node()
        {
            State = beginning,
            Parent = null
        };

        int idx = 0;
        while (idx < MagicPhrase.Length - TreeDeph)
        {
            var shortestNode = FillTree(rootNode, idx, idx + TreeDeph);
            idx += TreeDeph;
            resultBuilder.Append(shortestNode.State.TotalString);
            rootNode = new Node
            {
                Parent = null,
                State = new Gamestate
                {
                    CurrentRunes = shortestNode.State.CurrentRunes,
                    PlayerPosition = shortestNode.State.PlayerPosition,
                    TotalStringLength = 0,
                    TotalString = ""
                }
            };
        }

        var finalNode = FillTree(rootNode, idx, MagicPhrase.Length);
        resultBuilder.Append(finalNode.State.TotalString);

        sw.Stop();
        Console.Error.WriteLine(sw.ElapsedMilliseconds);
        Console.Error.WriteLine(resultBuilder.Length);
        Console.WriteLine(resultBuilder.ToString());
        //Console.WriteLine(shortestNode.State.TotalString.ToString());
        Console.ReadKey();
    }

    private static Node FillTree(Node thisNode, int currentPhraseIndex, int length)
    {
        if (currentPhraseIndex == length)
            return thisNode;

        
        char solveForLetter = MagicPhrase[currentPhraseIndex];
        thisNode.Childrean = GetPossibleSolutions(solveForLetter, thisNode.State)
            .Select(x => new Node { State = x, Parent = thisNode })
            .ToArray();

        Node shortestNode = null;

        foreach(var childNode in thisNode.Childrean)
        {
            PermCounter++;
            var result = FillTree(childNode, currentPhraseIndex + 1, length);
            if (result != null)
            {
                if (shortestNode == null || result.State.TotalStringLength < shortestNode.State.TotalStringLength)
                {
                    shortestNode = result;
                }
            }
        };

        return shortestNode;
    }

    private static Gamestate[] GetPossibleSolutions(char letter, Gamestate currentState)
    {
        List<Gamestate> possibleResultingGamestate = new List<Gamestate>();

        for (int i = 0; i < 30; i++)
        {
            string moveTo = GetMoveToRune(currentState.PlayerPosition, i);
            string changeTo = ChangeAndHitRune(i, letter, currentState.CurrentRunes, out char[] newRunes);

            Gamestate gamestate = new Gamestate()
            {
                CurrentRunes = newRunes,
                TotalStringLength = currentState.TotalStringLength + moveTo.Length + changeTo.Length,
                TotalString = currentState.TotalString + moveTo + changeTo,
                PlayerPosition = i
            };
            possibleResultingGamestate.Add(gamestate);
        }

        return possibleResultingGamestate.OrderBy(x => x.TotalStringLength).Take(MaxTake).ToArray();
    }

    public static string ChangeAndHitRune(int i, char nextLetter, char[] runes, out char[] newRunes)
    {
        int indexDestinationLetter = 0;
        while (runeValues[indexDestinationLetter] != nextLetter)
            indexDestinationLetter++;

        int indexCurrentLetter = 0;
        while (runeValues[indexCurrentLetter] != runes[i])
            indexCurrentLetter++;

        int offsetForwards = 0;
        int offsetBackwards = 0;

        if (indexDestinationLetter > indexCurrentLetter)
        {
            offsetForwards = Math.Abs(indexDestinationLetter - indexCurrentLetter);
            offsetBackwards = runeValues.Length - Math.Abs(indexCurrentLetter - indexDestinationLetter);
        } else
        {
            offsetForwards = runeValues.Length - Math.Abs(indexCurrentLetter - indexDestinationLetter); 
            offsetBackwards = Math.Abs(indexDestinationLetter - indexCurrentLetter);
        }
        newRunes = (char[])runes.Clone();
        newRunes[i] = nextLetter;

        if (offsetBackwards > offsetForwards)
            return add[offsetForwards] + '.';
        return substract[offsetBackwards] + '.';
    }   

    public static string GetMoveToRune(int currentPosition, int destinationPos)
    {
        int offsetForwards = 0;
        int offsetBackwards = 0;

        if (destinationPos > currentPosition)
        {
            offsetForwards = Math.Abs(destinationPos - currentPosition);
            offsetBackwards = 30 - Math.Abs(currentPosition - destinationPos);
        }
        else
        {
            offsetForwards = 30 - Math.Abs(currentPosition - destinationPos);
            offsetBackwards = Math.Abs(destinationPos - currentPosition);
        }

        if (offsetBackwards > offsetForwards)
            return goRight[offsetForwards];

        return goLeft[offsetBackwards];
    }
}