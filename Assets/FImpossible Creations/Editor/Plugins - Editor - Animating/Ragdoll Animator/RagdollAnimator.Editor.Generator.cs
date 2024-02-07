using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{

    [System.Serializable]
    public class RagdollGenerator
    {
        public RagdollGeneratorPlayer ragdollGen = new RagdollGeneratorPlayer();
        public Transform BaseTransform = null;
        public bool IsGenerateEnabled { get { return generateRagdoll; } }
        bool generateRagdoll = false;
        bool characterJoints = false;
        float ragdollSprings = 0f;
        float projDist = .05f;
        float projAngle = 60f;
        bool enableCollision = false;
        bool enablePreProcessing = false;

        public enum tweakRagd { None, Position, Scale };
        public tweakRagd ragdollTweak = tweakRagd.None;

        bool useSymmetry = false;
        public bool forceUpdateGenerate = false;


        public void Tab_RagdollGenerator(RagdollProcessor proc, bool assignProcessorBones)
        {
            var par = proc.GeneratorParameters;
            
            if (proc.IsPreGeneratedDummy)
            {
                EditorGUILayout.HelpBox("Pre generated ragdoll is not available for ragdoll generator / colliders adjustements, generate ragdoll and do adjustements before pre generating", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 116;

                EditorGUI.BeginChangeCheck();
                generateRagdoll = EditorGUILayout.Toggle("Generate Ragdoll", generateRagdoll, new GUILayoutOption[] { GUILayout.Width(160) });
                EditorGUIUtility.labelWidth = 0;
                
                //if (!generateRagdoll) GUI.enabled = false;
                //if (GUILayout.Button(characterJoints ? "Now Using Character Joints" : "Now Using Configurable Joints")) characterJoints = !characterJoints;
                EditorGUILayout.EndHorizontal();

                if (!generateRagdoll) GUI.enabled = true;
                if (!generateRagdoll) GUI.color = new Color(1f, 1f, 1f, 0.5f);

                if (proc.GeneratorParameters.Editor_DrawParameters(BaseTransform))
                {
                    if (generateRagdoll)
                        forceUpdateGenerate = true;
                }

                if (!generateRagdoll) GUI.color = Color.white;

                if (proc.LeftFist || proc.RightFist) par.generateFists = true; else par.generateFists = false;
                if (proc.LeftFoot || proc.RightFoot) par.generateFoots = true; else par.generateFoots = false;
                if (proc.LeftShoulder || proc.RightShoulder) par.generateShoulders = true; else par.generateShoulders = false;


                GUILayout.Space(5);


                if (!generateRagdoll) GUI.enabled = true;

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);


                EditorGUILayout.BeginHorizontal();
                ragdollTweak = (tweakRagd)EditorGUILayout.EnumPopup("Tweak Ragdoll Colliders", ragdollTweak);

                if (proc.BonesSetupMode == RagdollProcessor.EBonesSetupMode.HumanoidLimbs)
                {
                    GUILayout.Space(4);
                    EditorGUIUtility.labelWidth = 44;
                    useSymmetry = EditorGUILayout.Toggle(new GUIContent("Symm", "(EXPERIMENTAL) Use Symmetry for tweaking colliders"), useSymmetry, GUILayout.Width(64));
                    EditorGUIUtility.labelWidth = 0;
                }

                EditorGUILayout.EndHorizontal();


                GUILayout.Space(5);

                EditorGUILayout.HelpBox("Remember about correct collision LAYERS on bone transforms and on the movement controller!", MessageType.None);

                if (EditorGUI.EndChangeCheck() || forceUpdateGenerate)
                    if (generateRagdoll)
                        if (proc.RagdollDummyBase == null)
                        {
                            forceUpdateGenerate = false;
                            if (assignProcessorBones)
                            {
                                ragdollGen.SetAllBoneReferences(proc);
                            }

                            UpdateOrGenerateRagdoll(proc, characterJoints, proc.GeneratorParameters, 0f, 0f, ragdollSprings, true, projAngle, projDist, enableCollision, enablePreProcessing, false);
                        }

                GUILayout.Space(3);
                if (GUILayout.Button("Remove Ragdoll Components on Bones")) { RemoveRagdoll(proc); generateRagdoll = false; }

                EditorGUILayout.EndVertical();
            }

        }


        public void OnSceneGUI(RagdollProcessor proc)
        {
            if (ragdollTweak != tweakRagd.None)
            {
                generateRagdoll = false;
                bool tweakScale = ragdollTweak == tweakRagd.Scale;

                if (useSymmetry)
                    RagdollProcessor._editor_symmetryRef = BaseTransform;
                else
                    RagdollProcessor._editor_symmetryRef = null;


                if (proc.BonesSetupMode == RagdollProcessor.EBonesSetupMode.CustomLimbs)
                {
                    for (int i = 0; i < ragdollGen.BoneChains.Count; i++)
                    {
                        var chain = ragdollGen.BoneChains[i];
                        for (int b = 0; b < chain.BoneSetups.Count; b++)
                        {
                            var bone = chain.BoneSetups[b];
                            if (bone.t == null) continue;
                            Collider c = bone.t.GetComponent<Collider>();
                            if (c == null) continue;
                            RagdollProcessor.DrawColliderHandles(c, tweakScale, false);
                        }
                    }
                }
                else
                {
                    if (ragdollGen.LeftUpperArm.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.LeftUpperArm.RagdollCollider(), tweakScale, false, ragdollGen.RightUpperArm.RagdollCollider());
                    if (ragdollGen.RightUpperArm.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.RightUpperArm.RagdollCollider(), tweakScale, false, ragdollGen.LeftUpperArm.RagdollCollider());
                    if (ragdollGen.LeftForearm.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.LeftForearm.RagdollCollider(), tweakScale, false, ragdollGen.RightForearm.RagdollCollider());
                    if (ragdollGen.RightForearm.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.RightForearm.RagdollCollider(), tweakScale, false, ragdollGen.LeftForearm.RagdollCollider());

                    if (ragdollGen.LeftUpperLeg.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.LeftUpperLeg.RagdollCollider(), tweakScale, false, ragdollGen.RightUpperLeg.RagdollCollider());
                    if (ragdollGen.LeftLowerLeg.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.LeftLowerLeg.RagdollCollider(), tweakScale, false, ragdollGen.RightLowerLeg.RagdollCollider());
                    if (ragdollGen.RightUpperLeg.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.RightUpperLeg.RagdollCollider(), tweakScale, false, ragdollGen.LeftUpperLeg.RagdollCollider());
                    if (ragdollGen.RightLowerLeg.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.RightLowerLeg.RagdollCollider(), tweakScale, false, ragdollGen.LeftLowerLeg.RagdollCollider());

                    if (ragdollGen.PelvisBone.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.PelvisBone.RagdollCollider(), tweakScale, true);
                    if (ragdollGen.SpineRoot.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.SpineRoot.RagdollCollider(), tweakScale, true);
                    if (ragdollGen.Chest) if (ragdollGen.Chest.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.Chest.RagdollCollider(), tweakScale, true);
                    if (ragdollGen.Head.RagdollCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.Head.RagdollCollider(), tweakScale, true);

                    if (ragdollGen.LeftHand) if (ragdollGen.LeftHand.RagdollBCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.LeftHand.RagdollBCollider(), tweakScale, false, ragdollGen.RightHand.RagdollBCollider());
                    if (ragdollGen.RightHand) if (ragdollGen.RightHand.RagdollBCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.RightHand.RagdollBCollider(), tweakScale, false, ragdollGen.LeftHand.RagdollBCollider());
                    if (ragdollGen.LeftFoot) if (ragdollGen.LeftFoot.RagdollBCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.LeftFoot.RagdollBCollider(), tweakScale, false, ragdollGen.RightFoot.RagdollBCollider());
                    if (ragdollGen.RightFoot) if (ragdollGen.RightFoot.RagdollBCollider()) RagdollProcessor.DrawColliderHandles(ragdollGen.RightFoot.RagdollBCollider(), tweakScale, false, ragdollGen.LeftFoot.RagdollBCollider());
                }
            }

        }


        public void UpdateOrGenerateRagdoll(RagdollProcessor proc, bool characterJoints , RagdollGeneratorParameters par, float bounciness = 0f, float damper = 0f, float spring = 0f, bool projection = true, float projAngle = 90, float projDistance = 0.05f, bool enCollision = false, bool preProcessing = false, bool addToFoots = false)
        {
            ragdollGen.UpdateOrGenerateRagdoll(proc, par, characterJoints, bounciness, damper
            , spring,  projection, projAngle, projDistance, enCollision,
            preProcessing);
        }


        public void UpdateOrGenerateRagdoll(RagdollProcessor proc, bool characterJoints = true, float scale = 1f, float bounciness = 0f, float damper = 0f, float spring = 0f, float drag = 0.5f, float aDrag = 1f, float massToDistr = 65f, bool projection = true, float projAngle = 90, float projDistance = 0.05f, bool enCollision = false, bool preProcessing = false, bool addToFoots = false, RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate)
        {
            UpdateOrGenerateRagdoll(proc, characterJoints, scale, bounciness, damper
                , spring, drag, aDrag, massToDistr, projection, projAngle, projDistance, enCollision,
                preProcessing, addToFoots, interpolation);
        }


        public void RemoveRagdoll(RagdollProcessor proc)
        {
            if (proc.Pelvis == null) return;

            Ragdoll_RemoveFrom(proc.Pelvis);

            if (proc.BonesSetupMode == RagdollProcessor.EBonesSetupMode.HumanoidLimbs)
            {
                if (ragdollGen.bones == null)
                {
                    ragdollGen.SetAllBoneReferences(proc);
                }

                if (ragdollGen.bones != null)
                    foreach (Transform t in ragdollGen.bones)
                    {
                        if (t == null) continue;
                        Ragdoll_RemoveFrom(t);
                    }

                if (ragdollGen.bones != null)
                    foreach (Transform t in ragdollGen.bones)
                    {
                        if (t == null) continue;
                        if (t.childCount > 0)
                        {
                            Ragdoll_RemoveFrom(t.GetChild(0));
                            Ragdoll_RemoveFrom(t.GetLimbChild());
                        }
                    }
            }
            else
            {

                for (int c = 0; c < proc.CustomLimbsBonesChains.Count; c++)
                {
                    RagdollProcessor.BonesChain chain = proc.CustomLimbsBonesChains[c];
                    for (int b = 0; b < chain.BoneSetups.Count; b++)
                    {
                        var bone = chain.BoneSetups[b];
                        if (bone.t == null) continue;
                        Ragdoll_RemoveFrom(bone.t);
                    }
                }
            }

        }


        public void Ragdoll_RemoveFrom(Transform bone)
        {
            DestroyIfHave<ConfigurableJoint>(bone);
            DestroyIfHave<CharacterJoint>(bone);
            DestroyIfHaveIgnoreTriggers<CapsuleCollider>(bone);
            DestroyIfHaveIgnoreTriggers<SphereCollider>(bone);
            DestroyIfHaveIgnoreTriggers<BoxCollider>(bone);
            DestroyIfHave<Rigidbody>(bone);
        }


        public void DestroyIfHave<T>(Transform owner) where T : Component
        {
            if (owner == null) return;
            T comp = owner.GetComponent<T>();
            if (comp != null) GameObject.DestroyImmediate(comp);
        }


        public void DestroyIfHaveIgnoreTriggers<T>(Transform owner) where T : Collider
        {
            if (owner == null) return;
            T comp = owner.GetComponent<T>();
            if (comp != null) if (comp.isTrigger == false) GameObject.DestroyImmediate(comp);
        }


    }
}
