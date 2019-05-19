using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchGameEventPublisher : MonoBehaviour {

    public static CatchGameEventPublisher instance = null;

    public delegate void ChangePlayerSize(DogPlayerModel sender);
    public event ChangePlayerSize ChangePlayerSizeEvent;

    public delegate void PlayerAttackSheep(DogPlayerController sender);
    public event PlayerAttackSheep PlayerAttackSheepEvent;

    public delegate void PlayerHitBySheep(DogPlayerController sender);
    public event PlayerHitBySheep PlayerHitBySheepEvent;

    public delegate void AppearStar(CatchGameModel sender);
    public event AppearStar AppearStarEvent;

    public delegate void DisappearStar(CatchGameModel sender);
    public event DisappearStar DisappearStarEvent;

    public delegate void Restart(CatchGameController sender);
    public event Restart RestartEvent;

    public delegate void GetStar(CatchGameStar sender);
    public event GetStar GetStarEvent;

    void Awake()
    {
        instance = this;    
    }

    public void DoChangePlayerSize(DogPlayerModel sender)
    {
        if (ChangePlayerSizeEvent != null)
            ChangePlayerSizeEvent(sender);
    }

    public void DoPlayerAttackSheep(DogPlayerController sender)
    {
        if (PlayerAttackSheepEvent != null)
            PlayerAttackSheepEvent(sender);
    }

    public void DoPlayerHitBySheep(DogPlayerController sender)
    {
        if (PlayerHitBySheepEvent != null)
            PlayerHitBySheepEvent(sender);
    }

    public void DoAppearStar(CatchGameModel sender)
    {
        if (AppearStarEvent != null)
            AppearStarEvent(sender);
    }

    public void DoDisappearStar(CatchGameModel sender)
    {
        if (DisappearStarEvent != null)
            DisappearStarEvent(sender);
    }

    public void DoRestart(CatchGameController sender)
    {
        if (RestartEvent != null)
            RestartEvent(sender);
    }

    public void DoGetStar(CatchGameStar sender)
    {
        if (GetStarEvent != null)
            GetStarEvent(sender);
    }
}
