using UnityEngine;

// Estadisticas del jugador. Copia los valores del PlayerStatsSO al empezar.
[DefaultExecutionOrder(-100)] // su Awake corre antes que el de PlayerHealth y PlayerController
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO baseStats;

    public float MaxHealth { get; private set; } = 100f;
    public float MoveSpeed { get; private set; } = 5f;
    public float DamageMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        if (baseStats == null)
        {
            Debug.LogError("PlayerStats: falta asignar el PlayerStatsSO. Se usan valores por defecto.", this);
            return;
        }

        MaxHealth = baseStats.maxHealth;
        MoveSpeed = baseStats.moveSpeed;
        DamageMultiplier = baseStats.damageMultiplier;
    }

    // El BattleManager usa esto para calcular el dano del ataque.
    public float GetFinalDamage(float baseDamage) => baseDamage * DamageMultiplier;
}
