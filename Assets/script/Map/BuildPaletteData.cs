using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map/Build Palette", fileName = "BuildPalette_")]
public class BuildPaletteData : ScriptableObject
{
    [Header("Obstacles allowed in this map/mode")]
    public ObstacleData[] obstacles;
}
