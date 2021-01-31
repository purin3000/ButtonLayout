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
        public Slider sliderPrefab;
        public InputField inputFieldPrefab;
        public Text textPrefab;
        public Dropdown dropdownPrefab;
    }

    public class ButtonLayoutManager
    {
        public int fontSize;
        public int buttonHeight;
        public int buttonMargin;

        Vector2 baseSize;

        public BTLBuilder.BaseContainer container;

        public ButtonLayoutResources resources { get; }

        public ButtonLayoutManager(ButtonLayoutResources resources)
        {
            this.resources = resources;

            fontSize = 28;
            buttonHeight = fontSize + 10;
            buttonMargin = 2;
        }

        public void UpdateLayout()
        {
            var size = resources.rectTransform.rect.size;
            if (baseSize != size) {
                var position = Vector2.zero;
                container.ApplyLayout(this, position, size.x);
                baseSize = resources.rectTransform.rect.size;
            }
        }

        interface IBTLItem
        {
            Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width);
        }
    }

    public class BTLBuilder : System.IDisposable
    {
        private static BTLBuilder currentBuilder = null;
        
        private ButtonLayoutManager layoutManager { get; }

        public BTLBuilder(ButtonLayoutManager layoutManager)
        {
            currentBuilder = this;
            this.layoutManager = layoutManager;
            layoutManager.container = new BTLVertical();
        }

        void System.IDisposable.Dispose()
        {
            currentBuilder = null;
            layoutManager.UpdateLayout();
        }

        public Item AddItem(Item item)
        {
            layoutManager.container?.AddItem(item);
            return item;
        }

        public abstract class Item
        {
            protected ButtonLayoutResources resources => currentBuilder.layoutManager.resources;

            protected Item() => currentBuilder.AddItem(this);

            /// <summary>
            /// レイアウト調整を適用
            /// </summary>
            /// <param name="rect">描画可能座標とサイズ</param>
            /// <returns>描画サイズ</returns>
            public abstract Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width);
            public abstract int CalColumn();
        }

        public abstract class BaseContainer : Item, System.IDisposable
        {
            protected BaseContainer container;
            protected List<Item> childs = new List<Item>();

            public BaseContainer()
            {
                container = currentBuilder.layoutManager.container;
                currentBuilder.layoutManager.container = this;
            }

            void System.IDisposable.Dispose()
            {
                currentBuilder.layoutManager.container = container;
            }

            public void AddItem(Item item) => childs.Add(item);

            public int CalcChildColumn()
            {
                int total = 0;
                foreach (var item in childs) {
                    total += item.CalColumn();
                }
                return total;
            }

            public int CalcMaxColumn()
            {
                int total = 1;
                foreach (var item in childs) {
                    total = Mathf.Max(total, item.CalColumn());
                }
                return total;
            }
        }

        public abstract class BaseButton : Item
        {
            public string label { get; protected set; } = "";
            public Text text { get; protected set; } = null;
            public RectTransform rectTransform { get; protected set; } = null;

            protected T Setup<T>(T go, string objName, string labelStr) where T : Component
            {
                label = labelStr;

                if (!string.IsNullOrEmpty(labelStr)) {
                    text = go.GetComponentInChildren<Text>();
                    text.text = label;
                    text.fontSize = currentBuilder.layoutManager.fontSize;
                }

                rectTransform = go.GetComponent<RectTransform>();
                go.name = objName;
                return go;
            }

            protected T Setup2<T>(T go, string objName) where T : Component
            {
                rectTransform = go.GetComponent<RectTransform>();
                go.name = objName;
                return go;
            }

            public override Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width)
            {
                if (rectTransform) {
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(position.x + layoutManager.buttonMargin, position.y - layoutManager.buttonMargin);
                    rectTransform.sizeDelta = new Vector2(width - layoutManager.buttonMargin * 2, layoutManager.buttonHeight);

                    return new Vector2(width, layoutManager.buttonHeight + layoutManager.buttonMargin * 2);
                }
                return Vector2.zero;
            }

            public override int CalColumn() => 1;
        }
    }

    public class BTLVertical : BTLBuilder.BaseContainer
    {
        public override Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width)
        {
            if (childs.Count == 0) return Vector2.zero;

            var newSize = new Vector2(width, 0.0f);

            foreach (var item in childs) {
                var size = item.ApplyLayout(layoutManager, position, width);

                position.y += -size.y;
                newSize.y += size.y;
            }
            return newSize;
        }

        public override int CalColumn() => CalcMaxColumn();
    }

    public class BTLHorizontal : BTLBuilder.BaseContainer
    {
        public bool adjustWidth { get; }
        public BTLHorizontal(bool adjustWidth = true) { this.adjustWidth = adjustWidth; }

        public override Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width)
        {
            if (childs.Count == 0) return Vector2.zero;

            float totalColumn = CalcChildColumn();

            var newSize = Vector2.zero;
            foreach (var item in childs) {
                var width2 = width / childs.Count;
                if (adjustWidth) {
                    width2 = width * item.CalColumn() / totalColumn;
                }

                var size = item.ApplyLayout(layoutManager, position, width2);

                if (newSize.x * newSize.y < size.x * size.x) {
                    newSize = size;
                }

                position.x += width2;
            }
            return newSize;
        }

        public override int CalColumn() => CalcChildColumn();
    }

    public class BTLButton : BTLBuilder.BaseButton
    {
        public Button button { get; }
        public BTLButton(string labelStr, System.Action<Button> action = null)
        {
            button = Setup(GameObject.Instantiate(resources.buttonPrefab, resources.rectTransform.transform), labelStr, labelStr);
            if (action != null) {
                button.onClick.AddListener(() => action(button));
            }
        }
    }

    public class BTLToggle : BTLBuilder.BaseButton
    {
        public Toggle toggle { get; }
        public BTLToggle(string labelStr, System.Action<Toggle, bool> action = null)
        {
            toggle = Setup(GameObject.Instantiate(resources.togglePrefab, resources.rectTransform.transform), labelStr, labelStr);
            if (action != null) {
                toggle.onValueChanged.AddListener((ret) => action(toggle, ret));
            }
        }
    }

    public class BTLSlider : BTLBuilder.BaseButton
    {
        public Slider slider { get; }
        public BTLSlider(string labelStr, System.Action<Slider, float> action = null, float initValue = 0.0f)
        {
            slider = Setup2(GameObject.Instantiate(resources.sliderPrefab, resources.rectTransform.transform), labelStr);
            slider.value = initValue;
            if (action != null) {
                slider.onValueChanged.AddListener((ret) => action(slider, ret));
            }
        }
    }

    public class BTLInputField : BTLBuilder.BaseButton
    {
        public InputField inputField { get; }
        public BTLInputField(string labelStr, System.Action<InputField, string> action = null, string initValue = "")
        {
            inputField = Setup(GameObject.Instantiate(resources.inputFieldPrefab, resources.rectTransform.transform), labelStr, labelStr);
            inputField.text = initValue;
            if (action != null) {
                inputField.onValueChanged.AddListener((ret) => action(inputField, ret));
            }
        }
    }

    public class BTLText : BTLBuilder.BaseButton
    {
        public BTLText(string labelStr)
        {
            Setup(GameObject.Instantiate(resources.textPrefab, resources.rectTransform.transform), labelStr, labelStr);
        }
    }

    public class BTLDropdown : BTLBuilder.BaseButton
    {
        public Dropdown dropdown { get; }
        public BTLDropdown(string labelStr, System.Action<Dropdown, int> action = null, int initValue = 0)
        {
            dropdown = Setup2(GameObject.Instantiate(resources.dropdownPrefab, resources.rectTransform.transform), labelStr);
            dropdown.value = initValue;
            if (action != null) {
                dropdown.onValueChanged.AddListener((ret) => action(dropdown, ret));
            }
        }
    }
}



