using UnityEngine;

// Vida del jugador. Se coloca en el mismo objeto que PlayerController.
// La vida maxima sale de PlayerStats.
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour
{
    private PlayerStats stats;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => stats.MaxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        currentHealth = stats.MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (currentHealth <= 0f)
            Die();
    }

    // Recupera vida sin pasarse de la vida maxima.
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        currentHealth = Mathf.Min(stats.MaxHealth, currentHealth + amount);
    }

    private void Die()
    {
        // Deja al jugador sin control al morir.
        if (TryGetComponent(out PlayerController controller))
            controller.enabled = false;
    }
}
