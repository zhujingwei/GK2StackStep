using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LazyBearTechnology;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GK2StackStep
{
    [BepInPlugin(Guid, "Stack Step", "1.0.0")]
    public class StackStepPlugin : BaseUnityPlugin
    {
        internal const string Guid = "com.gk2mod.stackstep";
        internal const string Tag = "[StackStep]";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            try
            {
                // Parameter types are spelled out: UIItemCountWindow inherits a second Open()
                // overload from LazyWindow, and a bare name lookup throws AmbiguousMatchException.
                MethodInfo sliderUpdate = AccessTools.Method(typeof(SmartSlider), "Update", new Type[0]);
                MethodInfo windowOpen = AccessTools.Method(typeof(UIItemCountWindow), "Open", new[] { typeof(UIItemCountWindowData) });
                MethodInfo changeValue = AccessTools.Method(typeof(SmartSlider), "ChangeValue", new[] { typeof(int), typeof(bool) });
                FieldInfo sliderField = AccessTools.Field(typeof(UIItemCountWindow), "slider");
                FieldInfo headerLabelField = AccessTools.Field(typeof(UIItemCountWindow), "headerLabel");
                FieldInfo okButtonField = AccessTools.Field(typeof(UIItemCountWindow), "okBtn");
                FieldInfo backButtonField = AccessTools.Field(typeof(UIItemCountWindow), "backBtn");
                FieldInfo tipsField = AccessTools.Field(typeof(UIItemCountWindow), "lazyButtonTips");
                FieldInfo tipsLabelField = AccessTools.Field(typeof(LazyButtonTipsStr), "label");

                if (sliderUpdate == null || windowOpen == null || changeValue == null || sliderField == null
                    || headerLabelField == null || okButtonField == null || backButtonField == null
                    || tipsField == null || tipsLabelField == null)
                {
                    Log.LogError(Tag + " game members missing: SmartSlider.Update=" + (sliderUpdate != null)
                        + ", UIItemCountWindow.Open=" + (windowOpen != null)
                        + ", SmartSlider.ChangeValue=" + (changeValue != null)
                        + ", UIItemCountWindow.slider=" + (sliderField != null)
                        + ", UIItemCountWindow.headerLabel=" + (headerLabelField != null)
                        + ", UIItemCountWindow.okBtn=" + (okButtonField != null)
                        + ", UIItemCountWindow.backBtn=" + (backButtonField != null)
                        + ", UIItemCountWindow.lazyButtonTips=" + (tipsField != null)
                        + ", LazyButtonTipsStr.label=" + (tipsLabelField != null));
                    return;
                }

                StackStepPatches.ChangeValue = changeValue;
                StackStepPatches.SliderField = sliderField;
                StackStepPatches.HeaderLabelField = headerLabelField;
                StackStepPatches.OkButtonField = okButtonField;
                StackStepPatches.BackButtonField = backButtonField;
                StackStepPatches.TipsField = tipsField;
                StackStepPatches.TipsLabelField = tipsLabelField;

                Harmony harmony = new Harmony(Guid);
                harmony.Patch(sliderUpdate, prefix: new HarmonyMethod(typeof(StackStepPatches), nameof(StackStepPatches.UpdatePrefix)));
                harmony.Patch(windowOpen, postfix: new HarmonyMethod(typeof(StackStepPatches), nameof(StackStepPatches.OpenPostfix)));

                Log.LogInfo(Tag + " v1.0.0 ready - LB removes one group, RB adds one group, hold repeats every 0.2s after 0.6s");
            }
            catch (Exception ex)
            {
                Log.LogError(Tag + " failed to initialise: " + ex);
            }
        }
    }

    internal static class StackStepPatches
    {
        internal static MethodInfo ChangeValue;
        internal static FieldInfo SliderField;
        internal static FieldInfo HeaderLabelField;
        internal static FieldInfo OkButtonField;
        internal static FieldInfo BackButtonField;
        internal static FieldInfo TipsField;
        internal static FieldInfo TipsLabelField;

        // GamepadController.rewiredBindings maps GamepadButton to a Rewired action id:
        // LB = 6, RB = 7. Reading the same ids yields exactly the physical bumpers the
        // game itself uses, without depending on which GameKey happens to be bound.
        // LB removes a group, RB adds one.
        private const int ActionLb = 6;
        private const int ActionRb = 7;

        private const float HoldDelay = 0.6f;
        private const float HoldInterval = 0.2f;

        // The hint sits in the empty band under the OK / Cancel row, measured from that row.
        private const float HintBottomMargin = 6f;
        private const float HintMinBottomInset = 40f;
        private const float HintWidth = 360f;
        private const float HintHeight = 32f;
        // Fraction of a tip line height used as the gap between the game's tip row and the hint.
        private const float TipsGapFactor = 0.35f;
        private const string HintObjectName = "StackStepHint";

        // Keyed by the game's own language ids (LLBase.languages). The "1" prefix reads wrong in
        // CJK, so those carry the numeral-free wording. Languages without a checked translation
        // (ja, ko, th, vn) are deliberately absent and fall back to English.
        private static readonly Dictionary<string, string> GroupWords =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "en", "1 stack" },
                { "de", "1 Stapel" },
                { "fr", "1 pile" },
                { "es", "1 pila" },
                { "it", "1 pila" },
                { "pt-br", "1 pilha" },
                { "ru", "1 стопка" },
                { "uk-ua", "1 стопка" },
                { "pl", "1 stos" },
                { "tr", "1 yığın" },
                { "zh_cn", "一组" },
                { "zh_cht", "一組" },
            };

        private static UIItemCountWindow currentWindow;
        private static SmartSlider currentSlider;
        private static int step = 1;

        private static TextMeshProUGUI hint;
        private static bool hintNeedsPlacement;
        private static int hintPlacementFrame;

        private static bool holding;
        private static int holdDirection;
        private static float nextJumpTime;

        private static bool lbWasDown;
        private static bool rbWasDown;

        internal static void OpenPostfix(UIItemCountWindow __instance, UIItemCountWindowData data)        {
            try
            {
                currentWindow = __instance;
                currentSlider = SliderField.GetValue(__instance) as SmartSlider;
                holding = false;
                holdDirection = 0;
                lbWasDown = IsHeld(ActionLb);
                rbWasDown = IsHeld(ActionRb);

                step = 1;
                if (data != null && data.Item != null)
                {
                    ItemDef def = data.Item.Definition;
                    if (def != null)
                    {
                        step = Math.Max(1, def.stackCount);
                    }
                }

                ShowHint(__instance);

                GamepadType pad = LazyInput.CurrentGamepadType;
                StackStepPlugin.Log.LogInfo(StackStepPlugin.Tag + " count window opened: "
                    + (data != null && data.Item != null ? data.Item.id : "<none>")
                    + ", one group = " + step
                    + ", gamepad = " + (pad != null ? Enumeration.GetNameOfStaticField<GamepadType>(pad.value) : "<none>")
                    + ", hint = " + (hint != null ? hint.text : "<none>"));
            }
            catch (Exception ex)
            {
                currentWindow = null;
                currentSlider = null;
                StackStepPlugin.Log.LogError(StackStepPlugin.Tag + " Open postfix failed: " + ex);
            }
        }

        internal static bool UpdatePrefix(SmartSlider __instance)
        {
            try
            {
                bool lb = IsHeld(ActionLb);
                bool rb = IsHeld(ActionRb);
                bool lbPressed = lb && !lbWasDown;
                bool rbPressed = rb && !rbWasDown;
                lbWasDown = lb;
                rbWasDown = rb;

                if (__instance != currentSlider || currentWindow == null || !currentWindow.IsShownAndTop)
                {
                    return true;
                }

                UpdateHintVisibility();

                // RB is the "+" side, LB is the "-" side.
                int direction = lb == rb ? 0 : (rb ? 1 : -1);
                if (direction == 0)
                {
                    holding = false;
                    holdDirection = 0;
                    return true;
                }

                float now = Time.unscaledTime;
                if (!holding || direction != holdDirection)
                {
                    if (!(direction > 0 ? rbPressed : lbPressed))
                    {
                        // Bumper was already down before this window opened - wait for a real press.
                        return false;
                    }
                    holding = true;
                    holdDirection = direction;
                    nextJumpTime = now + HoldDelay;
                    Jump(__instance, direction);
                    return false;
                }

                if (now >= nextJumpTime)
                {
                    nextJumpTime = now + HoldInterval;
                    Jump(__instance, direction);
                }

                // Swallow the frame so the slider cannot also apply its own +/-1 for the
                // key these bumpers may be bound to (IncSlider / DecSlider).
                return false;
            }
            catch (Exception ex)
            {
                StackStepPlugin.Log.LogError(StackStepPlugin.Tag + " Update prefix failed: " + ex);
                return true;
            }
        }

        private static void Jump(SmartSlider slider, int direction)
        {
            // Land on whole-group boundaries so repeated presses stay on clean numbers:
            // with a group of 50, up goes 1 -> 50 -> 100 and down goes 51 -> 50, 100 -> 50.
            int current = slider.Value;
            int target = direction > 0
                ? current / step * step + step
                : (current - 1) / step * step;
            ChangeValue.Invoke(slider, new object[] { target - current, true });
        }

        private static bool IsHeld(int actionId)
        {
            if (!ReInput.isReady)
            {
                return false;
            }

            Player player = ReInput.players.GetPlayer(0);
            return player != null && player.GetButton(actionId);
        }

        private static void UpdateHintVisibility()
        {
            if (hint == null)
            {
                return;
            }

            // Place on the frame after the window opened: the game only fills the tip row during
            // Open, and its rendered bounds stay stale until the canvas has rebuilt once.
            if (hintNeedsPlacement && Time.frameCount >= hintPlacementFrame)
            {
                PlaceHint();
                hintNeedsPlacement = false;
            }

            bool wanted = LazyInput.IsGamepadActive;
            if (hint.gameObject.activeSelf != wanted)
            {
                hint.gameObject.SetActive(wanted);
            }
        }

        private static void ShowHint(UIItemCountWindow window)
        {
            if (window.transform == null)
            {
                return;
            }

            TextMeshProUGUI header = HeaderLabelField.GetValue(window) as TextMeshProUGUI;
            TMP_Text tips = FindTipsLabel(window);

            if (hint == null)
            {
                GameObject go = new GameObject(HintObjectName, typeof(RectTransform));
                go.transform.SetParent(window.transform, false);
                hint = go.AddComponent<TextMeshProUGUI>();
                hint.raycastTarget = false;
                hint.enableAutoSizing = false;
                hint.alignment = TextAlignmentOptions.Top;
                hint.textWrappingMode = TextWrappingModes.NoWrap;
                hint.overflowMode = TextOverflowModes.Overflow;

                // The window stacks its own children; keep the hint out of that flow.
                LayoutElement layout = go.AddComponent<LayoutElement>();
                layout.ignoreLayout = true;
            }

            ApplyNativeFont(hint, header);
            hint.fontSize = ResolveFontSize(tips, header);
            hint.color = tips != null ? tips.color : (header != null ? header.color : Color.white);
            hint.text = BuildHintText();

            hintNeedsPlacement = true;
            hintPlacementFrame = Time.frameCount + 1;
            UpdateHintVisibility();
        }

        private static void PlaceHint()
        {
            // window.rectTransform is an inner content container that does not even contain the
            // OK / Cancel row, so both candidate anchors are measured against the window root.
            RectTransform root = currentWindow != null ? currentWindow.transform as RectTransform : null;
            if (hint == null || root == null)
            {
                return;
            }

            RectTransform rect = hint.rectTransform;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(HintWidth, HintHeight);

            string anchoredTo;
            if (TryPlaceUnderTips(root, rect))
            {
                anchoredTo = "the game's own +/- tips row";
            }
            else
            {
                anchoredTo = "the ok/cancel row";

                float inset = HintMinBottomInset;
                float buttonBottom = FindButtonRowBottom();
                if (!float.IsNaN(buttonBottom))
                {
                    // World y has to come back into the window root's local space before it can be
                    // compared against rect.yMin.
                    float buttonBottomLocal = root.InverseTransformPoint(new Vector3(0f, buttonBottom, 0f)).y;
                    inset = buttonBottomLocal - HintBottomMargin - root.rect.yMin;
                }

                rect.anchoredPosition = new Vector2(0f, inset);
            }

            StackStepPlugin.Log.LogInfo(StackStepPlugin.Tag + " hint anchored to " + anchoredTo
                + ": window=" + root.rect.width + "x" + root.rect.height
                + ", fontSize=" + hint.fontSize
                + ", position=" + rect.position
                + ", anchoredPosition=" + rect.anchoredPosition);
        }

        /// <summary>
        /// The window's own "+" / "-" tips (UIItemCountWindow.PrintTips) are the key hints the game
        /// already draws for this window, so the mod hint hangs one row lower, centred on them. The
        /// target point comes from the tip text's rendered glyph bounds and is pushed through the
        /// window root's world transform, so no inner rect of the window has to be guessed.
        /// </summary>
        private static bool TryPlaceUnderTips(RectTransform root, RectTransform rect)
        {
            TMP_Text tips = FindTipsLabel(currentWindow);
            if (tips == null)
            {
                return false;
            }

            Bounds bounds = tips.textBounds;
            if (bounds.size.y <= 0f)
            {
                return false;
            }

            Vector3 bottomCenter = tips.transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0f));
            Vector3 gap = tips.transform.TransformVector(new Vector3(0f, -bounds.size.y * TipsGapFactor, 0f));
            rect.localPosition = root.InverseTransformPoint(bottomCenter + gap);

            StackStepPlugin.Log.LogInfo(StackStepPlugin.Tag + " tips row bounds: label=" + tips.name
                + ", size=" + bounds.size + ", center=" + bounds.center
                + ", worldBottomCenter=" + bottomCenter
                + ", hintLocalPosition=" + rect.localPosition);
            return true;
        }

        /// <summary>Lowest world y of the OK / Cancel row, or NaN when neither button is reachable.</summary>
        private static float FindButtonRowBottom()
        {
            float bottom = float.NaN;
            bottom = MinBottom(bottom, OkButtonField);
            bottom = MinBottom(bottom, BackButtonField);
            return bottom;
        }

        private static float MinBottom(float current, FieldInfo buttonField)
        {
            if (buttonField == null)
            {
                return current;
            }

            Component button = buttonField.GetValue(currentWindow) as Component;
            if (button == null)
            {
                return current;
            }

            RectTransform rect = button.transform as RectTransform;
            if (rect == null)
            {
                return current;
            }

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float y = corners[0].y;
            return float.IsNaN(current) ? y : Mathf.Min(current, y);
        }

        private static string BuildHintText()
        {
            string group = GroupWord();
            // "+" first so the row reads the same way as the game's own "+ -" tip row above it.
            return ButtonHint(GamepadButton.RB, false) + "+" + group
                + "   "
                + ButtonHint(GamepadButton.LB, true) + "-" + group;
        }

        /// <summary>What the bumper is called on the controller the player is holding.</summary>
        private static string BumperLabel(bool leftBumper)
        {
            GamepadType pad = LazyInput.CurrentGamepadType;
            int family = pad != null ? pad.value : GamepadType.Xbox_XboxController.value;

            if (family == GamepadType.Sony_DualShock.value || family == GamepadType.Sony_DualSense.value)
            {
                return leftBumper ? "L1" : "R1";
            }
            if (family >= GamepadType.Switch_JoyCon_Dual.value)
            {
                return leftBumper ? "L" : "R";
            }
            return leftBumper ? "LB" : "RB";
        }

        /// <summary>
        /// "one group" in the language the game is running in. The game has no locale entry for a
        /// stack of items (checked the shipped locale table), so the wording is carried by the mod.
        /// </summary>
        private static string GroupWord()
        {
            string lang = LLBase.CurrentLang;
            if (!string.IsNullOrEmpty(lang))
            {
                string word;
                if (GroupWords.TryGetValue(lang, out word))
                {
                    return word;
                }
                int cut = lang.IndexOf('-');
                if (cut > 0 && GroupWords.TryGetValue(lang.Substring(0, cut), out word))
                {
                    return word;
                }
            }
            return GroupWords["en"];
        }

        private static string ButtonHint(GamepadButton button, bool leftBumper)
        {
            GameKey key = FindKeyForButton(button);
            if (key != null)
            {
                string icon = ControllerIconLibrary.GetIconId(key, null, trailingSpace: true);
                // GetIconId hands back an empty <sprite> tag when the key has no icon in the library.
                if (!string.IsNullOrEmpty(icon) && !icon.Contains("name=\"\""))
                {
                    return icon;
                }
            }
            return "[" + BumperLabel(leftBumper) + "] ";
        }

        private static GameKey FindKeyForButton(GamepadButton button)
        {
            GameBindings bindings = LazyInput.GameBindings;
            if (bindings == null || bindings.gamepadBindings == null)
            {
                return null;
            }

            for (int i = 0; i < bindings.gamepadBindings.Count; i++)
            {
                GamepadBinding binding = bindings.gamepadBindings[i];
                if (binding != null && binding.gamepadButton != null && binding.gamepadButton.value == button.value)
                {
                    return binding.gameKey;
                }
            }
            return null;
        }

        private static TMP_Text FindTipsLabel(UIItemCountWindow window)
        {
            // Go through the window's own field: the tips component's label can be a child object,
            // in which case GetComponent<TextMeshProUGUI>() on the component itself finds nothing.
            LazyButtonTipsStr tips = TipsField.GetValue(window) as LazyButtonTipsStr;
            if (tips == null)
            {
                tips = window.GetComponentInChildren<LazyButtonTipsStr>(true);
            }
            if (tips == null)
            {
                return null;
            }

            TMP_Text label = TipsLabelField.GetValue(tips) as TMP_Text;
            if (label != null)
            {
                return label;
            }

            label = tips.GetComponent<TextMeshProUGUI>();
            return label != null ? label : tips.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        // The game's own UI resolves its TMP font through LazyFontData so that CJK languages
        // get the matching font asset; anything drawn by a mod has to do the same itself.
        private static void ApplyNativeFont(TextMeshProUGUI text, TextMeshProUGUI fallbackSource)
        {
            TMP_FontAsset font = null;
            try
            {
                LazyFontData data = FindFontData();
                if (data != null)
                {
                    font = data.GetFontAssetFor(LLBase.CurrentLang, false, false);
                }
            }
            catch (Exception ex)
            {
                StackStepPlugin.Log.LogWarning(StackStepPlugin.Tag + " native font lookup failed: " + ex.Message);
            }

            if (font == null && fallbackSource != null)
            {
                text.font = fallbackSource.font;
                text.fontSharedMaterial = fallbackSource.fontSharedMaterial;
                return;
            }

            text.font = font;
            if (font != null)
            {
                text.fontSharedMaterial = font.material;
            }
        }

        private static LazyFontData FindFontData()
        {
            LazyFontData[] all = Resources.FindObjectsOfTypeAll<LazyFontData>();
            if (all == null)
            {
                return null;
            }

            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && string.Equals(all[i].name, "small_font", StringComparison.OrdinalIgnoreCase))
                {
                    return all[i];
                }
            }
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name != null && all[i].name.IndexOf("small", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return all[i];
                }
            }
            return null;
        }

        private static float ResolveFontSize(TMP_Text tips, TMP_Text header)
        {
            if (tips != null && tips.fontSize > 0f)
            {
                return Mathf.Clamp(tips.fontSize * 0.85f, 14f, 22f);
            }
            if (header != null && header.fontSize > 0f)
            {
                return Mathf.Clamp(header.fontSize * 0.45f, 14f, 22f);
            }
            return 18f;
        }

    }
}
