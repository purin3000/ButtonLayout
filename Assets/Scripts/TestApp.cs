using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

using BTL;

namespace testapp
{
    public class TestApp : MonoBehaviour
    {
        [SerializeField]
        Text textTitle = null;

        [SerializeField]
        Text textLog = null;

        [SerializeField]
        RectTransform buttonLayout = null;

        [SerializeField]
        Button buttonPrefab = null;

        Button firstButton;

        public void Start()
        {
            using (new ButtonLayoutManager(buttonLayout, buttonPrefab)) {
                using (new BTLHorizontal()) {
                    using (new BTLVertical()) {
                        firstButton = new BTLButton("Button1", () => Debug.Log("press button")).button;
                        new BTLButton("Log1", () => textLog.text = "Log1");
                        new BTLButton("Log2", () => textLog.text = "Log2");
                    }
                    using (new BTLVertical()) {
                        new BTLButton("Button1");
                        new BTLButton("Button1");
                        new BTLButton("Button1");
                        new BTLButton("Button1");
                        new BTLButton("Button1");
                    }
                }
                new BTLButton("Button1");
            }

            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }
}

