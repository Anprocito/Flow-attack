using UnityEngine;

public enum ActionType { Attack, Defend, Heal }

// Click derecho en Project > Create > Battle > Action
[CreateAssetMenu(menuName = "Battle/Action")]
public class ActionData : ScriptableObject
{
    public ActionType type;
    public string displayName = "Ataque";

    [Header("Valor")]
    public float baseValue = 10f;      // valor a nivel 1
    public float valuePerLevel = 2f;   // cuanto sube por nivel

    [Header("Minijuego")]
    public int noteCount = 8;
    public float noteInterval = 0.5f;  // segundos entre notas
    public float noteTravelTime = 1.5f;// tiempo que tarda la nota en caer
    [Range(1, 4)] public int lanes = 4;

    public float ValueAtLevel(int level) => baseValue + valuePerLevel * (level - 1);
}
