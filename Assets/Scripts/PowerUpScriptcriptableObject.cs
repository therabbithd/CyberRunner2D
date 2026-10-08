using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpScriptableObject", menuName = "PowerUps/PowerUpData")]
public class PowerUpScriptableObject : ScriptableObject
{
    [SerializeField] private string PowerUpType;
    [SerializeField] private float TimeLimit;
    [SerializeField] private float PowerUpValue;

    public string PowerUpType1 { get => PowerUpType; set => PowerUpType = value; }
    public float TimeLimit1 { get => TimeLimit; set => TimeLimit = value; }
    public float PowerUpValue1 { get => PowerUpValue; set => PowerUpValue = value; }
}
