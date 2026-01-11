using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map/Weapon Loadout", fileName = "WeaponLoadout_")]
public class WeaponLoadoutData : ScriptableObject
{
    [Header("Weapons allowed in this map/mode")]
    public WeaponData[] weapons;
}
