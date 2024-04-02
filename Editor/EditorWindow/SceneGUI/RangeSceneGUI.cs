using System;
using System.Collections.Generic;
using System.Reflection;
using ACTSkill;
using UnityEditor;
using UnityEngine;

namespace ACTSkillEditor
{
    public class RangeSceneGUI : SceneGUIBase
    {
        private Dictionary<Type, IRangeDrawer> drawerDict;
        private Quaternion? startRotation;

        //UnityEditor.Handles.RotationHandleIds.xyz
        private static FieldInfo rotationHandleIdsxyzField;
        //UnityEditor.Handles.RotationHandleIds.@default
        private static PropertyInfo rotationHandleIdsDefaultProperty;
        //UnityEditor.Handles.RotationHandleParam.Default
        private static object rotationHandleParamDefault;
        // internal static Quaternion DoRotationHandle(
        //     Handles.RotationHandleIds ids,
        //     Quaternion rotation,
        //     Vector3 position,
        //     Handles.RotationHandleParam param)
        private static MethodInfo doRotationHandleMethod;
        private static object[] paramObjs = new object[4];

        static RangeSceneGUI()
        {
            var rotationHandleIdsType = typeof(Handles).GetNestedType("RotationHandleIds", BindingFlags.Public | BindingFlags.NonPublic);
            rotationHandleIdsxyzField = rotationHandleIdsType.GetField("xyz",
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            rotationHandleIdsDefaultProperty = rotationHandleIdsType.GetProperty("default", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            rotationHandleParamDefault = typeof(Handles)
                .GetNestedType("RotationHandleParam", BindingFlags.Public | BindingFlags.NonPublic)
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .GetValue(null);
            
            doRotationHandleMethod = typeof(Handles).GetMethod("DoRotationHandle", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static int GetDefaultXYZRotationHandleId(object rotationHandleIds)
        {
            return (int)rotationHandleIdsxyzField.GetValue(rotationHandleIds);
        }
        
        public RangeSceneGUI(ACTSkillEditorWindow owner) : base(owner)
        {
            drawerDict = new Dictionary<Type, IRangeDrawer>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IRangeDrawer>())
            {
                try
                {
                    if (type.GetCustomAttribute(typeof(CustomRangeDrawerAttribute), true) is not CustomRangeDrawerAttribute attribute ||
                        attribute.TargetType == null) continue;
                    if (drawerDict.ContainsKey(attribute.TargetType)) continue;
                    if (Activator.CreateInstance(type) is not IRangeDrawer drawer) continue;
                    drawerDict.Add(attribute.TargetType, drawer);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public override void OnSceneGUI(SceneView sceneView)
        {
            if (Owner.CurState == null || !Owner.Target) return;

            List<IRange> ranges;
            bool enable;
            
            //Attack range
            if (Owner.CurFrameConfig?.AttackRange?.ModifyRange == true)
            {
                enable = true;
                ranges = Owner.CurFrameConfig.AttackRange.Ranges;
            }
            else
            {
                enable = false;
                ranges = Owner.CurState.GetAttackRange(Owner.SelectedFrameIndex)?.Ranges;
            }
            DrawRanges(ranges, Owner.Target.transform.localToWorldMatrix, enable, Owner.SelectedAttackRangeIndex, new Color(1, 0, 0, 0.25f));
            
            
            //Body range
            if (Owner.CurFrameConfig?.BodyRange?.ModifyRange == true)
            {
                enable = true;
                ranges = Owner.CurFrameConfig.BodyRange.Ranges;
            }
            else
            {
                enable = false;
                ranges = Owner.CurState.GetBodyRange(Owner.SelectedFrameIndex)?.Ranges;
            }
            DrawRanges(ranges, Owner.Target.transform.localToWorldMatrix, enable, Owner.SelectedBodyRangeIndex, new Color(0, 0, 1, 0.25f));
        }

        private void DrawRanges(IReadOnlyList<IRange> ranges, Matrix4x4 localToWorld, bool enable, int selectedIndex, Color color)
        {
            if (ranges == null) return;
            // Do not use scale.
            Matrix4x4 localToWorldNoScale = Matrix4x4.TRS(localToWorld.GetPosition(), localToWorld.rotation, Vector3.one);
            Matrix4x4 oldMatrix = Handles.matrix;
            
            for (int i = 0, count = ranges.Count; i < count; i++)
            {
                IRange range = ranges[i];
                if (range == null) continue;
                Handles.matrix = localToWorldNoScale;
                DrawRange(range, color);
                if (enable && i == selectedIndex)
                {
                    Handles.matrix = oldMatrix;
                    ControlRange(range, localToWorldNoScale);
                }
            }
            Handles.matrix = oldMatrix;
        }

        private void DrawRange(IRange range, Color color)
        {
            if (drawerDict.TryGetValue(range.GetType(), out var drawer))
                drawer.Draw(range, color);
        }

        private void ControlRange(IRange range, Matrix4x4 matrix)
        {
            Vector3? offset = range.GetOffset();
            Vector3? rotation = range.GetRotation();
            Vector3? size = range.GetSize();

            PivotRotation pivotRotation = Tools.pivotRotation;
            //Change to target matrix
            Vector3 newOffset = matrix.MultiplyPoint3x4(offset ?? Vector3.zero);
            Quaternion newRotation = matrix.rotation * Quaternion.Euler(rotation ?? Vector3.zero);
            Quaternion handleRotation = pivotRotation == PivotRotation.Global ? Quaternion.identity : newRotation;
            Vector3 newSize = Vector3.Scale(matrix.lossyScale, size ?? Vector3.one);
            float handleSize = HandleUtility.GetHandleSize(newOffset);
            switch (Tools.current)
            {
                case Tool.Move:
                    if (offset.HasValue)
                        newOffset = Handles.DoPositionHandle(newOffset, handleRotation);
                    break;
                case Tool.Rotate:
                    if (rotation.HasValue)
                    {
                        var eventType = Event.current.type;
                        Quaternion doRotationHandle;
                        if (pivotRotation == PivotRotation.Global)
                        {
                            var rotationHandleIdsDefault = rotationHandleIdsDefaultProperty.GetValue(null);
                            paramObjs[0] = rotationHandleIdsDefault;
                            paramObjs[1] = handleRotation;
                            paramObjs[2] = newOffset;
                            paramObjs[3] = rotationHandleParamDefault;
                            EditorGUI.BeginChangeCheck();
                            doRotationHandle = (Quaternion)doRotationHandleMethod.Invoke(null, paramObjs);
                            if (EditorGUI.EndChangeCheck())
                            {
                                startRotation ??= newRotation;

                                if (GUIUtility.hotControl == GetDefaultXYZRotationHandleId(rotationHandleIdsDefault))
                                    //Free, delta with input rotate
                                    newRotation = doRotationHandle * newRotation;
                                else
                                    //Fix axis, delta with start drag rotation. Otherwise return input rotate
                                    //So judge is it same angle. Same angle means not drag, do not change rotation
                                    newRotation = doRotationHandle * (startRotation ?? newRotation);
                            }
                        }
                        else
                        {
                            EditorGUI.BeginChangeCheck();
                            doRotationHandle = Handles.DoRotationHandle(handleRotation, newOffset);
                            if (EditorGUI.EndChangeCheck())
                                newRotation = doRotationHandle;
                        }
                        if (eventType == EventType.MouseUp)
                            startRotation = null;
                    }
                    break;
                case Tool.Scale:
                    if (size.HasValue)
                        newSize = Handles.DoScaleHandle(newSize, newOffset, handleRotation, handleSize);
                    break;
            }

            //Revert to ori matrix
            Matrix4x4 inverseMatrix = matrix.inverse;
            newOffset = inverseMatrix.MultiplyPoint3x4(newOffset);
            newRotation = inverseMatrix.rotation * newRotation;
            newSize = Vector3.Scale(inverseMatrix.lossyScale, newSize);
            bool change = false;
            if (newOffset != offset)
            {
                Owner.RecordObject("Change range offset");
                range.SetOffset(newOffset);
                change = true;
            }
            if (newRotation.eulerAngles != rotation)
            {
                Owner.RecordObject("Change range rotation");
                range.SetRotation(newRotation.eulerAngles);
                change = true;
            }
            if (newSize != size)
            {
                Owner.RecordObject("Change range size");
                range.SetSize(newSize);
                change = true;
            }
            if (change && Owner)
                Owner.Repaint();
        }
    }
}
