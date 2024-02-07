#if UNITY_EDITOR
using FIMSpace.FEditor;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollProcessor
    {
        public enum EViewCategory { Setup, Play, Extra }
        public EViewCategory _EditorCategory = EViewCategory.Setup;

        [HideInInspector] public bool _EditorDrawBones = true;
        [HideInInspector] public bool _EditorDrawHumanoidExtra = false;
        [HideInInspector] public bool _EditorDrawGenerator = false;
        [HideInInspector] public bool _EditorDrawMore = false;

        [NonSerialized] public int _EditorSelectedChain = -1;
        [NonSerialized] public RagdollProcessor.RagdollBoneSetup _EditorSelectedBone = null;

        static bool _displayHipsPinSettings = false;

        public static void Editor_DrawTweakGUI(SerializedProperty sp_param, RagdollProcessor proc)
        {
            EditorGUILayout.PropertyField(sp_param);
            bool freeFall = sp_param.boolValue;
            sp_param.Next(false);

            float amount = sp_param.floatValue;

            Color preC = GUI.color;
            if (freeFall && amount < 0.5f) GUI.color = Color.yellow;
            EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
            if (freeFall && amount < 0.5f) GUI.color = preC;

            EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
            EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);

            GUILayout.Space(6);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            // Blend on collision
            EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
            GUILayout.Space(2);

            if (proc.BlendOnCollision)
            {

                if (proc.BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                {
                    // constant blends
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    // Sensitive collision
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                }
                else if (proc.BonesSetupMode == EBonesSetupMode.CustomLimbs)
                {
                    EditorGUI.BeginChangeCheck();

                    for (int i = 0; i < proc.CustomLimbsBonesChains.Count; i++)
                    {
                        var chain = proc.CustomLimbsBonesChains[i];
                        if (chain.BlendOnCollisions == false) continue;
                        chain.ConstantRagdoll = EditorGUILayout.Slider("Constant " + chain.ChainName + " Ragdoll:", chain.ConstantRagdoll, 0f, 1f);
                        if (Application.isPlaying)
                        {
                            GUI.enabled = false;
                            if (chain.blendOnCollisionMin > 0.001f)
                            { EditorGUILayout.Slider("Blend:", chain.blendOnCollisionMin, 0f, 1f); }
                            GUI.enabled = true;
                        }
                    }

                    sp_param.Next(false); sp_param.Next(false); sp_param.Next(false);sp_param.Next(false);sp_param.Next(false);
                    if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(sp_param.serializedObject.targetObject);
                }
            }
            else
            {
                sp_param.Next(false); sp_param.Next(false); sp_param.Next(false);sp_param.Next(false);sp_param.Next(false);
            }

            EditorGUILayout.EndVertical();

            // spring damping
            EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
            EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);

            if (proc.HipsPin)
            {
                FGUI_Inspector.FoldHeaderStart(ref _displayHipsPinSettings, "Hips Pin Adjustments", EditorStyles.helpBox);
                if (_displayHipsPinSettings)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
                    GUILayout.Space(3);
                }
                else
                {
                    sp_param.Next(false); sp_param.Next(false); sp_param.Next(false); sp_param.Next(false);
                }
                GUILayout.EndVertical();
            }
            else
            {
                sp_param.Next(false); sp_param.Next(false); sp_param.Next(false); sp_param.Next(false);
            }

            // more custom
            EditorGUILayout.PropertyField(sp_param); //sp_param.Next(false);
            //EditorGUILayout.PropertyField(sp_param); sp_param.Next(false);
            //EditorGUILayout.PropertyField(sp_param);

            if (proc.lminArmsAnim > 0f || proc.minRArmsAnim > 0f || proc.minSpineAnim > 0f || proc.minHeadAnim > 0f || proc.minChestAnim > 0f)
            {
                bool preE = GUI.enabled; GUI.enabled = false;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayoutOption wdth = GUILayout.Width(90);

                if (proc.lminArmsAnim > 0.0001f)
                {
                    EditorGUILayout.Slider("Left Arm Blend In", proc.lminArmsAnim, 0f, 1f);

                    if (proc.posingLeftUpperArm.collisions.EnteredCollisions.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(proc.posingLeftUpperArm.transform, typeof(Transform), true);
                        EditorGUILayout.LabelField("Collides With", wdth);
                        EditorGUILayout.ObjectField(proc.posingLeftUpperArm.collisions.EnteredCollisions[0], typeof(Transform), true);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (proc.posingLeftForeArm.collisions.EnteredCollisions.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(proc.posingLeftForeArm.transform, typeof(Transform), true);
                        EditorGUILayout.LabelField("Collides With", wdth);
                        EditorGUILayout.ObjectField(proc.posingLeftForeArm.collisions.EnteredCollisions[0], typeof(Transform), true);
                        EditorGUILayout.EndHorizontal();
                    }

                }

                if (proc.minRArmsAnim > 0.0001f)
                {
                    EditorGUILayout.Slider("Right Arm Blend In", proc.minRArmsAnim, 0f, 1f);

                    if (proc.posingRightUpperArm.collisions.EnteredCollisions.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(proc.posingRightUpperArm.transform, typeof(Transform), true);
                        EditorGUILayout.LabelField("Collides With", wdth);
                        EditorGUILayout.ObjectField(proc.posingRightUpperArm.collisions.EnteredCollisions[0], typeof(Transform), true);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (proc.posingRightForeArm.collisions.EnteredCollisions.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(proc.posingRightForeArm.transform, typeof(Transform), true);
                        EditorGUILayout.LabelField("Collides With", wdth);
                        EditorGUILayout.ObjectField(proc.posingRightForeArm.collisions.EnteredCollisions[0], typeof(Transform), true);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (proc.minHeadAnim > 0.0001f)
                {
                    EditorGUILayout.Slider("Head Blend In", proc.minHeadAnim, 0f, 1f);

                    if (proc.posingHead.collisions.EnteredCollisions.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(proc.posingHead.transform, typeof(Transform), true);
                        EditorGUILayout.LabelField("Collides With", wdth);
                        EditorGUILayout.ObjectField(proc.posingHead.collisions.EnteredCollisions[0], typeof(Transform), true);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (proc.minChestAnim > 0.0001f)
                {
                    EditorGUILayout.Slider("Chest Blend In", proc.minChestAnim, 0f, 1f);

                    if (proc.posingChest != null)
                        if (proc.posingChest.transform != null)
                            if (proc.posingChest.collisions.EnteredCollisions.Count > 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.ObjectField(proc.posingChest.transform, typeof(Transform), true);
                                EditorGUILayout.LabelField("Collides With", wdth);
                                EditorGUILayout.ObjectField(proc.posingChest.collisions.EnteredCollisions[0], typeof(Transform), true);
                                EditorGUILayout.EndHorizontal();
                            }

                    if (proc.posingSpineStart != null)
                        if (proc.posingSpineStart.transform != null)
                            if (proc.posingSpineStart.collisions.EnteredCollisions.Count > 0)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.ObjectField(proc.posingSpineStart.transform, typeof(Transform), true);
                                EditorGUILayout.LabelField("Collides With", wdth);
                                EditorGUILayout.ObjectField(proc.posingSpineStart.collisions.EnteredCollisions[0], typeof(Transform), true);
                                EditorGUILayout.EndHorizontal();
                            }
                }

                if (proc.minSpineAnim > 0.0001f)
                {
                    EditorGUILayout.Slider("Spine Blend In", proc.minSpineAnim, 0f, 1f);
                }

                EditorGUILayout.EndVertical();
                GUI.enabled = preE;
            }

            GUILayout.Space(5);
            FGUI_Inspector.FoldHeaderStart(ref proc._EditorDrawMore, "More Individual Limbs Settings", FGUI_Resources.BGInBoxStyle);
            if (proc._EditorDrawMore)
            {

                if (proc.BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                {
                    sp_param.Next(false); EditorGUILayout.PropertyField(sp_param);
                    sp_param.Next(false); EditorGUILayout.PropertyField(sp_param);
                    FGUI_Inspector.DrawUILine(0.3f, 0.3f, 1, 4);

                    sp_param.Next(false); EditorGUILayout.PropertyField(sp_param);
                    sp_param.Next(false); EditorGUILayout.PropertyField(sp_param);
                    FGUI_Inspector.DrawUILine(0.3f, 0.3f, 1, 4);

                    sp_param.Next(false); EditorGUILayout.PropertyField(sp_param);
                    sp_param.Next(false); EditorGUILayout.PropertyField(sp_param);
                    GUILayout.Space(6);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    for (int i = 0; i < proc.CustomLimbsBonesChains.Count; i++)
                    {
                        var chain = proc.CustomLimbsBonesChains[i];
                        chain.MusclesForce = EditorGUILayout.Slider(chain.ChainName + " Muscles Force:", chain.MusclesForce, 0f, 1f);
                    }

                    if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(sp_param.serializedObject.targetObject);
                }
            }

            GUILayout.EndVertical();

            //Editor_MainSettings(sp_ragProcessor.FindPropertyRelative("SideSwayPower"));
            //GUILayout.Space(2);
            //Editor_RootSwaySettings(sp_ragProcessor.FindPropertyRelative("FootsOrigin"));
            //GUILayout.Space(2);
            //Editor_SpineLeanSettings(sp_ragProcessor.FindPropertyRelative("SpineBlend"));
        }

        public void DrawSetupGizmos()
        {
            if (Pelvis && BaseTransform)
            {
                if (BonesSetupMode == RagdollProcessor.EBonesSetupMode.HumanoidLimbs)
                {
                    UnityEditor.Handles.color = new Color(0.3f, 1f, 0.3f, 0.4f);
                    Gizmos.color = new Color(0.7f, 1f, 0.7f, 0.2f);


                    float scaleRef = (Pelvis.position - BaseTransform.position).magnitude;

                    Gizmos.DrawLine(Pelvis.position, BaseTransform.position);

                    Gizmos.color = new Color(0.7f, 1f, 0.7f, 0.5f);
                    Gizmos.DrawWireSphere(Pelvis.position, scaleRef * 0.03f);
                    Gizmos.DrawWireSphere(BaseTransform.position, scaleRef * 0.03f);
                    Color dCol = Gizmos.color;
                    Color dhCol = UnityEditor.Handles.color;

                    if (SpineStart)
                    {
                        //Gizmos.DrawLine(SpineStart.position, Pelvis.position);
                        FGUI_Handles.DrawBoneHandle(Pelvis.position, SpineStart.position);
                        Gizmos.DrawWireSphere(SpineStart.position, scaleRef * 0.02f);

                        if (Chest)
                        {
                            //Gizmos.DrawLine(SpineStart.position, Chest.position);
                            FGUI_Handles.DrawBoneHandle(SpineStart.position, Chest.position);
                            Gizmos.DrawWireSphere(Chest.position, scaleRef * 0.02f);

                            if (Head)
                            {
                                FGUI_Handles.DrawBoneHandle(Chest.position, Head.position);
                                //Gizmos.DrawLine(Head.position, Chest.position);
                            }
                        }
                        else
                        {
                            if (Head)
                            {
                                FGUI_Handles.DrawBoneHandle(SpineStart.position, Head.position);
                                //Gizmos.DrawLine(Head.position, SpineStart.position);
                            }
                        }
                    }

                    if (LeftUpperArm)
                    {


                        if (Chest)
                        {
                            bool isPar = false;

                            if (Chest.childCount > 2)
                            {
                                if (LeftUpperArm.parent == Chest) isPar = true;
                                if (LeftUpperArm.parent)
                                {
                                    if (LeftUpperArm.parent.parent == Chest) isPar = true;
                                    if (LeftUpperArm.parent.parent)
                                    {
                                        if (LeftUpperArm.parent.parent.parent == Chest) isPar = true;
                                        if (LeftUpperArm.parent.parent.parent)
                                            if (LeftUpperArm.parent.parent.parent.parent == Chest) isPar = true;
                                    }
                                }
                            }

                            if (!isPar)
                            {
                                if (Chest.childCount > 2)
                                    UnityEditor.Handles.color = Color.yellow * 0.4f;
                                else
                                    UnityEditor.Handles.color = Color.yellow * 0.6f;

                                FGUI_Handles.DrawBoneHandle(Chest.position, LeftUpperArm.position);
                                UnityEditor.Handles.Label(LeftUpperArm.position, new GUIContent("[!]", "Try assigning parent of shoulders as 'Chest' instead of using '" + Chest.name + "'"));
                            }
                            else Gizmos.DrawLine(Chest.position, LeftUpperArm.position);
                            if (!isPar) UnityEditor.Handles.color = dhCol;

                        }


                        Gizmos.DrawWireSphere(LeftUpperArm.position, scaleRef * 0.03f);
                        if (LeftForeArm)
                        {
                            //Gizmos.DrawLine(LeftForeArm.position, LeftUpperArm.position);
                            FGUI_Handles.DrawBoneHandle(LeftUpperArm.position, LeftForeArm.position);
                            Gizmos.DrawWireSphere(LeftForeArm.position, scaleRef * 0.03f);

                            if (LeftForeArm.childCount > 0)
                            {
                                Transform ch = LeftForeArm.GetChild(0);
                                //Gizmos.DrawLine(LeftForeArm.position, ch.position);
                                FGUI_Handles.DrawBoneHandle(LeftForeArm.position, ch.position);
                                Gizmos.DrawWireSphere(ch.position, scaleRef * 0.03f);
                            }
                        }
                    }

                    if (RightUpperArm)
                    {


                        if (Chest)
                        {
                            bool isPar = false;

                            if (Chest.childCount > 2)
                            {
                                if (RightUpperArm.parent == Chest) isPar = true;
                                if (RightUpperArm.parent)
                                {
                                    if (RightUpperArm.parent.parent == Chest) isPar = true;
                                    if (RightUpperArm.parent.parent)
                                    {
                                        if (RightUpperArm.parent.parent.parent == Chest) isPar = true;
                                        if (RightUpperArm.parent.parent.parent)
                                            if (RightUpperArm.parent.parent.parent.parent == Chest) isPar = true;
                                    }
                                }
                            }

                            if (!isPar)
                            {
                                if (Chest.childCount > 2)
                                    UnityEditor.Handles.color = Color.yellow * 0.4f;
                                else
                                    UnityEditor.Handles.color = Color.yellow * 0.6f;

                                UnityEditor.Handles.Label(RightUpperArm.position, new GUIContent("[!]", "Try assigning parent of shoulders as 'Chest' instead of using '" + Chest.name + "'"));
                                FGUI_Handles.DrawBoneHandle(Chest.position, RightUpperArm.position);
                            }
                            else Gizmos.DrawLine(Chest.position, RightUpperArm.position);
                            if (!isPar) UnityEditor.Handles.color = dhCol;

                        }


                        Gizmos.DrawWireSphere(RightUpperArm.position, scaleRef * 0.03f);
                        if (RightForeArm)
                        {
                            //Gizmos.DrawLine(RightForeArm.position, RightUpperArm.position);
                            FGUI_Handles.DrawBoneHandle(RightUpperArm.position, RightForeArm.position);
                            Gizmos.DrawWireSphere(RightForeArm.position, scaleRef * 0.03f);
                            if (RightForeArm.childCount > 0)
                            {
                                Transform ch = RightForeArm.GetChild(0);
                                //Gizmos.DrawLine(RightForeArm.position, ch.position);
                                FGUI_Handles.DrawBoneHandle(RightForeArm.position, ch.position);
                                Gizmos.DrawWireSphere(ch.position, scaleRef * 0.03f);
                            }
                        }
                    }

                    if (LeftUpperLeg)
                    {

                        if (Pelvis.childCount < 3) Gizmos.color = Color.yellow;
                        if (Pelvis.childCount < 2) Gizmos.color = Color.red;
                        Gizmos.DrawLine(LeftUpperLeg.position, Pelvis.position);
                        if (Pelvis.childCount < 3) Gizmos.color = dCol;

                        Gizmos.DrawWireSphere(LeftUpperLeg.position, scaleRef * 0.03f);
                        if (LeftLowerLeg)
                        {
                            FGUI_Handles.DrawBoneHandle(LeftUpperLeg.position, LeftLowerLeg.position);
                            Gizmos.DrawWireSphere(LeftLowerLeg.position, scaleRef * 0.03f);
                            if (LeftLowerLeg.childCount > 0)
                            {
                                Transform ch = LeftLowerLeg.GetChild(0);
                                FGUI_Handles.DrawBoneHandle(LeftLowerLeg.position, ch.position);
                                Gizmos.DrawWireSphere(ch.position, scaleRef * 0.03f);
                            }
                        }
                    }

                    if (RightUpperLeg)
                    {
                        if (Pelvis.childCount < 3) Gizmos.color = Color.yellow;
                        if (Pelvis.childCount < 2) Gizmos.color = Color.red;
                        Gizmos.DrawLine(RightUpperLeg.position, Pelvis.position);
                        if (Pelvis.childCount < 3) Gizmos.color = dCol;

                        Gizmos.DrawWireSphere(RightUpperLeg.position, scaleRef * 0.03f);
                        if (RightLowerLeg)
                        {
                            FGUI_Handles.DrawBoneHandle(RightUpperLeg.position, RightLowerLeg.position);
                            Gizmos.DrawWireSphere(RightLowerLeg.position, scaleRef * 0.03f);
                            if (RightLowerLeg.childCount > 0)
                            {
                                Transform ch = RightLowerLeg.GetChild(0);
                                FGUI_Handles.DrawBoneHandle(RightLowerLeg.position, ch.position);
                                Gizmos.DrawWireSphere(ch.position, scaleRef * 0.03f);
                            }
                        }
                    }

                }
                else if (BonesSetupMode == RagdollProcessor.EBonesSetupMode.CustomLimbs)
                {
                    for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                    {
                        var chain = CustomLimbsBonesChains[i];
                        bool chainSelected = _EditorSelectedChain == i;

                        Handles.color = Color.HSVToRGB((0.3f + i * 0.1f) % 1f, 0.85f, 0.75f);
                        Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, chainSelected ? 1f : 0.5f);
                        Gizmos.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, chainSelected ? 0.8f : 0.5f);

                        for (int b = 0; b < chain.BoneSetups.Count; b++)
                        {
                            var bone = chain.BoneSetups[b];
                            if (bone.t == null) continue;
                            if (bone.t.childCount == 0) continue;

                            bool boneSelected = _EditorSelectedBone == bone;

                            Transform nxt = bone.t.GetChild(0);
                            if (b < chain.BoneSetups.Count - 1) nxt = chain.BoneSetups[b + 1].t;

                            if (nxt)
                            {
                                FGUI_Handles.DrawBoneHandle(bone.t.position, nxt.position, 1, true);
                                if (boneSelected)
                                {
                                    Vector3 off = (bone.t.position - nxt.position).magnitude * Vector3.right * 0.01f;
                                    FGUI_Handles.DrawBoneHandle(bone.t.position + off, nxt.position + off, 1, true);
                                    FGUI_Handles.DrawBoneHandle(bone.t.position - off, nxt.position - off, 1, true);
                                }
                            }
                        }

                    }


                    Handles.color = new Color(0.4f, 1f, 0.4f, 0.2f);
                    float pelvisToBase = Vector3.Distance(Pelvis.position, BaseTransform.position);
                    Handles.DrawWireDisc(Pelvis.position, BaseTransform.up, pelvisToBase * 0.15f);
                    Handles.color = new Color(.25f, .25f, .25f, .225f);

                    for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                    {
                        var chain = CustomLimbsBonesChains[i];
                        if (chain.BoneSetups.Count == 0) continue;
                        if (chain.BoneSetups[0].t == null) continue;

                        Transform attachBody = chain.GetAttachTransform(this);
                        if (attachBody)
                        {
                            //Handles.color = new Color(.25f, .25f, .25f, .4f);
                            //Handles.DrawDottedLine(chain.BoneSetups[0].t.position, attachBody.position, 2f);

                            //Handles.color = new Color(.25f, .25f, .25f, .2f);
                            FGUI_Handles.DrawBoneHandle(attachBody.position, chain.BoneSetups[0].t.position, 0.85f, true); ;
                        }
                    }

                    Handles.color = new Color(.25f, .25f, .25f, .4f);
                    float refScale = Vector3.Distance(Pelvis.position, BaseTransform.position) * 0.125f;
                    Handles.DrawDottedLine(Pelvis.position, BaseTransform.position, 2f);
                    Handles.DrawLine(BaseTransform.position - BaseTransform.forward * refScale, BaseTransform.position + BaseTransform.forward * refScale);
                    Handles.DrawLine(BaseTransform.position - BaseTransform.right * refScale, BaseTransform.position + BaseTransform.right * refScale);

                }

                UnityEditor.Handles.color = Color.white;
                Gizmos.color = Color.white;
            }
        }


        internal void DrawGizmos()
        {
            if (Pelvis == null) return;
            Gizmos.DrawLine(Pelvis.position, Pelvis.TransformPoint(PelvisToBase));

            if (RagdollLimbs != null)
                if (Application.isPlaying)
                {
                    Handles.color = new Color(0.4f, 1f, 0.4f, 0.8f);

                    if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                    {
                        foreach (var item in RagdollLimbs)
                        {
                            if (item == null) continue;
                            if (item.transform.parent == null) continue;
                            if (item.transform == posingPelvis.transform) continue;


                            if (item.transform == posingLeftLowerLeg.transform || item.transform == posingRightLowerLeg.transform)
                            {
                                if (item.transform.childCount > 0) FGUI_Handles.DrawBoneHandle(item.transform.position, item.transform.GetChild(0).position, 0.6f);
                            }
                            else if (item.transform == posingLeftForeArm.transform)
                            {
                                FGUI_Handles.DrawBoneHandle(item.transform.position, item.transform.TransformPoint(LForearmToHand), 0.6f);
                            }
                            else if (item.transform == posingRightForeArm.transform)
                            {
                                FGUI_Handles.DrawBoneHandle(item.transform.position, item.transform.TransformPoint(RForearmToHand), 0.6f);
                            }
                            else if (item.transform == posingHead.transform)
                            {
                                FGUI_Handles.DrawBoneHandle(item.transform.position, item.transform.TransformPoint(HeadToTip), 0.6f);
                            }

                            Joint j = item.GetComponent<Joint>();
                            if (j == null) continue;
                            if (j.connectedBody == null) continue;

                            FGUI_Handles.DrawBoneHandle(j.connectedBody.transform.position, item.transform.position, 0.6f);
                        }
                    }
                    else
                    {
                        foreach (var item in RagdollLimbs)
                        {
                            if (item == null) continue;
                            if (item.transform.parent == null) continue;
                            if (item.transform == posingPelvis.transform) continue;

                            Joint j = item.GetComponent<Joint>();
                            if (j == null) continue;
                            if (j.connectedBody == null) continue;

                            FGUI_Handles.DrawBoneHandle(j.connectedBody.transform.position, item.transform.position, 0.6f);

                            for (int c = 0; c < CustomLimbsBonesChains.Count; c++)
                            {
                                var chain = CustomLimbsBonesChains[c];
                                if (chain.BoneSetups.Count == 0) continue;
                                var lastBone = chain.BoneSetups[chain.BoneSetups.Count - 1];
                                if (j.transform == lastBone.Posing.transform)
                                {
                                    if (j.transform.childCount > 0) FGUI_Handles.DrawBoneHandle(item.transform.position, item.transform.GetChild(0).position, 0.45f);
                                    break;
                                }
                            }
                        }
                    }

                    Handles.color = Color.white;
                }
        }

    }
}

#endif