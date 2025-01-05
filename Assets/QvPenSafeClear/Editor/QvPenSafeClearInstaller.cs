using System;
using System.Collections.Generic;
using QvPen.Udon.UI;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using net.ts7m.qvpen_safe_clear.udon;

namespace net.ts7m.qvpen_safe_clear.editor {
    [Obsolete("Use latest QvPen instead.")]
    public class QvPenSafeClearInstallerWindow: EditorWindow {
        private const string PropNameCanUseEveryone = "canUseEveryone";
        private const string PropNameCanUseInstanceOwner = "canUseInstanceOwner";
        private const string PropNameCanUseOwner = "canUseOwner";
        private const string PropNameCanUseMaster = "canUseMaster";
        private const string PropNameIsGlobalEvent  = "isGlobalEvent";
        private const string PropNameOnlySendToOwner = "onlySendToOwner";
        private const string PropNameUdonSharpBehaviour = "udonSharpBehaviour";
        private const string PropNameUdonSharpBehaviours = "udonSharpBehaviours";
        private const string PropNameCustomEventName = "customEventName";
        private const string PropNameRequireDoubleClick = "requireDoubleClick";
        private const string PropNameDoubleClickDuration = "doubleClickDuration";

        private const string ClearCustomEventName = "Clear";

        private const string TextDeprecated = "この拡張機能は非推奨です。代わりに最新の QvPen を使用してください。";
        private const string TextInstallTargets = "インストール先オブジェクト";
        private const string TextClearInstallTargets = "リストを空にする";
        private const string TextInstall = "インストール";
        private const string TextInstallAll = "すべてインストール";
        private const string TextScanScene = "シーン内を探す";
        private const string TextDoubleRequired = "ダブルクリックを有効にする";
        private const string TextDoubleClickDuration = "ダブルクリックの間隔";
        private readonly string _textHintSummary = $"リストに置き換える QvPen_InteractionButton コンポーネントを追加し、 「{TextInstallAll}」 ボタンまたは 「{TextInstall}」 ボタンにを押すことでコンポーネントを置き換えます。 「{TextScanScene}」 ボタンでアクティブシーン内のすべての QvPen_InteractionButton コンポーネントを自動で追加します。";

        private List<QvPen_InteractButton> _clearButtons = new();
        private Vector2 _scrollPos = Vector2.zero;
        private bool _doubleClickRequired = true;
        private float _doubleClickDuration = 0.5f;

        private string _getCustomEventName(QvPen_InteractButton button) {
            var obj = new SerializedObject(button);
            return obj.FindProperty(PropNameCustomEventName).stringValue;
        }

        [MenuItem("Window/QvPen Safe Clear Installer")]
        private static void ShowWindow() {
            GetWindow<QvPenSafeClearInstallerWindow>("QvPen Safe Clear Installer");
        }

        private void OnGUI() {
            EditorGUILayout.HelpBox(TextDeprecated, MessageType.Warning);

            EditorGUILayout.HelpBox(this._textHintSummary, MessageType.Info);

            if (GUILayout.Button(TextInstallAll)) {
                foreach (var button in this._clearButtons) {
                    this.Install(button);
                }
            }

            GUILayout.Space(10);

            this._doubleClickRequired = EditorGUILayout.Toggle(TextDoubleRequired, this._doubleClickRequired);
            this._doubleClickDuration = EditorGUILayout.FloatField(TextDoubleClickDuration, this._doubleClickDuration);

            GUILayout.Space(10);

            GUILayout.Label(TextInstallTargets, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(TextClearInstallTargets)) {
                this._clearButtons.Clear();
            }
            if (GUILayout.Button(TextScanScene)) {
               this.Scan();
            }

            EditorGUILayout.EndHorizontal();

            this._scrollPos = EditorGUILayout.BeginScrollView(this._scrollPos);

            for (var i = 0; i < this._clearButtons.Count; i++) {
                var button = this._clearButtons[i];
                if (!button) continue;

                EditorGUILayout.BeginHorizontal();

                var modifiedItem = (QvPen_InteractButton) EditorGUILayout.ObjectField(button, typeof(QvPen_InteractButton), true);
                if (modifiedItem != button) {
                    this._clearButtons[i] = modifiedItem;
                    // While we would really like to delete the item here, we instead just ignore them
                    // because we don't like to delete items in the iteration and
                    // the lifespan of the window is supposed to be short.
                }

                if (GUILayout.Button(TextInstall)) {
                    this.Install(button);
                }

                EditorGUILayout.EndHorizontal();
            }

            var newObj = (QvPen_InteractButton) EditorGUILayout.ObjectField(null, typeof(QvPen_InteractButton), true);
            if (newObj) {
                this._clearButtons.Add(newObj);
            }

            GUILayout.EndScrollView();
        }

