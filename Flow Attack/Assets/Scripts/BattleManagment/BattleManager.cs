using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Maneja todo: acercarse a un enemigo, apretar E, turnos y victoria/derrota.
// Se pone en un GameObject vacio. Agrega solo BattleUI y RhythmMinigame.
[RequireComponent(typeof(BattleUI), typeof(RhythmMinigame))]
public class BattleManager : MonoBehaviour
{
    [Header("Exploracion")]
    public float interactRange = 2.5f;
    public Key interactKey = Key.E;

    [Header("Acciones del jugador (valor base)")]
    public float playerAttackValue = 20f;
    public float playerHealValue = 25f;
    public float maxMultiplier = 2.5f;   // multiplicador con 100% de precision

    [Header("Dificultad: se aplica por cada accion ya tomada en el combate")]
    public int baseNotes = 4;            // +1 nota por accion
    public float baseTravelTime = 1.6f;  // segundos de caida
    public float minTravelTime = 0.7f;
    public float travelReductionPerAction = 0.1f;
    public float baseInterval = 0.6f;    // segundos entre notas
    public float minInterval = 0.3f;
    public float intervalReductionPerAction = 0.04f;

    enum State { Explore, PlayerMenu, Minigame, EnemyTurn, Ended }
    State state = State.Explore;

    BattleUI ui;
    RhythmMinigame minigame;
    PlayerController controller;
    PlayerHealth playerHealth;
    PlayerStats playerStats;
    CombatEnemy[] enemies;
    CombatEnemy current;
    int actionsTaken;
    int defeatedCount;

    void Awake()
    {
        ui = GetComponent<BattleUI>();
        minigame = GetComponent<RhythmMinigame>();
    }

    void Start()
    {
        controller = FindFirstObjectByType<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("BattleManager: no hay PlayerController en la escena.", this);
            enabled = false;
            return;
        }
        playerHealth = controller.GetComponent<PlayerHealth>();
        playerStats = controller.GetComponent<PlayerStats>();
        enemies = FindObjectsByType<CombatEnemy>(FindObjectsSortMode.None);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        switch (state)
        {
            case State.Explore:
                UpdateExplore(kb);
                break;
            case State.PlayerMenu:
                if (kb.digit1Key.wasPressedThisFrame) ChooseAction(true);
                else if (kb.digit2Key.wasPressedThisFrame) ChooseAction(false);
                break;
        }
    }

    // ---------- Exploracion ----------

    void UpdateExplore(Keyboard kb)
    {
        CombatEnemy near = null;
        float best = interactRange;
        foreach (CombatEnemy e in enemies)
        {
            if (e.defeated) continue;
            Vector3 d = e.transform.position - controller.transform.position;
            d.y = 0f;
            if (d.magnitude < best) { best = d.magnitude; near = e; }
        }

        ui.SetPrompt(near != null ? $"Presiona {interactKey} para combatir con {near.displayName}" : "");

        if (near != null && kb[interactKey].wasPressedThisFrame)
            StartBattle(near);
    }

    // ---------- Combate ----------

    void StartBattle(CombatEnemy enemy)
    {
        CancelInvoke(nameof(ClearLog));
        current = enemy;
        actionsTaken = 0;

        controller.enabled = false;                       // frena el movimiento
        playerHealth.Heal(playerHealth.MaxHealth);        // siempre arrancas con la vida llena

        ui.SetPrompt("");
        ui.SetLog($"¡Combate contra {enemy.displayName}!");
        ShowPlayerMenu();
    }

    void ShowPlayerMenu()
    {
        state = State.PlayerMenu;
        RefreshInfo();
        ui.SetMenu("[1] Atacar        [2] Curar");
    }

    void RefreshInfo()
    {
        ui.SetInfo($"Jugador {playerHealth.CurrentHealth:0}/{playerHealth.MaxHealth:0}     |     " +
                   $"{current.displayName} {current.Hp:0}/{current.maxHp:0}");
    }

    void ChooseAction(bool attack)
    {
        state = State.Minigame;
        ui.SetMenu("");

        // Cada accion ya tomada suma una nota y acelera el minijuego.
        int n = actionsTaken;
        int notes = baseNotes + n;
        float travel = Mathf.Max(minTravelTime, baseTravelTime - travelReductionPerAction * n);
        float interval = Mathf.Max(minInterval, baseInterval - intervalReductionPerAction * n);
        actionsTaken++;

        minigame.Begin(notes, travel, interval, accuracy => OnMinigameDone(attack, accuracy));
    }

    void OnMinigameDone(bool attack, float accuracy)
    {
        float mult = accuracy * maxMultiplier; // 0 sin aciertos, 2.5 con todo perfecto

        if (attack)
        {
            float dmg = playerStats.GetFinalDamage(playerAttackValue) * mult;
            current.TakeDamage(dmg);
            ui.SetLog($"Atacaste por {dmg:0}  (x{mult:0.00})");
        }
        else
        {
            float heal = playerHealValue * mult;
            playerHealth.Heal(heal);
            ui.SetLog($"Te curaste {heal:0}  (x{mult:0.00})");
        }

        RefreshInfo();

        if (current.IsDead) { WinBattle(); return; }
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        state = State.EnemyTurn;
        yield return new WaitForSeconds(1.2f);

        if (current.ChooseHeal())
        {
            current.Heal(current.healValue);
            ui.SetLog($"{current.displayName} se curó {current.healValue:0}");
        }
        else
        {
            playerHealth.TakeDamage(current.attackValue);
            ui.SetLog($"{current.displayName} te golpeó por {current.attackValue:0}");
        }
        RefreshInfo();

        yield return new WaitForSeconds(1.2f);

        if (playerHealth.IsDead) { LoseBattle(); yield break; }
        ShowPlayerMenu();
    }

    void WinBattle()
    {
        current.defeated = true;
        current.gameObject.SetActive(false);
        defeatedCount++;
        ui.SetInfo("");

        if (defeatedCount >= enemies.Length)
        {
            state = State.Ended;
            ui.SetLog("¡VICTORIA! Derrotaste a todos los enemigos");
            return;
        }

        ui.SetLog($"Derrotaste a {current.displayName}  ({defeatedCount}/{enemies.Length})");
        Invoke(nameof(ClearLog), 2.5f);
        controller.enabled = true; // devuelve el movimiento
        state = State.Explore;
    }

    void LoseBattle()
    {
        state = State.Ended;
        ui.SetMenu("");
        ui.SetLog("DERROTA");
        // PlayerHealth ya deshabilita el PlayerController al morir.
    }

    void ClearLog() => ui.SetLog("");
}
