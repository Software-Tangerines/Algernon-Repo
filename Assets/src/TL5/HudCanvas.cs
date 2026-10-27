using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/*
F6 HudCanvas: builds all of F6's UI from code at runtime (canvas, HUD bar, menu panels, buttons, sliders).

Building in code means no prefabs and no edits to the shared scenes, so F6 can't cause scene merge
conflicts. HudFacade owns one of these and decides WHAT to show; this class only knows HOW things look.
To restyle the UI, change the colours and sizes here.

Layout is designed for 1920x1080 and scales to any resolution (CanvasScaler).
*/
public class HudCanvas
{
    // ---------- Style ----------
    public static readonly Color TextColor = Color.white;
    public static readonly Color UrgentColor = new Color(1f, 0.25f, 0.25f);
    private static readonly Color DistortionColor = new Color(1f, 0.85f, 0.3f);
    private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.7f);
    private static readonly Color BoxColor = new Color(0.1f, 0.1f, 0.14f, 0.95f);
    private static readonly Color ButtonColor = new Color(0.22f, 0.24f, 0.32f);
    private const int SortingOrder = 100;      // draws above other teams' canvases (e.g. TL6's PlayerHUD)

    // One menu panel: a dimmed full-screen backdrop with a centred box of title, body text and controls.
    public class Panel
    {
        public GameObject Root;
        public RectTransform Box;
        public TMP_Text Title;
        public TMP_Text Body;
        public readonly List<Selectable> Controls = new List<Selectable>();

        // What keyboard/gamepad focus lands on when the panel opens.
        public GameObject FirstControl
        {
            get
            {
                foreach (var c in Controls)
                    if (c != null && c.gameObject.activeSelf && c.interactable) return c.gameObject;
                return null;
            }
        }

        // Body text is hidden when empty so it doesn't leave a gap.
        public void SetBody(string text)
        {
            Body.text = text;
            Body.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
    }

    public readonly GameObject Root;
    public readonly GameObject HudBar;
    public readonly TMP_Text TimerText;
    public readonly TMP_Text DistortionText;
    public readonly TMP_Text ObjectiveText;
    public readonly TMP_Text DeathsText;

    private readonly RectTransform canvasRect;

    public HudCanvas(Transform parent)
    {
        Root = new GameObject("F6_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Root.transform.SetParent(parent, false);

        var canvas = Root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;

        var scaler = Root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = (RectTransform)Root.transform;

        // HUD bar: timer top-centre, distortion warning under it, objectives and deaths top-right.
        // TL6's health readout is expected top-left, so this layout leaves that corner free.
        var bar = NewRect("HudBar", canvasRect);
        Stretch(bar);
        HudBar = bar.gameObject;
        TimerText = NewText("Timer", bar, 64, TextAlignmentOptions.Top, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(400f, 80f));
        DistortionText = NewText("Distortion", bar, 34, TextAlignmentOptions.Top, new Vector2(0.5f, 1f), new Vector2(0f, -105f), new Vector2(1000f, 50f));
        DistortionText.color = DistortionColor;
        ObjectiveText = NewText("Objective", bar, 38, TextAlignmentOptions.TopRight, new Vector2(1f, 1f), new Vector2(-40f, -24f), new Vector2(700f, 50f));
        DeathsText = NewText("Deaths", bar, 32, TextAlignmentOptions.TopRight, new Vector2(1f, 1f), new Vector2(-40f, -76f), new Vector2(700f, 50f));

        EnsureEventSystem();
    }

    // ---------- Panels and controls ----------

    public Panel CreatePanel(string name, string title)
    {
        var root = NewRect(name, canvasRect);
        Stretch(root);
        var dim = root.gameObject.AddComponent<Image>();
        dim.color = DimColor;                 // also blocks clicks from reaching the game behind it

        var box = NewRect("Box", root);
        box.anchorMin = box.anchorMax = box.pivot = new Vector2(0.5f, 0.5f);
        box.sizeDelta = new Vector2(640f, 0f);
        box.gameObject.AddComponent<Image>().color = BoxColor;

        var layout = box.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(48, 48, 40, 40);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        box.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var panel = new Panel { Root = root.gameObject, Box = box };
        panel.Title = NewLayoutText("Title", box, title, 56, FontStyles.Bold);
        panel.Body = NewLayoutText("Body", box, "", 32, FontStyles.Normal);
        panel.SetBody("");

        root.gameObject.SetActive(false);
        return panel;
    }

    public Button AddButton(Panel panel, string label, Action onClick)
    {
        var go = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        go.name = "Button_" + label;
        go.transform.SetParent(panel.Box, false);
        go.AddComponent<LayoutElement>().preferredHeight = 68f;
        go.GetComponent<Image>().color = ButtonColor;

        var text = go.GetComponentInChildren<TMP_Text>();
        text.text = label;
        text.fontSize = 34;
        text.color = TextColor;

        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => onClick());
        panel.Controls.Add(button);
        return button;
    }

    // A label line followed by a 0..1 slider.
    public Slider AddSlider(Panel panel, string label, float value, Action<float> onChanged)
    {
        NewLayoutText("Label_" + label, panel.Box, label, 28, FontStyles.Normal);

        var go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        go.name = "Slider_" + label;
        go.transform.SetParent(panel.Box, false);
        go.AddComponent<LayoutElement>().preferredHeight = 32f;

        var slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(v => onChanged(v));
        panel.Controls.Add(slider);
        return slider;
    }

    // Removes every button/slider (used to rebuild the level-select list). Title and body stay.
    public void ClearControls(Panel panel)
    {
        foreach (var c in panel.Controls)
            if (c != null) UnityEngine.Object.Destroy(c.gameObject);
        panel.Controls.Clear();
    }

    // Moves keyboard focus to a control so arrow keys / Enter work without a mouse.
    public static void Focus(GameObject target)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(target);
    }

    // UI clicks need exactly one EventSystem. Breana's Menu scene has one; create one if this scene doesn't.
    // The project uses the new Input System, so it gets the Input System UI module.
    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
        new GameObject("F6_EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // ---------- Small builders ----------

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // A fixed-position HUD text anchored to a screen edge.
    private static TMP_Text NewText(string name, RectTransform parent, float size, TextAlignmentOptions align,
                                    Vector2 anchor, Vector2 position, Vector2 boxSize)
    {
        var rt = NewRect(name, parent);
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = boxSize;

        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = align;
        text.color = TextColor;
        text.raycastTarget = false;            // HUD text must never eat clicks meant for the game
        text.text = "";
        return text;
    }

    // A text line inside a panel's vertical layout.
    private static TMP_Text NewLayoutText(string name, RectTransform parent, string content, float size, FontStyles style)
    {
        var rt = NewRect(name, parent);
        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TextColor;
        text.raycastTarget = false;
        return text;
    }
}
