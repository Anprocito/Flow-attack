using System;
using System.Collections.Generic;
using UnityEngine;

public enum Judgement { Miss, Good, Perfect }

public struct MinigameResult
{
    public float total;        // suma de valores ganados
    public float maxPossible;  // valor total de la accion (100% de efectividad)
    public int perfect, good, miss;
    public float Accuracy => maxPossible > 0f ? total / maxPossible : 0f;
}

public class RhythmMinigame : MonoBehaviour
{
    [Header("UI")]
    public RectTransform[] laneRoots;   // un RectTransform por carril (la linea de golpeo en y = 0)
    public RectTransform notePrefab;    // prefab de nota (Image)
    public float spawnHeight = 600f;    // altura desde donde caen las notas

    [Header("Input")]
    public KeyCode[] laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

    [Header("Ventanas de timing (segundos)")]
    public float perfectWindow = 0.06f;
    public float goodWindow = 0.15f;
    public float goodMultiplier = 0.6f;

    class Note
    {
        public int lane;
        public float time;
        public float value;
        public RectTransform view;
        public bool done;
    }

    readonly List<Note> notes = new List<Note>();
    ActionData action;
    Action<MinigameResult> onFinished;
    MinigameResult result;
    float startTime, travelTime;
    bool running;

    public void Begin(ActionData data, int level, Action<MinigameResult> finished)
    {
        action = data;
        onFinished = finished;
        travelTime = data.noteTravelTime;

        float totalValue = data.ValueAtLevel(level);
        float valuePerNote = totalValue / data.noteCount;
        result = new MinigameResult { maxPossible = totalValue };

        // Generar secuencia
        notes.Clear();
        int lastLane = -1;
        for (int i = 0; i < data.noteCount; i++)
        {
            int lane;
            do { lane = UnityEngine.Random.Range(0, data.lanes); }
            while (data.lanes > 1 && lane == lastLane);
            lastLane = lane;

            var view = Instantiate(notePrefab, laneRoots[lane]);
            notes.Add(new Note
            {
                lane = lane,
                time = travelTime + i * data.noteInterval, // la primera llega tras la caida inicial
                value = valuePerNote,
                view = view
            });
        }

        startTime = Time.time;
        running = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!running) return;
        float t = Time.time - startTime;

        // Mover notas
        foreach (var n in notes)
        {
            if (n.done) continue;
            float k = (n.time - t) / travelTime; // 1 = arriba, 0 = linea de golpeo
            n.view.anchoredPosition = new Vector2(0f, Mathf.LerpUnclamped(0f, spawnHeight, k));
        }

        // Input
        for (int lane = 0; lane < action.lanes; lane++)
        {
            if (Input.GetKeyDown(laneKeys[lane])) TryHit(lane, t);
        }

        // Notas que se pasaron = Miss
        foreach (var n in notes)
        {
            if (!n.done && t - n.time > goodWindow) Resolve(n, Judgement.Miss);
        }

        // Fin
        if (notes.TrueForAll(n => n.done))
        {
            running = false;
            gameObject.SetActive(false);
            onFinished?.Invoke(result);
        }
    }

    void TryHit(int lane, float t)
    {
        Note target = null;
        foreach (var n in notes)
        {
            if (n.done || n.lane != lane) continue;
            if (Mathf.Abs(n.time - t) > goodWindow) continue;
            if (target == null || n.time < target.time) target = n;
        }
        if (target == null) return; // pulsar de mas no penaliza (puedes agregar penalizacion aqui)

        float diff = Mathf.Abs(target.time - t);
        Resolve(target, diff <= perfectWindow ? Judgement.Perfect : Judgement.Good);
    }

    void Resolve(Note n, Judgement j)
    {
        n.done = true;
        if (n.view) Destroy(n.view.gameObject);

        switch (j)
        {
            case Judgement.Perfect: result.total += n.value; result.perfect++; break;
            case Judgement.Good:    result.total += n.value * goodMultiplier; result.good++; break;
            default:                result.miss++; break;
        }
        // Aqui puedes disparar feedback visual/sonoro por juicio
    }
}
