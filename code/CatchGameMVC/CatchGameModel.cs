using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CatchGameModel : MonoBehaviour {

    /* [견고성] 애초에 기존코드 수정방지
     * 애초에 기존코드를 수정하지 않도록 하는 것이 오류를 막는 지름길이다.
     * 오래된 코드를 수정하는 작업은 예기치 못한 오류를 불러 올 공산이 매우 크기 때문이다.
     * 
     * 이를 위해서 인스펙터뷰를 적극 활용하였다.
     * 예를 들면 게임 난이도를 인스펙터뷰에서 수정할 수 있으며, 이 과정에서 기존 코드를 직접 보거나 건드릴 필요가 없다.
     * (인스펙터뷰 스크린샷)
     */
    [Header("Info")]
    [SerializeField]
    ItemSet.ItemNameType _rewardItemNameType;   /// 게임 클리어시 보상아이템 정보
    
    [Header("Refer")]    
    [SerializeField]
    CatchGameStageEnvSet _stageEnvSet;                      /// 스테이지 환경(맵) 정보    
    [SerializeField]
    CatchGameLevelMgr _levelMgr;                                  /// 난이도 정보                    
    [SerializeField]
    ObjectPool _sheepPool;                                                 /// 오브젝트 풀링 -> 필드몬스터 양
    [SerializeField]
    CatchGameStarPool _starPool;                                     /// 오브젝트 풀링 -> 필드아이템 별

    DogPlayerController _playerController;                       /// 플레이어 컨트롤러 (게임시작과 함께 플레이어 초기화 용도)
    MinigameData _data;                                                      /// 게임 세이브 관련 클래스

    public int CurrentStageLevel { get; private set; }         /// 현재 스테이지 난이도

    /// [필드몬스터] 양 관련 변수
    public int CurrentSheepCount { get; private set; }      /// 생존해있는 양(필드몬스터)의 수
    int _initSheepCount;                                                        /// 처음 환경(맵)에 생성한 양의 수
    GameObject[] _sheeps;                                                   /// 환경에 생성된 양들을 참조하는 오브젝트

    /// [필드아이템] 별 관련 변수
    public int CurrentStarCount { get; private set; }           /// 플레이어가 획득한 별의 수
    GameObject[] _stars;                                                        /// 환경에 생성된 별들을 참조하는 오브젝트
    int _createdStarCount = 0;                                               /// 현재까지 생성된 별의 수
    const int _maxStarCount = 3;                                          /// 환경에 생성할 수 있는 별의 총 개수

    /* 초기화를 두개의 함수로 분리
     * 게임시작시 단 한번 초기화해도 되는 부분(Awake 함수)과 
     * 게임을 재도전할때마다 초기화해야되는 부분(Start 함수)을 분리하였다.
     * 이를 통해 단 한번 초기화해도 되는 부분이 여러번 호출되어 CPU가 낭비되는 일이 없도록 하였다.
     */
    /* [클린성] 가독성을 위해서 Enumeration 이용
     * 코드의 가독성을 위해서 Enumeration을 적극이용하는 편이다.
     * 또한 int형으로도 형변환이 되어 index를 가독성있게 표현하는데 큰 역할을 한다.
     */
    /// <summary>
    /// 게임 입장시, "단 한번만" 초기화해도 되는 부분을 이 함수에서 초기화한다.
    /// </summary>
    void Awake()
    {
        _playerController = GameObject.FindWithTag("Player").GetComponent<DogPlayerController>();
        var minigameDataSet = GameObject.FindWithTag("Managers").GetComponentInChildren<MinigameDataSet>();
        _data = minigameDataSet.GetMinigameData(MinigameType.Type.CatchGame); 
        Initialize();
    }

    /* [클린성] 함수의 배치순서
     * 전체 함수의 배치순서는 호출순서대로 배치하였다.
     * 예를 들어 A함수에서 B함수와 C함수를 순서대로 호출했다면, 함수의 배치순서는 A-B-C가 된다.
     * 이러한 배치는 코드를 위에서 아래로 내려가면서 읽을때 내용전반을 자연스럽게 이해할 수 있도록 돕는다.
     */
    /// <summary>
    /// 게임 재도전을 할때마다, 초기화해야되는 부분을 이 함수에서 초기화한다. 
    /// </summary>
    public void Initialize()
    {
        StopAllCoroutines();
        CurrentStageLevel = _data.CurrentStageLevel;
        var currentStageEnv = GetCurrentStageEnv(CurrentStageLevel);        // 현재 환경(맵) 정보
        var currentStageInfo = GetCurrentStageInfo(CurrentStageLevel);       // 현재 난이도 정보

        _sheepPool.DeactivateAllObejcts();
        _stageEnvSet.ActivateCurrentStageEnv(CurrentStageLevel);
        
        InitializePlayer(currentStageEnv); 
        InitializeSheep(currentStageEnv, currentStageInfo);
        InitializeStar(currentStageInfo);
    }

    /* [클린성] 함수 하나당 10줄 이하
     * 함수 길이에 내 나름제로 10줄 이하라는 제약을 두었다.
     * 길이가 짧은 코드일수록 읽기편하기 때문이다.
     * 
     * 사실 이보다 더 중요한 이유가 있다. 이러한 제약이 함수가 한가지 행동만을 하도록 강제할 수 있기 때문이다.
     * 이렇게 되면 함수의 책임이 명확해져 디버깅하기 쉬워진다.
     */
    /// <summary>
    /// 현재 환경(맵) 정보를 반환한다.
    /// </summary>
    /// <param name="stageLevel">스테이지 난이도</param>
    /// <returns></returns>
    public CatchGameStageEnv GetCurrentStageEnv(int stageLevel)
    {
        return _stageEnvSet.StageEnvs[stageLevel];
    }

    /// <summary>
    /// 현재 난이도 정보를 반환한다.
    /// </summary>
    /// <param name="stageLevel">스테이지 난이도</param>
    /// <returns></returns>
    public CatchGameLevelMgr.StageInfo GetCurrentStageInfo(int stageLevel)
    {
        return _levelMgr.StageInfos[stageLevel];
    }

   /* [클린성] var
 * var 타입을 이용하면 코드의 가독성을 향상시켜준다.
 * 타입을 변수이름에서 유추할 수 있도록 하였다.
 */
    /// <summary>
    /// 플레이어의 시작위치와 가속도(눈이 쌓인 맵인 경우 더 미끄러움)를 초기화한다.
    /// </summary>
    /// <param name="currentStageEnv"> 맵 정보 </param>    
    void InitializePlayer(CatchGameStageEnv currentStageEnv)
    {
        var playerStartPos = currentStageEnv.PlayerStartPos.transform.position;
        float playerAcceleration = currentStageEnv.Acceleration;
        _playerController.OnInit(playerStartPos, playerAcceleration);        
    }

    /// <summary>
    /// 필드 몬스터인 양을 생성 및 초기화한다.
    /// </summary>
    /// <param name="currentStageEnv"> 맵 정보 </param>
    /// <param name="currentStageInfo"> 난이도 정보 </param>
    void InitializeSheep(CatchGameStageEnv currentStageEnv, CatchGameLevelMgr.StageInfo currentStageInfo)
    {
        _sheeps = ActivateSheep(currentStageInfo, currentStageEnv.GenGrounds);
        _initSheepCount = currentStageInfo.SheepInfos.Length;
        CurrentSheepCount = _initSheepCount;
    }

    /// <summary>
    /// 오브젝트 풀링 방식으로 양을 생성 및 초기화한다.
    /// </summary>
    /// <param name="stageInfo"> 난이도 정보 </param>
    /// <param name="genGrounds"> 양이 젠(생성)될 수 있는 게임오브젝트(ex 맵의 바닥) 배열</param>
    /// <returns> 생성된 양들을 참조하는 배열 </returns>
    GameObject[] ActivateSheep(CatchGameLevelMgr.StageInfo stageInfo, GameObject[] genGrounds)
    {                
        int sheepCount = stageInfo.SheepInfos.Length;
        GameObject[] sheeps = new GameObject[sheepCount];

        for (int i = 0; i < sheepCount; i++)
        {
            var currentSheepInfo = stageInfo.SheepInfos[i];
            GameObject currentSheep = _sheepPool.GetAvalialbeObject();
            currentSheep.SetActive(true);
            currentSheep.GetComponent<SheepController>().OnInit(currentSheepInfo, GetRandomPointOnGenGround(genGrounds));
            sheeps[i] = currentSheep;
        }
        return sheeps;
    }

    /// <summary>
    ///  필드아이템인 별을 생성 및 초기화한다.
    /// </summary>
    /// <param name="currentStageInfo"> 난이도 정보 </param>
    void InitializeStar(CatchGameLevelMgr.StageInfo currentStageInfo)
    {
        _starPool.DeactivateAllStars();
        CurrentStarCount = 0;
        _createdStarCount = 0;
        _stars = _starPool.GetStars(currentStageInfo.StarTypes);
        AppearStar();
    }
    
    /// <summary>
    /// 필드에 있는 모든 양을 사라지게 한다.
    /// </summary>
    public void DeactivateAllSheeps()
    {
        _sheepPool.DeactivateAllObejcts();
    }
   
    /* [독립성] 서로가 서로를 참조하는 경우 방지
     * 두 객체가 서로를 참조(상호참조)하는 경우 예기치 않은 예외가 발생할 확률이 굉장히 높다.
     * 대표적으로 초기화되기 이전에 다른 객체의 멤버함수를 호출해버릴 수 있다.
     * 애초에 두 객체가 서로를 참조하지 않도록 설계해야되겠지만, 꼭 그래야하는 경우에는 이벤트를 이용하면 된다.
     * 다른 객체를 직접적으로 참조하지 않아도, 그 객체의 멤버함수를 호출할 수 있기 때문에, 상호참조의 고리를 끊을 수 있다.
     */
    /// <summary>
    /// 필드아이템인 별이 생성될 조건을 만족하면 필드 위에 생성하고, 특정 시간이 지나면 사라지게 만든다.
    /// </summary>
    public void AppearStar()
    {
        if (IsAppearStar() == false) return;

        var currentStageEnv = GetCurrentStageEnv(CurrentStageLevel);         // 현재 환경(맵) 정보
        var currentStageInfo = GetCurrentStageInfo(CurrentStageLevel);        // 현재 난이도 정보
        var currentStar = _stars[_createdStarCount];
        currentStar.SetActive(true);
        currentStar.transform.position = GetRandomPointOnGenGround(currentStageEnv.GenGrounds);
        StartCoroutine(DisappearStar(currentStar, currentStageInfo.StarExistDuration));
        ++_createdStarCount;
        CatchGameEventPublisher.instance.DoAppearStar(this); 
    }

    /// <summary>
    /// 필드아이템인 별이 생성될 조건을 만족하는지 그 여부를 반환한다.
    /// </summary>
    /// <returns> 별이 생성될 조건을 만족하는가의 여부 </returns>
    public bool IsAppearStar()
    {
        if (_createdStarCount >= _maxStarCount)
            return false;
        int catchSheepCount = _initSheepCount - CurrentSheepCount;
        float ratio = ((_initSheepCount * 1.0f) * ((_createdStarCount * 1.0f + 1) / (_maxStarCount + 1))); 
        return catchSheepCount >= ratio;
    }

    /* [보완점] 재활용될 여지가 있는 함수
     * 이 함수는 CatchGame 이외의 미니게임에서 재활용될 여지가 크다. 
     * 따라서 따로 클래스를 만들어서 분리시키는 것이 좋을 것 같다.
     */
    /// <summary>
    /// 오브젝트를 젠할 위치를 랜덤으로 반환한다.
    /// </summary>
    /// <param name="genGrounds"> 오브젝트가 젠될 수 있는 게임오브젝트(ex 맵의 바닥) 배열 </param>
    /// <returns> 오브젝트를 젠할 위치 </returns>
    Vector3 GetRandomPointOnGenGround(GameObject[] genGrounds)
    {
        int randomGenGroundIndex = Random.Range(0, genGrounds.Length);
        var genGroundCollider = genGrounds[randomGenGroundIndex].GetComponent<Collider>();
        Vector3 stageCenter = genGroundCollider.transform.position;
        float stageWidthX = genGroundCollider.bounds.extents.x;
        float stageWidthZ = genGroundCollider.bounds.extents.z;
        Vector3 randomPoint = new Vector3(Random.Range(stageCenter.x - stageWidthX, stageCenter.x + stageWidthX), 
                                                                            genGroundCollider.transform.position.y, 
                                                                           Random.Range(stageCenter.z - stageWidthZ, stageCenter.z + stageWidthZ));
        return randomPoint;
    }

    /// <summary>
    /// 정해진 대기시간이 지나면 별을 사라지게 한다.
    /// </summary>
    /// <param name="star"> 사라지게 할 별 </param>
    /// <param name="waitDuration"> 대기시간 </param>
    IEnumerator DisappearStar(GameObject star, float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        if (star.GetComponent<CatchGameStar>().IsCatched == false) 
        {
            star.SetActive(false);
            CatchGameEventPublisher.instance.DoDisappearStar(this);
        }        
    }
    
    /// <summary>
    /// 필드에 있는 별 중에 하나를 아무거나 반환한다.
    /// </summary>
    /// <returns></returns>
    public GameObject GetStarOnField()
    {
        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i].activeSelf && _stars[i].GetComponent<CatchGameStar>().IsCatched == false)
                return _stars[i];                
        }
        return null;
    }

    /* [유연성] 매개변수는 유연성 확보수단
     * 매개변수를 통해 함수를 쉽게 확장할 수 있다.
     * 내 코드를 예로 들어 설명하면, 현재 내 게임에서는 별의 개수를 2이상 증가시킬 수 있는 함수가 필요없다. 
     * 하지만 추후에 추가될 수 있기 때문에, 이 함수에 int형을 매개변수로 두어 함수 확장가능성에 대비하였다.
     */
    /// <summary>
    /// 현재 습득한 별의 수를 증가시킨다.
    /// </summary>
    /// <param name="value">증가시킬 개수</param>
    public void IncreaseStarCount(int value)
    {
        CurrentStarCount += value;
    }

    /// <summary>
    /// 플레이어가 공격할 수 있는 가장 가까운 양을 반환한다.
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearestSheepPlayerCanHit()
    {
        return _sheepPool.GetNearestSheepPlayerCanHit(_playerController.gameObject);
    }

    /// <summary>
    /// 현재의 양의 수를 감소시킨다.
    /// </summary>
    /// <param name="value">감소시킬 수</param>
    public void DecreaseSheepCount(int value)
    {
        CurrentSheepCount -= value;
    }

    /* [클린성] 지역변수로 함수의 반환결과 저장
     * 되도록이면 지역변수로 함수의 반환결과를 저장하였다.
     * 이렇게 되면 함수의 반환결과에 이름(변수명)을 붙이는 꼴이되며, 가독성 향상에 도움이 되기 때문이다.
     */
    /// <summary>
    /// 게임클리어한 경우, 리워드를 받고 메인화면(홈)으로 빠져나오는 작업을 처리한다.
    /// </summary>
    /// <param name="itemData">인벤토리 정보</param>
    /// <param name="itemSet">아이템 정보</param>
    public void GoHome(ItemData itemData, ItemSet itemSet)
    {
        Destroy(Camera.main.gameObject);
        var rewardItemInfo = itemSet.GetItemInfo(_rewardItemNameType);
        int rewardItemCount = GetRewardItemCount();
        itemData.IncreaseItem(rewardItemInfo, rewardItemCount);
        _data.ClearCurrentStage(CurrentStarCount);
        gameObject.SetActive(false);
        SceneManager.LoadScene("Home");
    }

    /// <summary>
    /// 게임실패한 경우, 메인화면(홈)으로 빠져나오는 작업을 처리한다.
    /// </summary>
    public void GoHomeWithFail()
    {
        Destroy(Camera.main.gameObject);
        gameObject.SetActive(false);
        SceneManager.LoadScene("Home");
    }

    /// <summary>
    /// 보상아이템 개수를 반환한다.
    /// </summary>
    /// <returns></returns>
    public int GetRewardItemCount()
    {
        /// 이전에 게임했던것보다 더 많은 별을 습득했으면, 더 습득한 별만큼 보상아이템을 받는다.
        int prevStarCount = _data.StarCountPerStage[CurrentStageLevel]; /// 현재까지 게임에서 습득한 별의 최대 수
        int rewardItemCount = CurrentStarCount - prevStarCount;
        if (rewardItemCount < 0)
            return 0;
        else
            return rewardItemCount;
    }

    /// <summary>
    /// 보상아이템 정보를 반환한다.
    /// </summary>
    /// <param name="itemSet">아이템 정보</param>
    /// <returns></returns>
    public ItemInfo GetRewardItemInfo(ItemSet itemSet)
    {
        var rewardItemInfo = itemSet.GetItemInfo(_rewardItemNameType);
        return rewardItemInfo;
    }

}
