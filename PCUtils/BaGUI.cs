using UnityEngine;
using System;
using System.Collections.Generic;

namespace BaGUI
{
    public class Panel
    {
        public Vector2 Position;
        public float Width = 300f;
        public bool Draggable = true;
        private bool dragging = false;
        private Vector2 dragOffset;

        public string Header;
        public List<PanelItem> Items = new List<PanelItem>();

        public Panel(string header, Vector2 position)
        {
            Header = header;
            Position = position;
        }
        
        float SquareSlider(Rect rect, float value, float min, float max)
        {
            float barHeight = 4f;
            Rect bar = new Rect(rect.x, rect.y + rect.height / 2 - barHeight / 2, rect.width, barHeight);

            GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
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
        
        bool SquareCheckbox(Rect rect, ref bool value, string label)
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
        
        bool SolidButton(Rect rect, string label, Color color)
        {
            Color old = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            GUI.color = Color.white;

            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            GUI.Label(rect, label, textStyle);

            GUI.color = old;

            return Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        public void Draw()
        {
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = Color.white },
                wordWrap = true
            };

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14 };
            GUIStyle checkboxStyle = new GUIStyle(GUI.skin.toggle) { fontSize = 14, normal = { textColor = Color.white } };

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

            float yOffset = 35f;
            foreach (var item in Items)
            {
                if (item.Type == PanelItemType.Text)
                    yOffset += bodyStyle.CalcHeight(new GUIContent(item.Label), Width - 10) + 5f;
                else if (item.Type == PanelItemType.Button || item.Type == PanelItemType.Checkbox)
                    yOffset += 30f;
                else if (item.Type == PanelItemType.Slider)
                    yOffset += 45f;
                else if (item.Type == PanelItemType.Section)
                {
                    yOffset += 30f;
                    if (!item.Collapsed)
                        foreach (var sub in item.SubItems)
                            yOffset += (sub.Type == PanelItemType.Text ? bodyStyle.CalcHeight(new GUIContent(sub.Label), Width - 20) :
                                sub.Type == PanelItemType.Slider ? 45f : 30f) + 5f;
                }
            }

            float panelHeight = yOffset + 10f;
            Rect panelRect = new Rect(Position.x, Position.y, Width, panelHeight);

            GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(Position.x + 5, Position.y + 2, Width - 10, 30), Header, headerStyle);

            float y = Position.y + 35f;
            foreach (var item in Items)
            {
                switch (item.Type)
                {
                    case PanelItemType.Text:
                    {
                        float textHeight = bodyStyle.CalcHeight(new GUIContent(item.Label), Width - 10);
                        GUI.Label(new Rect(Position.x + 5, y, Width - 10, textHeight), item.Label, bodyStyle);
                        y += textHeight + 5f;
                        break;
                    }
                    case PanelItemType.Button:
                    {
                        Rect btn = new Rect(Position.x + 5, y, Width - 10, 25);

                        if (SolidButton(btn, item.Label, new Color(0.25f, 0.25f, 0.25f, 1f)))
                            item.Action?.Invoke();
                        y += 30f;
                        break;
                    }
                    case PanelItemType.Checkbox:
                    {
                        SquareCheckbox(new Rect(Position.x + 5, y, Width - 10, 25), ref item.BoolValue, item.Label);
                        y += 30f;
                        break;
                    }
                    case PanelItemType.Slider:
                    {
                        GUI.Label(new Rect(Position.x + 5, y, Width - 10, 20),
                            item.Label + " : " + item.FloatValue.ToString("0.00"), bodyStyle);
                        item.FloatValue = SquareSlider(
                            new Rect(Position.x + 5, y + 20, Width - 10, 20),
                            item.FloatValue,
                            item.Min,
                            item.Max
                        );
                        break;
                    }
                    case PanelItemType.Section:
                    {
                        Rect btn = new Rect(Position.x + 5, y, Width - 10, 25);

                        if (SolidButton(btn, item.Label + (item.Collapsed ? " [+]" : " [-]"),
                                new Color(0.2f, 0.2f, 0.2f, 1f)))
                            item.Collapsed = !item.Collapsed;
                        y += 30f;

                        if (!item.Collapsed)
                            foreach (var sub in item.SubItems)
                            {
                                switch (sub.Type)
                                {
                                    case PanelItemType.Text:
                                    {
                                        float subHeight = bodyStyle.CalcHeight(new GUIContent(sub.Label), Width - 20);
                                        GUI.Label(new Rect(Position.x + 15, y, Width - 20, subHeight), sub.Label,
                                            bodyStyle);
                                        y += subHeight + 5f;
                                        break;
                                    }

                                    case PanelItemType.Button:
                                    {
                                        Rect subBtn = new Rect(Position.x + 15, y, Width - 20, 25);

                                        if (SolidButton(subBtn, sub.Label, new Color(0.25f, 0.25f, 0.25f, 1f)))
                                            sub.Action?.Invoke();
                                        y += 30f;
                                        break;
                                    }
                                    case PanelItemType.Checkbox:
                                    {
                                        SquareCheckbox(new Rect(Position.x + 15, y, Width - 20, 25), ref sub.BoolValue, sub.Label);
                                        y += 30f;
                                        break;
                                    }
                                    case PanelItemType.Slider:
                                    {
                                        GUI.Label(new Rect(Position.x + 15, y, Width - 20, 20),
                                            sub.Label + " : " + sub.FloatValue.ToString("0.00"), bodyStyle);
                                        sub.FloatValue = SquareSlider(
                                            new Rect(Position.x + 15, y + 20, Width - 20, 20),
                                            sub.FloatValue,
                                            sub.Min,
                                            sub.Max
                                        );
                                        y += 45f;
                                        break;
                                    }
                                }
                            }
                    }
                        break;
                }
            }
        }
    }

    public enum PanelItemType { Text, Button, Checkbox, Slider, Section }

    public class PanelItem
    {
        public PanelItemType Type;
        public string Label;
        public Action Action;

        public bool BoolValue;

        public float FloatValue;
        public float Min;
        public float Max;

        public bool Collapsed = true;
        public List<PanelItem> SubItems = new List<PanelItem>();

        public PanelItem(string label, PanelItemType type, Action action = null)
        {
            Label = label;
            Type = type;
            Action = action;
        }

        public PanelItem(string label, float min, float max, float defaultValue)
        {
            Label = label;
            Type = PanelItemType.Slider;
            Min = min;
            Max = max;
            FloatValue = defaultValue;
        }
    }
}