using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map/Map Catalog 2D", fileName = "MapCatalog2D_")]
public class MapCatalog2D : ScriptableObject
{
    [Header("Maps (Level Order)")]
    public MapData2D[] maps;
}
