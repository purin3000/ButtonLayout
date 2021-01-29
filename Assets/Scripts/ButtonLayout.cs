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
            buttonMargin = 3;
#endif
        }

        void System.IDisposable.Dispose()
        {
            var position = Vector2.zero;
            var size = rectTransform.rect.size;
            container.draw(new Rect(position, size));

            instance = null;
        }

        public Item addItem(Item item)
        {
            container?.addItem(item);
            return item;
        }

        public abstract class Item
        {
            protected ButtonLayoutManager layoutManager { get => instance; }
            protected Item()
            {
                layoutManager.addItem(this);
            }

            public abstract void draw(Rect rect);
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

            public void addItem(Item item)
            {
                childs.Add(item);
            }
        }
    }

    public class BTLVertical : ButtonLayoutManager.Container
    {
        public override void draw(Rect rect)
        {
            if (childs.Count == 0) return;

            //Debug.Log($"BTVertical {rect}");
            var height = layoutManager.buttonHeight;

            var position = rect.position;
            var size = new Vector2(rect.width, height);

            foreach (var item in childs) {
                item.draw(new Rect(position, size));

                position.y -= height + layoutManager.buttonMargin;
            }
        }
    }

    public class BTLHorizontal : ButtonLayoutManager.Container
    {
        public override void draw(Rect rect)
        {
            if (childs.Count == 0) return;

            //Debug.Log($"BTHorizontal {rect}");
            var width = rect.width / childs.Count;

            var position = rect.position;
            var size = new Vector2(width, rect.height);

            foreach (var item in childs) {
                item.draw(new Rect(position, size));

                position.x += width + layoutManager.buttonMargin;
            }
        }
    }

    public class BTLButton : ButtonLayoutManager.Item
    {
        public Button button { get; }

        public BTLButton(string text, UnityAction action = null)
        {
            button = GameObject.Instantiate(layoutManager.buttonPrefab, layoutManager.rectTransform.transform);

            var textComp = button.GetComponentInChildren<Text>();
            if (textComp) {
                textComp.text = text;
                textComp.fontSize = layoutManager.fontSize;
            }

            if (action != null) {
                button.onClick.AddListener(action);
            }
        }

        public override void draw(Rect rect)
        {
            //Debug.Log("BTButton");

            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.localPosition = rect.position;
            rectTransform.sizeDelta = new Vector2(rect.width, layoutManager.buttonHeight);
        }
    }
}
