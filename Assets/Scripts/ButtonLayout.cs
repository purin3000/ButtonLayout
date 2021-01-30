using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BTL
{
    public class ButtonLayoutManager : System.IDisposable
    {
        private static ButtonLayoutManager instance = null;

        public int fontSize;
        public int buttonHeight;
        public int buttonMargin;

        public Container container;

        public RectTransform rectTransform { get; }
        public Button buttonPrefab { get; }

        public ButtonLayoutManager(RectTransform rectTransform, Button buttonPrefab)
        {
            instance = this;

            this.rectTransform = rectTransform;
            this.buttonPrefab = buttonPrefab;
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
            var size = rectTransform.rect.size;
            container.ApplyLayout(new Rect(position, size));

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
            protected Item()
            {
                layoutManager.AddItem(this);
            }

            /// <summary>
            /// レイアウト調整を適用
            /// </summary>
            /// <param name="rect">描画可能座標とサイズ</param>
            /// <returns>描画サイズ</returns>
            public abstract Vector2 ApplyLayout(Rect rect);
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

            public void AddItem(Item item)
            {
                childs.Add(item);
            }
        }
    }

    public class BTLVertical : ButtonLayoutManager.Container
    {
        public override Vector2 ApplyLayout(Rect rect)
        {
            if (childs.Count == 0) return Vector2.zero;

            var position = rect.position;
            var newSize = new Vector2(rect.width, 0.0f);

            foreach (var item in childs) {
                var size = item.ApplyLayout(new Rect(position, rect.size));

                position.y += -(size.y);
                newSize.y += size.y;
            }
            return newSize;
        }
    }

    public class BTLHorizontal : ButtonLayoutManager.Container
    {
        public override Vector2 ApplyLayout(Rect rect)
        {
            if (childs.Count == 0) return Vector2.zero;

            var width = rect.width / childs.Count;

            var position = rect.position;
            var newSize = Vector2.zero;

            foreach (var item in childs) {
                var size = item.ApplyLayout(new Rect(position, new Vector2(width, rect.height)));

                if (newSize.x * newSize.y < size.x * size.x) {
                    newSize = size;
                }

                position.x += width;
            }
            return newSize;
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
            button = GameObject.Instantiate(layoutManager.buttonPrefab, layoutManager.rectTransform.transform);
            button.name = labelStr;

            text = button.GetComponentInChildren<Text>();
            rectTransform = button.GetComponent<RectTransform>();

            if (action != null) {
                button.onClick.AddListener(() => action(button));
            }
        }

        public override Vector2 ApplyLayout(Rect rect)
        {
            if (text) {
                text.text = label;
                text.fontSize = layoutManager.fontSize;
            }

            if (rectTransform) {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.localPosition = new Vector2(rect.position.x + layoutManager.buttonMargin, rect.position.y - layoutManager.buttonMargin);
                rectTransform.sizeDelta = new Vector2(rect.width - layoutManager.buttonMargin * 2, layoutManager.buttonHeight);

                return new Vector2(rect.width, layoutManager.buttonHeight + layoutManager.buttonMargin * 2);
            }
            return Vector2.zero;
        }
    }
}
