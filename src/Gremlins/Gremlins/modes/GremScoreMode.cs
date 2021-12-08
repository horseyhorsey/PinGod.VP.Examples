using Godot;

namespace Gremlins.modes
{
    public class GremScoreMode : ScoreMode
    {
        public override void GetPlayerScoreLabels()
        {
            for (int i = 0; i < ScoreLabels.Length; i++)
            {
                var name = $"ScoreP{i + 1}";
                ScoreLabels[i] = this.GetNode($"{name}/{name}") as Label;
                ScoreLabels[i].Text = string.Empty;
            }
        }
    }
}
