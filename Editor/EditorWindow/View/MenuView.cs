using System;
using ACTSkill;
using CustomizationInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class MenuView : ViewBase
    {
        public MenuView(ACTSkillEditorWindow owner) : base(owner)
        {
        }

        protected override void OnGUI(Rect contentRect)
        {
            GUILayout.BeginArea(contentRect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                if (Owner.ConfigAsset)
                    Owner.Save();
                else
                    Owner.SaveAs();
            }
            
            if (GUILayout.Button("Save As", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Owner.SaveAs();
            }
            
            if (GUILayout.Button(EditorUtil.TempContent("Reload", "Revert current change and reload from config"),
                EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Owner.Reload();
            }
            
            if (GUILayout.Button(EditorUtil.TempContent("Clear Config", "Clear machine config data"),
                EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Clear machine config data", "Are you sure to clear the machine config data?", "Confirm", "Cancel"))
                    Owner.ClearConfig();
            }
            
            EditorGUILayout.Space();

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            Owner.Target = EditorGUILayout.ObjectField("Target", Owner.Target, typeof(GameObject), true) as GameObject;
            var asset = Owner.ConfigAsset;
            Owner.ConfigAsset = EditorGUILayout.ObjectField("Config", Owner.ConfigAsset, typeof(TextAsset), true) as TextAsset;
            if (asset != Owner.ConfigAsset && Owner.ConfigAsset)
            {
                if (EditorUtility.DisplayDialog("Reload", "Would you want to reload machine config?", "Yes", "No"))
                    Owner.Reload();
            }
            EditorGUIUtility.labelWidth = oldLabelWidth;

            var content = EditorUtil.TempImageOrTextContent("Setting", GUIStyleHelper.SettingTexture);
            var style = EditorStyles.toolbarButton;
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.Width(26));
            if (GUI.Button(rect, content, style))
            {
                var menu = new GenericMenu();
                
                var frameRate = Owner.CurMachineProperty.FindPropertyRelative(nameof(MachineConfig.FrameRate)).intValue;
                bool hasOn = false;
                bool on = frameRate == 24;
                hasOn |= on;
                menu.AddItem(new GUIContent("Frame Rate/Film: 24 fps"), on, SetFrameRate, 24);
                on = frameRate == 25;
                hasOn |= on;
                menu.AddItem(new GUIContent("Frame Rate/PAL: 25 fps"), on, SetFrameRate, 25);
                on = frameRate == 30;
                hasOn |= on;
                menu.AddItem(new GUIContent("Frame Rate/HD: 30 fps"), on, SetFrameRate, 30);
                on = frameRate == 60;
                hasOn |= on;
                menu.AddItem(new GUIContent("Frame Rate/Game: 60 fps"), on, SetFrameRate, 60);
                if (hasOn)
                    menu.AddItem(new GUIContent("Frame Rate/Customize"), false, SetCustomizeFrameRate, (frameRate, rect));
                else
                    menu.AddItem(new GUIContent($"Frame Rate/Customize: {frameRate} fps"), true, SetCustomizeFrameRate, (frameRate, rect));

                menu.ShowAsContext();
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void SetFrameRate(object userData)
        {
            var property = Owner.CurMachineProperty.FindPropertyRelative(nameof(MachineConfig.FrameRate));
            if (property == null || userData is not int frameRate || frameRate <= 0) return;
            property.intValue = frameRate;
            Owner.ApplyModifiedProperties();
        }
        
        private void SetCustomizeFrameRate(object userData)
        {
            if (userData is not ValueTuple<int, Rect> data) return;
            var window = CustomizeFrameRateWindow.Create(data.Item1);
            window.WindowClosed += newFrameRate =>
            {
                if (newFrameRate > 0)
                    SetFrameRate(newFrameRate);
                else if (Owner)
                    Owner.ShowNotification("Can not set 0 or negative frame rate!", 3, LogType.Error);
            };
            window.ShowAsDropDown(data.Item2);
        }
    }
}
