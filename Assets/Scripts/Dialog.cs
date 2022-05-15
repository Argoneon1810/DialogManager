using UnityEngine;
using TMPro;

namespace DialogManager {
    [System.Serializable]
    public class Dialog {
        public enum DialogError {
            NO_DIALOG = -1,
            INVALID_INDEX = -2
        }

        public bool enabled;
        public NameTag nameTag;
        public Content content;
        public Modification modification;

        [System.Serializable]
        public class NameTag {
            public bool enabled;
            public string text;
            public TMPro.TextOverflowModes mode;
            public int bgIndex;                 //list of BG should be under DialogManager
            public Color fontColor;
            [Header("Field 'fontAsset' being a null is normal if you want this to be a default font.")]
            public TMP_FontAsset fontAsset;

            public NameTag() {
                enabled = true;
                text = "";
                mode = TMPro.TextOverflowModes.Truncate;
                bgIndex = 0;
                fontColor = Color.black;
                fontAsset = DialogManager.Instance ? DialogManager.Instance.defaultFont : null;
            }
        }
        
        [System.Serializable]
        public class Content {
            public string text;
            public TMPro.TextOverflowModes mode;
            public int bgIndex;                 //list of BG should be under DialogManager
            public Color fontColor;
            [Header("Field 'fontAsset' being a null is normal if you want this to be a default font.")]
            public TMP_FontAsset fontAsset;

            public Content() {
                text = "";
                mode = TMPro.TextOverflowModes.Truncate;
                bgIndex = 0;
                fontColor = Color.black;
                fontAsset = DialogManager.Instance ? DialogManager.Instance.defaultFont : null;
            }
        }

        public Dialog() {
            enabled = true;
            nameTag = new NameTag();
            content = new Content();
        }
    }   
}