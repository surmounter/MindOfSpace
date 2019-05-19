using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatchGameController : MonoBehaviour {
    /* [클린성] 네이밍
     * property는 대문자로시작, 멤버변수는 _로 시작, 멤버함수는 대문자 동사로 시작, 이벤트 관련함수는 On전언
     */
    
    /* [견고성] 캡슐화
     * A 클래스의 멤버변수는 A 클래스의 멤버함수에서만 수정할 수 있다.
     * B 클래스에서 A클래스의 멤버함수를 경유하지 않고 직접적으로 A클래스의 멤버변수를 수정할 수 없다.
     */
    
    [Header("Refer")]
    [SerializeField]
    CatchGameModel _model;                              /// MVC 패턴에서 Model
    [SerializeField]    
    CatchGameView _view;                                    /// MVC 패턴에서 View

    DogPlayerController _playerController;         ///플레이어를 참조하는 객체 (플레이어에게 게임 이벤트 발생시 알림)
    ItemData _itemData;                                         /// 인벤토리 정보
    ItemSet _itemSet;                                              /// 아이템 정보


    /* [독립성] 이벤트 사용
     * 이벤트는 클래스 간의 결합도를 낮추고, 확장성을 높이는데 매우 큰 도움이 된다.
     * 이벤트를 통해 객체를 직접적으로 참조하지 않아도, 그 객체의 특정함수를 호출할 수 있기 때문이다.
     * 또한 기존 클래스와 통신하고 싶을때, 기존 클래스와 연관된 이벤트에 연결해서 새로운 클래스를 만들면 된다.
     * 이 과정에서 기존 클래스(코드)를 수정할 필요가 없다. (그래서 유연성을 높이는데 큰 기여를 한다.)
     * 
     * 이 장점을 살리기 위해서 나는 MVC모델들 사이의 의존도를 약하게 만들기 위한 일환으로 이벤트를 많이 사용한다.
     * 코드를 보면 Player관련 MVC모델과 CatchGame MVC모델이 이벤트를 통해 연결되는 과정을 볼 수 있다.
     * PS) 물론 이벤트를 이용할때 이벤트에 연결된 함수들의 호출순서를 통제하기 어렵다는 단점이 있다.
    */
    /// <summary>
    /// 변수를 초기화하고, 이벤트를 연결한다. 
    /// </summary>
    void Start()
    {
        _playerController = GameObject.FindWithTag("Player").GetComponent<DogPlayerController>();
        _itemData = GameObject.FindWithTag("Managers").GetComponentInChildren<ItemData>();
        _itemSet = GameObject.FindWithTag("Managers").GetComponentInChildren<ItemSet>();
        _playerController.OnUpdateSheepIndicator(_model.GetNearestSheepPlayerCanHit());
        _view.UpdateSheepCountPanel(_model.CurrentSheepCount);

        CatchGameEventPublisher.instance.PlayerAttackSheepEvent += OnPlayerAttackSheep; /// 싱글턴 패턴 사용
        CatchGameEventPublisher.instance.PlayerHitBySheepEvent += OnPlayerHitBySheep;
        CatchGameEventPublisher.instance.AppearStarEvent += OnAppearStar;
        CatchGameEventPublisher.instance.DisappearStarEvent += OnDisappearStar;
        CatchGameEventPublisher.instance.GetStarEvent += OnGetStar;
    }


    /* [견고성] 쌍으로 둬야하는 것
     * 평소에 코드를 짜는 습관으로 코드의 오류를 미리 방지할 수 있다는 생각을 품고 있다.
     * 그 중에 하나로, 이벤트를 등록하기 위한 코드를 쓸때, 해제할 시점에 해제를 위한 코드도 동시에 미리 써놓는다.
     * C++의 경우 new - delete도 이와 같이 코드를 쓴다.
     */
    /// <summary>
    /// 이 객체가 소멸되기 전에, 이벤트를 해제한다.
    /// </summary>
    void OnDestroy()
    {
        CatchGameEventPublisher.instance.PlayerAttackSheepEvent -= OnPlayerAttackSheep;
        CatchGameEventPublisher.instance.PlayerHitBySheepEvent -= OnPlayerHitBySheep;
        CatchGameEventPublisher.instance.AppearStarEvent -= OnAppearStar;
        CatchGameEventPublisher.instance.DisappearStarEvent -= OnDisappearStar;
        CatchGameEventPublisher.instance.GetStarEvent -= OnGetStar;
    }

    /* [견고성] 테스트 단위 함수 분리
     * 테스트 단위로 함수를 쪼갠다.
     * 이를 위한 방법으로 함수 하나에는 하나의 기능(행동)만을 하도록 노력해서 코드를 짰다. 
     * 그 기능에 문제가 생기면, 그 기능을 담당하는 함수를 바로 보면 되기 때문에 디버깅하기도 쉬워진다.
     */    
    /* [고난] 게임은 함께 만드는 것이다.
     * 게임은 함께 만든다는 사실은, 팀원 서로가 짠 코드를 봐야할 상황이 많다는 것을 의미한다.
     * 내가 코드를 짜는 시간보다 동료가 짠 코드를 보는 시간이 훨씬 더 많을 때도 비일비재했다.
     * 
     * 1.
     * 서로의 코드를 파악하는 시간을 단축시키기 위해선 코드를 최대한 클린하게 짜야한다.
     * 지금 당장 코드를 클린하게 짜는 것이 시간이 좀 더 소모될 수 있으나, 장기적으로 보면 시간을 크게 단축시킨다는 것을 뼈저리게 느낀적이 많았다.
     * 그 이후 "클린코드"와 "C# 코딩의 기술:실전편" 책을 읽어보는 등 지금까지도 코드를 어떻게하면 클린하게 짤 수 있는지를 고민하고 있다. 
     * 
     * 2.
     * 디자인패턴
     * 디자인패턴에 대해 서로가 알고 있으면, 내가 사용한 디자인패턴의 이름하나라 코드 전체 구조를 단번에 이해시킬 수 있게 된다.
     * 따라서 동료로 하여금 빠르게 나의 코드를 이해시킬 수 있다.
     */     
    /// <summary>
    /// 플레이어가 필드몬스터인 양을 공격할때, 관련변수와 UI(view)를 업데이트하고, 모든 양을 잡으면 게임 클리어시킨다.
    /// </summary>
    /// <param name="sender"></param>
    void OnPlayerAttackSheep(DogPlayerController sender)
    {
        _model.DecreaseSheepCount(1);
        if(_model.IsAppearStar())
            _model.AppearStar();
        _view.UpdateSheepCountPanel(_model.CurrentSheepCount);
        _playerController.OnUpdateSheepIndicator(_model.GetNearestSheepPlayerCanHit());
        AudioMgr.instance.PlaySoundEffect("Kick");
                
        if (_model.CurrentSheepCount <= 0)
            OnGameClear();        
    }

    /// <summary>
    /// 플레이어가 필드몬스터인 양에게 공격받았다면, 미션실패이며 재도전할지를 물어본다.
    /// </summary>
    /// <param name="sender"></param>
    void OnPlayerHitBySheep(DogPlayerController sender)
    {
        _model.DeactivateAllSheeps();
        _view.SwitchUI(_view.AskRestartPanel.gameObject, true);
        AudioMgr.instance.PlaySoundAndResumeBGM("Fail");
    }

    /* [보완점] 하드코딩을 피하자.
     * 하드코딩을 하면, 디버깅이 어렵고 유연성이 떨어진다.
     * 하지만 내 코드에서 하드코딩이 쓰인 부분이 보인다.
     * 이 부분은 보완되어야 하는 부분 중에 하나로, info message와 bgm name을 따로 관리하는 클래스를 만들어서
     * 그 클래스에서 이름을 받아오는 방향으로 짜면 좋을 것 같다.
     */    
    /// <summary>
    /// 필드아이템인 별이 필드에 등장할때 호출되는 함수
    /// </summary>
    /// <param name="sender"></param>
    void OnAppearStar(CatchGameModel sender)
    {
        _playerController.OnAppearStar(_model.GetStarOnField());
        _view.ShowInfo("별이 나타났습니다. 별이 사라지기 전에 얼른 가져가야 합니다.");
        AudioMgr.instance.PlaySoundEffect("Info");
    }

    /// <summary>
    /// 필드아이템인 별이 필드에서 사라질때 호출되는 함수
    /// </summary>
    /// <param name="sender"></param>
    void OnDisappearStar(CatchGameModel sender)
    {
        _playerController.OnDisappearStar(_model.GetStarOnField());
        _view.ShowInfo("별이 사라졌습니다.");
        AudioMgr.instance.PlaySoundEffect("InfoFail");
    }

    /// <summary>
    /// 필드아이템인 별을 먹었을때 호출되는 함수
    /// </summary>
    /// <param name="sender"></param>
    public void OnGetStar(CatchGameStar sender)
    {
        _model.IncreaseStarCount(1);
        _playerController.OnEnterStar(_model.GetStarOnField());
        _view.UpdateStarCountPanel(_model.CurrentStarCount);
        AudioMgr.instance.PlaySoundEffect("GetStar");
    }


    /* [견고성] 초기화(혹은 업데이트) 순서는 명확히!
    * 가장 먼저 생각해야할 방법은 초기화(혹은 업데이트)를 해야하는 순서가 상관없도록 코드를 짜는 것이다.
    * 초기화순서가 상관없는 경우에도 초기화 순서를 정해둬야 디버깅하기 쉬워진다.
    * 예를 들어 나는 MVC 모델에서 controller -> model ->  view 순으로 업데이트를 한다.
    */
    /// <summary>
    /// 플레이어가 게임 재도전 버튼을 누를때 호출되는 함수
    /// </summary>
    public void OnClickRestartButton()
    {        
        _model.Initialize();
        _view.Initialize();
        _view.UpdateSheepCountPanel(_model.CurrentSheepCount);
        _view.UpdateStarCountPanel(_model.CurrentStarCount);
        CatchGameEventPublisher.instance.DoRestart(this);
        _playerController.OnUpdateSheepIndicator(_model.GetNearestSheepPlayerCanHit());
    }

    /// <summary>
    ///  플레이어가 게임 재도전 안함 버튼을 누를때 호출되는 함수
    /// </summary>
    public void OnClickRestartNoButton()
    {
        _model.GoHomeWithFail();
    }
    
    /// <summary>
    /// 게임 클리어시 호출되는 함수
    /// </summary>
    public void OnGameClear()
    {
        var rewardItemInfo = _model.GetRewardItemInfo(_itemSet);
        int rewardItemCount = _model.GetRewardItemCount();
        _view.UpdateGameClearPanel(_model.CurrentStarCount, rewardItemInfo, rewardItemCount);
        _view.SwitchUI(_view.GameClearPanel.gameObject, true);
        AudioMgr.instance.PlaySoundAndResumeBGM("Clear");
    }

    /// <summary>
    /// 홈으로 돌아가기 버튼을 눌렀을때 호출되는 함수
    /// </summary>
    public void OnEnterGoHomeButton()
    {
        _model.GoHome(_itemData, _itemSet);
    }

    /// <summary>
    /// 시야를 가리는 별을 먹었을때 호출되는 함수
    /// </summary>
    public void OnEnterNarrowSightStar()
    {
        _view.SwitchUI(_view.NarrowSightPanel, true);
    }
}
