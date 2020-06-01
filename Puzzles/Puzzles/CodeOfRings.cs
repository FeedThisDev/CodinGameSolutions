using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/


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

    static string MagicPhrase;
    static int PermCounter = 0;
    static char[] runeValues = " ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    static Dictionary<char, int> runeLetterToIndex = new Dictionary<char, int>();

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

    static int MaxTake = 4;
    static int TreeDeph = 6;
    static int SurroundingRadius = 4;

    static void Main(string[] args)
    {

        for (int i = 0; i < runeValues.Length; i++)
        {
            runeLetterToIndex.Add(runeValues[i], i);
        }

        //for (int j = 1; j < 7; j++)
        //{
        //    for (int jj = 1; jj < 7; jj++)
        //    {
        //        for (int jjj = 1; jjj < 7; jjj++)
        //        {


                     //MaxTake = j;
                     //TreeDeph = jj;
                     //SurroundingRadius = jjj;


                    Stopwatch sw = new Stopwatch();
        sw.Start();
        StringBuilder resultBuilder = new StringBuilder();
        char[] runes = new char[30];
        for (int i = 0; i < 30; i++)
            runes[i] = ' ';


        //MagicPhrase = "THREE RINGS FOR THE ELVEN KINGS UNDER THE SKY SEVEN FOR THE DWARF LORDS IN THEIR HALLS OF STONE NINE FOR MORTAL MEN DOOMED TO DIE ONE FOR THE DARK LORD ON HIS DARK THRONEIN THE LAND OF MORDOR WHERE THE SHADOWS LIE ONE RING TO RULE THEM ALL ONE RING TO FIND THEM ONE RING TO BRING THEM ALL AND IN THE DARKNESS BIND THEM IN THE LAND OF MORDOR WHERE THE SHADOWS LIE";
        MagicPhrase = Console.ReadLine(); // "UMNE TALMAR RAHTAINE NIXENEN UMIR";


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
                    //Console.Error.WriteLine($"{MaxTake} {TreeDeph} {SurroundingRadius}: {sw.ElapsedMilliseconds}ms {resultBuilder.Length}");
                    //Console.Error.WriteLine(PermCounter);
                    //Console.Error.WriteLine(resultBuilder.Length);
                    Console.WriteLine(resultBuilder.ToString());
        //        }
        //    }
        //}

        //Console.WriteLine(shortestNode.State.TotalString.ToString());
        //Console.ReadKey();
    }

    private static List<int> GetSurroundingPositions(int playerPosition)
    {
        List<int> result = new List<int>();
        for (int i = playerPosition - SurroundingRadius; i < playerPosition + SurroundingRadius; i++)
        {
            if (i < 0)
            {
                result.Add(i + 30);
            }
            else if(i >= 30)
            {
                result.Add(i - 30);
            }
            else
            {
                result.Add(i);
            }
        }
        return result;
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

        foreach (var childNode in thisNode.Childrean)
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

        foreach(var i in GetSurroundingPositions(currentState.PlayerPosition))
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
        int offset = GetLetterDifference(nextLetter, runes[i]);
        newRunes = (char[])runes.Clone();
        newRunes[i] = nextLetter;

        if (offset >= 0)
            return add[offset] + '.';
        return substract[-offset] + '.';
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

    public static int GetLetterDifference(char letter1,char letter2)
    {
        int indexCurrentLetter = runeLetterToIndex[letter2];
        int indexDestinationLetter = runeLetterToIndex[letter1];

        int offsetForwards = 0;
        int offsetBackwards = 0;

        if (indexDestinationLetter > indexCurrentLetter)
        {
            offsetForwards = Math.Abs(indexDestinationLetter - indexCurrentLetter);
            offsetBackwards = runeValues.Length - Math.Abs(indexCurrentLetter - indexDestinationLetter);
        }
        else
        {
            offsetForwards = runeValues.Length - Math.Abs(indexCurrentLetter - indexDestinationLetter);
            offsetBackwards = Math.Abs(indexDestinationLetter - indexCurrentLetter);
        }

        if (offsetBackwards > offsetForwards)
            return offsetForwards;
        return -offsetBackwards;
    }

}