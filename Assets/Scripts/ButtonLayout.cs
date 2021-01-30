using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTL
{
    public class ButtonLayout : MonoBehaviour
    {

    }

    [System.Serializable]
    public class ButtonLayoutResources
    {
        public RectTransform rectTransform;
        public Button buttonPrefab;
        public Toggle togglePrefab;
    }

    public class ButtonLayoutManager
    {
        public int fontSize;
        public int buttonHeight;
        public int buttonMargin;

        public BTLBuilder.BaseContainer container;

        public ButtonLayoutResources resources { get; }

        public ButtonLayoutManager(ButtonLayoutResources resources)
        {
            this.resources = resources;

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

        public void UpdateLayout()
        {
            var position = Vector2.zero;
            var size = resources.rectTransform.rect.size;
            container.ApplyLayout(this, position, size.x);
        }
    }

    public class BTLBuilder : System.IDisposable
    {
        private static BTLBuilder instance = null;
        
        private ButtonLayoutManager layoutManager { get; }

        public BTLBuilder(ButtonLayoutManager layoutManager)
        {
            instance = this;

            this.layoutManager = layoutManager;

            layoutManager.container = new BTLVertical();
        }

        void System.IDisposable.Dispose()
        {
            instance = null;

            layoutManager.UpdateLayout();
        }

        public Item AddItem(Item item)
        {
            layoutManager.container?.AddItem(item);
            return item;
        }

        public abstract class Item
        {
            protected BTLBuilder builder { get => instance; }
            protected ButtonLayoutResources resources => builder.layoutManager.resources;
            protected Item() => builder.AddItem(this);

            /// <summary>
            /// レイアウト調整を適用
            /// </summary>
            /// <param name="rect">描画可能座標とサイズ</param>
            /// <returns>描画サイズ</returns>
            public abstract Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width);
            public virtual int CalColumn() => 1;
        }

        public abstract class BaseContainer : Item, System.IDisposable
        {
            protected BaseContainer container;
            protected List<Item> childs = new List<Item>();

            public BaseContainer()
            {
                container = builder.layoutManager.container;
                builder.layoutManager.container = this;
            }

            void System.IDisposable.Dispose()
            {
                builder.layoutManager.container = container;
            }

            public void AddItem(Item item) => childs.Add(item);
        }

        public abstract class BaseButton : Item
        {
            public string label { get; protected set; }
            public Text text { get; protected set; }
            public RectTransform rectTransform { get; protected set; }
            protected T Setup<T>(T go, string labelStr) where T : Component
            {
                label = labelStr;
                text = go.GetComponentInChildren<Text>();
                rectTransform = go.GetComponent<RectTransform>();
                go.name = labelStr;
                return go;
            }
            public override Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width)
            {
                if (text) {
                    text.text = label;
                    text.fontSize = layoutManager.fontSize;
                }

                if (rectTransform) {
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    rectTransform.pivot = new Vector2(0, 1);
                    rectTransform.anchoredPosition = new Vector2(position.x + layoutManager.buttonMargin, position.y - layoutManager.buttonMargin);
                    rectTransform.sizeDelta = new Vector2(width - layoutManager.buttonMargin * 2, layoutManager.buttonHeight);

                    //rectTransform.SetAnchorWithKeepingPosition(new Vector2(0, 0), new Vector2(1, 1));
                    //rectTransform.anchorMin = new Vector2(0, 0);
                    //rectTransform.anchorMax = new Vector2(1, 1);
                    //rectTransform.anchoredPosition = new Vector2(layoutManager.buttonMargin, layoutManager.buttonMargin);
                    //rectTransform.sizeDelta = new Vector2(layoutManager.buttonMargin, layoutManager.buttonHeight);

                    return new Vector2(width, layoutManager.buttonHeight + layoutManager.buttonMargin * 2);
                }
                return Vector2.zero;
            }
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

    public class BTLHorizontal : BTLBuilder.BaseContainer
    {
        public bool adjustWidth { get; }
        public BTLHorizontal(bool adjustWidth = true) { this.adjustWidth = adjustWidth; }

        public override Vector2 ApplyLayout(ButtonLayoutManager layoutManager, Vector2 position, float width)
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

                var size = item.ApplyLayout(layoutManager, position, width2);

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

    public class BTLButton : BTLBuilder.BaseButton
    {
        public Button button { get; }
        public BTLButton(string labelStr, System.Action<Button> action = null)
        {
            button = Setup(GameObject.Instantiate(resources.buttonPrefab, resources.rectTransform.transform), labelStr);
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
            toggle = Setup(GameObject.Instantiate(resources.togglePrefab, resources.rectTransform.transform), labelStr);
            if (action != null) {
                toggle.onValueChanged.AddListener((ret) => action(toggle, ret));
            }
        }
    }


    public static class RectTransformExtension
    {
        /// <summary>
        /// 座標を保ったままPivotを変更する
        /// </summary>
        /// <param name="rectTransform">自身の参照</param>
        /// <param name="targetPivot">変更先のPivot座標</param>
        public static void SetPivotWithKeepingPosition(this RectTransform rectTransform, Vector2 targetPivot)
        {
            var diffPivot = targetPivot - rectTransform.pivot;
            rectTransform.pivot = targetPivot;
            var diffPos = new Vector2(rectTransform.sizeDelta.x * diffPivot.x, rectTransform.sizeDelta.y * diffPivot.y);
            rectTransform.anchoredPosition += diffPos;
        }
        /// <summary>
        /// 座標を保ったままPivotを変更する
        /// </summary>
        /// <param name="rectTransform">自身の参照</param>
        /// <param name="x">変更先のPivotのx座標</param>
        /// <param name="y">変更先のPivotのy座標</param>
        public static void SetPivotWithKeepingPosition(this RectTransform rectTransform, float x, float y)
        {
            rectTransform.SetPivotWithKeepingPosition(new Vector2(x, y));
        }
        /// <summary>
        /// 座標を保ったままAnchorを変更する
        /// </summary>
        /// <param name="rectTransform">自身の参照</param>
        /// <param name="targetAnchor">変更先のAnchor座標 (min,maxが共通の場合)</param>
        public static void SetAnchorWithKeepingPosition(this RectTransform rectTransform, Vector2 targetAnchor)
        {
            rectTransform.SetAnchorWithKeepingPosition(targetAnchor, targetAnchor);
        }
        /// <summary>
        /// 座標を保ったままAnchorを変更する
        /// </summary>
        /// <param name="rectTransform">自身の参照</param>
        /// <param name="x">変更先のAnchorのx座標 (min,maxが共通の場合)</param>
        /// <param name="y">変更先のAnchorのy座標 (min,maxが共通の場合)</param>
        public static void SetAnchorWithKeepingPosition(this RectTransform rectTransform, float x, float y)
        {
            rectTransform.SetAnchorWithKeepingPosition(new Vector2(x, y));
        }
        /// <summary>
        /// 座標を保ったままAnchorを変更する
        /// </summary>
        /// <param name="rectTransform">自身の参照</param>
        /// <param name="targetMinAnchor">変更先のAnchorMin座標</param>
        /// <param name="targetMaxAnchor">変更先のAnchorMax座標</param>
        public static void SetAnchorWithKeepingPosition(this RectTransform rectTransform, Vector2 targetMinAnchor, Vector2 targetMaxAnchor)
        {
            var parent = rectTransform.parent as RectTransform;
            if (parent == null) { Debug.LogError("Parent cannot find."); }

            var diffMin = targetMinAnchor - rectTransform.anchorMin;
            var diffMax = targetMaxAnchor - rectTransform.anchorMax;
            // anchorの更新
            rectTransform.anchorMin = targetMinAnchor;
            rectTransform.anchorMax = targetMaxAnchor;
            // 上下左右の距離の差分を計算
            var diffLeft = parent.rect.width * diffMin.x;
            var diffRight = parent.rect.width * diffMax.x;
            var diffBottom = parent.rect.height * diffMin.y;
            var diffTop = parent.rect.height * diffMax.y;
            // サイズと座標の修正
            rectTransform.sizeDelta += new Vector2(diffLeft - diffRight, diffBottom - diffTop);
            var pivot = rectTransform.pivot;
            rectTransform.anchoredPosition -= new Vector2(
                 (diffLeft * (1 - pivot.x)) + (diffRight * pivot.x),
                 (diffBottom * (1 - pivot.y)) + (diffTop * pivot.y)
            );
        }
        /// <summary>
        /// 座標を保ったままAnchorを変更する
        /// </summary>
        /// <param name="rectTransform">自身の参照</param>
        /// <param name="minX">変更先のAnchorMinのx座標</param>
        /// <param name="minY">変更先のAnchorMinのy座標</param>
        /// <param name="maxX">変更先のAnchorMaxのx座標</param>
        /// <param name="maxY">変更先のAnchorMaxのy座標</param>
        public static void SetAnchorWithKeepingPosition(this RectTransform rectTransform, float minX, float minY, float maxX, float maxY)
        {
            rectTransform.SetAnchorWithKeepingPosition(new Vector2(minX, minY), new Vector2(maxX, maxY));
        }
    }

}



