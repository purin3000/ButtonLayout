using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTL
{
    [System.Serializable]
    public class ButtonLayoutResources
    {
        public RectTransform rectTransform;
        public Button buttonPrefab;
        public Toggle togglePrefab;
    }

    public class ButtonLayoutManager : System.IDisposable
    {
        private static ButtonLayoutManager instance = null;

        public int fontSize;
        public int buttonHeight;
        public int buttonMargin;

        public Container container;

        public ButtonLayoutResources resources { get; }
        
        public ButtonLayoutManager(ButtonLayoutResources resources)
        {
            instance = this;

            this.resources = resources;
            container = new BTLVertical();

#if UNITY_TVOS
            fontSize = 24;
            buttonHeight = fontSize + 4;
            buttonMargin = 3;

#else
            fontSize = 28;
            buttonHeight = fontSize + 10;
            buttonMargin = 2;
#endif
        }

        void System.IDisposable.Dispose()
        {
            var position = Vector2.zero;
            var size = resources.rectTransform.rect.size;
            container.ApplyLayout(position, size.x);

            instance = null;
        }

        public Item AddItem(Item item)
        {
            container?.AddItem(item);
            return item;
        }

        public abstract class Item
        {
            protected ButtonLayoutManager layoutManager { get => instance; }
            protected Item() => layoutManager.AddItem(this);

            /// <summary>
            /// レイアウト調整を適用
            /// </summary>
            /// <param name="rect">描画可能座標とサイズ</param>
            /// <returns>描画サイズ</returns>
            public abstract Vector2 ApplyLayout(Vector2 position, float width);
            public virtual int CalColumn() => 1;
        }

        public abstract class Container : Item, System.IDisposable
        {
            protected Container container;
            protected List<Item> childs = new List<Item>();

            public Container()
            {
                container = layoutManager.container;
                layoutManager.container = this;
            }

            void System.IDisposable.Dispose()
            {
                layoutManager.container = container;
            }

            public void AddItem(Item item) => childs.Add(item);
        }
    }

    public class BTLVertical : ButtonLayoutManager.Container
    {
        public override Vector2 ApplyLayout(Vector2 position, float width)
        {
            if (childs.Count == 0) return Vector2.zero;

            var newSize = new Vector2(width, 0.0f);

            foreach (var item in childs) {
                var size = item.ApplyLayout(position, width);

                position.y += -(size.y);
                newSize.y += size.y;
            }
            return newSize;
        }

        public override int CalColumn()
        {
            int column = 1;
            foreach (var item in childs) {
                column = Mathf.Max(column, item.CalColumn());
            }
            return column;
        }
    }

    public class BTLHorizontal : ButtonLayoutManager.Container
    {
        public bool adjustWidth { get; }
        public BTLHorizontal(bool adjustWidth = true) { this.adjustWidth = adjustWidth; }

        public override Vector2 ApplyLayout(Vector2 position, float width)
        {
            if (childs.Count == 0) return Vector2.zero;

            float totalColumn = 0;
            foreach (var item in childs) {
                totalColumn += item.CalColumn();
            }

            var newSize = Vector2.zero;

            foreach (var item in childs) {

                var width2 = width / childs.Count;
                if (adjustWidth) {
                    width2 = width * item.CalColumn() / totalColumn;
                }

                var size = item.ApplyLayout(position, width2);

                if (newSize.x * newSize.y < size.x * size.x) {
                    newSize = size;
                }

                position.x += width2;
            }
            return newSize;
        }

        public override int CalColumn()
        {
            int column = 0;
            foreach (var item in childs) {
                column += item.CalColumn();
            }
            return column;
        }
    }

    public class BTLButton : ButtonLayoutManager.Item
    {
        public string label { get; }
        public Text text { get; }
        public Button button { get; }
        public RectTransform rectTransform { get; }
        public BTLButton(string labelStr, System.Action<Button> action = null)
        {
            label = labelStr;
            button = GameObject.Instantiate(layoutManager.resources.buttonPrefab, layoutManager.resources.rectTransform.transform);
            button.name = labelStr;

            text = button.GetComponentInChildren<Text>();
            rectTransform = button.GetComponent<RectTransform>();

            if (action != null) {
                button.onClick.AddListener(() => action(button));
            }
        }

        public override Vector2 ApplyLayout(Vector2 position, float width)
        {
            if (text) {
                text.text = label;
                text.fontSize = layoutManager.fontSize;
            }

            if (rectTransform) {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.localPosition = new Vector2(position.x + layoutManager.buttonMargin, position.y - layoutManager.buttonMargin);
                rectTransform.sizeDelta = new Vector2(width - layoutManager.buttonMargin * 2, layoutManager.buttonHeight);

                return new Vector2(width, layoutManager.buttonHeight + layoutManager.buttonMargin * 2);
            }
            return Vector2.zero;
        }
    }

    public class BTLToggle : ButtonLayoutManager.Item
    {
        public string label { get; }
        public Text text { get; }
        public Toggle toggle { get; }
        public RectTransform rectTransform { get; }
        public BTLToggle(string labelStr, System.Action<Toggle, bool> action = null)
        {
            label = labelStr;
            toggle = GameObject.Instantiate(layoutManager.resources.togglePrefab, layoutManager.resources.rectTransform.transform);
            toggle.name = labelStr;

            text = toggle.GetComponentInChildren<Text>();
            rectTransform = toggle.GetComponent<RectTransform>();

            if (action != null) {
                toggle.onValueChanged.AddListener((ret) => action(toggle, ret));
            }
        }

        public override Vector2 ApplyLayout(Vector2 position, float width)
        {
            if (text) {
                text.text = label;
                text.fontSize = layoutManager.fontSize;
            }

            if (rectTransform) {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.localPosition = new Vector2(position.x + layoutManager.buttonMargin, position.y - layoutManager.buttonMargin);
                rectTransform.sizeDelta = new Vector2(width - layoutManager.buttonMargin * 2, layoutManager.buttonHeight);

                return new Vector2(width, layoutManager.buttonHeight + layoutManager.buttonMargin * 2);
            }
            return Vector2.zero;
        }
    }
}
