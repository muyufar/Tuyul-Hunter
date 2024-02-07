using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu("FImpossible Creations/Ragdoll Animator Stabilizer")]
    public class RagdollAnimatorStabilizer : MonoBehaviour
    {
        public RagdollAnimator Ragdoll;
        public Transform StabilizeTowards;
        [Tooltip("If null then using hips. You can assign there animator bone or pre-generated ragdoll dummy bone.")]
        public Transform OptionalToPush = null;

        [Space(4)]
        [Range(0f, 1f)] public float StabilizePower = 0.75f;
        [Range(0f, 1f)] public float StabilizeRotationPower = 0.75f;
        [Range(0f, 1f)] public float StabilizeReactionSpeed = 0.75f;

        [Space(8)]
        [Range(0f, 1f)] public float PowerBoost = 0.0f;
        [Range(0f, 1f)] public float HardForceStability = 0.0f;
        public bool ForceStabilityIsKinematic = true;

        [Space(10)]
        [Tooltip("Checking legs stand state to make character fall or stick towards stability position")]
        public bool UseStabilityCheck = false;
        [Tooltip("Foot bones if using ragdoll feets, lower legs if not using ragdoll foot bones")]
        public List<Transform> StandingCheckBones;

        [Range(0f, 1f)]
        [Tooltip("Higher value = easier to standing detect")]
        public float StandingCheckTolerance = 0.5f;

        [Tooltip("Disable to call stabilize behaviour even if character lies on the ground")]
        public bool StandOnlyIfHipsUp = true;
        [Tooltip("Only if enabled StandOnlyIfHipsUp")]
        [Range(0.1f, 0.5f)] public float FallOnHipsHeight = 0.3f;

        public enum EStandingState { Fall, BarelyStands, StableStanding }
        public EStandingState StandingState { get; private set; }

        Vector3 hipsHeight;


        List<StabilityBoneControl> stabilityBones;

        float stabilizingSmoother = 1f;
        float sd_stabSmoother = 0f;
        bool wasForceStab = false;



        void Start()
        {
            if (StabilizeTowards == null)
            {
                StabilizeTowards = Ragdoll.Parameters.Pelvis;
            }

            for (int i = StandingCheckBones.Count - 1; i >= 0; i--)
                if (StandingCheckBones[i] == null) StandingCheckBones.RemoveAt(i);

            if (StandingCheckBones.Count <= 0) UseStabilityCheck = false;
            else
            {
                stabilityBones = new List<StabilityBoneControl>();
                for (int i = 0; i < StandingCheckBones.Count; i++)
                {
                    stabilityBones.Add(new StabilityBoneControl(StandingCheckBones[i], Ragdoll));
                }
            }

            hipsHeight = Ragdoll.Parameters.BaseTransform.InverseTransformPoint(Ragdoll.Parameters.Pelvis.position);
        }

        Transform _toPushFlag = null;
        RagdollProcessor.PosingBone toStabilize; 

        void FixedUpdate()
        {
            if (Ragdoll.Parameters.Initialized == false) return;

            if (OptionalToPush == null)
            {
                toStabilize = Ragdoll.Parameters.GetPelvisBone();
            }

            if (OptionalToPush != _toPushFlag)
            {
                _toPushFlag = OptionalToPush;
                if (OptionalToPush != null)
                {
                    var posBone = Ragdoll.Parameters.User_GetPosingBoneByAnimatorBone(OptionalToPush);
                    if ( posBone == null) posBone = Ragdoll.Parameters.User_GetPosingBoneWithRagdollDummyBone(OptionalToPush);
                    toStabilize = posBone;
                }
            }

            if (toStabilize == null) return;

            Rigidbody hipsRig = toStabilize.rigidbody;
            float stabilizeMultiplier = 1f;


            if (UseStabilityCheck && stabilityBones != null)
            {
                bool someNotStanding = false;
                bool allNotStanding = true;

                float accumulatedStability = 0f;

                for (int i = 0; i < stabilityBones.Count; i++)
                {
                    stabilityBones[i].Update();

                    if (stabilityBones[i].IsStanding(StandingCheckTolerance))
                        allNotStanding = false;
                    else
                        someNotStanding = true;

                    accumulatedStability += Mathf.Max(0f, stabilityBones[i].Stability);
                }

                if (allNotStanding)
                    StandingState = EStandingState.Fall;
                else if (someNotStanding)
                    StandingState = EStandingState.BarelyStands;
                else
                    StandingState = EStandingState.StableStanding;


                if (StandingState == EStandingState.Fall)
                {
                    stabilizeMultiplier = 0f;
                }
                else if (StandingState == EStandingState.BarelyStands)
                {
                    stabilizeMultiplier = Mathf.Min(1.5f, accumulatedStability);
                }
            }

            hipsTooLow = false;

            if (UseStabilityCheck)
                if (stabilizeMultiplier > 0f)
                {
                    if (StandOnlyIfHipsUp)
                    {
                        float maxfootHipsHeightDiff = 0f;
                        for (int i = 0; i < stabilityBones.Count; i++)
                        {
                            float diff = ToRagdollLocal(stabilityBones[i].GetHeelPosition()).y - ToRagdollLocal(Ragdoll.Parameters.GetPelvisBone().transform.position).y;
                            diff = Mathf.Abs(diff);
                            if (diff > maxfootHipsHeightDiff) maxfootHipsHeightDiff = diff;
                        }

                        if (maxfootHipsHeightDiff < hipsHeight.y * FallOnHipsHeight)
                        {
                            hipsTooLow = true;
                            stabilizeMultiplier = 0f;
                        }
                    }
                }

            Vector3 targetStablePosition = StabilizeTowards.position;
            Quaternion targetStableRotation = StabilizeTowards.rotation;

            if (StabilizeReactionSpeed >= 1f) stabilizingSmoother = stabilizeMultiplier;
            else
            {
                stabilizingSmoother = Mathf.SmoothDamp(stabilizingSmoother, stabilizeMultiplier, ref sd_stabSmoother,
                    Mathf.Lerp(0.1f, 0.003f, StabilizeReactionSpeed), float.MaxValue, Time.fixedDeltaTime);
            }

            if (stabilizingSmoother > 0f)
            {
                float targetPower = StabilizePower * stabilizingSmoother;
                if (targetPower > 1f) targetPower = 1f;

                float deltaDiv = Time.fixedDeltaTime * (50f - (PowerBoost * 49f));
                deltaDiv = Mathf.Clamp(deltaDiv, 0.005f, 1f);

                Vector3 targetStableVelo = (targetStablePosition - hipsRig.position) / deltaDiv;
                hipsRig.velocity = Vector3.Lerp(hipsRig.velocity, targetStableVelo, targetPower);

                Quaternion targetStableRotVelo = targetStableRotation * Quaternion.Inverse(hipsRig.rotation);

                Vector3 stabilizeAngularVelo = FEngineering.QToAngularVelocity(targetStableRotVelo, true);
                hipsRig.maxAngularVelocity = 10f;

                targetPower = StabilizeRotationPower * stabilizingSmoother;
                if (targetPower > 1f) targetPower = 1f;

                hipsRig.angularVelocity = Vector3.Lerp(hipsRig.angularVelocity, stabilizeAngularVelo, targetPower);
            }

            // Custom - controllable gravity
            //Vector3 grav = new Vector3(0f, -15f, 0f);
            //hipsRig.AddForce(grav * hipsRig.mass);

            if (HardForceStability > 0f)
            {
                wasForceStab = true;

                if (ForceStabilityIsKinematic) hipsRig.isKinematic = true;
                else
                {
                    hipsRig.velocity *= 0.05f + (1f - HardForceStability) * 0.95f;
                }

                hipsRig.position = Vector3.Lerp(hipsRig.position, targetStablePosition, HardForceStability);
                hipsRig.rotation = Quaternion.Lerp(hipsRig.rotation, targetStableRotation, HardForceStability * StabilizeRotationPower);
            }
            else
            {
                if (wasForceStab)
                {
                    hipsRig.isKinematic = false;
                    if (ForceStabilityIsKinematic) wasForceStab = false;
                }
            }

        }

        Vector3 ToRagdollLocal(Vector3 pos)
        {
            return Ragdoll.Parameters.BaseTransform.InverseTransformPoint(pos);
        }

        public bool hipsTooLow { get; private set; }

        class StabilityBoneControl
        {
            public Transform bone;
            public RagdollProcessor.PosingBone ragdollBone;
            private CollisionStayHelper coll;
            Vector3 localUp;
            Vector3 floorVector;

            [DefaultExecutionOrder(1)]
            class CollisionStayHelper : MonoBehaviour
            {
                public Collision lastCollision = null;
                private void OnCollisionStay(Collision collision)
                {
                    lastCollision = collision;
                }

                //private void FixedUpdate()
                //{
                //    lastCollision = null;
                //}
            }


            public StabilityBoneControl(Transform b, RagdollAnimator rag)
            {
                bone = b;
                ragdollBone = rag.Parameters.User_GetPosingBoneByAnimatorBone(b);
                coll = ragdollBone.transform.gameObject.AddComponent<CollisionStayHelper>();
                Stability = 1f;

                localUp = bone.InverseTransformDirection(rag.Parameters.BaseTransform.up);

                floorVector = bone.transform.position;
                floorVector = rag.Parameters.BaseTransform.InverseTransformPoint(floorVector);
                floorVector.y = 0f;
                floorVector = rag.Parameters.BaseTransform.TransformPoint(floorVector);
                floorVector = bone.transform.InverseTransformPoint(floorVector);
            }

            public Vector3 GetHeelPosition()
            {
                return ragdollBone.transform.TransformPoint(floorVector);
            }

            public float Stability { get; private set; }

            public void Update()
            {
                if (coll.lastCollision != null)
                {
                    float best = -1f;

                    Vector3 desiredNorm = ragdollBone.transform.TransformDirection(localUp);

                    var collision = coll.lastCollision;

                    for (int k = 0; k < collision.contactCount; k++)
                    {
                        Vector3 collisionNorm = collision.GetContact(k).normal;
                        float dot = Vector3.Dot(collisionNorm, desiredNorm);
                        //UnityEngine.Debug.DrawRay(collision.GetContact(0).point, desiredNorm, Color.green, 1.01f);
                        //UnityEngine.Debug.DrawRay(collision.GetContact(0).point, collisionNorm, Color.red, 1.01f);
                        //UnityEngine.Debug.Log(bone.name + " dot = " + dot);
                        if (dot > best) best = dot;
                    }

                    Stability = best;
                }
                else
                {
                    Stability = -0.01f;
                }
            }

            public bool IsStanding(float tolerance)
            {
                return (Stability >= 1f - tolerance);
            }
        }


        #region Editor Code
#if UNITY_EDITOR
        [HideInInspector]
        public bool _Editor_HideInfo = false;
#endif
        #endregion


    }


    #region Editor Class

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(RagdollAnimatorStabilizer))]
    public class RagdollAnimatorStabilizerEditor : UnityEditor.Editor
    {
        public RagdollAnimatorStabilizer Get { get { if (_get == null) _get = (RagdollAnimatorStabilizer)target; return _get; } }
        private RagdollAnimatorStabilizer _get;

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Get._Editor_HideInfo == false)
            {
                UnityEditor.EditorGUILayout.HelpBox("This component will try keeping character pelvis in sync with played animation. It can also detect if legs are standing on ground to make character fall/stand stable accordingly to the ragdoll pose.", UnityEditor.MessageType.Info);
                var rect = GUILayoutUtility.GetLastRect();
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) { Get._Editor_HideInfo = true; }
            }

            if (Get.Ragdoll != null)
            {
                if (Get.Ragdoll.Parameters.FreeFallRagdoll == false)
                {
                    UnityEditor.EditorGUILayout.HelpBox("This works only with Ragdoll 'Free fall' enabled!", UnityEditor.MessageType.None);
                }
            }

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                if (Get.UseStabilityCheck)
                {
                    GUI.enabled = false;
                    UnityEditor.EditorGUILayout.EnumPopup("Debug Stand State:", Get.StandingState);
                    GUI.enabled = true;

                    if (Get.HardForceStability > 0f)
                    {
                        UnityEditor.EditorGUILayout.HelpBox("Using |HardForceStability| to move character back to stable pose", UnityEditor.MessageType.None);
                    }
                    else if (Get.hipsTooLow)
                    {
                        UnityEditor.EditorGUILayout.HelpBox("|StandOnlyIfHipsUp| Hips too low - charater Fall!", UnityEditor.MessageType.None);
                    }
                    if (Get.StandingState == RagdollAnimatorStabilizer.EStandingState.Fall)
                    {
                        UnityEditor.EditorGUILayout.HelpBox("|UseStabilityCheck| Character Lost Stability! FALL! Ignoring stabilizer factors and making character fall on the ground!", UnityEditor.MessageType.None);
                    }
                }
            }

            GUILayout.Space(4f);
            UnityEditor.EditorGUILayout.HelpBox("This component will be improved in future versions.", UnityEditor.MessageType.None);

        }
    }

#endif

    #endregion


}