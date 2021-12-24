using Godot;
using System.Collections.Generic;
using System.Linq;

public enum MotuLadder
{
    None,
    PartComplete,
    Complete
}

/// <summary>
/// Add player properties here. Rename the class and file if you wish
/// </summary>
public class MotuPlayer : PinGodPlayer
{
    public MotuPlayer()
    {
        CreatePlayerLadder();
    }

    private void CreatePlayerLadder()
    {
        MotuLadderComplete = new Dictionary<string, MotuLadder>
    {
        { "beastman", MotuLadder.None },
        { "stratos", MotuLadder.None },
        { "manatarms", MotuLadder.None },
        { "orko", MotuLadder.None },
        { "ripper", MotuLadder.None },
        { "skeletor", MotuLadder.None },
        { "grayskull", MotuLadder.None },
        { "roton", MotuLadder.None },
        { "adam", MotuLadder.None },
        //{ "motu", MotuLadder.None },
        };
    }

    bool _beastShotDirectionRight = true;
    public bool[] AdamModesRunning { get; internal set; } = new bool[3];
    public bool[] AdamTargets { get; set; } = new bool[4];
    public int BankedCombos { get; private set; }
    public bool[] BeastActiveTargets { get; set; } = new bool[6];
    public int BeastManScore { get; set; } = MotuConstant._275K;
    public bool GrayskullModeEnded { get; internal set; }
    public bool GrayskullModeEnding { get; internal set; }
    public bool[] GrayskullProgress { get; set; } = new bool[6];
    public bool HasPlayedSorceress { get; set; }
    public int IHavePowerComboAmount { get; set; }
    public bool IsAdamDoubleScoring { get; set; }
    public bool IsAdamSuperBumperRunning { get; set; }
    public bool IsAdamSuperSpinnersRunning { get; set; }
    public bool IsBeastModeRunning { get; set; }

    public bool IsComboAvailable { get; internal set; }

    public bool IsFastScoringRunning { get; set; }

    public bool IsGraySkullMultiballModeEnded { get; internal set; }

    public bool IsGrayskullMultiballRunning { get; set; }

    public bool IsGraySkullReady { get; set; }

    public bool IsManAtArmsReady { get; set; }

    public bool IsManAtArmsRunning { get; set; }

    public bool IsMotuMultiballRunning { get; set; }

    public bool IsMysteryReady { get; set; }

    public bool IsRipperModeEnded { get; internal set; }

    public bool IsRipperModeEnding { get; internal set; }

    public bool IsRipperMultiballReady { get; set; }

    public bool IsRipperMultiballRunning { get; set; }

    public bool IsRotonReady { get; set; }

    public bool IsSkeleMultiballEnded { get; internal set; }

    public bool IsSkeleMultiballEnding { get; internal set; }

    public bool IsSkeleMultiballRunning { get; set; }

    public bool IsSkillshotRunning { get; set; }

    public bool IsSorceressComplete { get; set; }

    public bool IsSorceressRunning { get; set; }

    public bool IsSpecialLit { get; internal set; }

    public bool IsStratosRunning { get; set; }

    public bool IsWizardReady { get; set; }

    /// <summary>
    /// lane shots
    /// </summary>
    public bool[] ManAtArms { get; set; } = new bool[3];

    public int ManAtArmsBonusLevel { get; internal set; } = MotuConstant._50K;

    public int ManAtArmsLevel { get; internal set; } = 0;

    /// <summary>
    /// The progress when manatarms is running
    /// </summary>
    public bool[] ManAtArmsProgress { get; set; } = new bool[4];

    public int ManAtArmsRunningScore { get; internal set; } = MotuConstant._250K;

    public Dictionary<string, MotuLadder> MotuLadderComplete { get; set; }

    internal bool IsSorceressReady() => IsWizardReady && !IsSorceressRunning && !IsSorceressComplete && !HasPlayedSorceress;

    public bool[] OrkoRunningShots { get; internal set; } = new bool[4];

    public bool[] OrkoShots { get; set; } = new bool[4];

    public int OrkoTimesToComplete { get; internal set; }

    public bool RipperDoubleSuperjackpotReady { get; internal set; }

    public int RipperDoubleSuperJackpotsCollected { get; internal set; }

    public bool[] RipperShots { get; set; } = new bool[6];

    public int RippersOpen { get; set; }

