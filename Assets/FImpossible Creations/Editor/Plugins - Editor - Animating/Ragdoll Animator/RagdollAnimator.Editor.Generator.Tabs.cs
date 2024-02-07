using FIMSpace.AnimationTools;
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    public partial class RagdollAnimatorEditor
    {

        public int _selectedCustomChain { get { return Get.Parameters._EditorSelectedChain; } set { Get.Parameters._EditorSelectedChain = value; } }
        public RagdollProcessor.RagdollBoneSetup _selectedBoneSetup { get { return Get.Parameters._EditorSelectedBone; } set { Get.Parameters._EditorSelectedBone = value; } }

        public enum EBonesChainDrawMode { BonesSetup, ChainSettings }
        public EBonesChainDrawMode _chainModeDraw = EBonesChainDrawMode.BonesSetup;

        public void Tabs_DrawSetup(SerializedProperty sp_ragProcessor, RagdollProcessor proc)
        {
            Color preC = GUI.color;

            // Generating buttons etc.
            GUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("   " + FGUI_Resources.GetFoldSimbol(proc._EditorDrawBones, 10, "►") + "  " + "  Bones Setup", FGUI_Resources.Tex_Bone), FGUI_Resources.FoldStyle, GUILayout.Height(22))) { proc._EditorDrawBones = !proc._EditorDrawBones; SceneView.RepaintAll(); }
            if (proc._EditorDrawBones)
            {
                RagdollProcessor.EBonesSetupMode preMode = proc.BonesSetupMode;

                EditorGUILayout.PropertyField(sp_ragProcessor.FindPropertyRelative("BonesSetupMode"), GUIContent.none);
                serializedObject.ApplyModifiedProperties();

                #region Conversion from humanoid to custom setup

                if (proc.BonesSetupMode != preMode)
                {
                    if (proc.BonesSetupMode == RagdollProcessor.EBonesSetupMode.CustomLimbs)
                    {
                        // Check for conversion possibility
                        if (proc.Pelvis && proc.SpineStart && proc.Head && proc.LeftUpperArm && proc.LeftForeArm && proc.LeftUpperLeg && proc.LeftLowerLeg)
                        {
                            if (proc.CustomLimbsBonesChains.Count < 5) // Custom chains not yet set
                            {
                                proc.CustomLimbsBonesChains.Clear();
                                //
                                proc.GetPelvisSetupBone().MassPercentage = 0.18f;

                                RagdollProcessor.BonesChain spineCh = new RagdollProcessor.BonesChain();
                                spineCh.ChainName = "Spine";
                                spineCh.BlendOnCollisions = true;
                                spineCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.SpineStart, RagdollProcessor.RagdollBoneSetup.EColliderType.Box, proc.Chest ? 0.18f : 0.36f));
                                if (proc.Chest) spineCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.Chest, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, 0.18f));
                                if (proc.Head) spineCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.Head, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, 0.09f));
                                proc.CustomLimbsBonesChains.Add(spineCh);


                                RagdollProcessor.BonesChain armCh = new RagdollProcessor.BonesChain();
                                armCh.ChainName = "Left Arm";
                                armCh.BlendOnCollisions = true;
                                if (proc.LeftShoulder) armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftShoulder, RagdollProcessor.RagdollBoneSetup.EColliderType.Sphere, 0.005f));
                                armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftUpperArm, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, proc.LeftShoulder ? 0.02f : 0.015f));
                                armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftForeArm, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, proc.LeftFist ? 0.015f : 0.011f));
                                if (proc.LeftFist) armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftFist, RagdollProcessor.RagdollBoneSetup.EColliderType.Box, 0.004f));
                                proc.CustomLimbsBonesChains.Add(armCh);



                                armCh = new RagdollProcessor.BonesChain();
                                armCh.ChainName = "Right Arm";
                                armCh.BlendOnCollisions = true;
                                if (proc.RightShoulder) armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightShoulder, RagdollProcessor.RagdollBoneSetup.EColliderType.Sphere, 0.005f));
                                armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightUpperArm, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, proc.RightShoulder ? 0.02f : 0.015f));
                                armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightForeArm, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, proc.RightFist ? 0.015f : 0.011f));
                                if (proc.RightFist) armCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightFist, RagdollProcessor.RagdollBoneSetup.EColliderType.Box, 0.004f));
                                proc.CustomLimbsBonesChains.Add(armCh);


                                RagdollProcessor.BonesChain legCh = new RagdollProcessor.BonesChain();
                                legCh.ChainName = "Left Leg";
                                legCh.BlendOnCollisions = true;
                                legCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftUpperLeg, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, 0.1f));
                                legCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftLowerLeg, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, proc.LeftFist ? 0.065f : 0.07f));
                                if (proc.LeftFoot) legCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.LeftFoot, RagdollProcessor.RagdollBoneSetup.EColliderType.Box, 0.005f));
                                proc.CustomLimbsBonesChains.Add(legCh);


                                legCh = new RagdollProcessor.BonesChain();
                                legCh.ChainName = "Right Leg";
                                legCh.BlendOnCollisions = true;
                                legCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightUpperLeg, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, 0.1f));
                                legCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightLowerLeg, RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule, proc.RightFist ? 0.065f : 0.07f));
                                if (proc.RightFoot) legCh.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup(proc.RightFoot, RagdollProcessor.RagdollBoneSetup.EColliderType.Box, 0.005f));
                                proc.CustomLimbsBonesChains.Add(legCh);

                            }
                        }
                    }
                }

                #endregion

            }

            if (proc.BonesSetupMode == RagdollProcessor.EBonesSetupMode.HumanoidLimbs)
            {
                if (proc._EditorDrawHumanoidExtra) GUI.backgroundColor = Color.green;
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Tweaks, "Display extra bones slots fields for more precise ragdoll dummy setup if required. (you need to fill the basic slots fields first - upper arms, lower legs etc.)"), FGUI_Resources.ButtonStyle, GUILayout.Width(21), GUILayout.Height(18)))
                { proc._EditorDrawHumanoidExtra = !proc._EditorDrawHumanoidExtra; }
                GUI.backgroundColor = preC;
            }

            EditorGUILayout.EndHorizontal();

            if (proc._EditorDrawBones)
            {
                GUILayout.Space(4);
                SerializedProperty sp_BaseTransform = sp_ragProcessor.FindPropertyRelative("BaseTransform");

                if (sp_BaseTransform.objectReferenceValue == null) GUI.backgroundColor = new Color(1f, 0.75f, 0.2f, 1f);
                EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                GUI.backgroundColor = preC;

                EditorGUILayout.BeginHorizontal();

                if (_selectedBoneSetup == proc.GetPelvisSetupBone()) GUI.backgroundColor = Color.green;
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Tweaks, "Display extra parameters and collider adjusting parameters for the pelvis bone (ragdoll core bone)"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18)))
                {
                    if (_selectedBoneSetup == proc.GetPelvisSetupBone())
                        _selectedBoneSetup = null;
                    else
                        _selectedBoneSetup = proc.GetPelvisSetupBone();

                    SceneView.RepaintAll();
                }

                GUILayout.Space(2);

                if (sp_BaseTransform.objectReferenceValue == null) GUI.backgroundColor = new Color(1f, 0.75f, 0.2f, 1f); else GUI.backgroundColor = preC;
                EditorGUIUtility.labelWidth = 108;
                EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = preC;


                if (_selectedBoneSetup == proc.GetPelvisSetupBone())
                {
                    DrawBoneSetupGUI();
                }
                else
                {

                    if (proc.BonesSetupMode == RagdollProcessor.EBonesSetupMode.HumanoidLimbs)
                    {

                        GUILayout.Space(8);
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);

                        SerializedProperty sp_Extra = sp_ragProcessor.FindPropertyRelative("LeftFist");
                        SerializedProperty sp_ExtraShld = sp_ragProcessor.FindPropertyRelative("LeftShoulder");

                        if (sp_BaseTransform.objectReferenceValue == null)
                            EditorGUILayout.PropertyField(sp_BaseTransform, new GUIContent("Chest (Optional)", sp_BaseTransform.tooltip));
                        else
                            EditorGUILayout.PropertyField(sp_BaseTransform);

                        sp_BaseTransform.Next(false);

                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        GUILayout.Space(8);
                        EditorGUI.BeginChangeCheck();
                        if (proc._EditorDrawHumanoidExtra) if (Get.Parameters.LeftUpperArm != null) { GUILayout.Space(5); GUI.color = Color.white * 0.9f; EditorGUILayout.PropertyField(sp_ExtraShld, new GUIContent(sp_ExtraShld.displayName + "(Optional)")); GUI.color = preC; }

                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); if (Get.Parameters.LeftUpperArm != null) Get.Parameters.LeftForeArm = Get.Parameters.LeftUpperArm.GetChild(0); }
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (proc._EditorDrawHumanoidExtra) if (Get.Parameters.LeftUpperArm != null) { GUI.color = Color.white * 0.9f; EditorGUI.indentLevel++; EditorGUILayout.PropertyField(sp_Extra, new GUIContent(sp_Extra.displayName + "(Optional)")); EditorGUI.indentLevel--; GUI.color = preC; }
                        sp_Extra.Next(false);
                        GUILayout.Space(5);
                        if (proc._EditorDrawHumanoidExtra) if (Get.Parameters.LeftUpperArm != null) { GUILayout.Space(5); sp_ExtraShld.Next(false); GUI.color = Color.white * 0.9f; EditorGUILayout.PropertyField(sp_ExtraShld, new GUIContent(sp_ExtraShld.displayName + "(Optional)")); GUI.color = preC; }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); if (Get.Parameters.RightUpperArm != null) Get.Parameters.RightForeArm = Get.Parameters.RightUpperArm.GetChild(0); }
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (proc._EditorDrawHumanoidExtra) if (Get.Parameters.RightUpperArm != null) { GUI.color = Color.white * 0.9f; EditorGUI.indentLevel++; EditorGUILayout.PropertyField(sp_Extra, new GUIContent(sp_Extra.displayName + "(Optional)")); EditorGUI.indentLevel--; GUI.color = preC; }
                        sp_Extra.Next(false);
                        GUILayout.Space(8);
                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); if (Get.Parameters.LeftUpperLeg != null) Get.Parameters.LeftLowerLeg = Get.Parameters.LeftUpperLeg.GetChild(0); }
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (proc._EditorDrawHumanoidExtra) if (Get.Parameters.LeftLowerLeg != null) { GUI.color = Color.white * 0.9f; EditorGUI.indentLevel++; EditorGUILayout.PropertyField(sp_Extra, new GUIContent(sp_Extra.displayName + "(Optional)")); EditorGUI.indentLevel--; GUI.color = preC; }
                        sp_Extra.Next(false);
                        GUILayout.Space(5);
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); if (Get.Parameters.RightUpperLeg != null) Get.Parameters.RightLowerLeg = Get.Parameters.RightUpperLeg.GetChild(0); }
                        EditorGUILayout.PropertyField(sp_BaseTransform); sp_BaseTransform.Next(false);
                        if (proc._EditorDrawHumanoidExtra) if (Get.Parameters.RightLowerLeg != null) { GUI.color = Color.white * 0.9f; EditorGUI.indentLevel++; EditorGUILayout.PropertyField(sp_Extra, new GUIContent(sp_Extra.displayName + "(Optional)")); EditorGUI.indentLevel--; GUI.color = preC; }
                        sp_Extra.Next(false);
                        GUILayout.Space(2);

                        if (Get.ObjectWithAnimator)
                        {
                            bool isHuman = false;
                            Animator anim = Get.ObjectWithAnimator.GetComponent<Animator>();
                            if (anim) isHuman = anim.isHuman;

                            if (!isHuman)
                            {
                                EditorGUILayout.HelpBox("Detected Generic or Legacy Rig, you want to try auto-detect limbs? (Verify them)", MessageType.None);

                                if (GUILayout.Button(new GUIContent("  Run Auto-Limb Detection Algorithm\n  <size=10>(Character must contain correct T-Pose)\n(And be Facing it's Z-Axis)</size>", FGUI_Resources.TexWaitIcon), FGUI_Resources.ButtonStyleR, GUILayout.Height(52)))
                                {
                                    SkeletonRecognize.SkeletonInfo info = new SkeletonRecognize.SkeletonInfo(Get.ObjectWithAnimator);

                                    #region Assigning found bones

                                    int assigned = 0;
                                    if (info.LeftArms > 0)
                                    {
                                        if (info.ProbablyLeftArms[0].Count > 2)
                                        {
                                            assigned += 2;
                                            Get.Parameters.LeftUpperArm = info.ProbablyLeftArms[0][1];
                                            Get.Parameters.LeftForeArm = info.ProbablyLeftArms[0][2];
                                        }
                                    }

                                    if (info.RightArms > 0)
                                    {
                                        if (info.ProbablyRightArms[0].Count > 2)
                                        {
                                            assigned += 2;
                                            Get.Parameters.RightUpperArm = info.ProbablyRightArms[0][1];
                                            Get.Parameters.RightForeArm = info.ProbablyRightArms[0][2];
                                        }
                                    }


                                    if (info.LeftLegs > 0)
                                    {
                                        if (info.ProbablyLeftLegs[0].Count > 1)
                                        {
                                            assigned += 2;
                                            Get.Parameters.LeftUpperLeg = info.ProbablyLeftLegs[0][0];
                                            Get.Parameters.LeftLowerLeg = info.ProbablyLeftLegs[0][1];
                                        }
                                    }

                                    if (info.RightLegs > 0)
                                    {
                                        if (info.ProbablyRightLegs[0].Count > 1)
                                        {
                                            assigned += 2;
                                            Get.Parameters.RightUpperLeg = info.ProbablyRightLegs[0][0];
                                            Get.Parameters.RightLowerLeg = info.ProbablyRightLegs[0][1];
                                        }
                                    }

                                    if (info.ProbablyHead)
                                    {
                                        assigned += 1;
                                        Get.Parameters.Head = info.ProbablyHead;
                                    }

                                    if (info.ProbablyHips)
                                    {
                                        assigned += 1;
                                        Get.Parameters.Pelvis = info.ProbablyHips;
                                    }

                                    if (info.SpineChainLength > 1)
                                    {
                                        assigned += 2;
                                        Get.Parameters.SpineStart = info.ProbablySpineChain[0];

                                        int shortSp = info.ProbablySpineChainShort.Count;

                                        if (shortSp < 3)
                                            Get.Parameters.Chest = info.ProbablySpineChainShort[1];
                                        else
                                            if (shortSp > 2)
                                            Get.Parameters.Chest = info.ProbablySpineChainShort[shortSp - 1];

                                        if (Get.Parameters.Chest == Get.Parameters.Head) Get.Parameters.Chest = Get.Parameters.Chest.parent;
                                    }

                                    if (assigned < 2)
                                        EditorUtility.DisplayDialog("Auto Detection Report", "Couldn't detect bones on the current rig!", "Ok");
                                    else
                                        EditorUtility.DisplayDialog("Auto Detection Report", "Found and Assigned " + assigned + " bones to help out faster setup. Please verify the new added bones", "Ok");

                                    #endregion
                                }

                            }
                        }

                    }
                    else // Custom Limbs Menu
                    {

                        EditorGUILayout.PropertyField(sp_ragProcessor.FindPropertyRelative("Head"), new GUIContent("Head (check tooltip)", "For custom limbs setup the head reference is used for extra features, like computing pelvis-to-head rotation / reposing position etc.")); sp_BaseTransform.Next(false);

                        GUILayout.Space(4);

                        if (proc.CustomLimbsBonesChains.Count == 0)
                        {
                            EditorGUILayout.HelpBox("Prepare bones chains for all limbs of your model. There will be required spine bones chain and the limbs bones chains like arms, legs, tail etc.", MessageType.None);
                        }


                        GUILayout.Space(5);


                        if (Get.Parameters.Pelvis == null || Get.Parameters.Head == null)
                        {
                            EditorGUILayout.HelpBox("Assign Pelvis and Head references before unlocking more options here!", MessageType.Info);

                            GUI.color = new Color(1f, 1f, 1f, 0.5f);

                            if (GUILayout.Button("+ Add Bones Chain +", FGUI_Resources.ButtonStyle))
                            {
                                EditorUtility.DisplayDialog("Assign Lacking References", "Assign Pelvis and Head references before unlocking more options here!", "Ok");
                            }

                            GUI.color = new Color(1f, 1f, 1f, 1f);
                        }
                        else
                        {

                            GUILayout.BeginHorizontal();

                            GUILayout.Space(10);
                            string _title = "+ Add Bone Chain +";

                            if (Get.Parameters.CustomLimbsBonesChains.Count == 0)
                            {
                                GUI.backgroundColor = Color.green;
                                _title = "+ Add First Bone Chain +";
                            }

                            if (GUILayout.Button(_title, FGUI_Resources.ButtonStyle))
                            {
                                string targetName = "Chain " + proc.CustomLimbsBonesChains.Count;
                                if (proc.CustomLimbsBonesChains.Count == 0) targetName = "Spine";
                                else if (proc.CustomLimbsBonesChains.Count == 1) targetName = "Right Leg";
                                else if (proc.CustomLimbsBonesChains.Count == 2) targetName = "Left Leg";
                                else if (proc.CustomLimbsBonesChains.Count == 3) targetName = "Right Arm";
                                else if (proc.CustomLimbsBonesChains.Count == 4) targetName = "Left Arm";

                                var chain = new RagdollProcessor.BonesChain();
                                chain.ChainName = targetName;
                                proc.CustomLimbsBonesChains.Add(chain);

                                EditorUtility.SetDirty(Get);
                                _selectedCustomChain = proc.CustomLimbsBonesChains.Count - 1;
                            }

                            if (Get.Parameters.CustomLimbsBonesChains.Count == 0) GUI.backgroundColor = Color.white;

                            GUILayout.Space(10);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Space(10);


                        if (proc.CustomLimbsBonesChains.Count > 0)
                        {
                            GUILayout.Space(4);

                            #region Drawing selector

                            float width = 0;
                            float maxW = EditorGUIUtility.currentViewWidth;

                            GUILayout.BeginHorizontal();
                            for (int i = 0; i < proc.CustomLimbsBonesChains.Count; i++)
                            {
                                var cl = proc.CustomLimbsBonesChains[i];

                                GUIContent content = new GUIContent(cl.ChainName);
                                Vector2 size = EditorStyles.miniButton.CalcSize(content);
                                size.x += 2;

                                width += size.x + 2;
                                if (width > maxW - 14)
                                {
                                    width = size.x + 2;
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                }

                                if (_selectedCustomChain == i) GUI.backgroundColor = Color.green;

                                if (DrawChainSelector(size.x, content, cl))
                                {
                                    _selectedBoneSetup = null;
                                    if (_selectedCustomChain == i) _selectedCustomChain = -1;
                                    else _selectedCustomChain = i;
                                    SceneView.RepaintAll();
                                }

                                if (_selectedCustomChain == i) GUI.backgroundColor = preC;
                            }
                            GUILayout.EndHorizontal();

                            #endregion

                            if (_selectedCustomChain >= proc.CustomLimbsBonesChains.Count) _selectedCustomChain = 0;
                            GUILayout.Space(4);

                            int chainToRemove = -1;
                            if (_selectedCustomChain > -1)
                            {
                                EditorGUI.BeginChangeCheck();
                                var chain = proc.CustomLimbsBonesChains[_selectedCustomChain];
                                FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 4);

                                GUILayout.BeginHorizontal();
                                EditorGUIUtility.labelWidth = 44;
                                chain.ChainName = EditorGUILayout.TextField("Title:", chain.ChainName);
                                EditorGUIUtility.labelWidth = 0;
                                //GUILayout.Space(8);
                                //_chainModeDraw = (EBonesChainDrawMode)EditorGUILayout.EnumPopup(_chainModeDraw, GUILayout.Width(120));

                                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f, 1f);
                                if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(21), GUILayout.Height(18)))
                                {
                                    chainToRemove = _selectedCustomChain;
                                }
                                GUI.backgroundColor = preC;

                                GUILayout.EndHorizontal();

                                GUILayout.Space(4);

                                GUILayout.BeginHorizontal();
                                chain.BlendOnCollisions = EditorGUILayout.Toggle(new GUIContent("Blend On Collisions:", "If you want to use feature for blending in limb when some collision on it's colliders occurs (when Free-Fall is DISABLED)\nIt should be disabled for the legs."), chain.BlendOnCollisions);
                                EditorGUILayout.HelpBox("← should be disabled for legs", MessageType.None);
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                chain.Detach = EditorGUILayout.Toggle(new GUIContent("Detach Chain:", "Detach chain in the hierarchy to help out animating ragdoll with some custom procedural algorithms like Tail Animator or others."), chain.Detach);
                                if (chain.Detach)
                                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "Don't use 'Detach' if you don't know what is it for, check the tooltip by entering 'Detach Chain:'"));
                                GUILayout.EndHorizontal();

                                chain.MusclesForce = EditorGUILayout.Slider(new GUIContent("Muscles Force:", "Multiplier for spring value for the whole bones chain. You can see the same parameter in 'Play' category under 'Individual Settings' foldout"), chain.MusclesForce, 0f, 1f);

                                GUILayout.Space(4);

                                FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 4);

                                if (_chainModeDraw == EBonesChainDrawMode.BonesSetup)
                                {

                                    if (_selectedBoneSetup != null && _selectedBoneSetup != proc.GetPelvisSetupBone())
                                    {
                                        DrawBoneSetupGUI(true, chain);
                                    }
                                    else
                                    {
                                        GUILayout.Space(12);

                                        #region Drawing Bones Chain


                                        int toRemove = -1;

                                        var Bones = chain.BoneSetups;

                                        GUIContent _guic_mass = new GUIContent(" ", "Target Mass in percents: It's using the Ragdoll Generator 'Target Mass of whole body' value");
                                        GUIContent _guic_percent = new GUIContent("%", "[Click for tooltip] Target Mass in percents: It's using the Ragdoll Generator 'Target Mass of whole body' value");

                                        for (int i = 0; i < Bones.Count; i++)
                                        {
                                            var b = Bones[i];

                                            EditorGUILayout.BeginHorizontal();

                                            GUILayout.Space(30);

                                            if (_selectedBoneSetup == b) GUI.backgroundColor = Color.green;
                                            if (GUILayout.Button(FGUI_Resources.Tex_Tweaks, FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18)))
                                            {
                                                if (_selectedBoneSetup == b)
                                                    _selectedBoneSetup = null;
                                                else
                                                    _selectedBoneSetup = b;

                                                SceneView.RepaintAll();
                                            }
                                            GUI.backgroundColor = preC;
                                            GUILayout.Space(8);

                                            #region Child Bone Shortcut

                                            if (i > 0)
                                                if (Bones[i - 1] != null)
                                                    if (Bones[i - 1].t != null)
                                                        if (Bones[i - 1].t.childCount > 0)
                                                        {
                                                            Transform tgt = Bones[i - 1].t.GetChild(0);
                                                            if (Bones[i].t != tgt)
                                                                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_UpFold, "Assign child of the upper bone to this field"), FGUI_Resources.ButtonStyle, GUILayout.Width(16), GUILayout.Height(16)))
                                                                {
                                                                    Bones[i].t = (Bones[i - 1].t.GetChild(0));
                                                                    SceneView.RepaintAll();
                                                                }
                                                        }

                                            #endregion

                                            Transform preT = b.t;
                                            if (preT == null) GUI.backgroundColor = Color.yellow;
                                            Transform newT = (Transform)EditorGUILayout.ObjectField(b.t, typeof(Transform), true);
                                            GUI.backgroundColor = preC;
                                            if (newT != preT) { b.t = (newT); SceneView.RepaintAll(); }

                                            EditorGUIUtility.labelWidth = 7;

                                            //GUI.SetNextControlName("_rav" + i);

                                            if (b.MassPercentage > .0975f)
                                                b.MassPercentage = EditorGUILayout.FloatField(_guic_mass, Mathf.Round(b.MassPercentage * 100f), GUILayout.Width(32)) * 0.01f;
                                            else
                                            {
                                                int nWdth = 39;
                                                if (Mathf.Round(b.MassPercentage * 100) == b.MassPercentage * 100f) nWdth = 32;

                                                b.MassPercentage = EditorGUILayout.FloatField(_guic_mass, b.MassPercentage * 100f, GUILayout.Width(nWdth)) * 0.01f;
                                            }

                                            //if( GUI.GetNameOfFocusedControl() == "_rav"+i )
                                            //{

                                            //}
                                            //else
                                            //{

                                            //}

                                            //b.MassPercentage = EditorGUILayout.FloatField(_guic_mass, Mathf.Round(b.MassPercentage * 100), GUILayout.Width(32)) * 0.01f;
                                            //float distp = b.MassPercentage * 100f;
                                            //if (distp > 10f) distp = Mathf.Round(distp); else distp = (float)System.Math.Round(distp, 2);
                                            //EditorGUILayout.LabelField(distp + "%", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));

                                            EditorGUIUtility.labelWidth = 0;

                                            if (GUILayout.Button(_guic_percent, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(10)))
                                            {
                                                EditorUtility.DisplayDialog("Ragdoll Mass Guide", "The average humanoid mass of different limbs:\n\nHead: 9%\nWhole Torso Trunk: 54%\nSingle Arm: 5%\nSingle Leg: 17%", "Ok");
                                            }

                                            GUILayout.Space(4);
                                            if (b.ColliderType == RagdollProcessor.RagdollBoneSetup.EColliderType.Mesh && b.ColliderMesh == null) b.ColliderType = RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule;
                                            b.ColliderType = (RagdollProcessor.RagdollBoneSetup.EColliderType)EditorGUILayout.EnumPopup(b.ColliderType, GUILayout.Width(70));
                                            GUILayout.Space(4);

                                            #region Child Bone Shortcut

                                            if (i > 0)
                                                if (Bones[i] != null)
                                                    if (Bones[i].t != null)
                                                        if (Bones[i].t.childCount > 0)
                                                        {
                                                            Transform tgt = Bones[i].t.GetChild(0);
                                                            if (i < Bones.Count - 1) if (Bones[i + 1].t == tgt) tgt = null;

                                                            if (tgt != null)
                                                            {
                                                                GUI.color = new Color(.9f, .9f, .9f, .5f);
                                                                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Assign child of this bone to this field"), FGUI_Resources.ButtonStyle, GUILayout.Width(14), GUILayout.Height(16)))
                                                                {
                                                                    Bones[i].t = (Bones[i].t.GetChild(0));
                                                                    SceneView.RepaintAll();
                                                                }
                                                                GUI.color = preC;
                                                            }
                                                        }

                                            #endregion

                                            GUILayout.Space(4);

                                            GUILayout.Space(2);
                                            if (GUILayout.Button("X", GUILayout.Width(20)))
                                            {
                                                toRemove = i;
                                                SceneView.RepaintAll();
                                            }

                                            GUILayout.Space(30);

                                            EditorGUILayout.EndHorizontal();


                                            // XAXA
                                            if (Bones[i].t == proc.Pelvis) EditorGUILayout.HelpBox("Chain bone should not be the same as 'Pelvis Bone'!", MessageType.Error);
                                            if (Bones[i].t == proc.BaseTransform) EditorGUILayout.HelpBox("Chain bone should not be the same as 'Base Trasnform'!", MessageType.Error);


                                            GUILayout.Space(1);
                                            GUI.color = new Color(1f, 1f, 1f, 0.5f);

                                            GUILayout.Label("◊", FGUI_Resources.HeaderStyle);
                                            GUI.color = preC;
                                            GUILayout.Space(1);
                                        }

                                        if (toRemove > -1)
                                        {
                                            if (chain.BoneSetups.ContainsIndex(toRemove, true))
                                            {
                                                chain.BoneSetups.RemoveAt(toRemove);
                                            }
                                        }

                                        GUILayout.Space(5);


                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.Space(30);

                                        if (Bones.Count == 0)
                                        {
                                            GUI.backgroundColor = Color.green;

                                            if (GUILayout.Button("+ Add First Bone Field +"))
                                            {
                                                chain.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup());
                                                SceneView.RepaintAll();
                                            }

                                            GUI.backgroundColor = Color.white;
                                        }
                                        else
                                        {
                                            if (GUILayout.Button("+ Add Next Bone Field +"))
                                            {
                                                chain.BoneSetups.Add(new RagdollProcessor.RagdollBoneSetup());
                                                SceneView.RepaintAll();
                                            }
                                        }


                                        GUILayout.Space(30);

                                        EditorGUILayout.EndHorizontal();


                                        GUILayout.Space(2);

                                        #endregion
                                    }

                                }
                                else if (_chainModeDraw == EBonesChainDrawMode.ChainSettings)
                                {

                                }

                                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(Get);

                            }

                            if (chainToRemove >= 0)
                            {
                                proc.CustomLimbsBonesChains.RemoveAt(chainToRemove);
                                _selectedCustomChain -= 1;
                            }
                        }


                        GUILayout.Space(10);

                    }

                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(9);

            FGUI_Inspector.FoldHeaderStart(ref proc._EditorDrawGenerator, "  Ragdoll Generator", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_Collider);

            if (proc._EditorDrawGenerator)
            {
                GUILayout.Space(3);
                if (generator == null)
                {
                    generator = new RagdollGenerator();
                    generator.BaseTransform = Get.ObjectWithAnimator != null ? Get.ObjectWithAnimator : Get.transform;
                    generator.ragdollGen.SetAllBoneReferences(Get.Parameters);
                    //generator.SetAllBoneReferences(Get.Parameters.Pelvis, Get.Parameters.SpineStart, Get.Parameters.Chest, Get.Parameters.Head, Get.Parameters.LeftUpperArm, Get.Parameters.LeftForeArm, Get.Parameters.RightUpperArm, Get.Parameters.RightForeArm, Get.Parameters.LeftUpperLeg, Get.Parameters.LeftLowerLeg, Get.Parameters.RightUpperLeg, Get.Parameters.RightLowerLeg);
                }

                generator.Tab_RagdollGenerator(Get.Parameters, true);

            }
            else
            {
                if (generator != null)
                    generator.ragdollTweak = RagdollGenerator.tweakRagd.None;
            }

            GUILayout.EndVertical();

            GUILayout.Space(9);

            FGUI_Inspector.FoldHeaderStart(ref drawAdditionalSettings, "  Additional Settings", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_GearSetup);

            if (drawAdditionalSettings)
            {
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                GUILayout.Space(3);

                // Hips pin and animate pelvis
                EditorGUILayout.BeginHorizontal();
                DisableOnPlay();
                SerializedProperty sp_ext = sp_ragProcessor.FindPropertyRelative("HipsPin");
                EditorGUILayout.PropertyField(sp_ext); sp_ext.NextVisible(false);
                DisableOnPlay(false);
                GUILayout.FlexibleSpace();
                if (proc.HipsPin == false)
                {
                    EditorGUIUtility.labelWidth = 120;
                    EditorGUILayout.PropertyField(sp_ext, new GUIContent("Animate Pelvis", sp_ext.tooltip));
                    EditorGUIUtility.labelWidth = 0;
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(1);


                SerializedProperty sp_fixRoot = sp_ragProcessor.FindPropertyRelative("FixRootInPelvis");
                SerializedProperty sp_fixRootcpy = sp_fixRoot.Copy();
                sp_fixRoot.Next(false);


                // fix root in pelvis and calibrate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_fixRootcpy);
                sp_fixRootcpy.Next(false);
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(sp_fixRootcpy);
                EditorGUILayout.EndHorizontal();





                GUILayout.Space(6);
                // Root Bone Field
                GUI.color = new Color(0.85f, 1f, 0.85f, Application.isPlaying ? 0.5f : 1f);
                GUI.backgroundColor = new Color(0.85f, 1f, 0.85f, 1f);
                var sproot = sp_ObjectWithAnimator.Copy(); sproot.Next(false);
                EditorGUILayout.PropertyField(sproot);
                GUILayout.Space(3);
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                DisableOnPlay();

                // Auto destr and pre-generate
                var sp = sp_ObjectWithAnimator.Copy(); sp.Next(false); sp.Next(false);
                EditorGUILayout.BeginHorizontal();
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                sp.Next(false);


                if (Get.RootBone)
                {
                    bool rigidbodiesPreset = false;

                    if (Get.Parameters.BonesSetupMode == RagdollProcessor.EBonesSetupMode.HumanoidLimbs)
                    {
                        if (Get.Parameters.LeftUpperArm)
                            rigidbodiesPreset = (Get.Parameters.LeftUpperArm.GetComponent<Rigidbody>());
                    }
                    else
                    {
                        if (Get.Parameters.Pelvis)
                            rigidbodiesPreset = (Get.Parameters.Pelvis.GetComponent<Rigidbody>());
                    }


                    if (Get.PreGenerateDummy || rigidbodiesPreset)
                    {
                        if (Application.isPlaying) GUI.enabled = false;

                        GUILayout.FlexibleSpace();
                        bool preV = sp.boolValue;
                        EditorGUILayout.PropertyField(sp);
                        if (preV != sp.boolValue)
                        {
                            triggerGenerateRagd = true;
                        }

                        #region Debugging Backup
                        //if (sp.boolValue)
                        //{
                        //    if (GUILayout.Button("Pre Generate Dummy"))
                        //    {
                        //        sp.boolValue = !sp.boolValue;
                        //        Get.Parameters.PreGenerateDummy(Get.ObjectWithAnimator, Get.RootBone);
                        //    }
                        //}
                        //else
                        //{
                        //    GUI.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 1f);
                        //    if (GUILayout.Button("Undo Dummy"))
                        //    {
                        //        sp.boolValue = !sp.boolValue;
                        //        Get.Parameters.RemovePreGeneratedDummy();
                        //    }
                        //    GUI.backgroundColor = Color.white;
                        //}
                        #endregion

                        if (Application.isPlaying) GUI.enabled = true;
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                // Target container for dummy
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 200;
                sp.Next(false); EditorGUILayout.PropertyField(sp);
                EditorGUIUtility.labelWidth = 0;
                if (Get.TargetParentForRagdollDummy == null)
                {
                    if (GUILayout.Button("Self", GUILayout.Width(40)))
                    {
                        Get.TargetParentForRagdollDummy = Get.transform;
                        EditorUtility.SetDirty(Get);
                    }
                }
                EditorGUILayout.EndHorizontal();
                DisableOnPlay(false);


                GUILayout.EndVertical();
                GUILayout.Space(6);
                FGUI_Inspector.FoldHeaderStart(ref drawCorrectionsSettings, "  Corrections", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_Tweaks);

                if (drawCorrectionsSettings)
                {
                    sp_ext = sp_ragProcessor.FindPropertyRelative("StartAfterTPose");

                    GUILayout.Space(3);
                    EditorGUILayout.BeginHorizontal();
                    DisableOnPlay(); EditorGUILayout.PropertyField(sp_ext);
                    sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext); DisableOnPlay(false);
                    sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
                    EditorGUILayout.EndHorizontal();

                    DisableOnPlay();
                    sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
                    DisableOnPlay(false);
                    //var sp2 = sp_ObjectWithAnimator.Copy(); sp2.Next(false);
                    //sp2.Next(false); EditorGUILayout.PropertyField(sp2);

                    GUILayout.Space(3);

                }

                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }

            GUILayout.EndVertical();
        }


        private void DrawBoneSetupGUI(bool extraValues = false, RagdollProcessor.BonesChain chain = null)
        {
            Color preC = GUI.color;

            if (generator == null || !generator.IsGenerateEnabled)
                EditorGUILayout.HelpBox("Enable Ragdoll Generator to see changes dynamically on the scene!", MessageType.None);

            if (_selectedBoneSetup != null)
            {
                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = Color.green;

                if (extraValues)
                    if (GUILayout.Button(FGUI_Resources.Tex_Tweaks, FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18)))
                    {
                        _selectedBoneSetup = null;
                        SceneView.RepaintAll();
                    }

                if (_selectedBoneSetup != null)
                {

                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(_selectedBoneSetup.t, typeof(Transform), true);
                    GUI.enabled = true;

                    GUI.backgroundColor = preC;

                    if (chain != null)
                    {
                        if (GUILayout.Button("◄", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(18))) { _selectedBoneSetup = chain.GetNext(_selectedBoneSetup, false); }
                        if (GUILayout.Button("►", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(18))) { _selectedBoneSetup = chain.GetNext(_selectedBoneSetup, true); }
                    }

                    EditorGUILayout.EndHorizontal();

                    FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 4);

                    _selectedBoneSetup.ColliderType = (RagdollProcessor.RagdollBoneSetup.EColliderType)EditorGUILayout.EnumPopup("Collider Type:", _selectedBoneSetup.ColliderType);
                    if (_selectedBoneSetup.ColliderType == RagdollProcessor.RagdollBoneSetup.EColliderType.Mesh)
                    {
                        if (!extraValues)
                        {
                            _selectedBoneSetup.ColliderType = RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule;
                            EditorUtility.SetDirty(Get);
                        }
                        else
                        {
                            _selectedBoneSetup.ColliderMesh = (Mesh)EditorGUILayout.ObjectField(_selectedBoneSetup.ColliderMesh, typeof(Mesh), false);
                            GUILayout.Space(5);
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Bone Mass", "It's using the Ragdoll Generator 'Target Mass of whole body' value"), GUILayout.Width(100));
                    _selectedBoneSetup.MassPercentage = GUILayout.HorizontalSlider(_selectedBoneSetup.MassPercentage, 0f, 1f);

                    float distp = _selectedBoneSetup.MassPercentage * 100f;
                    if (distp > 10f) distp = Mathf.Round(distp); else distp = (float)System.Math.Round(distp, 2);
                    EditorGUILayout.LabelField(distp + "%", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();

                    if (extraValues)
                        _selectedBoneSetup.MuscleForceMultiplier = EditorGUILayout.Slider("Muscle Power Multiplier:", _selectedBoneSetup.MuscleForceMultiplier, 0f, 2f);

                    GUILayout.Space(7);
                    EditorGUILayout.LabelField("You can change parameters below when using Ragdoll Generator", EditorStyles.centeredGreyMiniLabel);
                    //EditorGUILayout.LabelField("XAX");
                    if (_selectedBoneSetup.ColliderType != RagdollProcessor.RagdollBoneSetup.EColliderType.Mesh)
                    {
                        EditorGUI.BeginChangeCheck();

                        string title = "Collider Radius Multiplier:";
                        if (_selectedBoneSetup.ColliderType == RagdollProcessor.RagdollBoneSetup.EColliderType.Box) title = "Box Scale Multiplier:";


                        EditorGUILayout.BeginHorizontal();

                        if (_selectedBoneSetup.Generator_Extra) GUI.backgroundColor = Color.green;
                        if (GUILayout.Button(FGUI_Resources.Tex_Sliders, FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18))) { _selectedBoneSetup.Generator_Extra = !_selectedBoneSetup.Generator_Extra; }
                        GUI.backgroundColor = preC;

                        if (!_selectedBoneSetup.Generator_Extra)
                            _selectedBoneSetup.Generator_ScaleMul = EditorGUILayout.Slider(title, _selectedBoneSetup.Generator_ScaleMul, 0f, 4f);
                        else
                            _selectedBoneSetup.Generator_ScaleMul = EditorGUILayout.FloatField(title, _selectedBoneSetup.Generator_ScaleMul);

                        if (extraValues)
                        {
                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Prepare, "Apply settings of this bone to all bones in the chain"), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18)))
                            {
                                var refSett = _selectedBoneSetup;
                                for (int b = 0; b < chain.BoneSetups.Count; b++) chain.BoneSetups[b].PasteFrom(refSett);
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        if (_selectedBoneSetup.ColliderType == RagdollProcessor.RagdollBoneSetup.EColliderType.Box)
                        {
                            GUILayout.Space(-2);
                            _selectedBoneSetup.Generator_BoxScale = EditorGUILayout.Vector3Field("Box Size Multiplier:", _selectedBoneSetup.Generator_BoxScale);
                        }
                        else
                        {
                            if (_selectedBoneSetup.ColliderType != RagdollProcessor.RagdollBoneSetup.EColliderType.Sphere)
                            {
                                GUILayout.Space(-2);

                                if (!_selectedBoneSetup.Generator_Extra)
                                    _selectedBoneSetup.Generator_LengthMul = EditorGUILayout.Slider("Collider Generator Length:", _selectedBoneSetup.Generator_LengthMul, 0f, 4f);
                                else
                                    _selectedBoneSetup.Generator_LengthMul = EditorGUILayout.FloatField("Collider Generator Length:", _selectedBoneSetup.Generator_LengthMul);
                            }

                            if (_selectedBoneSetup.ColliderType == RagdollProcessor.RagdollBoneSetup.EColliderType.Capsule)
                            {
                                EditorGUIUtility.labelWidth = 170;
                                _selectedBoneSetup.Generator_OverrideCapsuleDir = (RagdollProcessor.RagdollBoneSetup.ECapsDirOverride)EditorGUILayout.EnumPopup("Override Capsule Direction:", _selectedBoneSetup.Generator_OverrideCapsuleDir);
                                EditorGUIUtility.labelWidth = 0;
                            }
                        }


                        if (_selectedBoneSetup.Generator_Extra)
                        {
                            _selectedBoneSetup.Generator_Offset = EditorGUILayout.Vector3Field("Center Offset:", _selectedBoneSetup.Generator_Offset);
                        }
                        else
                        {
                            GUILayout.Space(-2);
                            _selectedBoneSetup.Generator_LengthOffset = EditorGUILayout.Slider("Collider Generator Offset:", _selectedBoneSetup.Generator_LengthOffset, -1f, 1f);
                            GUILayout.Space(4);
                        }


                        GUILayout.Space(4);


                        if (EditorGUI.EndChangeCheck())
                        {
                            if (generator != null) generator.forceUpdateGenerate = true;
                            Repaint();
                        }
                    }

                }


                GUILayout.Space(6);

                if (GUILayout.Button("Back"))
                {
                    _selectedBoneSetup = null;
                    SceneView.RepaintAll();
                }

                GUILayout.Space(2);
            }
        }

        public void Tabs_DrawTweaking(SerializedProperty sp_ragProcessor, RagdollProcessor proc)
        {
            if (generator != null) generator.ragdollTweak = RagdollGenerator.tweakRagd.None;

            GUILayout.Space(-4);

            EditorGUILayout.BeginVertical(FGUI_Resources.ViewBoxStyle); // ----------
            SerializedProperty sp_param = sp_ragProcessor.FindPropertyRelative("FreeFallRagdoll");

            RagdollProcessor.Editor_DrawTweakGUI(sp_param, Get.Parameters);

            EditorGUILayout.EndVertical();
        }


        public void Tabs_DrawExtra(SerializedProperty sp_ragProcessor, RagdollProcessor proc)
        {
            SerializedProperty sp_ext = sp_ragProcessor.FindPropertyRelative("ExtendedAnimatorSync");
            EditorGUILayout.PropertyField(sp_ext);

            // Repose
            EditorGUILayout.BeginHorizontal();
            sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext); sp_ext.NextVisible(false);

            EditorGUIUtility.labelWidth = 104;
            if (proc.ReposeMode != RagdollProcessor.EBaseTransformRepose.None) { GUILayout.Space(6); EditorGUILayout.PropertyField(sp_ext, new GUIContent("Full Ragdoll Only:", sp_ext.tooltip), GUILayout.Width(122)); }
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(5);
            GUILayout.BeginHorizontal(); // Ignore Self
            sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
            GUILayout.FlexibleSpace(); // Mass Multiplier
            sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
            GUILayout.EndHorizontal();

            GUILayout.Space(5); // Phys Material
            sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);

            GUILayout.Space(5); // Limits turn-off
            EditorGUIUtility.labelWidth = 220;
            EditorGUILayout.BeginHorizontal();
            sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
            GUILayout.FlexibleSpace();
            sp_ext.NextVisible(false);

            if (Application.isPlaying == false)
            {
                EditorGUIUtility.labelWidth = 90;
                EditorGUILayout.PropertyField(sp_ext); // Hide Dummy
            }
            else
            {
                if (Get.Parameters.HideDummy)
                {
                    EditorGUIUtility.labelWidth = 100;
                    GUI.color = new Color(1f, 1f, 1f, 0.7f);
                    EditorGUILayout.ObjectField("Hidden Dummy:", Get.Parameters.RagdollDummyBase, typeof(Transform), true);
                    GUI.color = Color.white;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;


            GUILayout.Space(5); // Colliders List
            EditorGUI.indentLevel++;
            //EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
            sp_ext.NextVisible(false); EditorGUILayout.PropertyField(sp_ext);
            EditorGUI.indentLevel--;
            //EditorGUILayout.EndVertical();

            GUILayout.Space(5); // Events Receiver
            sp_ext = sp_ragProcessor.FindPropertyRelative("SendCollisionEventsTo");
            EditorGUILayout.PropertyField(sp_ext);

            if (sp_ext.objectReferenceValue != null)
            {
                sp_ext.Next(false); EditorGUILayout.PropertyField(sp_ext);
                EditorGUILayout.HelpBox("The SendCollisionEventsTo game object must have attached component with 'IRagdollAnimatorReceiver' interface implemented\n\nOR   The Second Approach:\nmust have attached component with public methods like:\n'ERagColl(RagdollCollisionHelper c)' or 'ERagCollExit(RagdollCollisionHelper c)' to handle collision events.\nYou can get component 'RagdollCollisionHelper' to identify which limb hitted something.", MessageType.Info);
            }
            else
            {
                sp_ext.Next(false);
            }

            GUILayout.Space(4);
            sp_ext.Next(false);
            if (Get.Parameters.SendCollisionEventsTo == null)
                EditorGUILayout.PropertyField(sp_ext);
            sp_ext.Next(false);

            EditorGUILayout.BeginHorizontal();

            int wdth2 = sp_ext.boolValue ? 192 : 100;
            EditorGUIUtility.labelWidth = wdth2;
            EditorGUILayout.PropertyField(sp_ext, GUILayout.Width(wdth2 + 20));
            if (sp_ext.boolValue)
            {
                EditorGUIUtility.labelWidth = 65;
                sp_ext.Next(false);
                EditorGUILayout.PropertyField(sp_ext, new GUIContent("Set Layer:", "Change collision Layer of Colliders left on the Animator"));
            }
            else
            {
                proc.ChangeAnimatorCollidersLayerTo = 0;
                if (proc.Pelvis) proc.ChangeAnimatorCollidersLayerTo = proc.Pelvis.gameObject.layer;

                EditorGUIUtility.labelWidth = 180;
                sp_ext.Next(false);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            DrawRareSettings(sp_ext);

        }

        bool displayRareSettings = false;
        void DrawRareSettings(SerializedProperty sp_ext)
        {
            FGUI_Inspector.FoldHeaderStart(ref displayRareSettings, "  Rare Settings", FGUI_Resources.BGInBoxStyle, FGUI_Resources.TexAddIcon);

            if ( displayRareSettings)
            {
                EditorGUIUtility.labelWidth = 292;
                sp_ext.Next(false);
                EditorGUILayout.PropertyField(sp_ext);
                EditorGUIUtility.labelWidth = 260;
                sp_ext.Next(false);
                EditorGUILayout.PropertyField(sp_ext);

                GUILayout.Space(4);
                sp_ext.Next(false);
                EditorGUILayout.PropertyField(sp_ext);

                EditorGUIUtility.labelWidth = 0;
            }

            GUILayout.EndVertical();
        }

        bool DrawChainSelector(float width, GUIContent label, RagdollProcessor.BonesChain anim)
        {
            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Width(width))) { GUI.FocusControl(""); return true; }
            return false;
        }


    }
}
