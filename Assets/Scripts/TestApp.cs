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
        ButtonLayoutResources resources;

        public void Start()
        {
            textTitle.text = "ButtonLayout Example";

            var layoutManager = new ButtonLayoutManager(resources);

            using (new BTLBuilder(layoutManager)) {

                var firstButton = new BTLButton("Button1", onClick).button;

                using (new BTLHorizontal()) {
                    using (new BTLVertical()) {
                        new BTLButton("Button2-1", onClick);
                        new BTLButton("Button2-2", onClick);

                        using (new BTLHorizontal()) {
                            using (new BTLVertical()) {
                                new BTLButton("Button3-1", onClick);
   
                            }
                            using (new BTLVertical()) {
                                new BTLButton("Button3-2", onClick);
                                new BTLButton("Button3-3", onClick);
                            }
                        }
                    }
                    using (new BTLVertical()) {
                        new BTLButton("Button2-3", onClick);
                        new BTLToggle("Button2-3", onValueChanged);
                    }
                }

                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }
        }

        void onClick(Button button) => textLog.text = button.name;
        void onValueChanged(Toggle toggle, bool ret) => textLog.text = $"{toggle.name}:{ret}";
    }
}

