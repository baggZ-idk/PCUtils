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
            var section = new Section(this, label);
            Elements.Add(section);
            return section;
        }

        public void Draw()
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = BaGUISettings.FontSizeHeader,
                fontStyle = FontStyle.Bold,
                normal = { textColor = BaGUISettings.TextColor }
            };

            Rect headerRect = new Rect(Position.x, Position.y, Width, 30);

            if (Draggable && Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
            {
                dragging = true;
                dragOffset = Event.current.mousePosition - Position;
            }

            if (Event.current.type == EventType.MouseUp)
                dragging = false;

            if (dragging)
                Position = Event.current.mousePosition - dragOffset;

            float panelHeight = 35f;
            foreach (var e in Elements)
                panelHeight += e.GetHeight(0);

            Rect panelRect = new Rect(Position.x, Position.y, Width, panelHeight + 10);

            GUI.color = BaGUISettings.PanelBackground;
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.color = BaGUISettings.PanelHeader;
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(Position.x + 5, Position.y + 2, Width - 10, 30), Header, headerStyle);

            float y = Position.y + 35f;
            foreach (var e in Elements)
                y += e.Draw(y, 0);
        }

        public float Slider(Rect rect, float value, float min, float max)
        {
            float barHeight = 4f;
            Rect bar = new Rect(rect.x, rect.y + rect.height / 2 - barHeight / 2, rect.width, barHeight);
            GUI.color = BaGUISettings.InputColor;
            GUI.DrawTexture(bar, Texture2D.whiteTexture);

            float t = (value - min) / (max - min);
            float handleX = rect.x + t * rect.width - 6;
            Rect handle = new Rect(handleX, rect.y + rect.height / 2 - 6, 12, 12);

            GUI.color = Color.white;
            GUI.DrawTexture(handle, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    float mouseT = Mathf.InverseLerp(rect.x, rect.x + rect.width, Event.current.mousePosition.x);
                    value = Mathf.Lerp(min, max, mouseT);
                }
            }

            return Mathf.Clamp(value, min, max);
        }

        public bool Checkbox(Rect rect, ref bool value, string label)
        {
            Rect box = new Rect(rect.x, rect.y + 4, 16, 16);
            Color old = GUI.color;
            GUI.color = value ? Color.white : new Color(0.35f, 0.35f, 0.35f, 1f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(new Rect(rect.x + 22, rect.y, rect.width - 22, rect.height), label);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                return true;
            }
            return false;
        }

        public bool Button(Rect rect, string label, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = BaGUISettings.FontSizeButton,
                normal = { textColor = BaGUISettings.TextColor }
            };
            GUI.Label(rect, label, textStyle);

            GUI.color = old;

            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        public string TextField(Rect rect, string value)
        {
            Color old = GUI.color;
            GUI.color = BaGUISettings.InputColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;

            GUIStyle style = new GUIStyle(GUI.skin.textField);
            style.normal.background = null;
            style.hover.background = null;
            style.active.background = null;
            style.focused.background = null;
            style.onNormal.background = null;
            style.onHover.background = null;
            style.onActive.background = null;
            style.onFocused.background = null;
            style.normal.textColor = BaGUISettings.TextColor;
            style.padding = new RectOffset(4, 4, 2, 2);

            return GUI.TextField(rect, value, style);
        }
    }

    public abstract class UIElement
    {
        public Panel Parent;
        public string Label;
        public float Indent = 0f;

        public UIElement(Panel parent, string label)
        {
            Parent = parent;
            Label = label;
        }

        public abstract float Draw(float y, float parentIndent);
        public abstract float GetHeight(float parentIndent);
    }

    public class Section : UIElement
    {
        public List<UIElement> Elements = new List<UIElement>();
        public bool Collapsed = true;

        public Section(Panel parent, string label) : base(parent, label) { }

        public void AddLabel(string text)
        {
            Elements.Add(new Label(Parent, text));
        }

        public Button AddButton(string label, Action onClick)
        {
            var button = new Button(Parent, label, onClick);
            Elements.Add(button);
            return button;
        }

        public Slider AddSlider(string label, float min, float max, float defaultValue)
        {
            var slider = new Slider(Parent, label, min, max, defaultValue);
            Elements.Add(slider);
            return slider;
        }

        public Checkbox AddCheckbox(string label, bool defaultValue)
        {
            var checkbox = new Checkbox(Parent, label, defaultValue);
            Elements.Add(checkbox);
            return checkbox;
        }

        public TextInput AddTextInput(string label, string defaultValue)
        {
            var input = new TextInput(Parent, label, defaultValue);
            Elements.Add(input);
            return input;
        }

        public override float Draw(float y, float parentIndent)
        {
            float height = 0f;

            Rect buttonRect = new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, 25);
            if (Parent.Button(buttonRect, Label, BaGUISettings.SectionButton))
                Collapsed = !Collapsed;

            height += 30f;

            if (!Collapsed)
            {
                foreach (var e in Elements)
                    height += e.Draw(y + height, parentIndent + 15f);
            }

            return height;
        }

        public override float GetHeight(float parentIndent)
        {
            float height = 30f;
            if (!Collapsed)
            {
                foreach (var e in Elements)
                    height += e.GetHeight(parentIndent + 15f);
            }
            return height;
        }
    }

    public class Slider : UIElement
    {
        public float Value;
        public float Min;
        public float Max;
        public Action<float> OnValueChanged;

        public Slider(Panel parent, string label, float min, float max, float def) : base(parent, label)
        {
            Min = min;
            Max = max;
            Value = def;
        }

        public override float Draw(float y, float parentIndent)
        {
            GUI.Label(new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, 20),
                Label + " : " + Value.ToString("0.00"));

            float oldValue = Value;
            Value = Parent.Slider(new Rect(Parent.Position.x + 5 + parentIndent, y + 20, Parent.Width - 10 - parentIndent, 20), Value, Min, Max);

            if (oldValue != Value)
                OnValueChanged?.Invoke(Value);

            return 45f;
        }

        public override float GetHeight(float parentIndent) => 45f;
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
            Parent.Checkbox(new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, 25), ref Value, Label);

            if (old != Value)
                OnValueChanged?.Invoke(Value);

            return 30f;
        }

        public override float GetHeight(float parentIndent) => 30f;
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
            GUI.Label(new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, 20), Label);
            string old = Value;
            Value = Parent.TextField(new Rect(Parent.Position.x + 5 + parentIndent, y + 20, Parent.Width - 10 - parentIndent, 22), Value);
            if (old != Value)
                OnValueChanged?.Invoke(Value);
            return 50f;
        }

        public override float GetHeight(float parentIndent) => 50f;
    }

    public class Button : UIElement
    {
        public Action OnClick;

        public Button(Panel parent, string label, Action onClick) : base(parent, label)
        {
            OnClick = onClick;
        }

        public override float Draw(float y, float parentIndent)
        {
            Rect r = new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, 25);
            if (Parent.Button(r, Label, BaGUISettings.ButtonColor))
                OnClick?.Invoke();
            return 30f;
        }

        public override float GetHeight(float parentIndent) => 30f;
    }

    public class Label : UIElement
    {
        public Label(Panel parent, string text) : base(parent, text) { }

        public override float Draw(float y, float parentIndent)
        {
            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = BaGUISettings.TextColor }, wordWrap = true };
            float height = bodyStyle.CalcHeight(new GUIContent(Label), Parent.Width - 10 - parentIndent);
            GUI.Label(new Rect(Parent.Position.x + 5 + parentIndent, y, Parent.Width - 10 - parentIndent, height), Label, bodyStyle);
            return height + 5f;
        }

        public override float GetHeight(float parentIndent)
        {
            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            return bodyStyle.CalcHeight(new GUIContent(Label), Parent.Width - 10 - parentIndent) + 5f;
        }
    }

    public static class BaGUI
    {
        public static Slider CreateSlider(Panel parent, string name, float min, float max, float def) => new Slider(parent, name, min, max, def);
        public static Checkbox CreateCheckbox(Panel parent, string name, bool def) => new Checkbox(parent, name, def);
        public static Button CreateButton(Panel parent, string name, Action onClick) => new Button(parent, name, onClick);
        public static Label CreateLabel(Panel parent, string text) => new Label(parent, text);
        public static Section CreateSection(Panel parent, string name) => new Section(parent, name);
        
        public static TextInput CreateTextInput(Panel parent, string name, string def) => new TextInput(parent, name, def);
    }
}