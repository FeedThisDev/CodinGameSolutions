using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{

    static char[] runeValues = " ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    static char[] runes = new char[30];
    static void Main(string[] args)
    {
        for (int i = 0; i < 30; i++)
            runes[i] = ' ';

        string magicPhrase = "AZ";// Console.ReadLine();//"UMNE TALMAR RAHTAINE NIXENEN UMIR";// 
        //Dictionary<char, int> letterToIndex = new Dictionary<char, int>();

        runes.AsParallel().ForAll(x => x = ' ');

        StringBuilder result = new StringBuilder();

        int currentPosition = 0;

        for(int i = 0; i < magicPhrase.Length; i++)
        {
            char nextLetter = magicPhrase[i];
            string printThis = GetShortestLetterSolution(nextLetter, ref currentPosition, ref runes);
            result.Append(printThis);
        }

        Console.WriteLine(result.ToString());
    }

    public struct Gamestate
    {
        public int DecisionMade { get; set; }
        public int CurrentStringLength { get; set; }
        public char[] CurrentRunes { get; set; }
        public int PlayerPosition { get; set; }
    }

    private static Gamestate[] GetPossibleSolutions(char nextLetter, int currentPosition, char[] runes)
    {
        List<Gamestate> possibleResultingGamestate = new List<Gamestate>();
        //Möglichkeit 1-30: Letter verändern und nehmen
        int newPositionShortest = 0;
        char[] shortestNewRuins = null;
        string shortestString = null;
        for(int i = 0; i < 30; i++)
        {
            int newPosition = 0;
            char[] newRunes = null;
            string moveTo = GetMoveToRune(currentPosition, i, out newPosition);
            string changeTo = ChangeAndHitRune(i, nextLetter, out newRunes);
            Gamestate gamestate = new Gamestate()
            {
                CurrentRunes = newRunes,
                CurrentStringLength = moveTo.Length + changeTo.Length,
                PlayerPosition = newPosition,
                DecisionMade = i
            };
            possibleResultingGamestate.Add(gamestate);
        }

        return possibleResultingGamestate.ToArray();
    }

    private static string ChangeAndHitRune(int i, char nextLetter, out char[] newRunes)

    {         StringBuilder result = new StringBuilder();
        int indexDestinationLetter = 0;
        while (runeValues[indexDestinationLetter] != nextLetter)
            indexDestinationLetter++;

        int indexCurrentLetter = 0;
        while (runeValues[indexCurrentLetter] != runes[i])
            indexCurrentLetter++;

        int offset = indexDestinationLetter - indexCurrentLetter;

        if(Math.Abs(offset) > (runeValues.Length / 2)){
            offset = -( runeValues.Length - offset );
        }
        char manipChar = offset < 0 ? '-' : '+';
        for(int j = 0; j< Math.Abs(offset);j++)
            result.Append(manipChar);
        result.Append('.');
        newRunes = (char[]) runes.Clone();
        newRunes[i] = nextLetter;

        return result.ToString();
    }   

    private static string GetMoveToRune(int currentPosition, int destinationPos, out int newPosition)
    {
        StringBuilder goRight = new StringBuilder();
        //go right
        int curPos = currentPosition;

        while(curPos != destinationPos)
        {
            goRight.Append(">");
            curPos++;
            if (curPos == 30)
                curPos = 0;
        }

        //go left
        StringBuilder goLeft = new StringBuilder();
        curPos = currentPosition;
        while(curPos != destinationPos)
        {
            goLeft.Append('<');
            curPos--;
            if (curPos == -1)
                curPos = 29;
        }

        newPosition = destinationPos;

        if (goLeft.Length < goRight.Length)
            return goLeft.ToString();
        return goRight.ToString();
    }
}