using UnityEngine;

namespace DialogManager {
    [System.Serializable]
    public class Modification {
        public enum Anchor {
            TopLeft,
            TopCenter,
            TopRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            Center,
            CenterLeft,
            CenterRight
        }
        public enum InterpolationMode {
            Instant,
            Linear,
            EaseIn,
            EaseOut,
            EaseInAndOut,
            Custom
        }
        public Anchor anchor;
        public float padding, margin, pixelPerUnitMultiplier;
        public InterpolationMode interpolationMode;
        public AnimationCurve interpolationCurve;

        public Modification() {
            anchor = Anchor.BottomLeft;
            padding = 0;
            margin = 0;
            margin = 10;
            interpolationMode = InterpolationMode.Instant;
            interpolationCurve = AnimationCurve.Constant(0,1,1);
        }
        public class ModificationBuilder {
            Modification modification;

            public ModificationBuilder() {
                modification = new Modification();
            }

            public ModificationBuilder SetAnchor(Modification.Anchor anchor) {
                modification.anchor = anchor;
                return this;
            }

            public ModificationBuilder SetPadding(float padding) {
                modification.padding = padding;
                return this;
            }

            public ModificationBuilder SetMargin(float margin) {
                modification.margin = margin;
                return this;
            }

            public ModificationBuilder SetPixelPerUnitMultiplier(float pixelPerUnitMultiplier) {
                modification.pixelPerUnitMultiplier = pixelPerUnitMultiplier;
                return this;
            }

            public ModificationBuilder SetInterpolationMode(InterpolationMode interpolationMode) {
                modification.interpolationMode = interpolationMode;
                return this;
            }

            public ModificationBuilder SetInterpolationCurve(AnimationCurve interpolationCurve) {
                modification.interpolationCurve = interpolationCurve;
                return this;
            }

            public Modification Build() {
                return modification;
            }
        }
    }   
}