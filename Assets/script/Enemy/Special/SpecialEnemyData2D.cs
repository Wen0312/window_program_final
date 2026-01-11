using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpecialEnemyData2D
/// 特殊敵人資料（資料驅動）
/// - 僅存「能力清單」與可調參數，不存 runtime 狀態
/// - runtime 狀態由 SpecialEnemyController2D 在每個敵人實例上建立
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemy/SpecialEnemyData2D", fileName = "SpecialEnemyData2D")]
public class SpecialEnemyData2D : ScriptableObject
{
    [Header("Identity")]
    public string id = "special_enemy";
    public string displayName = "Special Enemy";

    [Header("Abilities")]
    public List<SpecialEnemyAbilityData2D> abilities = new List<SpecialEnemyAbilityData2D>();
}