        // Collect all QvPen_InteractButton with customEventName "clear" from the active scene
        private void Scan() {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in rootObjects) {
                var components = root.GetComponentsInChildren<QvPen_InteractButton>();
                foreach (var button in components) {
                    if (this._clearButtons.IndexOf(button) >= 0) continue;
                    if (this._getCustomEventName(button) != ClearCustomEventName) continue;
                    this._clearButtons.Add(button);
                }
            }
        }

        // Migrate from QvPen_InteractButton to QvPenInteractButtonExtended
        // preserving original properties
        private void Install(QvPen_InteractButton button) {
            if (!button) return;

            var gameObj = button.gameObject;
            var buttonEx = gameObj.AddUdonSharpComponent<QvPenInteractButtonExtended>();
            var sButton = new SerializedObject(button);
            var sButtonEx = new SerializedObject(buttonEx);

            var canUseEveryone = sButton.FindProperty(PropNameCanUseEveryone).boolValue;
            var canUseInstanceOwner = sButton.FindProperty(PropNameCanUseInstanceOwner).boolValue;
            var canUseObjectOwner = sButton.FindProperty(PropNameCanUseOwner).boolValue;
            var canUseMaster = sButton.FindProperty(PropNameCanUseMaster).boolValue;
            var isGlobalEvent = sButton.FindProperty(PropNameIsGlobalEvent).boolValue;
            var onlySendToOwner = sButton.FindProperty(PropNameOnlySendToOwner).boolValue;
            var udonSharpBehaviour = sButton.FindProperty(PropNameUdonSharpBehaviour).objectReferenceValue;
            var udonSharpBehaviours = sButton.FindProperty(PropNameUdonSharpBehaviours);
            var customEventName = sButton.FindProperty(PropNameCustomEventName).stringValue;

            sButtonEx.FindProperty(PropNameCanUseEveryone).boolValue = canUseEveryone;
            sButtonEx.FindProperty(PropNameCanUseInstanceOwner).boolValue = canUseInstanceOwner;
            sButtonEx.FindProperty(PropNameCanUseOwner).boolValue = canUseObjectOwner;
            sButtonEx.FindProperty(PropNameCanUseMaster).boolValue = canUseMaster;
            sButtonEx.FindProperty(PropNameIsGlobalEvent).boolValue = isGlobalEvent;
            sButtonEx.FindProperty(PropNameOnlySendToOwner).boolValue = onlySendToOwner;
            sButtonEx.FindProperty(PropNameUdonSharpBehaviour).objectReferenceValue = udonSharpBehaviour;
            sButtonEx.FindProperty(PropNameCustomEventName).stringValue = customEventName;

            var exUdonSharpBehaviours = sButtonEx.FindProperty(PropNameUdonSharpBehaviours);
            exUdonSharpBehaviours.arraySize = udonSharpBehaviours.arraySize;
            for (var i = 0; i < udonSharpBehaviours.arraySize; i++) {
                exUdonSharpBehaviours.GetArrayElementAtIndex(i).objectReferenceValue = udonSharpBehaviours.GetArrayElementAtIndex(i).objectReferenceValue;
            }

            sButtonEx.FindProperty(PropNameRequireDoubleClick).boolValue = this._doubleClickRequired;
            sButtonEx.FindProperty(PropNameDoubleClickDuration).floatValue = this._doubleClickDuration;

            sButtonEx.ApplyModifiedPropertiesWithoutUndo();

            UdonSharpEditorUtility.DestroyImmediate(button);
        }
    }
}