    public bool RipperSuperjackpotReady { get; internal set; }

    public bool[] RotonTargets { get; internal set; } = new bool[4];

    internal void ResetAfterCompletion()
    {
        IsWizardReady = false;
        IsSorceressComplete = false;
        ResetSkeletorMultiball();
        IsMotuMultiballRunning = false;
        IsRotonReady = false;
        IsGraySkullReady = false;
        IsRipperMultiballReady = false;
        IsManAtArmsReady = false;
        ManAtArms = new bool[3];
        AdamTargets = new bool[4];
        CreatePlayerLadder();
        ResetSkeletorMultiball();
        HasPlayedSorceress = false;
        OrkoShots = new bool[4];
        
    }

    public int SkeletorHits { get; set; }

    /// <summary>
    /// Amount of hits needed to start the multiball. Starts at 7 max at 10
    /// </summary>
    public int SkeletorHitTarget { get; set; } = 7;

    public int SkeletorMultiballsComplete { get; internal set; }

    public int SkeletorShotCnt { get; set; } = 6;

    public bool SkillshotAwarded { get; set; }

    public int SkillshotRampLeftAward { get; set; } = MotuConstant._250K;

    public int SkillshotRampRightAward { get; set; } = MotuConstant._300K;

    public bool SkillshotRightRampOn { get; set; }

    public int SkillshotScoopAward { get; set; } = MotuConstant._500K;

    public int StratosScore { get; set; } = MotuConstant._750K;

    /// <summary>
    /// Shots needed to start the stratos mode. Starts on 3 then increments by 2 each stage
    /// </summary>
    public int StratosShots { get; private set; }

    /// <summary>
    /// Current stage, increment when stratos is played
    /// </summary>
    public int StratosStage { get; private set; } = 3;

    /// <summary>
    /// Starts at 10K
    /// </summary>
    public long SuperBumperScore { get; set; } = MotuConstant._10K;

    public int TeelaCount { get; set; }

    /// <summary>
    /// Advances the progress to GraySkullMultiBall and sets IsGraySkullReady
    /// </summary>
    public void AdvanceGraySkull()
    {
        if (!IsGrayskullMultiballRunning && !IsMotuMultiballRunning && !IsSkeleMultiballRunning && !IsGraySkullReady)
        {
            if (GrayskullProgress.Any(x => !x))
            {
                for (int i = 0; i < GrayskullProgress.Length; i++)
                {
                    if (!GrayskullProgress[i])
                    {
                        GrayskullProgress[i] = true;
                        break;
                    }
                }

                if (!GrayskullProgress.Any(x => !x))
                    IsGraySkullReady = true;
            }
            else
            {
                IsGraySkullReady = true;
            }
        }
    }

    /// <summary>
    /// Advances stratos shots if stratos not running. Sets <see cref="IsStratosRunning"/>
    /// </summary>
    public void AdvanceStratos()
    {
        if (!IsStratosRunning)
        {
            StratosShots++;
            if (StratosShots == StratosStage)
            {
                StratosStage += 2; //increment shots needed
                StratosShots = 0;
                IsStratosRunning = true;
            }
        }
    }

    public void ChangeActiveBeastTarget()
    {
        if (_beastShotDirectionRight && !BeastActiveTargets[5])
        {
            var lastNum = BeastActiveTargets[BeastActiveTargets.Length - 1];
            for (int i = BeastActiveTargets.Length - 1; i > 0; i--)
            {
                BeastActiveTargets[i] = BeastActiveTargets[i - 1];
            }
            BeastActiveTargets[0] = lastNum;

            //change direction if on last shot
            _beastShotDirectionRight = !BeastActiveTargets[5];
        }
        else
        {
            var firstNum = BeastActiveTargets[0];
            for (int i = 0; i < BeastActiveTargets.Length - 1; i++)
            {
                BeastActiveTargets[i] = BeastActiveTargets[i + 1];
            }
            BeastActiveTargets[BeastActiveTargets.Length - 1] = firstNum;

            _beastShotDirectionRight = firstNum;
        }
    }

    public bool CheckAndSetWizardMode()
    {
        if (IsSorceressRunning || IsWizardReady)
        {
            return true;
        }

        if (!MotuLadderComplete.ContainsValue(MotuLadder.None))
        {
            //all modes completed, are they part or full
            if (!MotuLadderComplete.ContainsValue(MotuLadder.PartComplete))
            {
                //fully completed                
                IsSorceressComplete = true;
            }

            IsWizardReady = true;
        }

        return true;
    }

