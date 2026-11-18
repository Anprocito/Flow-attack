using UnityEngine;
using UnityEngine.UI;

// Crea toda la interfaz por codigo (canvas, textos, imagenes).
// No hay que armar nada en el Inspector.
public class BattleUI : MonoBehaviour
{
    public RectTransform Root { get; private set; }

    Font font;
    Text prompt, info, menu, log;

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

        info   = MakeText(Root, "Info",   new Vector2(0.5f, 0.93f), 30);
        log    = MakeText(Root, "Log",    new Vector2(0.5f, 0.85f), 28);
        menu   = MakeText(Root, "Menu",   new Vector2(0.5f, 0.12f), 38);
        prompt = MakeText(Root, "Prompt", new Vector2(0.5f, 0.12f), 34);
    }

    public void SetPrompt(string s) => prompt.text = s;
    public void SetInfo(string s)   => info.text = s;
    public void SetMenu(string s)   => menu.text = s;
    public void SetLog(string s)    => log.text = s;

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
