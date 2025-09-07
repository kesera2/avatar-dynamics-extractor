using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace dev.kesera2.physbone_extractor
{
    internal sealed class AvatarDynamicsExtractorWindow : EditorWindow
    {
        private AvatarDynamicsExtractorService _service;
        private static int _selectedLanguage = 0;

        [MenuItem("Tools/kesera2/Avatar Dynamics Extractor")]
        public static void ShowWindow()
        {
            var window = GetWindow<AvatarDynamicsExtractorWindow>(Settings.WindowTitle);
            window.minSize = Settings.windowMinSize;
        }

        private void OnEnable()
        {
            _service = new AvatarDynamicsExtractorService();
            Init();
        }

        private void Init()
        {
            Localization.LoadLocalization(_selectedLanguage);
        }

        private void DrawSelectLanguage()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                var selectedLanguage =
                    EditorGUILayout.Popup(_selectedLanguage, Localization.DisplayNames,
                        Settings.LanguageGuiLayoutOptions);
                if (_selectedLanguage != selectedLanguage)
                    Localization.LoadLocalization(Localization.SupportedLanguages[selectedLanguage]);
                _selectedLanguage = selectedLanguage;
            }
        }

        private void OnGUI()
        {
            DrawSelectLanguage();
            DrawLogo();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(Localization.S("label.prefab.root"), Settings.LabelGuiLayoutOptions);
                    if (_service.PrefabRoot)
                    {
                        EditorGUILayout.LabelField(Localization.S("label.search.root"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.avatar_dynamics"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.pb"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.pbc"), Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.contacts"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("option.separate.contacts"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.contact_sender"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.contact_receiver"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.position_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.rotation_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.parent_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.scale_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.aim_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("label.name.lookat_constraints"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("option.remove.original"),
                            Settings.LabelGuiLayoutOptions);
                        EditorGUILayout.LabelField(Localization.S("option.keep.pb.version"),
                            Settings.LabelGuiLayoutOptions);
                    }
                }

                using (new GUILayout.VerticalScope())
                {
                    DrawAvatarRootField();
                    if (_service.PrefabRoot)
                    {
                        DrawSearchRootField();
                        _service.AvatarDynamicsGameObjectName = EditorGUILayout.TextField(_service.AvatarDynamicsGameObjectName);
                        _service.PbGameObjectName = EditorGUILayout.TextField(_service.PbGameObjectName);
                        _service.PbColliderGameObjectName = EditorGUILayout.TextField(_service.PbColliderGameObjectName);
                        _service.ContactsGameObjectName = EditorGUILayout.TextField(_service.ContactsGameObjectName);
                        _service.SplitContacts = EditorGUILayout.Toggle(_service.SplitContacts);
                        using (new EditorGUI.DisabledScope(!_service.SplitContacts))
                        {
                            _service.ContactSenderGameObjectName = EditorGUILayout.TextField(_service.ContactSenderGameObjectName);
                            _service.ContactReceiverGameObjectName = EditorGUILayout.TextField(_service.ContactReceiverGameObjectName);
                        }

                        _service.ConstraintsGameObjectName = EditorGUILayout.TextField(_service.ConstraintsGameObjectName);
                        _service.VrcPositionConstraintName = EditorGUILayout.TextField(_service.VrcPositionConstraintName);
                        _service.VrcRotationConstraintName = EditorGUILayout.TextField(_service.VrcRotationConstraintName);
                        _service.VrcParentConstraintName = EditorGUILayout.TextField(_service.VrcParentConstraintName);
                        _service.VrcScaleConstraintName = EditorGUILayout.TextField(_service.VrcScaleConstraintName);
                        _service.VrcAimConstraintName = EditorGUILayout.TextField(_service.VrcAimConstraintName);
                        _service.VrcLookAtConstraintName = EditorGUILayout.TextField(_service.VrcLookAtConstraintName);
                        _service.IsDeleteEnabled = EditorGUILayout.Toggle(_service.IsDeleteEnabled);
                        _service.IsKeepPbVersion = EditorGUILayout.Toggle(_service.IsKeepPbVersion);
                    }
                }
            }

            if (!_service.ValidatePrefabRoot()) return;
            using (new EditorGUI.DisabledScope(!_service.CanExecute()))
            {
                if (GUILayout.Button(Localization.S("button.extract")))
                {
                    if (!DisplayConfirmDialog())
                    {
                        return;
                    }

                    var result = _service.ExecuteExtraction();
                    if (result)
                    {
                        DisplayEndDialog();
                    }
                }
            }

            DisplayInfo();
            _service.ValidateSearchRoot();
            DisplayWarnSearchRoot();
        }

        private void DrawSearchRootField()
        {
            _service.UpdateSearchRoot();

            using (new EditorGUILayout.HorizontalScope())
            {
                _service.SearchRoot =
                    (GameObject)EditorGUILayout.ObjectField(_service.SearchRoot, typeof(GameObject), true);
            }
        }

        private void DrawAvatarRootField()
        {
            var avatarRoot = (GameObject)EditorGUILayout.ObjectField(_service.PrefabRoot,
                typeof(GameObject), true);
            _service.SetPrefabRoot(avatarRoot);
        }

        private static void DrawLogo()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!File.Exists(Settings.GetLogoPath())) return;
                var fileData = File.ReadAllBytes(Settings.GetLogoPath());
                var logo = new Texture2D(2, 2); // サイズは後で自動調整される
                logo.LoadImage(fileData); // PNG, JPG 形式の画像をロード
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(new GUIContent(logo), GUILayout.Height(100), GUILayout.Width(400));
                GUILayout.FlexibleSpace();
            }
        }

        private void DisplayInfo()
        {
            if (_service.SearchRoot)
            {
                EditorGUILayout.HelpBox(Localization.S("info.description"), MessageType.Info);
            }
        }

        private void DisplayWarnSearchRoot()
        {
            if (!_service.SearchRoot)
            {
                EditorGUILayout.HelpBox(Localization.S("warn.need.assign.search.gameobject"), MessageType.Warning);
            }
        }

        private bool DisplayConfirmDialog()
        {
            var message = Localization.S("msg.dialog.message") + "\n";
            if (_service.IsDeleteEnabled)
            {
                message += "\n" + Localization.S("msg.dialog.remove.original");
            }
            else
            {
                message += "\n" + Localization.S("msg.dialog.keep.original");
            }

            if (_service.IsKeepPbVersion)
            {
                message += "\n" + Localization.S("msg.dialog.keep.pb.version");
            }
            else
            {
                message += "\n" + Localization.S("msg.dialog.update.pb.version");
            }

            // メッセージボックスを表示
            return EditorUtility.DisplayDialog(
                Localization.S("msg.dialog.title"), // ダイアログのタイトル
                message, // メッセージ
                "OK",
                "Cancel"
            );
        }

        private void DisplayEndDialog()
        {
            var (pbCount, pbcCount, senderCount, receiverCount, constraintCount) = _service.GetCopiedComponentCounts();
            EditorUtility.DisplayDialog(
                Localization.S("msg.dialog.title"), // ダイアログのタイトル
                string.Format(Localization.S("msg.dialog.end"), pbCount, pbcCount, senderCount, receiverCount, constraintCount),
                "OK"
            );
        }
    }
}