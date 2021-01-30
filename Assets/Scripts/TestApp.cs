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
                    firstButton = new BTLButton("Button1", onClick).button;
                    using (new BTLVertical()) {
                        new BTLButton("Button2", onClick);
                        using (new BTLHorizontal()) {
                            using (new BTLVertical()) {
                                new BTLButton("Button3", onClick);
                                new BTLButton("Button4", onClick);
                            }
                            using (new BTLVertical()) {
                                new BTLButton("Button5", onClick);
                                new BTLButton("Button6", onClick);
                            }
                        }
                    }
                }
            }

            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }

        void onClick(Button button) => textLog.text = button.name;
    }
}

