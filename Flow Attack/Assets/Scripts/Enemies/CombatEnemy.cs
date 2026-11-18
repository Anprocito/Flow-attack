using UnityEngine;

// Se pone en cada enemigo del mapa (capsula). No se mueve ni ataca solo:
// solo guarda sus valores fijos para el combate por turnos.
public class CombatEnemy : MonoBehaviour
{
    public string displayName = "Enemigo";
    public float maxHp = 80f;
    public float attackValue = 10f;   // dano fijo al atacar
    public float healValue = 12f;     // curacion fija
    [Range(0f, 1f)]
    [Tooltip("Si su vida baja de este porcentaje, intenta curarse.")]
    public float healBelowPercent = 0.4f;

    public float Hp { get; private set; }
    public bool IsDead => Hp <= 0f;

    [System.NonSerialized] public bool defeated;
    bool healedLast;

    void Awake() => Hp = maxHp;

    public void TakeDamage(float amount) => Hp = Mathf.Max(0f, Hp - amount);
    public void Heal(float amount) => Hp = Mathf.Min(maxHp, Hp + amount);

    // true = curarse, false = atacar. Nunca se cura dos turnos seguidos.
    public bool ChooseHeal()
    {
        bool heal = !healedLast && Hp < maxHp * healBelowPercent;
        healedLast = heal;
        return heal;
    }
}
