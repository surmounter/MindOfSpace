using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 근처에 다가가면 특정확률로 순간이동하는 특수능력을 필드아이템인 별에 부여한다.
/// </summary>
public class CatchGameTeleport : MonoBehaviour, CatchGameStarProperty {

    [SerializeField]
    EffectPool _effect;                                                                                           /// 특수능력(순간이동) 발동시 나타나는 이펙트 (오브젝트 풀링을 이용)
    /* 오브젝트 풀링
     * 가능하면 최대한 오브젝트를 생성하는 작업을 게임 중간에 하지 않도록 한다. 이를 위해서 오브젝트 풀링 방식을 애용하였다.
     * 왜냐하면 오브젝트를 생성하는 작업은 굉장히 비싸기 때문이며, 
     * 비싼작업을 게임시작 로딩타임에 몰아서 게임 중간에 프레임이 튀는 현상(랙)을 방지하기 위함이다.
     */
    public CatchGameStarTriggerType TriggerType { get; private set; }       /// 별의 특수능력이 발동되는 순간
    const float _initTeleportSuccessPercent = 70.0f;                                      /// 순간이동 초기성공확률 (순간이동할수록 성공확률이 낮아진다.)
    float _teleportSuccessPercent;                                                                    /// 현재 순간이동 성공확률
    CatchGameStageEnvSet _stageEnvSet;                                                      /// 현재 맵 정보

    /// <summary>
    /// 게임시작시, 단 한번만 초기화해줘도 되는 부분을 이 함수에서 처리한다.
    /// </summary>
    void Awake()
    {
        TriggerType = CatchGameStarTriggerType.OnSight; /// 별의 시야에 플레이어가 들어오면, 특수능력(순간이동)이 발동된다.
        _teleportSuccessPercent = _initTeleportSuccessPercent;
        _stageEnvSet = GameObject.FindWithTag("StageEnv").GetComponent<CatchGameStageEnvSet>();                    
    }

    /// <summary>
    /// 플레이어가 게임에 실패하여 재도전할때마다, 초기화해줘야 하는 부분을 이 함수에서 처리한다.
    /// </summary>
    public void Initialize()
    {
        _teleportSuccessPercent = _initTeleportSuccessPercent;
    }

    /* [독립성] 두번째 매개변수로 CatchGameStarTriggerType을 둔 이유
         * 이 함수 내부에서 유효성검사(이 함수가 호출될 수 있는 경우인지 검사)를 해주기 위해서이다.
         * 따라서 이 함수를 호출하는 객체는 두번째 매개변수의 값을 적절히 전달해주면 별도의 유효성검사를 하지 않아도 된다.
         * 이는, 새로운 특성효과(새로운 클래스)를 추가한다고 하더라도, 이 함수를 호출하는 객체를 수정하지 않게 해준다. (독립성을 확보해준다.)
         */
    /// <summary>
    /// 특수능력(순간이동)을 발동처리하는 함수이다.
    /// </summary>
    /// <param name="target"> 특수능력를 적용시키는 대상 </param>
    /// <param name="type"> 유효성 검사 </param>
    public void PlayEffect(GameObject target, CatchGameStarTriggerType type)
    {        
        if (type != TriggerType) return;
        if (IsTeleportSuccess(_teleportSuccessPercent))
        {
            Vector3 posBeforeTeleport = transform.position;
            transform.position = _stageEnvSet.GetRandomPointOnGenGround(); /// 오브젝트가 젠될 수 있는 위치를 랜덤으로 반환한다.
            _effect.ShowEffect(posBeforeTeleport + Vector3.up * 2.0f);
            AudioMgr.instance.PlaySoundEffect("Teleport");
        }            
        _teleportSuccessPercent -= 20.0f;
    }

    /*[클린성] 함수화
         * if(Random.Range(0.0f, 100.0f) < _teleportSuccessPercent)해서 한문장으로 정리할 수 있는 부분을 함수화했다.
         * 함수화해서 if(IsTeleportSuccess(_teleportSuccessPercent)) 코드를 쓰는 것이 훨씬 가독성이 좋다.
         */

    /*[유연성]  재활용
    * 뿐만 아니라 다른 함수에서 재활용이 가능해지며, 
    * 순간이동 성공여부와 관련된 수정사항이 있는 경우 이 함수 내부만 수정해주면 된다.
    */
    /// <summary>
    /// 순간이동 성공여부를 반환한다.
    /// </summary>
    /// <param name="teleportSuccessPercent">순간이동 성공확률</param>
    /// <returns>순간이동 성공여부</returns>
    bool IsTeleportSuccess(float teleportSuccessPercent)
    {
        float teleportSuccessCriteria = Random.Range(0.0f, 100.0f);
        return teleportSuccessCriteria < _teleportSuccessPercent;
    }
}
