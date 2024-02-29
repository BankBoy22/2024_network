using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�÷��̾� �̺�Ʈ Ŭ����
public class PlayerEvents : MonoBehaviour
{
    // �̺�Ʈ�� �����Ѵ�.
    public static event Action<int , Vector3> PlayerPositionChanged; // �÷��̾��� ��ġ�� ����Ǿ��� �� �߻��ϴ� �̺�Ʈ
    public static event Action<int , int> PlayerAttacked; // �÷��̾ �������� �� �߻��ϴ� �̺�Ʈ
    public static event Action<int , int> PlayerDamaged; // �÷��̾ ���ظ� �Ծ��� �� �߻��ϴ� �̺�Ʈ
    public static event Action<int , bool> PlayerDead; // �÷��̾ �׾��� �� �߻��ϴ� �̺�Ʈ
    public static event Action<int , bool> PlayerJump; // �÷��̾ �������� �� �߻��ϴ� �̺�Ʈ
    // Flip �̺�Ʈ�� �����Ѵ�.
    public static event Action<int , bool> PlayerFlipped;

    // �̺�Ʈ�� �߻���Ű�� �޼ҵ带 �����Ѵ�. (�÷��̾� �ѹ����� �Բ� ����)
    public static void OnPlayerPositionChanged(int PlayerID,Vector3 newPosition)
    {
        if (PlayerPositionChanged != null)
        {
            PlayerPositionChanged(PlayerID, newPosition); 
        }
    }

    // �÷��̾ �������� �� ȣ��ȴ�.
    public static void OnPlayerAttacked(int PlayerID, int attackType)
    {
        PlayerAttacked?.Invoke(PlayerID, attackType);// null�� �ƴ� ���� �̺�Ʈ�� �߻���Ų��.
    }

    // �÷��̾ ���ظ� �Ծ��� �� ȣ��ȴ�.
    public static void OnPlayerDamaged(int PlayerID, int damageAmount)
    {
        PlayerDamaged?.Invoke(PlayerID, damageAmount);
    }

    // �÷��̾ ������ ���� �� ȣ��ȴ�.
    public static void OnPlayJump(int PlayerID, bool isJumping)
    {
        PlayerJump?.Invoke(PlayerID, isJumping);
    }

    //�÷��̾��� �¿�  ���� �̺�Ʈ�� �߻���Ų��.
    public static void OnPlayerFlipped(int PlayerID, bool facingRight)
    {
        PlayerFlipped?.Invoke(PlayerID, facingRight); 
    }

    //�÷��̾ �׾��� �� ȣ��ȴ�.
    public static void OnPlayerDead(int PlayerID, bool isDead)
    {
        PlayerDead?.Invoke(PlayerID, isDead);
    }
}
