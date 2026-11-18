using UnityEngine;

[System.Serializable]
public class Combatant
{
    public string name;
    public int level = 1;
    public float maxHp = 100f;
    public float hp = 100f;
    public float shield;

    public void TakeDamage(float dmg)
    {
        float absorbed = Mathf.Min(shield, dmg);
        shield -= absorbed;
        hp = Mathf.Max(0f, hp - (dmg - absorbed));
    }

    public void Heal(float amount) => hp = Mathf.Min(maxHp, hp + amount);
    public bool IsDead => hp <= 0f;
}

public class BattleManager : MonoBehaviour
{
    public Combatant player;
    public Combatant enemy;
    public RhythmMinigame minigame;
    public ActionData enemyAttack;   // el enemigo usa el mismo sistema de valor base

    bool playerTurn = true;
    bool busy;

    // Conecta estos metodos a los botones de la UI pasando el ActionData
    public void OnActionSelected(ActionData action)
    {
        if (!playerTurn || busy) return;
        busy = true;
        minigame.Begin(action, player.level, result => ApplyPlayerAction(action, result));
    }

    void ApplyPlayerAction(ActionData action, MinigameResult r)
    {
        switch (action.type)
        {
            case ActionType.Attack: enemy.TakeDamage(r.total); break;
            case ActionType.Defend: player.shield += r.total; break;
            case ActionType.Heal:   player.Heal(r.total); break;
        }
        Debug.Log($"{action.displayName}: {r.total:0.0} ({r.Accuracy:P0}) P:{r.perfect} G:{r.good} M:{r.miss}");

        busy = false;
        if (enemy.IsDead) { Debug.Log("Victoria"); return; }

        playerTurn = false;
        Invoke(nameof(EnemyTurn), 1f);
    }

    void EnemyTurn()
    {
        // Version simple: el enemigo hace su valor completo. Luego puedes cambiarlo
        // por un minijuego de esquiva donde los aciertos reducen este dano.
        float dmg = enemyAttack.ValueAtLevel(enemy.level);
        player.TakeDamage(dmg);
        Debug.Log($"Enemigo golpea por {dmg:0.0}");

        if (player.IsDead) { Debug.Log("Derrota"); return; }
        playerTurn = true;
    }
}
