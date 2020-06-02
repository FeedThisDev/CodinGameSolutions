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


public class Match
{
    public int Index;
    public int Length;
    public int Repetitions;
}

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

    static int MaxTake = 1;
    static int TreeDeph = 1;
    static int SurroundingRadius = 15;

    static Dictionary<int, Match> FoundMatches;

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


        //MagicPhrase = "THREE RINGS FOR THE ELVEN KINGS UNDER THE SKY SEVEN FOR THE DWARF LORDS IN THEIR HALLS OF STONE NINE FOR MORTAL MEN DOOMED TO DIE ONE FOR THE DARK LORD ON HIS DARK THRONEIN THE LAND OF MORDOR WHERE THE SHADOWS LIE ONE RING TO RULE THEM ALL ONE RING TO FIND THEM ONE RING TO BRING THEM ALL AND IN THE DARKNESS BIND THEM IN THE LAND OF MORDOR WHERE THE ABABABABABABABABABABASHADOWS LIE";
        MagicPhrase = "CABCABCABCACABC"; // Console.ReadLine(); // "UMNE TALMAR RAHTAINE NIXENEN UMIR";

        FoundMatches = FindRepeatingSequences(MagicPhrase).ToDictionary(key => key.Index, val => val);

        TreeDeph = MagicPhrase.Length;

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
        Console.Error.WriteLine($"{MaxTake} {TreeDeph} {SurroundingRadius}: {sw.ElapsedMilliseconds}ms {resultBuilder.Length}");
        //Console.Error.WriteLine(PermCounter);
        Console.Error.WriteLine(resultBuilder.Length);
        Console.WriteLine(resultBuilder.ToString());
        //        }
        //    }
        //}

        //Console.WriteLine(shortestNode.State.TotalString.ToString());
        Console.ReadKey();
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
        thisNode.Childrean = GetPossibleSolutions(solveForLetter, thisNode.State, ref currentPhraseIndex)
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

    private static Gamestate[] GetPossibleSolutions(char letter, Gamestate currentState,ref int currentPhraseIndex, bool AllowCompressed = true)
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

        if(AllowCompressed && FoundMatches.ContainsKey(currentPhraseIndex))
        {
            List<Gamestate> result = possibleResultingGamestate.OrderBy(x => x.TotalStringLength).Take(MaxTake).ToList();

            Gamestate compressedSolution = GetCompressed(letter, currentState, ref currentPhraseIndex);

            result.Add(compressedSolution);

            return result.ToArray();

        } else
        {
            return possibleResultingGamestate.OrderBy(x => x.TotalStringLength).Take(MaxTake).ToArray();
        }

    }

    private static Gamestate GetCompressed(char letter, Gamestate currentState, ref int currentPhraseIndex)
    {
        var match = FoundMatches[currentPhraseIndex];

        var childrean = GetPossibleSolutions2(MagicPhrase.Substring(match.Index, match.Length), letter, currentState, ref currentPhraseIndex);


    }

    private static Gamestate[] GetPossibleSolutions2(string word, char letter, Gamestate currentState, ref int currentPhraseIndex)
    {
        List<Gamestate> possibleResultingGamestate = new List<Gamestate>();

        foreach (var i in GetSurroundingPositions(currentState.PlayerPosition))
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

    private static string CompressWith(int runeToBeChanged, int i, char letter, char[] currentRunes, out char[] newRunes)
    {
        throw new NotImplementedException();
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

    private static List<Match> FindRepeatingSequences(string str)
    {
        List<Match> foundmatches = new List<Match>();

        // Create a new suffix array
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = i + 1; j < str.Length; j++)
            {
                int matches = 0;
                string subString = str.Substring(i, j - i);
                int k = i + (j - i);
                Match foundMatch = null;
                while (true)
                {
                    if (k + (j - i) > str.Length)
                        break;

                    string subString2 = str.Substring(k, j - i);
                    if (subString2.Equals(subString))
                    {
                        matches++;
                        k += (j - i);
                        foundMatch = new Match() { Index = i, Length = (j - i), Repetitions = matches };
                    }
                    else
                    {
                        break;
                    }
                }
                if (foundMatch != null)
                {
                    if (foundMatch.Length == 1 && foundMatch.Repetitions < 5)
                        break;
                    if (foundMatch.Length == 2 && foundMatch.Repetitions < 4)
                        break;


                    foundmatches.Add(foundMatch);
                    i += foundMatch.Length * foundMatch.Repetitions;
                    break;
                }
            }
        }

        return foundmatches;
    }

}