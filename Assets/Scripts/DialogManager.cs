using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace DialogManager {
    public class DialogManager : MonoBehaviour {
        #region Constant Values
        private const string NAMETAG_NAME_PREFIX    = "NameTag";
        private const string CONTENT_NAME_PREFIX   = "Content";
        private const int FLAG_LEFT                 = 0b100000;
        private const int FLAG_RIGHT                = 0b010000;
        private const int FLAG_H_CENTER             = 0b001000;
        private const int FLAG_TOP                  = 0b000100;
        private const int FLAG_BOTTOM               = 0b000010;
        private const int FLAG_V_CENTER             = 0b000001;
        #endregion



        #region public fields
        public static DialogManager Instance;
        public TMP_FontAsset defaultFont;
        #endregion

        #region serialized public fields
        [SerializeField] public Canvas targetCanvasToDrawDialog;
        [SerializeField] public Backgrounds backgrounds;
        [SerializeField] public List<Dialog> dialogs;

        [SerializeField] public bool bApplyMarginToNameTag = false;
        [SerializeField] public bool bApplyPaddingToNameTag = false;
        #endregion

        #region private fields
        Dictionary<Int32, GameObject>   nameTagBGInstances = new Dictionary<int, GameObject>(),
                                        contentBGInstances = new Dictionary<int, GameObject>();
        Modification plannedModification;
        GameObject currentActiveDialog;
        int nextIndex;
        #endregion



        #region Dispatch
        public int DispatchDialog() {
            if(dialogs.Count < 1) {
                Debug.Log("Error: No dialog has been set");
                return (int) Dialog.DialogError.NO_DIALOG;
            }

            if(dialogs.Count <= nextIndex) {
                Debug.Log("Error: Invalid index. \nNext index to call is: " + nextIndex + " while there are only " + dialogs.Count + " dialog entries in the list.");
                return (int) Dialog.DialogError.INVALID_INDEX;
            }

            ApplyPlannedModificationIfExistsAndDisplaySelectedDialog(dialogs[nextIndex++]);

            return nextIndex;
        }

        public void DispatchDialog(int index) {
            if(index < 0) {
                Debug.Log("Error: Invalid index. \nYour input was: " + index);
                return;
            }

            ApplyPlannedModificationIfExistsAndDisplaySelectedDialog(dialogs[index]);
        }

        //I know the name is visibly very long, but it does not violate C# conding standard so let be it.
        //(and clean code book states that long name is ok as long as it expresses what it does)
        //(and it is private method anyways)
        private void ApplyPlannedModificationIfExistsAndDisplaySelectedDialog(Dialog selectedDialog) {
            PrepareDialog(selectedDialog);
            bool plannedModificationExists = ApplyPlannedModificationIfExists();
            if(!plannedModificationExists)
                ApplyModification(selectedDialog.modification);
        }
        #endregion

        #region Modification
        public void ApplyModification(Modification modification) {
            RectTransform contentRT
                = currentActiveDialog.transform as RectTransform;
            RectTransform nameTagRT
                = currentActiveDialog.FindAllChildrenByNameThatContains(NAMETAG_NAME_PREFIX)[0].transform as RectTransform;

            int flag = ConvertAnchorToFlag(modification.anchor);
            
            UpdatePivotMatchingFlag(nameTagRT, flag);
            UpdatePivotMatchingFlag(contentRT, flag);

            ApplyPaddings(nameTagRT, contentRT, modification.padding);
            ApplyMargins(nameTagRT, contentRT, modification.margin);

            foreach(RoundnessMarker marker in currentActiveDialog.GetComponentsInChildren<RoundnessMarker>())
                ApplyPixelPerUnitMultiplierToImage(marker.transform, modification.pixelPerUnitMultiplier);

            MoveItemsMatchingFlag(nameTagRT, contentRT, flag);
        }

        public void PlanModification(Modification modification) {
            plannedModification = modification;
        }

        bool ApplyPlannedModificationIfExists() {
            if(plannedModification != null) {
                ApplyModification(plannedModification);
                plannedModification = null;
                return true;
            }
            return false;
        }
        #endregion

        #region Prepare Dialog
        private void PrepareDialog(Dialog currentDialog) {
            GameObject nameTag = PrepareNameTag(currentDialog.nameTag);
            GameObject content = PrepareContent(currentDialog.content);
            nameTag.transform.SetParent(content.transform);
            currentActiveDialog = content;
        }

        private GameObject PrepareNameTag(Dialog.NameTag nameTag) {
            if(!nameTagBGInstances.ContainsKey(nameTag.bgIndex)) {
                nameTagBGInstances.Add(
                    nameTag.bgIndex, 
                    Instantiate(
                        backgrounds.nameTagBGPrefabs[nameTag.bgIndex],
                        targetCanvasToDrawDialog.transform
                    )
                );
            }

            GameObject nameTagBG = nameTagBGInstances[nameTag.bgIndex];
            nameTagBG.name = NAMETAG_NAME_PREFIX + "#" + nameTag.bgIndex;

            TextMeshProUGUI tmproUGUI = nameTagBG.GetComponentInChildren<TextMeshProUGUI>();
            tmproUGUI.text = nameTag.text;
            tmproUGUI.overflowMode = nameTag.mode;

            return nameTagBG;
        }
        
        private GameObject PrepareContent(Dialog.Content content) {
            if(!contentBGInstances.ContainsKey(content.bgIndex))
                contentBGInstances.Add(content.bgIndex, Instantiate(backgrounds.contentBGPrefabs[content.bgIndex], targetCanvasToDrawDialog.transform));

            GameObject contentBG = contentBGInstances[content.bgIndex];
            contentBG.name = CONTENT_NAME_PREFIX + "#" + content.bgIndex;

            TextMeshProUGUI tmproUGUI = contentBG.GetComponentInChildren<TextMeshProUGUI>();
            tmproUGUI.text = content.text;
            tmproUGUI.overflowMode = content.mode;

            return contentBG;
        }
        #endregion

        #region Entry Scope
        void Awake() {
            Instance = Instance ? Instance : this;
            if(!IsBGPrefabsReady()) return;
            AssignDefaultFont();
            RefreshInspector();
        }

        private bool IsBGPrefabsReady() {
            if(backgrounds.nameTagBGPrefabs.Count < 1) {
                Debug.Log("There is no BG assigned for a nametag. This will cause error.");
                return false;
            }
            if(backgrounds.contentBGPrefabs.Count < 1) {
                Debug.Log("There is no BG assigned for content. This will cause error.");
                return false;
            }
            return true;
        }

        private void AssignDefaultFont() {
            foreach(Dialog dialog in dialogs) {
                if(dialog.nameTag.fontAsset == null) dialog.nameTag.fontAsset = defaultFont;
                if(dialog.content.fontAsset == null) dialog.content.fontAsset = defaultFont;
            }
        }
        
        private void RefreshInspector() {
            Editor[] es = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            foreach(Editor editor in es) {
                editor.Repaint();
            }
        }
        #endregion

        #region Margin And Padding
        private void ApplyPaddings(RectTransform nameTagRT, RectTransform contentRT, float padding) {
            if(bApplyPaddingToNameTag)
                ApplyValueToRectTransform(nameTagRT.GetComponentInChildren<PaddingMarker>().transform as RectTransform, padding);
            else
                ApplyValueToRectTransform(nameTagRT.GetComponentInChildren<PaddingMarker>().transform as RectTransform, 0);
            ApplyValueToRectTransform(contentRT.GetComponentInChildren<PaddingMarker>().transform as RectTransform, padding);
        }

        private void ApplyMargins(RectTransform nameTagRT, RectTransform contentRT, float margin) {
            if(bApplyMarginToNameTag)
                ApplyValueToRectTransform(nameTagRT.GetComponentInChildren<MarginMarker>().transform as RectTransform, margin);
            else
                ApplyValueToRectTransform(nameTagRT.GetComponentInChildren<MarginMarker>().transform as RectTransform, 0);
            ApplyValueToRectTransform(contentRT.GetComponentInChildren<MarginMarker>().transform as RectTransform, margin);
        }

        private void ApplyValueToRectTransform(RectTransform rt, float value) {
            Vector2 min = rt.offsetMin, max = rt.offsetMax;

            min.x = value;      //left
            min.y = value;      //bottom
            max.x = -value;      //right
            max.y = -value;   //top

            rt.offsetMin = min;
            rt.offsetMax = max;
        }
        #endregion

        #region Corner Position
        private float GetHeightMatchingScaler() {
            CanvasScaler scaler = targetCanvasToDrawDialog.GetComponent<CanvasScaler>();
            return Mathf.Lerp(
                scaler.referenceResolution.x,
                scaler.referenceResolution.y * Camera.main.aspect,
                scaler.matchWidthOrHeight
            );
        }

        private float GetWidthMatchingScaler() {
            CanvasScaler scaler = targetCanvasToDrawDialog.GetComponent<CanvasScaler>();
            return Mathf.Lerp(
                scaler.referenceResolution.x / Camera.main.aspect,
                scaler.referenceResolution.y,
                scaler.matchWidthOrHeight
            );
        }
        #endregion
    
        #region Move Dialog To Position
        private void MoveItemsMatchingFlag(RectTransform nameTagRT, RectTransform contentRT, int flag) {
            MoveContentToPosition(contentRT, flag);
            MoveNameTagToPosition(nameTagRT, contentRT, flag);
        }

        private void MoveContentToPosition(RectTransform contentRT, int flag) {
            Vector2 pos = contentRT.anchoredPosition;

            if((flag & FLAG_TOP) == FLAG_TOP) {
                pos.y = GetWidthMatchingScaler();
            } else if ((flag & FLAG_BOTTOM) == FLAG_BOTTOM) {
                pos.y = 0;
            } else {
                pos.y = GetWidthMatchingScaler() / 2;
            }

            contentRT.anchoredPosition = pos;
        }

        private void MoveNameTagToPosition(RectTransform nameTagRT, RectTransform contentRT, int flag) {
            Vector2 pos = nameTagRT.anchoredPosition;

            if ((flag & FLAG_RIGHT) == FLAG_RIGHT) {
                pos.x = 0 + contentRT.rect.width / 2;
            } else if ((flag & FLAG_LEFT) == FLAG_LEFT) {
                pos.x = 0 - contentRT.rect.width / 2;
            } else {
                pos.x = 0 - contentRT.rect.width / 2 + nameTagRT.rect.width / 2;
            }

            if((flag & FLAG_TOP) == FLAG_TOP) {
                pos.y = 0 - contentRT.rect.height / 2;
            } else if ((flag & FLAG_BOTTOM) == FLAG_BOTTOM) {
                pos.y = 0 + contentRT.rect.height / 2;
            } else {
                pos.y = 0 + contentRT.rect.height / 2 + nameTagRT.rect.height / 2;
            }

            nameTagRT.anchoredPosition = pos;
        }
        #endregion
        
        #region ETC
        private int ConvertAnchorToFlag(Modification.Anchor anchor) {
            int flag = 0;

            if(anchor.IsAnyOf(Modification.Anchor.TopLeft, Modification.Anchor.BottomLeft, Modification.Anchor.CenterLeft))
                flag = flag | FLAG_LEFT;
            else if (anchor.IsAnyOf(Modification.Anchor.TopRight, Modification.Anchor.BottomRight, Modification.Anchor.CenterRight))
                flag = flag | FLAG_RIGHT;
            else
                flag = flag | FLAG_H_CENTER;

            if(anchor.IsAnyOf(Modification.Anchor.TopCenter, Modification.Anchor.TopLeft, Modification.Anchor.TopRight))
                flag = flag | FLAG_TOP;
            else if (anchor.IsAnyOf(Modification.Anchor.BottomCenter, Modification.Anchor.BottomLeft, Modification.Anchor.BottomRight))
                flag = flag | FLAG_BOTTOM;
            else
                flag = flag | FLAG_V_CENTER;

            return flag;
        }
        
        private void UpdatePivotMatchingFlag(RectTransform rt, int flag) {
            Vector2 pivot = rt.pivot;

            if((flag & FLAG_LEFT) == FLAG_LEFT)
                pivot.x = 0;
            else if ((flag & FLAG_RIGHT) == FLAG_RIGHT)
                pivot.x = 1;
            else
                pivot.x = 0.5f;

            if((flag & FLAG_TOP) == FLAG_TOP)
                pivot.y = 1;
            else if ((flag & FLAG_BOTTOM) == FLAG_BOTTOM)
                pivot.y = 0;
            else 
                pivot.y = 0.5f;

            rt.pivot = pivot;
        }

        private void ApplyPixelPerUnitMultiplierToImage(Transform rt, float value) {
            rt.GetComponent<Image>().pixelsPerUnitMultiplier = value;
        }
        #endregion
    }

    public static class MyExtension {
        public static bool IsAnyOf(this Modification.Anchor originalAnchor, params Modification.Anchor[] compareTargets) {
            foreach(Modification.Anchor anchor in compareTargets) {
                if(anchor == originalAnchor)
                    return true;
            }
            return false;
        }

        public static GameObject[] FindAllChildrenByNameThatContains(this GameObject parent, string query) {
            List<GameObject> tempList = new List<GameObject>();
            for(int i = 0; i < parent.transform.childCount; ++i)
                if(parent.transform.GetChild(i).name.Contains(query))
                    tempList.Add(parent.transform.GetChild(i).gameObject);

            return tempList.ToArray();
        }
    }
}