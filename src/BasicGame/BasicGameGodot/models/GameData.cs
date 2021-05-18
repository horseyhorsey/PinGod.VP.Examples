using System;
using System.Collections.Generic;

public class GameData
{
    public int BallsPlayed { get; set; }
    public int BallsStarted { get; set; }
    public byte Credits { get; set; }
    public int GamesFinished { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public List<HighScore> HighScores { get; set; } = new List<HighScore>();
    public int Tilted { get; set; }
    public uint TimePlayed { get; set; }
}

public class HighScore
{
    public string Name { get; set; }
    public string Title { get; set; }
    public long Scores { get; set; }
    public DateTime Created { get; set; }
}
