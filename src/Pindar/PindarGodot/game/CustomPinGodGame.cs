public class CustomPinGodGame : PinGodGame
{
    /// <summary>
    /// override to create our own player type for this game
    /// </summary>
    /// <param name="name"></param>
    public override void CreatePlayer(string name)
    {
        Players.Add(new PindarPlayer() { Name = name, Points = 0 });
    }

    public PindarPlayer PindarPlayer => Player as PindarPlayer;

    public void PlayMusicForBonus()
    {
        if(PindarPlayer.BonusCount < 5)
        {            
            if (AudioManager.CurrentMusic != "pindar-intro" || !AudioManager.IsMusicPlaying())
                PlayMusic("pindar-intro");
        }
        else if(PindarPlayer.BonusCount < 10)
        {
            if (AudioManager.CurrentMusic != "pindar-1" || !AudioManager.IsMusicPlaying())
                PlayMusic("pindar-1");
        }
        else if (PindarPlayer.BonusCount < 15)
        {
            if (AudioManager.CurrentMusic != "pindar-2" || !AudioManager.IsMusicPlaying())
                PlayMusic("pindar-2");                
        }
        else if (PindarPlayer.BonusCount < 22)
        {
            if (AudioManager.CurrentMusic != "pindar-3" || !AudioManager.IsMusicPlaying())
                PlayMusic("pindar-3");
        }
        else
        {
            if (AudioManager.CurrentMusic != "pindar-4" || !AudioManager.IsMusicPlaying())
                PlayMusic("pindar-4");
        }
    }
}