    /// <summary>
    /// Sets the combo to ready.If already running then the combo amount is added
    /// </summary>
    /// <returns>true if combo already running. start a timer when return false</returns>
    public bool CheckForComboStatus()
    {
        if (!IsComboAvailable)
        {
            BankedCombos = 0;
            IsComboAvailable = true;
            return false;
        }
        else
        {
            IHavePowerComboAmount += 250000;
            BankedCombos++;
            return true;
        }
    }

    /// <summary>
    /// Progresses a ripper shot, sets mutliball running if all completed
    /// </summary>
    /// <param name="num"></param>
    /// <returns>true if shot target was success</returns>
    public bool CheckForRipperShot(int num)
    {
        if (IsRipperMultiballReady) return false;
        if (IsRipperMultiballRunning || IsSkeleMultiballRunning) return false;
        if (IsMotuMultiballRunning || IsSorceressRunning) return false;
        if (!RipperShots[num])
        {
            RipperShots[num] = true;
            CheckRipperMultiballReady();
            return true;
        }

        return false;
    }

    public bool IsAnyMultiballRunning() => IsSkeleMultiballRunning || IsRipperMultiballRunning || IsGrayskullMultiballRunning;

    /// <summary>
    /// Is the given mode fully completed?
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool IsModeCompleted(string mode) => MotuLadderComplete[mode] == MotuLadder.Complete;

    public void ResetBeastmanShots() => BeastActiveTargets = new bool[6];

    /// <summary>
    /// Sets the ladder for beastman and increments the <see cref="BeastManScore"/>
    /// </summary>
    public void SetBeastmanFound()
    {
        BeastManScore += MotuConstant._500K;
        SetLadder("beastman", MotuLadder.Complete);
        IsBeastModeRunning = false;
    }

    /// <summary>
    /// Sets a mode to complete or part complete. todo: play sound. Sorceress_WasteNoTime when sorcereess ready
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="ladder"></param>
    public bool SetLadder(string mode, MotuLadder ladder)
    {
        if (IsWizardReady) return true;

        if (MotuLadderComplete[mode] < ladder)
            MotuLadderComplete[mode] = ladder;

        return CheckAndSetWizardMode();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fullComplete"></param>
    public void SetWizardReady(bool fullComplete)
    {
        IsWizardReady = true;
        if (fullComplete)
        {
            IsSorceressComplete = true;
        }
    }

    /// <summary>
    /// Sets up beastman mode
    /// </summary>
    public void StartBeastman()
    {
        IsBeastModeRunning = true;
        ResetBeastmanShots();
        BeastActiveTargets[0] = true;
        SetLadder("beastman", MotuLadder.PartComplete);
    }

    public void StartDoubleScoring()
    {
        IsAdamDoubleScoring = true;
    }

    /// <summary>
    /// Reset available combos and all banked
    /// </summary>
    internal void ResetCombo()
    {
        IsComboAvailable = false;
        BankedCombos = 0;
    }

    internal void ResetOrkoShots()
    {
        OrkoShots = new bool[4];
    }

    internal void ResetRunningManAtArms()
    {
        IsManAtArmsRunning = false;
        IsManAtArmsReady = false;
        ManAtArmsProgress = new bool[4];
    }

    /// <summary>
    /// After a multiball has been played, reset all the hits and set the new target
    /// </summary>
    internal void ResetSkeletorMultiball()
    {
        IsSkeleMultiballRunning = false;
        SkeletorHits = 0;
        IsRotonReady = false;
        IsSkeleMultiballEnding = false;
        IsSkeleMultiballEnded = false;
    }

    /// <summary>
    /// Checks the ripper shots and sets the <see cref="IsRipperMultiballReady"/>
    /// </summary>
    private void CheckRipperMultiballReady()
    {
        if (RippersOpen == 0)
        {
            if (RipperShots.Count(x => x) >= 4)
            {
                SetRipperStarted();
            }
        }
        else if (RipperShots.Any(x => !x)) //all complete, 6
        {
            SetRipperStarted();
        }
    }

    private void SetRipperStarted()
    {
        IsRipperMultiballReady = true;
        RippersOpen++;
    }
}
