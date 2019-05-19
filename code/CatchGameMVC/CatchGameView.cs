using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatchGameView : MonoBehaviour {

   /* [견고성] 클래스의 역할을 세분화
 * 클래스의 역할을 적당히 세분화할수록 코드의 견고성이 높아진다.
 * 이렇게 하면, 책임이 명확해져 디버깅하기 쉬워진다.
 * 
 * 이 클래스를 예로들어 설명하면, 각 UI를 현재 클래스에서 모두 처리하는 것이 아니다.
 * 각 UI를 처리하는 역할을 전담하는 클래스를 각각 만들어준다. (클래스의 역할을 세분화한다)
 * 그리고 이를 중재하는 역할을 현재 클래스가 한다.
 */
    [Header("Sheep Count Panel")]
    [SerializeField]
    Text sheepCountText;                                                /// 필드에 남아있는 양의 마리수를 보여주는 UI

    [Header("Star Count Panel")]
    [SerializeField]
    StarCountPanel _starCountPanel;                          /// 현재 습득한 별의 수를 보여주는 UI

    [Header("Ask Restart Panel")]
    [SerializeField]
    AskRestartPanel _askRestartPanel;                        /// 게임실패시, 재도전 선택화면을 보여주는 UI
    public AskRestartPanel AskRestartPanel { get { return _askRestartPanel; } private set { _askRestartPanel = value; } }

    [Header("Game Clear Panel")]
    [SerializeField]
    GameClearUI _gameClearPanel;                             /// 게임클리어시, 클리어 정보를 보여주는 UI
    public GameClearUI GameClearPanel { get { return _gameClearPanel; } private set { _gameClearPanel = value; } }

    [Header("Show Info Panel")]
    [SerializeField]
    InfoPanel _infoPanel;                                                /// 게임에서 발생하고 있는 이벤트 알림을 보여주는 UI
    readonly float _infoDuration = 5.0f;                       /// 이벤트 알림을 플레이어에게 보여줄 지속시간

    [Header("Narrow Sight Panel")]
    [SerializeField]
    private GameObject _narrowSightPanel;              /// 플레이어의 시야를 가리는 UI  (장애물)
    public GameObject NarrowSightPanel { get { return _narrowSightPanel; } private set { _narrowSightPanel = value; } }

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize()
    {
        SwitchUI(NarrowSightPanel, false);
    }

    /// <summary>
    /// 이벤트가 발생하면, 그 정보를 플레이어에게 보여준다.
    /// </summary>
    /// <param name="text"> 알림 메시지 </param>
    public void ShowInfo(string text)
    {
        _infoPanel.ShowInfo(text, _infoDuration);
    }

    /// <summary>
    /// UI를 플레이어에게 보여주거나 사라지게 만든다.
    /// </summary>
    /// <param name="UI">UI컴포넌트</param>
    /// <param name="flag">플레이어에게 보여줄지의 여부</param>
    public void SwitchUI(GameObject UI, bool flag)
    {
        UI.SetActive(flag);
    }

    /// <summary>
    /// 현재 습득한 별의 수를 보여주는 UI를 업데이트한다.
    /// </summary>
    /// <param name="currentStarCount">현재 습득한 별의 수</param>
    public void UpdateStarCountPanel(int currentStarCount)
    {
        _starCountPanel.UpdateUI(currentStarCount);
    }

    /// <summary>
    /// 현재 필드위에 있는 양의 수를 보여주는 UI를 업데이트한다.
    /// </summary>
    /// <param name="currentSheepCount">현재 필드위에 있는 양의 수</param>
    public void UpdateSheepCountPanel(float currentSheepCount)
    {
        sheepCountText.text = currentSheepCount + "마리";
    }

    /// <summary>
    /// 게임 클리어시 클리어 정보를 보여주는 UI를 업데이트한다.
    /// </summary>
    /// <param name="currentStarCount">현재 습득한 별의 수</param>
    /// <param name="rewardItemInfo">보상아이템 정보</param>
    /// <param name="rewardItemCount">보상아이템 개수</param>
    public void UpdateGameClearPanel(int currentStarCount, ItemInfo rewardItemInfo, int rewardItemCount)
    {
        _gameClearPanel.UpdateUI(currentStarCount, rewardItemInfo, rewardItemCount);
    }
}
