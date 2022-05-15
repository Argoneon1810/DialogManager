using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogManager {
    public class DialogTrigger : MonoBehaviour {
        public void OnClick() {
            DialogManager.Instance.DispatchDialog();
        }
    }       
}