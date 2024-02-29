using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//플레이어 이벤트 클래스
public class PlayerEvents : MonoBehaviour
{
    // 이벤트를 정의한다.
    public static event Action<int , Vector3> PlayerPositionChanged; // 플레이어의 위치가 변경되었을 때 발생하는 이벤트
    public static event Action<int , int> PlayerAttacked; // 플레이어가 공격했을 때 발생하는 이벤트
    public static event Action<int , int> PlayerDamaged; // 플레이어가 피해를 입었을 때 발생하는 이벤트
    public static event Action<int , bool> PlayerDead; // 플레이어가 죽었을 때 발생하는 이벤트
    public static event Action<int , bool> PlayerJump; // 플레이어가 점프했을 때 발생하는 이벤트
    // Flip 이벤트를 정의한다.
    public static event Action<int , bool> PlayerFlipped;

    // 이벤트를 발생시키는 메소드를 정의한다. (플레이어 넘버까지 함께 전달)
    public static void OnPlayerPositionChanged(int PlayerID,Vector3 newPosition)
    {
        if (PlayerPositionChanged != null)
        {
            PlayerPositionChanged(PlayerID, newPosition); 
        }
    }

    // 플레이어가 공격했을 때 호출된다.
    public static void OnPlayerAttacked(int PlayerID, int attackType)
    {
        PlayerAttacked?.Invoke(PlayerID, attackType);// null이 아닐 때만 이벤트를 발생시킨다.
    }

    // 플레이어가 피해를 입었을 때 호출된다.
    public static void OnPlayerDamaged(int PlayerID, int damageAmount)
    {
        PlayerDamaged?.Invoke(PlayerID, damageAmount);
    }

    // 플레이어가 점프를 했을 때 호출된다.
    public static void OnPlayJump(int PlayerID, bool isJumping)
    {
        PlayerJump?.Invoke(PlayerID, isJumping);
    }

    //플레이어의 좌우  반전 이벤트를 발생시킨다.
    public static void OnPlayerFlipped(int PlayerID, bool facingRight)
    {
        PlayerFlipped?.Invoke(PlayerID, facingRight); 
    }

    //플레이어가 죽었을 때 호출된다.
    public static void OnPlayerDead(int PlayerID, bool isDead)
    {
        PlayerDead?.Invoke(PlayerID, isDead);
    }
}
