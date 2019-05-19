using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// 필드아이템인 별에 특수능력이 언제 발동될지를 명시
public enum CatchGameStarTriggerType { OnEnter = 0, OnSight, NumOfTypes, };

/* [독립성] 구현보다는 구성
 * 부모클래스를 상속하는 것보다, 인터페이스를 통해 구성하는 것을 애용하였다.
 * 부모클래스를 상속하면 자식클래스와의 결합도가 인터페이스를 통해 구성하는 것보다 높아진다. 
 * 또한 엄밀한 의미에서의 캡슐화를 위반한다. (protected 변수는 부모클래스와 자식클래스에서 모두 수정이 가능하다.)
 * 그래서 "스콧마이어스의 Effective C++"에서 protected 변수 선언을 자제하라 하였고, 
 * "Headfirst Design Pattern"에서 구현보다는 구성을 이용하라는 것을 디자인패턴 제 1원칙으로 제시하였다.
 */

/// <summary>
/// 필드아이템인 별에 속성(특수능력)을 부여해주기 위한 클래스를 만들기 위해선, 반드시 이 인터페이스를 상속받아야한다.
/// </summary>
public interface CatchGameStarProperty {

    CatchGameStarTriggerType TriggerType { get; }
    void Initialize();
    void PlayEffect(GameObject target, CatchGameStarTriggerType type);
}
