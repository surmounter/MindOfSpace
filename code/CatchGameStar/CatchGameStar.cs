using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* [견고성] 제약조건 걸기
 * 이 컴포넌트를  추가하려면 CatchGameStarProperty 컴포넌트가 그 전에 추가되어 있어야 하는 제약조건을 만들었다.
 * CatchGameStarProperty 클래스가 없다면 Awake() 함수에서 오류가 필히 발생하기 때문이다.
 * 발생할 수 있는 오류나 실수를 방지하기 위해 제약조건을 두는 것이 견고성을 확보하는 지름길이다.
 * 
 * Story. 
 * 혼자서 게임을 만들때보다 협업을 통해 게임을 만들때, 오류발생도가 현저히 높았었던 기억이 난다.
 * 코드를 짤때, 동료를 위해서 내 의도대로 코드가 동작하도록 제약조건을 꼼꼼히 추가해주는 것이 매우 중요하다는 것을 뼈저리게 느꼈었다.
 */
[RequireComponent(typeof(CatchGameStarProperty))]
public class CatchGameStar : MonoBehaviour {
    
    [Header("Child Component")]
    [SerializeField]
    GameObject _objectModel; /// 별의 3D 모델 (이 오브젝트만 Off해주면, 플레이어의 입장에서 별이 사라진 것처럼 보인다.)
    [SerializeField]
    GameObject _sight;               /// 시야 오브젝트 (오브젝트가 플레이어와 접촉여부를 통해 시야내에 플레이어가 들어왔는지 판별)
    [SerializeField]
    EffectPool _getEffect;            /// 플레이어가 별을 획득했을때 나타나는 이펙트

    public bool IsCatched { get; private set; }     /// 플레이어가 이 별을 획득했는지의 여부
    Collider _collider;                                             /// 별의 collider 컴포넌트(충돌판정, 별과 플레이어가 접촉했는지를 판정)

    /* [유연성] 전략패턴 (Strategy Pattern)
     *  CatchGameStarProperty 인터페이스를 상속받은 컴포넌트(클래스)가 무엇이냐에 따라 다른 특수능력을 가진 별이 된다.
     *  새로운 특수능력을 가진 별을 만들고 싶다면,
     *      CatchGameStarProperty 인터페이스를 상속받은 새로운 클래스를 만들고,
     *      오브젝트에 새로 만든 클래스를 추가해주기만 하면 된다. (그러면 Start()함수에 의해 _property 변수가 새로만든 클래스를 참조하게 된다.)
     *      중요한 점은 이 과정에서 기존의 코드를 수정해야하는 작업이 필요없기 때문에 변화에 유연하게 대처할 수 있다. 
     *      
     * Story.
     * 코드를 짜다보면 스파게티 코드(복잡하고 서로 의존도가 높은 코드)가 되는 경우가 많았었다.
     * 어떤 부분을 고치려하면 다른 부분도 수정해야하고, 다시 그 부분을 수정하면 또 다른 부분을 수정해야하고...
     * 코드간의 의존도가 높아질때마다 나는 그 해결책으로 디자인 패턴 교재를 들춰본다. 
     * 어려움을 겪고 난 뒤에 책을 보면 코드간의 결합도를 낮추려는 패턴들의 의도와 원리가 좀 더 마음에 와닿는다.
     * 전략패턴은 내가 정말 그 필요성을 마음으로 느꼈던 디자인패턴이었으며, 내가 가장 자주 이용하는 디자인패턴이다.
     */
    CatchGameStarProperty _property;            /// 별의 속성(특수능력)을 참조하는 객체

    /* [클린성] 선언한 순서대로 초기화
     * 멤버변수를 선언한 순서대로 초기화해준다. 사소한 부분이지만 때에 따라서 가독성이 크게 향상되는 경우도 있다.
     */
    /// <summary>
    /// 게임시작시 단 한번만 초기화해줘야하는 부분을 여기서 초기화해준다.
    /// </summary>
    void Start()
    {                
        _collider = GetComponent<Collider>();
        _property = GetComponent<CatchGameStarProperty>();
        CatchGameEventPublisher.instance.RestartEvent += OnRestart; /// 플레이어가 게임 재도전시 발생하는 이벤트를 수신한다.
    }

    /// <summary>
    /// 컴포넌트가 소멸될기전에 호출되는 함수, 이벤트를 연결해제해준다.
    /// </summary>
    void OnDestroy()
    {
        CatchGameEventPublisher.instance.RestartEvent -= OnRestart;
    }

    /// <summary>
    /// 플레이어가 게임을 재도전할 시 호출되는 함수. 
    /// 게임을 재시작할때마다 초기화해줘야하는 부분을 여기서 초기화해준다.
    /// </summary>
    /// <param name="sender"></param>
    void OnRestart(CatchGameController sender)
    {
        _objectModel.SetActive(true);
        _sight.SetActive(true);
        IsCatched = false;
        _collider.enabled = true;
        _property.Initialize();
    }

    /// <summary>
    /// 어떤 오브젝트가 별에 접촉하면 호출되는 함수
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) /// 접촉한 대상이 플레이어인 경우
        {
            IsCatched = true;
            _property.PlayEffect(other.gameObject, CatchGameStarTriggerType.OnEnter);
            CatchGameEventPublisher.instance.DoGetStar(this);
            _getEffect.ShowEffect(transform.position + Vector3.up * 2.0f);
            Hide();
        }
    }

    /// <summary>
    /// 별을 플레이어로부터 숨긴다.
    /// gameobject 전체를 Off시키는 것이 아니라, 별의 외형만 사라지게 만든다.
    /// gameobject 전체를 Off시켜버리면, 이벤트(플레이어가 게임 재시작시 발생하는 이벤트)를 수신받을 수 없고 그에 따른 대처도 불가능하다.
    /// </summary>
    void Hide()
    {
        _objectModel.SetActive(false);
        _sight.SetActive(false);
        _collider.enabled = false;
    }

    /// <summary>
    /// 플레이어가 별의 시야에 들어오면 호출되는 함수.
    /// </summary>
    /// <param name="player"> 플레이어를 참조하는 객체 </param>
    public void OnPlayerEnterSight(GameObject player)
    {        
        _property.PlayEffect(player, CatchGameStarTriggerType.OnSight);
    }
    
}
