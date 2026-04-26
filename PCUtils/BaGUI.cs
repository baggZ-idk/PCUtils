using UnityEngine;
using System;
using System.Collections.Generic;

namespace BaGUI
{
    public static class BaGUISettings
    {
        public static Color PanelBackground = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        public static Color PanelHeader = new Color(0.1f, 0.1f, 0.1f, 1f);
        public static Color ButtonColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        public static Color SectionButton = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static Color TextColor = Color.white;
        public static Color InputColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public static int FontSizeHeader = 18;
        public static int FontSizeBody = 14;
        public static int FontSizeButton = 14;
    }

    internal static class Styles
    {
        public static GUIStyle Header;
        public static GUIStyle Body;
        public static GUIStyle Button;
        public static GUIStyle TextField;
        static bool initialized;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            Header = new GUIStyle(GUI.skin.label)
            {
                fontSize = BaGUISettings.FontSizeHeader,
                fontStyle = FontStyle.Bold,
                normal = { textColor = BaGUISettings.TextColor }
            };


            Body = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = BaGUISettings.TextColor },
                wordWrap = true
            };

            Button = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = BaGUISettings.FontSizeButton,
                normal = { textColor = BaGUISettings.TextColor }
            };

            TextField = new GUIStyle(GUI.skin.textField)
            {
                normal = { textColor = BaGUISettings.TextColor },
                padding = new RectOffset(4, 4, 2, 2)
            };

            TextField.normal.background = null;
            TextField.focused.background = null;
        }
    }

    public class Panel
    {
        public Vector2 Position;
        public float Width = 300f;
        public bool Draggable = true;

        private bool dragging;
        private Vector2 dragOffset;

        public string Header;
        public List<UIElement> Elements = new List<UIElement>();

        public Panel(string header, Vector2 position)
        {
            Header = header;
            Position = position;
        }

        public Section CreateSection(string label)
        {
            var s = new Section(this, label);
            Elements.Add(s);
            return s;
        }

        public void Draw()
        {
            Styles.Init();

            float panelHeight = 35f;
            foreach (var e in Elements)
                panelHeight += e.GetHeight(0);

            Rect panelRect = new Rect(Position.x, Position.y, Width, panelHeight + 10);
            Rect headerRect = new Rect(Position.x, Position.y, Width, 30);

            HandleDrag(headerRect);

            GUI.color = BaGUISettings.PanelBackground;
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);

            GUI.color = BaGUISettings.PanelHeader;
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(Position.x + 5, Position.y + 2, Width - 10, 30), Header, Styles.Header);

            float y = Position.y + 35f;
            foreach (var e in Elements)
                y += e.Draw(y, 0);
        }

        void HandleDrag(Rect headerRect)
        {
            if (!Draggable) return;

            if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
            {
                dragging = true;
                dragOffset = Event.current.mousePosition - Position;
            }

            if (Event.current.type == EventType.MouseUp)
                dragging = false;

            if (dragging)
                Position = Event.current.mousePosition - dragOffset;
        }

        public float Slider(Rect rect, float value, float min, float max)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);

            float barHeight = 4f;
            Rect bar = new Rect(rect.x, rect.y + rect.height / 2 - barHeight / 2, rect.width, barHeight);
            GUI.color = BaGUISettings.InputColor;
            GUI.DrawTexture(bar, Texture2D.whiteTexture);

            float t = (value - min) / (max - min);
            float handleX = rect.x + t * rect.width - 6;
            Rect handle = new Rect(handleX, rect.y + rect.height / 2 - 6, 12, 12);

            GUI.color = Color.white;
            GUI.DrawTexture(handle, Texture2D.whiteTexture);

            switch (Event.current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        float mouseT = Mathf.InverseLerp(rect.x, rect.x + rect.width, Event.current.mousePosition.x);
                        value = Mathf.Lerp(min, max, mouseT);
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;
            }

            return Mathf.Clamp(value, min, max);
        }
        
        public bool Checkbox(Rect rect, ref bool value, string label)
        {
            Rect box = new Rect(rect.x, rect.y + 4, 16, 16);

            Color old = GUI.color;
            GUI.color = value ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);

            GUI.color = old;

            GUI.Label(
                new Rect(rect.x + 22, rect.y, rect.width - 22, rect.height),
                label
            );

            if (Event.current.type == EventType.MouseDown &&
                rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                Event.current.Use();
                return true;
            }

            return false;
        }

        public bool Button(Rect rect, string label, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(rect, label, Styles.Button);

            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        public string TextField(Rect rect, string value)
        {
            GUI.color = BaGUISettings.InputColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            return GUI.TextField(rect, value, Styles.TextField);
        }
    }

    public abstract class UIElement
    {
        public Panel Parent;
        public string Label;

        float cachedHeight;
        bool dirty = true;

        protected UIElement(Panel parent, string label)
        {
            Parent = parent;
            Label = label;
        }

        public abstract float Draw(float y, float indent);
        protected abstract float CalculateHeight(float indent);

        public float GetHeight(float indent)
        {
            if (dirty)
            {
                cachedHeight = CalculateHeight(indent);
                dirty = false;
            }
            return cachedHeight;
        }

        protected void MarkDirty() => dirty = true;
    }
    
    public class Checkbox : UIElement
    {
        public bool Value;
        public Action<bool> OnValueChanged;

        public Checkbox(Panel parent, string label, bool def) : base(parent, label)
        {
            Value = def;
        }

        public override float Draw(float y, float parentIndent)
        {
            bool old = Value;

            Parent.Checkbox(
                new Rect(
                    Parent.Position.x + 5 + parentIndent,
                    y,
                    Parent.Width - 10 - parentIndent,
                    25
                ),
                ref Value,
                Label
            );

            if (old != Value)
                OnValueChanged?.Invoke(Value);

            return 30f;
        }

        protected override float CalculateHeight(float parentIndent) => 30f;
    }
    
    public class TextInput : UIElement
    {
        public string Value;
        public Action<string> OnValueChanged;

        public TextInput(Panel parent, string label, string def) : base(parent, label)
        {
            Value = def;
        }

        public override float Draw(float y, float parentIndent)
        {
            GUI.Label(new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10, 20), Label);

            string old = Value;
            Value = Parent.TextField(
                new Rect(Parent.Position.x + 5 + parentIndent, y + 20, Parent.Width - 10, 22),
                Value
            );

            if (old != Value)
                OnValueChanged?.Invoke(Value);

            return 50f;
        }

        protected override float CalculateHeight(float indent) => 50f;
    }
    
    public class Label : UIElement
    {
        public Label(Panel parent, string text) : base(parent, text) { }

        public override float Draw(float y, float parentIndent)
        {
            GUI.Label(
                new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, 20),
                Label
            );

            return 20f;
        }

        protected override float CalculateHeight(float indent) => 20f;
    }

    public class Section : UIElement
    {
        public List<UIElement> Elements = new List<UIElement>();
        public bool Collapsed = true;

        public Section(Panel p, string l) : base(p, l) { }
        
        public Label AddLabel(string text)
        {
            var e = new Label(Parent, text);
            Elements.Add(e);
            return e;
        }

        public Slider AddSlider(string label, float min, float max, float def)
        {
            var e = new Slider(Parent, label, min, max, def);
            Elements.Add(e);
            return e;
        }

        public Checkbox AddCheckbox(string label, bool def)
        {
            var e = new Checkbox(Parent, label, def);
            Elements.Add(e);
            return e;
        }

        public TextInput AddTextInput(string label, string def)
        {
            var e = new TextInput(Parent, label, def);
            Elements.Add(e);
            return e;
        }

        public Button AddButton(string label, Action onClick)
        {
            var e = new Button(Parent, label, onClick);
            Elements.Add(e);
            return e;
        }

        public override float Draw(float y, float indent)
        {
            Rect r = new Rect(Parent.Position.x + 5 + indent, y, Parent.Width - 10 - indent, 25);

            if (Parent.Button(r, Label, BaGUISettings.SectionButton))
            {
                Collapsed = !Collapsed;
                MarkDirty();
            }

            float h = 30f;

            if (!Collapsed)
            {
                float cy = y + 30f;
                foreach (var e in Elements)
                {
                    float eh = e.Draw(cy, indent + 15);
                    cy += eh;
                    h += eh;
                }
            }

            return h;
        }

        protected override float CalculateHeight(float indent)
        {
            float h = 30f;

            if (!Collapsed)
                foreach (var e in Elements)
                    h += e.GetHeight(indent + 15);

            return h;
        }
    }

    public class Slider : UIElement
    {
        public float Value;
        public float Min, Max;
        public Action<float> OnValueChanged;

        public Slider(Panel p, string l, float min, float max, float def) : base(p, l)
        {
            Min = min; Max = max; Value = def;
        }

        public override float Draw(float y, float indent)
        {
            GUI.Label(new Rect(Parent.Position.x + 5 + indent, y, Parent.Width, 20), Label + " : " + Value.ToString("0.00"));

            float old = Value;
            Value = Parent.Slider(new Rect(Parent.Position.x + 5 + indent, y + 20, Parent.Width - 10 - indent, 20), Value, Min, Max);

            if (old != Value)
                OnValueChanged?.Invoke(Value);

            return 45f;
        }

        protected override float CalculateHeight(float indent) => 45f;
    }

    public class Button : UIElement
    {
        public Action OnClick;

        public Button(Panel p, string l, Action a) : base(p, l) => OnClick = a;

        public override float Draw(float y, float indent)
        {
            Rect r = new Rect(Parent.Position.x + 5 + indent, y, Parent.Width - 10 - indent, 25);
            if (Parent.Button(r, Label, BaGUISettings.ButtonColor))
                OnClick?.Invoke();
            return 30f;
        }

        protected override float CalculateHeight(float indent) => 30f;
    }
}
