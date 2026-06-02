namespace Core.Interfaces;

using System;

public class ScoreChangedEventArgs : EventArgs
{
    public int PreviousScore { get; }
    public int NewScore { get; }

    public ScoreChangedEventArgs(int p_previousScore, int p_newScore)
    {
        PreviousScore = p_previousScore;
        NewScore = p_newScore;
    }
}
