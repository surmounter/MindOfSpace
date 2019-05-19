using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchGameSlow :MonoBehaviour, CatchGameStarProperty {

    [SerializeField]
    EffectPool _effect;                                                                                         /// 특수능력(순간이동) 발동시 나타나는 이펙트 (오브젝트 풀링을 이용)

    public CatchGameStarTriggerType TriggerType { get; private set; }     /// 별의 속성(특수능력)이 발동되는 순간
    const float _downSpeedAmount = 1.0f;                                                   /// 플레이어의 속도를 얼마나 낮출지

    /// <summary>
    /// 게임시작시, 단 한번만 초기화해줘도 되는 부분을 이 함수에서 처리한다.
    /// </summary>
    void Awake()
    {
        TriggerType = CatchGameStarTriggerType.OnEnter; /// 플레이어가 별과 접촉하면 특수능력이 발동된다.
    }

    /// <summary>
    /// 플레이어가 게임에 실패하여 재도전할때마다, 초기화해줘야 하는 부분을 이 함수에서 처리한다.
    /// 별다르게 초기화해줘야하는 부분이 없다면 비워둔다. (함수 자체를 없애면 CatchGameStarProperty 인터페이스를 상속받을 수 없다.)
    /// </summary>
    public void Initialize()
    {
            
    }

    /// <summary>
    /// 특수능력(플레이어의 속도감소)를 처리하는 함수이다.
    /// </summary>
    /// <param name="target"> 특수능력을 적용할 대상 (플레이어) </param>
    /// <param name="triggerType"> 유효성 검사 </param>
    public void PlayEffect(GameObject target, CatchGameStarTriggerType triggerType)
    {
        if (triggerType != TriggerType) return;
        target.GetComponent<DogPlayerController>().OnEnterSlowDownStar(_downSpeedAmount);
        _effect.ShowEffect(transform.position + Vector3.up * 2.0f);
    }
}
