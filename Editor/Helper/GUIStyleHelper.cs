using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public static class GUIStyleHelper
    {
        public static readonly GUIStyle TitleMiddleCenter = new GUIStyle(EditorStyles.toolbar) {alignment = TextAnchor.MiddleCenter, fontSize = GUI.skin.label.fontSize};
        public static readonly GUIStyle LabelMiddleCenter = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
        public static readonly GUIStyle ViewBg = "hostview";
        public static readonly GUIStyle ItemHeadNormal = new GUIStyle("SelectionRect") {alignment = TextAnchor.MiddleCenter};
        public static readonly GUIStyle ItemHeadSelect = new GUIStyle("OL SelectedRow") {alignment = TextAnchor.MiddleCenter};
        public static readonly GUIStyle ItemBodyNormal = "ScrollViewAlt";
        public static readonly GUIStyle ItemBodySelect = "TE ElementBackground";

        private static GUIContent playButtonContent;
        public static GUIContent PlayButtonContent
        {
            get
            {
                if (playButtonContent == null)
                {
                    Texture2D texture = FindEditorResourcesTexture("d_Animation.Play@2x");
                    if (texture)
                        playButtonContent = new GUIContent(texture);
                    else
                        playButtonContent = new GUIContent("Play");
                }
                return playButtonContent;
            }
        }
        
        private static GUIContent prevKeyButtonContent;
        public static GUIContent PrevKeyButtonContent
        {
            get
            {
                if (prevKeyButtonContent == null)
                {
                    Texture2D texture = FindEditorResourcesTexture("d_Animation.PrevKey@2x");
                    if (texture)
                        prevKeyButtonContent = new GUIContent(texture);
                    else
                        prevKeyButtonContent = new GUIContent("Prev Key");
                }
                return prevKeyButtonContent;
            }
        }
        
        private static GUIContent nextKeyButtonContent;
        public static GUIContent NextKeyButtonContent
        {
            get
            {
                if (nextKeyButtonContent == null)
                {
                    Texture2D texture = FindEditorResourcesTexture("d_Animation.NextKey@2x");
                    if (texture)
                        nextKeyButtonContent = new GUIContent(texture);
                    else
                        nextKeyButtonContent = new GUIContent("Next Key");
                }
                return nextKeyButtonContent;
            }
        }
        
        private static GUIContent firstKeyButtonContent;
        public static GUIContent FirstKeyButtonContent
        {
            get
            {
                if (firstKeyButtonContent == null)
                {
                    Texture2D texture = FindEditorResourcesTexture("d_Animation.FirstKey@2x");
                    if (texture)
                        firstKeyButtonContent = new GUIContent(texture);
                    else
                        firstKeyButtonContent = new GUIContent("First Key");
                }
                return firstKeyButtonContent;
            }
        }
        
        private static GUIContent lastKeyButtonContent;
        public static GUIContent LastKeyButtonContent
        {
            get
            {
                if (lastKeyButtonContent == null)
                {
                    Texture2D texture = FindEditorResourcesTexture("d_Animation.LastKey@2x");
                    if (texture)
                        lastKeyButtonContent = new GUIContent(texture);
                    else
                        lastKeyButtonContent = new GUIContent("Last Key");
                }
                return lastKeyButtonContent;
            }
        }
        
        private static GUIContent refreshButtonContent;
        public static GUIContent RefreshButtonContent
        {
            get
            {
                if (refreshButtonContent == null)
                {
                    Texture2D texture = FindEditorResourcesTexture("d_RotateTool@2x");
                    if (texture)
                        refreshButtonContent = new GUIContent(texture);
                    else
                        refreshButtonContent = new GUIContent("Fresh");
                }
                return refreshButtonContent;
            }
        }

        private static Texture2D loopTexture;
        public static Texture2D LoopTexture
        {
            get
            {
                if (!loopTexture)
                    loopTexture = FindEditorResourcesTexture("d_preAudioLoopOff@2x");
                return loopTexture;
            }
        }
        
        private static Texture2D loopOffTexture;
        public static Texture2D LoopOffTexture
        {
            get
            {
                if (!loopOffTexture)
                    loopOffTexture = FindEditorResourcesTexture("playLoopOff");
                return loopOffTexture;
            }
        }
        
        private static Texture2D loopOnTexture;
        public static Texture2D LoopOnTexture
        {
            get
            {
                if (!loopOnTexture)
                    loopOnTexture = FindEditorResourcesTexture("playLoopOn");
                return loopOnTexture;
            }
        }
        
        private static Texture2D settingTexture;
        public static Texture2D SettingTexture
        {
            get
            {
                if (!settingTexture)
                    settingTexture = FindEditorResourcesTexture("d_SettingsIcon@2x");
                return settingTexture;
            }
        }

        // private static Object[] editorResources;
        // public static Object[] EditorResources
        // {
        //     get { return editorResources ??= AssetDatabase.LoadAllAssetsAtPath("Library/unity editor resources"); }
        // }
        
        public static Texture2D FindEditorResourcesTexture(string name)
        {
            return EditorGUIUtility.Load(name) as Texture2D;
            // if (EditorResources == null) return null;
            // foreach (var obj in EditorResources)
            // {
            //     if (obj.name == name && obj is Texture2D texture)
            //         return texture;
            // }
            // return null;
        }
    }
}
