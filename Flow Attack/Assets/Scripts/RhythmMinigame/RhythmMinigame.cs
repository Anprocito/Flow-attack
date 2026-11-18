using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Minijuego de ritmo. Recibe cantidad de notas y velocidad, y devuelve la precision (0 a 1).
// Perfect cuenta 1.0, Good cuenta goodMultiplier, Miss cuenta 0.
public class RhythmMinigame : MonoBehaviour
{
    [Header("Input")]
    public Key[] laneKeys = { Key.D, Key.F, Key.J, Key.K };

    [Header("Ventanas de timing (segundos)")]
    public float perfectWindow = 0.07f;
    public float goodWindow = 0.16f;
    public float goodMultiplier = 0.6f;

    [Header("Visual")]
    public float laneWidth = 90f;
    public float laneHeight = 420f;
    public float hitLineY = 60f;

    class Note
    {
        public int lane;
        public float time;
        public RectTransform view;
        public bool done;
    }

    readonly List<Note> notes = new List<Note>();
    BattleUI ui;
    RectTransform panel;
    RectTransform[] lanes;
    Text feedback;
    Action<float> onFinished;
    float startTime, travelTime, score;
    int noteCount;
    bool running;

    // count: cantidad de notas, travel: segundos que tarda en caer, interval: segundos entre notas.
    public void Begin(int count, float travel, float interval, Action<float> finished)
    {
        if (ui == null) ui = GetComponent<BattleUI>();
        if (panel == null) BuildUI();

        notes.Clear();
        score = 0f;
        noteCount = count;
        travelTime = travel;
        onFinished = finished;
        feedback.text = "";

        for (int i = 0; i < count; i++)
        {
            int lane = UnityEngine.Random.Range(0, lanes.Length);

            RectTransform view = ui.MakeImage(lanes[lane], "Note", new Color(0.3f, 0.9f, 1f)).rectTransform;
            view.anchorMin = view.anchorMax = new Vector2(0.5f, 0f);
            view.sizeDelta = new Vector2(laneWidth - 20f, 26f);

            notes.Add(new Note
            {
                lane = lane,
                time = travel + 0.4f + i * interval,
                view = view
            });
        }

        panel.gameObject.SetActive(true);
        startTime = Time.time;
        running = true;
        MoveNotes(0f);
    }

    void BuildUI()
    {
        int n = laneKeys.Length;

        panel = new GameObject("RhythmPanel", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        panel.SetParent(ui.Root, false);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        panel.GetComponent<Image>().raycastTarget = false;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.sizeDelta = new Vector2(n * laneWidth, laneHeight);
        panel.anchoredPosition = new Vector2(0f, -200f);

        lanes = new RectTransform[n];
        for (int i = 0; i < n; i++)
        {
            RectTransform rt = ui.MakeImage(panel, "Lane" + i, new Color(1f, 1f, 1f, 0.08f)).rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(laneWidth - 6f, laneHeight);
            rt.anchoredPosition = new Vector2((i - (n - 1) * 0.5f) * laneWidth, 0f);
            lanes[i] = rt;

            Text label = ui.MakeText(rt, "Key", new Vector2(0.5f, 0f), 28);
            label.text = laneKeys[i].ToString();
            label.rectTransform.anchoredPosition = new Vector2(0f, 25f);
        }

        RectTransform line = ui.MakeImage(panel, "HitLine", new Color(1f, 0.9f, 0.2f)).rectTransform;
        line.anchorMin = line.anchorMax = new Vector2(0.5f, 0f);
        line.sizeDelta = new Vector2(n * laneWidth, 4f);
        line.anchoredPosition = new Vector2(0f, hitLineY);

        feedback = ui.MakeText(panel, "Feedback", new Vector2(0.5f, -0.08f), 40);
    }

    void Update()
    {
        if (!running) return;
        float t = Time.time - startTime;

        MoveNotes(t);

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            for (int lane = 0; lane < lanes.Length; lane++)
                if (kb[laneKeys[lane]].wasPressedThisFrame) TryHit(lane, t);
        }

        // Notas que se pasaron = Miss
        foreach (Note n in notes)
            if (!n.done && t - n.time > goodWindow) Resolve(n, 0f, "MISS");

        if (notes.TrueForAll(n => n.done))
        {
            running = false;
            panel.gameObject.SetActive(false);
            onFinished?.Invoke(noteCount > 0 ? score / noteCount : 0f);
        }
    }

    void MoveNotes(float t)
    {
        float dist = laneHeight - hitLineY - 15f;
        foreach (Note n in notes)
        {
            if (n.done) continue;
            float k = (n.time - t) / travelTime; // 1 = arriba, 0 = linea de golpeo

            bool visible = k <= 1f;
            if (n.view.gameObject.activeSelf != visible) n.view.gameObject.SetActive(visible);
            n.view.anchoredPosition = new Vector2(0f, hitLineY + k * dist);
        }
    }

    void TryHit(int lane, float t)
    {
        Note target = null;
        foreach (Note n in notes)
        {
            if (n.done || n.lane != lane) continue;
            if (Mathf.Abs(n.time - t) > goodWindow) continue;
            if (target == null || n.time < target.time) target = n;
        }
        if (target == null) return; // pulsar de mas no penaliza

        float diff = Mathf.Abs(target.time - t);
        if (diff <= perfectWindow) Resolve(target, 1f, "PERFECT");
        else Resolve(target, goodMultiplier, "GOOD");
    }

    void Resolve(Note n, float value, string text)
    {
        n.done = true;
        score += value;
        feedback.text = text;
        if (n.view) Destroy(n.view.gameObject);
    }
}
