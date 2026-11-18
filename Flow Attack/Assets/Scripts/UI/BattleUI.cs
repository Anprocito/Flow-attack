using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Crea toda la interfaz por codigo (canvas, textos, imagenes).
// No hay que armar nada en el Inspector.
public class BattleUI : MonoBehaviour
{
    public RectTransform Root { get; private set; }

    Font font;
    Text prompt, info, menu, log;
    GameObject restartButton;

    void Awake()
    {
        // "LegacyRuntime.ttf" existe en Unity 2022.2+. Si no, usa una fuente del sistema.
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);

        var canvasGO = new GameObject("BattleCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Root = canvasGO.GetComponent<RectTransform>();

        info = MakeText(Root, "Info", new Vector2(0.5f, 0.93f), 30);
        log = MakeText(Root, "Log", new Vector2(0.5f, 0.85f), 28);
        menu = MakeText(Root, "Menu", new Vector2(0.5f, 0.12f), 38);
        prompt = MakeText(Root, "Prompt", new Vector2(0.5f, 0.12f), 34);

        BuildRestartButton();
    }

    // ---------- Reinicio ----------

    void BuildRestartButton()
    {
        EnsureEventSystem();

        Image bg = MakeImage(Root, "RestartButton", new Color(0.15f, 0.15f, 0.15f, 0.9f));
        bg.raycastTarget = true; // el boton necesita recibir clics

        RectTransform rt = bg.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.3f);
        rt.sizeDelta = new Vector2(320f, 80f);
        rt.anchoredPosition = Vector2.zero;

        Button button = bg.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        button.onClick.AddListener(Restart);

        Text label = MakeText(rt, "Label", new Vector2(0.5f, 0.5f), 34);
        label.text = "Reiniciar (R)";

        restartButton = bg.gameObject;
        restartButton.SetActive(false);
    }

    // El boton necesita un EventSystem compatible con el nuevo Input System.
    void EnsureEventSystem()
    {
        EventSystem es = FindFirstObjectByType<EventSystem>();
        if (es == null) es = new GameObject("EventSystem").AddComponent<EventSystem>();

        // El modulo viejo no funciona con el nuevo Input System.
        StandaloneInputModule legacy = es.GetComponent<StandaloneInputModule>();
        if (legacy != null) Destroy(legacy);

        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            var module = es.gameObject.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }

    public void ShowRestart(bool show) => restartButton.SetActive(show);

    // Recarga la escena actual. Debe estar en File > Build Settings.
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetPrompt(string s) => prompt.text = s;
    public void SetInfo(string s) => info.text = s;
    public void SetMenu(string s) => menu.text = s;
    public void SetLog(string s) => log.text = s;

    public Text MakeText(Transform parent, string name, Vector2 anchor, int size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1000f, 80f);
        rt.anchoredPosition = Vector2.zero;

        var outline = go.AddComponent<Outline>(); // para que se lea sobre la escena 3D
        outline.effectColor = Color.black;
        return t;
    }

    public Image MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }
}