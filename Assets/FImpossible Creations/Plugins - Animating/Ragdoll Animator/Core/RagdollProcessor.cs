using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    public partial class RagdollProcessor
    {
        internal RotationDriveMode RotationMode = RotationDriveMode.Slerp;

        [Tooltip("With FreeFallRagdoll enabled you should put ragdoll amount to 100% to make character fall on ground.\n\nWith FreeFallRagdoll disabled you can blend ragdoll with animator in few different ways.")]
        public bool FreeFallRagdoll = true;

        // Main Params

        [Tooltip("Constant amount of ragdoll on model.\n\nWith FreeFallRagdoll enabled you should put ragdoll amount to 100% to make character fall on ground.")][FPD_Suffix(0f, 1f)] public float RagdolledBlend = 1f;
        public float FinalRagdollBlend { get { return RagdolledBlend * User_BlendMultiplier; } }
        /// <summary> Internal ragdoll blend multiplier affecting all blend values of ragdoll animator, including partial blend </summary>
        [NonSerialized] public float User_BlendMultiplier = 1f;

        [Tooltip("How much strength should be used to rotate towards target pose")][Range(0f, 1f)] public float RotateToPoseForce = 0.75f;
        [Tooltip("Quality of unity physical iterations")][Range(1, 16)] public int UnitySolverIterations = 6;
        [Tooltip("If you want to smoothly enable partial ragdoll on bone collision (when ragdoll is NOT during free-fall state)")]
        public bool BlendOnCollision = false;
        private bool wasBlendOnCollision = true;
        private bool wasSensitiveColliding = false;

        [Space(4)]
        [Tooltip("Partial ragdoll amount - making arms always ragdolled with certain amount")][Range(0f, 1f)] public float ConstantArmsRagdoll = 0f;
        [Tooltip("Partial ragdoll amount - making head always ragdolled with certain amount")][Range(0f, 1f)] public float ConstantHeadRagdoll = 0f;
        [Tooltip("Partial ragdoll amount - making spine always ragdolled with certain amount")][Range(0f, 1f)] public float ConstantSpineRagdoll = 0f;

        [Tooltip("How sudden should be auto-blending to ragdoll on collisions when using partial ragdoll")][Range(0f, 1f)] public float BlendingSpeed = 0.5f;
        [Tooltip("If arms collision should trigger blending for upper spine to keep bones rotation more synced")] public bool SensitiveCollision = false;

        [FPD_Header("Spring and damp only with configurable joints")]
        [Tooltip("Spring applies more power towards target pose")]
        public float ConfigurableSpring = 1650;
        [NonSerialized] public float OutConfigurableSpring = 1650;

        [Tooltip("Damp power for physical joints - can reduce bouncy effect but require more spring")]
        public float ConfigurableDamp = 18;
        [Tooltip("If using Hips Pin feature - spring power for hips")]
        public float HipsPinSpring = 4000;
        [Tooltip("Additional Custom hips offset when using Hips Pin Feature (local/configurable space offset)")]
        public Vector3 HipsPositionOffset = Vector3.zero;
        [Tooltip("Boosting animator influence on the different axis of the hips pin")]
        public Vector3 HipsBoostMul = Vector3.one;
        [Tooltip("Experimental different way to animate hips pin")]
        public bool HipsPinV2 = true;
        //[Tooltip("Additional Custom hips rotation offset (for animation offset) when using Hips Pin Feature (in eulers -> 0-360)")]
        //public Vector3 HipsAxisRotation = Vector3.zero;

        [FPD_Header("More custom settings")]
        [Tooltip("Used only when performing Stad Up animation from ragdolled state!\n\nUsing method User_RepositionRoot() will move model to ragdoll position, it depends on your Stand Up animations - if stand up animation is placed fully in model center, set this slider to zero: then object will be positioned to start in model center\nIf stand up animation origin is placed in foots position (bottom of body) then set slider to 1: so model will be fitted to foot position")][Range(0f, 1f)] public float StandUpInFootPoint = 0f;

        [Space(6)]
        [Tooltip("If this limbs should have different muscle power than others")][Range(0f, 1f)] public float HeadForce = 1f;
        [Tooltip("If this limbs should have different muscle power than others")][Range(0f, 1f)] public float SpineForce = 1f;
        [Tooltip("If this limbs should have different muscle power than others")][Range(0f, 1f)] public float RightArmForce = 1f;
        [Tooltip("If this limbs should have different muscle power than others")][Range(0f, 1f)] public float LeftArmForce = 1f;
        [Tooltip("If this limbs should have different muscle power than others")][Range(0f, 1f)] public float LeftLegForce = 1f;
        [Tooltip("If this limbs should have different muscle power than others")][Range(0f, 1f)] public float RightLegForce = 1f;


        [Tooltip("Use SwitchAllExtendedAnimatorSync() method if you want to switch this feature through code!\n\nTrying to sync shoulder and additional spine bones on ragdoll to make ragdoll animated pose synced more precisely with animator.")]
        public ESyncMode ExtendedAnimatorSync = ESyncMode.None;
        public enum EBaseTransformRepose { None, BonesBoundsBottomCenter, HipsToFootPosition }
        [Tooltip("If character gets free-fall ragdolled and you want the BaseTransform to follow ragdoll dummy position, use this parameter")]
        public EBaseTransformRepose ReposeMode = EBaseTransformRepose.None;
        [Tooltip("Allow reposing only when ragdoll blend is >99%")] public bool ReposeOnlyOnFullRagdoll = false;

        [Tooltip("Ignore all self skeleton colliders collision")] public bool IgnoreSelfCollision = false;

        [Tooltip("Multiply mass (using initial mass) of the rigidbody (updated on start and on FreeFall Switching)")]
        public float MassMultiplier = 1f;

        [Tooltip("If not null, applying on start this physical material to all of the ragdoll dummy colliders")]
        public PhysicMaterial SetPhysicalMaterial = null;

        [Tooltip("(only for configurable joints) Removing rotation limits from ConfigurableJoints when not using 'Free Fall' mode: it can help animation matching to allow bones rotate as animator demands.")]
        public bool RemoveLimitsWhenNonFreeFall = false;
        [Tooltip("If you want to hide generated ragdoll dummy in hierarchy view for more clarity insight during playmode")]
        public bool HideDummy = false;

        [Tooltip("Ignoring collision with provided colliders (called just on start)")]
        public List<Collider> IgnoreCollisionWith = new List<Collider>();



        [Tooltip("(Experimental) Enabling feature of keeping pelvis with physics instead of making it kinematic")]
        public bool HipsPin = false;
        [Tooltip("Apply pelvis animation with the keyframe animation (not free fall mode).\nV2 is applying more spring force to the muscles when non-free-fall.")]
        public EAnimatePelvis TryAnimatePelvis = EAnimatePelvis.V1;

        /// <summary> It's 'Try Animate Pelvis' value but with new name </summary>
        public EAnimatePelvis AnimatePelvis { get { return TryAnimatePelvis; } set { TryAnimatePelvis = value; } }

        public enum EAnimatePelvis
        {
            None, V1, V2
        }


        [Tooltip("Initializing component after few frames")] public bool StartAfterTPose = false;
        [Tooltip("If you use plugin which is resetting automatically some rigidbody settings, you can enable it to avoid glitches")] public bool ResetIsKinematic = false;

        [Tooltip("Calling 'DontDestroyOnLoad, useful if you change scenes wich player character and you want to keep the same ragdoll animator")] public bool Persistent = false;
        [Tooltip("If your animator have enabled 'AnimatePhysics' update mode, you should enable it here too")] public bool AnimatePhysics = false;
        [Tooltip("If you want to add 'R_' name prefix to generated dummy transforms. It can be helpful to avoid errors of the animation rigging unity plugin naming conflicts.")] public bool AddNamePrefix = false;


        [Tooltip("If you encounter error with skeleton hips placed in foot position of the skeleton")] public bool FixRootInPelvis = true;
        [Tooltip("If some bones of your characters seems to fall without physical power, you can try enabling this")] public bool Calibrate = false;


        public enum EBonesSetupMode { HumanoidLimbs, CustomLimbs }
        public EBonesSetupMode BonesSetupMode = EBonesSetupMode.HumanoidLimbs;



        public List<BonesChain> CustomLimbsBonesChains = new List<BonesChain>();
        [SerializeField, HideInInspector] private RagdollBoneSetup _pelvisSetupBone;

        [System.Serializable]
        public class BonesChain
        {
            public string ChainName = "Bones Chain";
            public List<RagdollBoneSetup> BoneSetups = new List<RagdollBoneSetup>();

            public bool BlendOnCollisions = false;
            public bool Detach = false;
            public float ConstantRagdoll = 0f;
            public float MusclesForce = 1f;

            [NonSerialized] public float blendOnCollisionCulldown = 0f;
            [NonSerialized] public float blendOnCollisionMin = 0f;
            [NonSerialized] public float sd_blendVelo = 0f;

            public Transform GetAttachTransform(RagdollProcessor ragdollProcessor)
            {
                if (BoneSetups.Count == 0) return null;
                Transform startBone = BoneSetups[0].t;
                if (startBone == null) return null;

                Transform parent = startBone.parent;
                while (parent != null)
                {
                    if (ragdollProcessor.ContainsTransformInSetup(parent)) return parent;
                    parent = parent.parent;
                }

                return null;
            }

            public RagdollBoneSetup GetNext(RagdollBoneSetup bone, bool forward)
            {
                int index = GetIndex(bone);
                if (index == -1) return null;

                index += forward ? 1 : -1;
                if (index < 0) index = BoneSetups.Count - 1;
                if (index >= BoneSetups.Count) index = 0;

                return BoneSetups[index];
            }

            public int GetIndex(RagdollBoneSetup bone)
            {
                for (int i = 0; i < BoneSetups.Count; i++) if (BoneSetups[i] == bone) return i;
                return -1;
            }

            internal void InitializeForDummy(RagdollProcessor ragdollProcessor, Transform dummyRoot)
            {
                for (int b = 0; b < BoneSetups.Count; b++)
                {
                    var bone = BoneSetups[b];
                    if (bone.t == null) continue;
                    bone.SetPosingBone(FindChildByNameInDepth(bone.t.name, dummyRoot), ragdollProcessor);
                    bone.Posing.SetVisibleBone(bone.t);
                }

                for (int b = 0; b < BoneSetups.Count - 1; b++)
                {
                    var bone = BoneSetups[b];
                    var nbone = BoneSetups[b + 1];
                    if (bone.t == null) continue;
                    if (nbone.t == null) continue;
                    bone.Posing.child = nbone.Posing;
                }

                DetachBoneChainsIfDetachEnabled();
            }


            bool playmodeDetached = false;
            public void DetachBoneChainsIfDetachEnabled()
            {
                if (playmodeDetached) return;

#if UNITY_EDITOR
                if (Application.isPlaying == false) return;
#endif

                if (Detach)
                {
                    //for (int b = 0; b < BoneSetups.Count - 1; b++)
                    //{
                    //    var bone = BoneSetups[b];
                    //    var nbone = BoneSetups[b + 1];
                    //    if (bone.t == null) continue;
                    //    if (nbone.t == null) continue;
                    //    bone.Posing.DetachParent = bone.Posing.transform.parent;
                    //    bone.Posing.transform.parent = null;
                    //}

                    BoneSetups[0].Posing.DetachParent = BoneSetups[0].Posing.transform.parent;
                    BoneSetups[0].Posing.transform.parent = null;
                    playmodeDetached = true;
                }
            }

            internal void ApplyCollisionHelperInfo()
            {
                for (int b = 0; b < BoneSetups.Count; b++)
                {
                    var bone = BoneSetups[b];
                    if (bone.Posing == null) continue;
                    if (bone.Posing.rigidbody == null) continue;

                    RagdollCollisionHelper hlp = bone.Posing.rigidbody.GetComponent<RagdollCollisionHelper>();

                    if (hlp)
                    {
                        hlp.CustomBoneChainApplyInfo(this, b);
                    }
                }

            }
        }

        public bool ContainsTransformInSetup(Transform t)
        {
            if (t == Pelvis) return true;

            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs) ConvertHumanoidLimbsToCustomLimbs();

            for (int c = 0; c < CustomLimbsBonesChains.Count; c++)
            {
                var chain = CustomLimbsBonesChains[c];
                for (int b = 0; b < chain.BoneSetups.Count; b++)
                {
                    if (chain.BoneSetups[b] == null) continue;
                    if (chain.BoneSetups[b].t == t) return true;
                }
            }

            return false;
        }

        public void ConvertHumanoidLimbsToCustomLimbs()
        {

        }


        public RagdollBoneSetup GetPelvisSetupBone()
        {
            if (_pelvisSetupBone == null) _pelvisSetupBone = new RagdollBoneSetup();
            _pelvisSetupBone.t = Pelvis;
            return _pelvisSetupBone;
        }


        [System.Serializable]
        public class RagdollBoneSetup
        {
            public Transform t;

            public RagdollBoneSetup() { }
            public RagdollBoneSetup(Transform t, EColliderType collType, float mass = 0.1f)
            {
                this.t = t;
                ColliderType = collType;
                MassPercentage = mass;
            }

            public enum EColliderType { Capsule, Sphere, Box, Mesh, NoCollider_CustomColliderAsChild }
            public EColliderType ColliderType = EColliderType.Capsule;
            public Mesh ColliderMesh = null;

            [Tooltip("Percentage amount for the bone weight using 'Target Mass' value for ragdoll generator")]
            public float MassPercentage = .1f;

            public float MuscleForceMultiplier = 1f;

            public PosingBone Posing;
            public Transform DummyBone { get { return Posing.transform; } }

            // Ragdoll Generator Helpers
            public float Generator_ScaleMul = 1f;
            public float Generator_LengthMul = 1f;
            public float Generator_LengthOffset = 0f;
            public Vector3 Generator_BoxScale = Vector3.one;
            public Vector3 Generator_Offset = Vector3.zero;

            public ECapsDirOverride Generator_OverrideCapsuleDir = ECapsDirOverride.None;
            public enum ECapsDirOverride : int { None = -1, X_Axis = 0, Y_Axis = 1, Z_Axis = 2 }

            public bool Generator_Extra = false;

            internal void SetPosingBone(Transform dummyBone, RagdollProcessor ragdollProcessor)
            {
                Posing = new PosingBone(dummyBone, ragdollProcessor);
            }

            public void PasteFrom(RagdollBoneSetup refSett)
            {
                ColliderType = refSett.ColliderType;
                ColliderMesh = refSett.ColliderMesh;
                MassPercentage = refSett.MassPercentage;
                MuscleForceMultiplier = refSett.MuscleForceMultiplier;
                Generator_ScaleMul = refSett.Generator_ScaleMul;
                Generator_LengthMul = refSett.Generator_LengthMul;
                Generator_LengthOffset = refSett.Generator_LengthOffset;
                Generator_BoxScale = refSett.Generator_BoxScale;
                Generator_Offset = refSett.Generator_Offset;

                Generator_OverrideCapsuleDir = refSett.Generator_OverrideCapsuleDir;
            }
        }



        public bool Initialized { get; private set; } = false;

        //internal bool CaptureAnimator = true;

        public List<Rigidbody> RagdollLimbs { get; private set; }
        public List<Transform> Limbs { get; private set; }
        public List<PosingBone> RagdollSetup { get; private set; }

        public float LastestFreeFallStartTime { get; private set; }

        //bool haveFists = false;
        //bool haveFoots = false;

        Transform containerForDummy = null;
        public RagdollAnimator OwnerRagdollAnimatorComponent { get; private set; }

        internal void Initialize(MonoBehaviour caller, Transform objectWithAnimator, Transform customRagdollAnimator = null, Transform rootBone = null, Transform containerForDummy = null)
        {
            Initialized = false;
            OwnerRagdollAnimatorComponent = caller as RagdollAnimator;

            this.containerForDummy = containerForDummy;
            if (StartAfterTPose) caller.StartCoroutine(DelayedInitialize(caller, objectWithAnimator, customRagdollAnimator, rootBone));
            else Initialization(caller, objectWithAnimator, customRagdollAnimator, rootBone);
            if (ResetIsKinematic) User_SetAllKinematic(false);
        }


        IEnumerator DelayedInitialize(MonoBehaviour caller, Transform objectWithAnimator, Transform customRagdollAnimator = null, Transform rootBone = null)
        {
            yield return null;
            yield return new WaitForFixedUpdate();
            //yield return new WaitForSecondsRealtime(0.1f);
            Initialization(caller, objectWithAnimator, customRagdollAnimator, rootBone);
        }


        bool collisionHelpersAdded = false;

        public void EnsureCollisionHelpersAdded()
        {
            if (collisionHelpersAdded) return;

            PosingBone c = posingPelvis;

            while (c != null)
            {
                if (c.rigidbody != null)
                {
                    c.collisions = c.rigidbody.gameObject.AddComponent<RagdollCollisionHelper>().Initialize(this, c) as RagdollCollisionHelper;
                }

                c = c.child;
            }

            if (posingRightFist != null)
            {
                if (posingRightFist.rigidbody != null)
                {
                    RagdollCollisionHelper hlp = posingRightFist.rigidbody.GetComponent<RagdollCollisionHelper>();
                    if (hlp) hlp.ignores.Add(posingRightUpperLeg.transform);
                }
            }

            if (posingLeftFist != null)
                if (posingLeftFist.rigidbody != null)
                {
                    RagdollCollisionHelper hlp = posingLeftFist.rigidbody.GetComponent<RagdollCollisionHelper>();
                    if (hlp) hlp.ignores.Add(posingLeftUpperLeg.transform);
                }


            if (BonesSetupMode != EBonesSetupMode.HumanoidLimbs)
            {
                for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                {
                    CustomLimbsBonesChains[i].ApplyCollisionHelperInfo();
                }
            }

        }


        private MonoBehaviour parentCaller;
        void Initialization(MonoBehaviour caller, Transform objectWithAnimator, Transform customRagdollAnimator = null, Transform rootBone = null)
        {
            parentCaller = caller;
            PrepareRagdollDummy(objectWithAnimator, rootBone);
            CaptureAnimation();

            IsFadeRagdollCoroutineRunning = false;

            if (BaseTransform && Pelvis)
            {
                PelvisLocalForward = Pelvis.InverseTransformDirection(BaseTransform.forward);
                PelvisLocalRight = Pelvis.InverseTransformDirection(BaseTransform.right);
            }

            RagdollLimbs = new List<Rigidbody>();
            Limbs = new List<Transform>();

            PosingBone c = posingPelvis;

            while (c != null)
            {
                if (c.rigidbody != null)
                {
                    RagdollLimbs.Add(c.rigidbody);
                    Limbs.Add(c.transform);
                }

                c = c.child;
            }

            bool addragd = BlendOnCollision || SendCollisionEventsTo || AlwaysAddCollisionHelpers;
            if (addragd) EnsureCollisionHelpersAdded();

            posingPelvis.rigidbody.isKinematic = true;
            RefreshPelvisGuides();


            if (customRagdollAnimator)
            {
                if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                {
                    SetAnimationRefBones
                (
                FindChildByNameInDepth(Pelvis.name, customRagdollAnimator),
                FindChildByNameInDepth(SpineStart.name, customRagdollAnimator),
                FindChildByNameInDepth(Chest.name, customRagdollAnimator),
                FindChildByNameInDepth(Head.name, customRagdollAnimator),

                FindChildByNameInDepth(LeftUpperArm.name, customRagdollAnimator),
                FindChildByNameInDepth(LeftForeArm.name, customRagdollAnimator),
                FindChildByNameInDepth(RightUpperArm.name, customRagdollAnimator),
                FindChildByNameInDepth(RightForeArm.name, customRagdollAnimator),

                FindChildByNameInDepth(LeftUpperLeg.name, customRagdollAnimator),
                FindChildByNameInDepth(LeftLowerLeg.name, customRagdollAnimator),
                FindChildByNameInDepth(RightUpperLeg.name, customRagdollAnimator),
                FindChildByNameInDepth(RightLowerLeg.name, customRagdollAnimator)
                );
                }
                else
                {
                    UnityEngine.Debug.Log("[Ragdoll Animator] Custom seprated animator is not supported for custom limbs bone setup mode!");
                }
            }

            SwitchAllExtendedAnimatorSync(ExtendedAnimatorSync);

            if (Persistent)
            {
                GameObject.DontDestroyOnLoad(RagdollDummyBase);
            }

            //caller.StartCoroutine(LateFixed());

            RagdollSetup = new List<PosingBone>();
            c = posingPelvis;
            while (c != null)
            {
                RagdollSetup.Add(c);
                c = c.child;
            }

            if (HipsPin) BlendOnCollision = false;
            Initialized = true;

            wasBlendOnCollision = !BlendOnCollision;
            lastFreeFall = !FreeFallRagdoll;
            StoreHipsRootOffset();
        }


        public void SwitchAllExtendedAnimatorSync(ESyncMode mode)
        {
            PosingBone c = posingPelvis;

            while (c != null)
            {
                c.FullAnimatorSync = mode;
                c = c.child;
            }
        }

        public void CaptureAnimation(bool force = false)
        {
            if (Initialized == false) return;

            pelvisAnimatorPosition = posingPelvis.visibleBone.position;
            pelvisAnimatorLocalPosition = posingPelvis.visibleBone.localPosition;
            pelvisAnimatorRotation = posingPelvis.visibleBone.rotation;
            pelvisAnimatorLocalRotation = posingPelvis.visibleBone.localRotation;

            PosingBone c = posingPelvis;

            if (c.ConfigurableJoint) c.CaptureAnimator();

            if ((animator && animator.enabled) || force)
            {
                while (c != null)
                {
                    c.CaptureAnimator();
                    c = c.child;
                }
            }
        }

        Vector3 pelvisAnimatorPosition;

        Vector3 pelvisAnimatorLocalPosition;
        Quaternion pelvisAnimatorRotation;
        Quaternion pelvisAnimatorLocalRotation;

        float spineCulldown = 0f;
        float chestCulldown = 0f;
        float minChestAnim = 0f;
        float sd_minChestAnim = 0f;
        float minSpineAnim = 0f;
        float sd_minSpineAnim = 0f;
        float larmCulldown = 0f;
        float rarmCulldown = 0f;
        float minRArmsAnim = 0f;
        float lminArmsAnim = 0f;
        float sd_lminArmsAnim = 0f;
        float sd_minRArmsAnim = 0f;
        float headCulldown = 0f;
        float minHeadAnim = 0f;
        float sd_minHeadAnim = 0f;


        float fadeOutSpd = 0.4f;
        float fadeInSpd = 0.08f;
        void AnimateMinBlend(ref float val, ref float sd, float target, float tgtMul = 1f)
        {
            val = Mathf.SmoothDamp(val, target * tgtMul * User_BlendMultiplier, ref sd, target == 1f ? fadeInSpd : fadeOutSpd, Mathf.Infinity, delta);
        }

        internal void LateUpdate(bool captureAnimation = true)
        {
            if (Initialized == false) return;

            if (UpdatePhysics)
            {
                if (fixedUpdated == false) return;
                fixedUpdated = false;
            }

            float finalBlend = FinalRagdollBlend;

            #region (commented) Support second solution for animate physics mode -----

            //if (AnimatePhysics == EFixedMode.Late)
            //{
            //    if (!lateFixedIsRunning) { parentCaller.StartCoroutine(LateFixed()); }

            //    if (fixedAllow)
            //    {
            //        fixedAllow = false;
            //        callFixedCalculations = true;
            //    }
            //    //else return;
            //}
            //else if (lateFixedIsRunning) { lateFixedIsRunning = false; }

            #endregion

            if (ReposeMode != EBaseTransformRepose.None) UpdateReposing();

            fadeOutSpd = Mathf.LerpUnclamped(1f, 0.15f, BlendingSpeed);
            fadeInSpd = Mathf.LerpUnclamped(.25f, 0.03f, BlendingSpeed);
            bool chestColl = false; // For humanoid blend on collision

            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                #region Humanoid Blend On Collision Related

                #region Blending ragdoll limbs when collision occurs

                larmCulldown -= delta;
                rarmCulldown -= delta;
                spineCulldown -= delta;
                chestCulldown -= delta;
                headCulldown -= delta;


                if (BlendOnCollision)
                {
                    //if (posingRightUpperArm.Colliding || posingRightForeArm.Colliding) { AnimateMinBlend(ref minRArmsAnim, ref sd_minRArmsAnim, 1f); }
                    //else AnimateMinBlend(ref minRArmsAnim, ref sd_minRArmsAnim, ConstantArmsRagdoll);

                    //if (posingLeftUpperArm.Colliding || posingLeftForeArm.Colliding) { AnimateMinBlend(ref lminArmsAnim, ref sd_lminArmsAnim, 1f); }
                    //else AnimateMinBlend(ref lminArmsAnim, ref sd_lminArmsAnim, ConstantArmsRagdoll);

                    bool occured = false;
                    if (posingRightUpperArm.Colliding || posingRightForeArm.Colliding) { rarmCulldown = delta * 8f; occured = true; }
                    if (posingLeftUpperArm.Colliding || posingLeftForeArm.Colliding) { larmCulldown = delta * 8f; occured = true; }

                    if (SensitiveCollision)
                        if (occured)
                        {
                            chestColl = true;
                            chestCulldown = delta * 3f;
                        }

                    AnimateMinBlend(ref minRArmsAnim, ref sd_minRArmsAnim, rarmCulldown > 0f ? 1f : ConstantArmsRagdoll);
                    AnimateMinBlend(ref lminArmsAnim, ref sd_lminArmsAnim, larmCulldown > 0f ? 1f : ConstantArmsRagdoll);
                }
                else
                {
                    lminArmsAnim = ConstantArmsRagdoll;
                    minRArmsAnim = ConstantArmsRagdoll;
                    sd_lminArmsAnim = 0f;
                    sd_minRArmsAnim = 0f;
                }

                posingLeftUpperArm.internalRagdollBlend = lminArmsAnim;
                posingLeftForeArm.internalRagdollBlend = lminArmsAnim;
                posingRightUpperArm.internalRagdollBlend = minRArmsAnim;
                posingRightForeArm.internalRagdollBlend = minRArmsAnim;

                if (BlendOnCollision)
                {
                    if (posingHead.Colliding)
                    {
                        headCulldown = delta * 9f;

                        bool self = false;
                        //if (IgnoreSelfCollision) self = posingHead.CollidingOnlyWithSelf;

                        if (!self) chestColl = true;
                    }

                    AnimateMinBlend(ref minHeadAnim, ref sd_minHeadAnim, headCulldown > 0f ? 1f : ConstantHeadRagdoll);
                }
                else
                {
                    minHeadAnim = ConstantHeadRagdoll;
                    sd_minHeadAnim = 0f;
                }

                posingHead.internalRagdollBlend = minHeadAnim;

                if (BlendOnCollision)
                {

                    float spineMul = 1f;

                    bool coll = false;
                    if (posingSpineStart.Colliding) coll = true;
                    else if (posingChest != null) if (posingChest.Colliding) coll = true;

                    //if (coll)
                    //    if (IgnoreSelfCollision)
                    //    {
                    //        bool self = false;
                    //        if (posingChest.Colliding) if (posingChest.CollidingOnlyWithSelf) self = true;
                    //        if (!self) if (posingSpineStart.Colliding) if (posingSpineStart.CollidingOnlyWithSelf) self = true;
                    //        if (self) coll = false;
                    //    }

                    if (coll)
                    {
                        spineCulldown = delta * 10f;
                        chestCulldown = delta * 10f;
                    }
                    else
                    {
                        if (chestColl)
                        {
                            spineCulldown = delta * 6f;
                            chestCulldown = delta * 6f;
                            spineMul = 0.4f;
                            //AnimateMinBlend(ref minSpineAnim, ref sd_minSpineAnim, .4f);
                            //AnimateMinBlend(ref minChestAnim, ref sd_minChestAnim, 1f);
                        }
                        else
                        {
                            //spineCulldown = delta * 10f;
                            //chestCulldown = delta * 10f;
                        }
                    }

                    AnimateMinBlend(ref minSpineAnim, ref sd_minSpineAnim, spineCulldown > 0f ? 1f : ConstantSpineRagdoll, spineMul);
                    AnimateMinBlend(ref minChestAnim, ref sd_minChestAnim, chestCulldown > 0f ? 1f : ConstantSpineRagdoll);

                }
                else
                {
                    minSpineAnim = ConstantSpineRagdoll;
                    minChestAnim = ConstantSpineRagdoll;
                    sd_minSpineAnim = 0f;
                    sd_minChestAnim = 0f;
                }

                posingSpineStart.internalRagdollBlend = minSpineAnim;
                if (posingPelvis.ConfigurableJoint) posingPelvis.internalRagdollBlend = minSpineAnim;
                if (posingChest != null) posingChest.internalRagdollBlend = minChestAnim;

                #endregion

                #endregion
            }
            else
            {
                if (BlendOnCollision)
                {
                    bool anyTotalCollision = false;
                    minSpineAnim = 0f;

                    for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                    {
                        var chain = CustomLimbsBonesChains[i];

                        if (chain.BlendOnCollisions)
                        {
                            chain.blendOnCollisionCulldown -= delta;

                            bool anyCollides = false;

                            for (int b = 0; b < chain.BoneSetups.Count; b++)
                            {
                                var bone = chain.BoneSetups[b];
                                if (bone.Posing.Colliding) { anyCollides = true; break; }
                            }

                            if (SensitiveCollision) if (wasSensitiveColliding)
                                {
                                    chain.blendOnCollisionCulldown = 2f;
                                }

                            if (anyCollides)
                            {
                                chain.blendOnCollisionCulldown = 2f;
                                anyTotalCollision = true;
                            }

                            AnimateMinBlend(ref chain.blendOnCollisionMin, ref chain.sd_blendVelo, chain.blendOnCollisionCulldown > 0f ? 1f : chain.ConstantRagdoll);

                            for (int b = 0; b < chain.BoneSetups.Count; b++)
                            {
                                var bone = chain.BoneSetups[b];
                                bone.Posing.internalRagdollBlend = chain.blendOnCollisionMin;
                                if (chain.blendOnCollisionMin > minSpineAnim) minSpineAnim = chain.blendOnCollisionMin;
                            }
                        }


                        if (SensitiveCollision)
                        {
                            wasSensitiveColliding = anyTotalCollision;
                        }

                    }

                }
                else
                {

                    if (wasBlendOnCollision != BlendOnCollision)
                    {
                        wasBlendOnCollision = BlendOnCollision;
                        wasSensitiveColliding = false;

                        #region Refresh blend on switching

                        for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                        {
                            var chain = CustomLimbsBonesChains[i];
                            if (chain.BlendOnCollisions)
                            {
                                chain.blendOnCollisionCulldown -= delta;

                                for (int b = 0; b < chain.BoneSetups.Count; b++)
                                {
                                    var bone = chain.BoneSetups[b];
                                    bone.Posing.internalRagdollBlend = 0f;
                                }
                            }
                        }

                        #endregion

                    }

                }
            }


            if (captureAnimation) CaptureAnimation();


            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                // Animating not jointed ragdoll bones to keep model in sync
                if (animator)
                    for (int i = 0; i < toReanimateBones.Count; i++)
                    {
                        toReanimateBones[i].SyncRagdollBone(finalBlend, animator.enabled);
                    }

                #region Hard sync on collision


                if (finalBlend > 0f || minRArmsAnim > 0.01f || lminArmsAnim > 0.01f)
                {
                    float blend = finalBlend;
                    if (minRArmsAnim > 0.01f) blend = Mathf.Max(finalBlend, minRArmsAnim);
                    else if (lminArmsAnim > 0.01f) blend = Mathf.Max(finalBlend, lminArmsAnim);

                    // Blending shoulders and not ragdolled spine bones
                    //if (posingLeftUpperArm.parentFixer == null || posingLeftUpperArm.parentFixer.wasSyncing == false)
                    posingLeftUpperArm.visibleBone.parent.localRotation = Quaternion.LerpUnclamped(posingLeftUpperArm.visibleBone.parent.localRotation, posingLeftUpperArm.transform.parent.localRotation, blend);

                    //if (posingRightUpperArm.parentFixer == null || posingRightUpperArm.parentFixer.wasSyncing == false)
                    posingRightUpperArm.visibleBone.parent.localRotation = Quaternion.LerpUnclamped(posingRightUpperArm.visibleBone.parent.localRotation, posingRightUpperArm.transform.parent.localRotation, blend);
                }

                if (finalBlend > 0f || chestColl || minChestAnim > 0.01f)
                {
                    float blend = finalBlend;
                    if (chestColl || minChestAnim > 0.01f) blend = Mathf.Max(finalBlend, minChestAnim);

                    // Blending not ragdolled spine bones
                    if (posingChest != null) if (posingChest.visibleBone) posingChest.visibleBone.GetChild(0).localRotation = Quaternion.LerpUnclamped(posingChest.visibleBone.GetChild(0).localRotation, posingChest.transform.GetChild(0).localRotation, blend);
                    if (posingChest != null) if (posingChest.visibleBone) posingChest.visibleBone.parent.localRotation = Quaternion.LerpUnclamped(posingChest.visibleBone.parent.localRotation, posingChest.transform.parent.localRotation, blend);
                }

                if (finalBlend > 0f || minRArmsAnim > 0.01f || lminArmsAnim > 0.01f)
                {
                    float blend = finalBlend;
                    if (minRArmsAnim > 0.01f) blend = Mathf.Max(finalBlend, minRArmsAnim);
                    else if (lminArmsAnim > 0.01f) blend = Mathf.Max(finalBlend, lminArmsAnim);

                    // Blending shoulders and not ragdolled spine bones
                    if (posingLeftUpperArm.parentFixer != null)
                        if (posingLeftUpperArm.parentFixer.wasSyncing == false)
                        {
                            posingLeftUpperArm.visibleBone.parent.localRotation = Quaternion.LerpUnclamped(posingLeftUpperArm.visibleBone.parent.localRotation, posingLeftUpperArm.transform.parent.localRotation, blend);
                        }

                    if (posingRightUpperArm.parentFixer != null)
                        if (posingRightUpperArm.parentFixer.wasSyncing == false)
                        {
                            posingRightUpperArm.visibleBone.parent.localRotation = Quaternion.LerpUnclamped(posingRightUpperArm.visibleBone.parent.localRotation, posingRightUpperArm.transform.parent.localRotation, blend);
                        }
                }

                if (finalBlend > 0f || chestColl || minChestAnim > 0.01f)
                {
                    float blend = finalBlend;
                    if (chestColl || minChestAnim > 0.01f) blend = Mathf.Max(finalBlend, minChestAnim);

                    // Blending ragdolled spine bones on collision
                    if (posingSpineStart.transform) posingSpineStart.SyncAnimatorToRagdoll(blend);
                    if (posingChest != null) if (posingChest.transform) posingChest.SyncAnimatorToRagdoll(blend);
                }

                #endregion

            }



            #region Freefall switch

            if (lastFreeFall != FreeFallRagdoll)
            {
                lastFreeFall = FreeFallRagdoll;
                PosingBone c = posingPelvis;

                if (FreeFallRagdoll) // Switched to free fall
                {
                    _transitionNonFreeFall_OverrideMaxBlend = 0f;
                    _User_GetUpResetProbe();
                    LastestFreeFallStartTime = Time.time;

                    while (c != null)
                    {
                        c.rigidbody.mass = c.targetMass * MassMultiplier;
                        c = c.child;
                    }
                }
                else
                {
                    while (c != null)
                    {
                        c.rigidbody.mass = c.targetMass * 0.4f * MassMultiplier;
                        c = c.child;
                    }
                }


                if (RemoveLimitsWhenNonFreeFall)
                {
                    if (!FreeFallRagdoll)
                    {
                        foreach (var item in RagdollLimbs)
                        {
                            if (item == null) continue;
                            if (item.transform.parent == null) continue;
                            if (item.transform == posingPelvis.transform) continue;
                            var j = item.GetComponent<ConfigurableJoint>();
                            j.angularXMotion = ConfigurableJointMotion.Free;
                            j.angularYMotion = ConfigurableJointMotion.Free;
                            j.angularZMotion = ConfigurableJointMotion.Free;
                        }
                    }
                    else
                    {
                        foreach (var item in RagdollLimbs)
                        {
                            if (item == null) continue;
                            if (item.transform.parent == null) continue;
                            if (item.transform == posingPelvis.transform) continue;
                            var j = item.GetComponent<ConfigurableJoint>();
                            j.angularXMotion = ConfigurableJointMotion.Limited;
                            j.angularYMotion = ConfigurableJointMotion.Limited;
                            j.angularZMotion = ConfigurableJointMotion.Limited;
                        }
                    }
                }

            }

            #endregion



            UpdateBonesToRagdollPose();


            //if ((HipsPin == false && FreeFallRagdoll == false) || _transitionNonFreeFall_OverrideMaxBlend > 0f) ReposeHipsCalculations();

        }


        public void UpdateBonesToRagdollPose()
        {
            PosingBone c = posingPelvis;
            float finalBlend = FinalRagdollBlend;

            c.visibleBone.transform.position = Vector3.LerpUnclamped(c.visibleBone.transform.position, c.transform.position, finalBlend * c.user_internalRagdollBlend);

            while (c != null)
            {
                float blend = finalBlend * c.user_internalRagdollBlend;
                if (blend < c.internalRagdollBlend) blend = c.internalRagdollBlend;

                if (c == posingPelvis)
                {
                    QLerp(c.visibleBone, c.transform.rotation, blend);
                    //c.visibleBone.rotation = Quaternion.LerpUnclamped(c.visibleBone.rotation, c.transform.rotation, blend); 
                }
                else if (c.visibleBone)
                {
                    if (c.BrokenJoint)
                    {
                        if (blend >= 1f)
                        {
                            c.visibleBone.position = c.transform.position;
                            c.visibleBone.rotation = c.transform.rotation;
                        }
                        else
                        {
                            c.visibleBone.position = Vector3.Lerp(c.visibleBone.transform.position, c.transform.position, blend);
                            c.visibleBone.rotation = Quaternion.Lerp(c.visibleBone.transform.rotation, c.transform.rotation, blend);
                        }
                    }
                    else
                    {
                        if (c.DetachParent)
                        {
                            c.visibleBone.localRotation = FEngineering.QToLocal(c.DetachParent.rotation, c.transform.rotation);
                            //QLerp(c.visibleBone, c.transform.rotation, blend);
                        }
                        else
                            QLerpLocal(c.visibleBone, c.transform.localRotation, blend);
                    }
                }

                c = c.child;
            }
        }

        void QLerp(Transform a, Quaternion b, float blend)
        {
            if (blend <= 0f) return;
            if (blend >= 1f) { a.rotation = b; return; }
            a.rotation = Quaternion.Lerp(a.rotation, b, blend);
        }

        void QLerpLocal(Transform a, Quaternion b, float blend)
        {
            if (blend <= 0f) return;
            if (blend >= 1f) { a.localRotation = b; return; }
            a.localRotation = Quaternion.Lerp(a.localRotation, b, blend);
        }

        /// <summary> Last conditional hips blend (mostly internalBoneBlend * RagdollBlend + override by blend on collision) </summary>
        float latestHipsBlend = 1f;
        //bool latestHipsWasZero = true;

        /// <summary> Raising from zero to 1 when transitioning get up, after reaching 1 it goes instantly to zero (variable off state) </summary>
        float _transitionNonFreeFall_OverrideMaxBlend = 0f;
        void ReposeHipsCalculations()
        {
            if (posingPelvis.rigidbody.isKinematic || _transitionNonFreeFall_OverrideMaxBlend > 0f)
            {
                float blend = latestHipsBlend;
                if (containerForDummy == BaseTransform) blend = 1f;

                if (_transitionNonFreeFall_OverrideMaxBlend > 0f)
                    blend = _transitionNonFreeFall_OverrideMaxBlend * blend;

                if (blend < 0.0001f)
                {
                    if (!FreeFallRagdoll)
                    {
                        if (TryAnimatePelvis == EAnimatePelvis.None)
                        {
                            posingPelvis.transform.localPosition = posingPelvis.initialLocalPosition;
                            posingPelvis.transform.localRotation = posingPelvis.initialLocalRotation;
                        }
                        else
                        {
                            posingPelvis.transform.position = posingPelvis.AnimatorBone.position;
                            posingPelvis.transform.rotation = posingPelvis.AnimatorBone.rotation;
                        }
                    }
                }
                //if (_transitionNonFreeFall_OverrideMaxBlend > 0f)
                //{
                //    if (latestHipsWasZero)
                //    {
                //        posingPelvis.transform.position = posingPelvis.AnimatorBone.position;
                //        posingPelvis.transform.rotation = posingPelvis.AnimatorBone.rotation;
                //        latestHipsWasZero = false;
                //    }
                //}
                //else
                //{
                //    latestHipsWasZero = true;
                //}

                if (FixRootInPelvis)
                {
                    if (TryAnimatePelvis != EAnimatePelvis.None)
                    {
                        if (blend < .9999f)
                        {
                            posingPelvis.transform.position = Vector3.LerpUnclamped(posingPelvis.transform.position, pelvisAnimatorPosition, blend);
                            posingPelvis.transform.rotation = Quaternion.SlerpUnclamped(posingPelvis.transform.rotation, pelvisAnimatorRotation, blend);
                        }
                        else // blend == 1f
                        {
                            posingPelvis.transform.position = pelvisAnimatorPosition;
                            posingPelvis.transform.rotation = pelvisAnimatorRotation;
                        }

                        //posingPelvis.transform.localPosition = posingPelvis.transform.parent.InverseTransformPoint(pelvisAnimatorPosition);
                        //posingPelvis.transform.localRotation = pelvisAnimatorLocalRotation;
                    }
                    else
                    {
                        if (blend < .9999f)
                        {
                            posingPelvis.transform.localPosition = Vector3.LerpUnclamped(posingPelvis.transform.localPosition, posingPelvis.initialLocalPosition, blend);
                            posingPelvis.transform.localRotation = Quaternion.LerpUnclamped(posingPelvis.transform.localRotation, (posingPelvis.initialLocalRotation), blend);
                        }
                        else // blend == 1f
                        {
                            posingPelvis.transform.localPosition = posingPelvis.initialLocalPosition;
                            posingPelvis.transform.localRotation = posingPelvis.initialLocalRotation;
                        }
                    }
                }
                else
                {
                    if (TryAnimatePelvis != EAnimatePelvis.None)
                    {
                        if (blend < .9999f)
                        {
                            posingPelvis.transform.position = Vector3.LerpUnclamped(posingPelvis.transform.position, pelvisAnimatorPosition, blend);
                            posingPelvis.transform.rotation = Quaternion.LerpUnclamped(posingPelvis.transform.rotation, pelvisAnimatorRotation, blend);
                        }
                        else // blend == 1f
                        {
                            posingPelvis.transform.position = pelvisAnimatorPosition;
                            posingPelvis.transform.rotation = pelvisAnimatorRotation;
                        }

                        //posingPelvis.transform.localPosition = pelvisAnimatorLocalPosition;
                        //posingPelvis.transform.localRotation = pelvisAnimatorLocalRotation;
                    }
                    else
                    {
                        if (blend < .9999f)
                        {
                            posingPelvis.transform.localPosition = Vector3.LerpUnclamped(posingPelvis.transform.localPosition, posingPelvis.initialLocalPosition, blend);
                            posingPelvis.transform.localRotation = Quaternion.LerpUnclamped(posingPelvis.transform.localRotation, posingPelvis.initialLocalRotation, blend);
                        }
                        else // blend == 1f
                        {
                            posingPelvis.transform.localPosition = posingPelvis.initialLocalPosition;
                            posingPelvis.transform.localRotation = posingPelvis.initialLocalRotation;
                        }

                        //posingPelvis.transform.localPosition = Vector3.LerpUnclamped(posingPelvis.transform.localPosition, pelvisAnimatorLocalPosition, blend);
                        //posingPelvis.transform.localRotation = Quaternion.LerpUnclamped(posingPelvis.transform.localRotation, pelvisAnimatorLocalRotation, blend);
                    }
                }
            }
            else
            {
                //posingPelvis.rigidbody.position = Vector3.LerpUnclamped(posingPelvis.rigidbody.position, pelvisAnimatorPosition, blend);
                //posingPelvis.rigidbody.rotation = Quaternion.LerpUnclamped(posingPelvis.rigidbody.rotation, pelvisAnimatorRotation, blend);
            }
        }


        bool UpdatePhysics { get { return AnimatePhysics; } }//{ if (animator == null) return false; if (mecanim) return mecanim.updateMode == AnimatorUpdateMode.AnimatePhysics; else if (legacyAnim) return legacyAnim.animatePhysics; return false; } }
        internal void Update()
        {
            if (Initialized == false) return;
            if (UpdatePhysics) return;

            delta = Time.deltaTime;
            Calibration();
        }

        private void Calibration()
        {
            if (!Calibrate) return;

            PosingBone c = posingPelvis;

            if (animator)
                if (animator.enabled)
                {
                    while (c != null)
                    {
                        if (c.visibleBone) c.visibleBone.localRotation = c.initialLocalRotation;
                        c = c.child;
                    }
                }
        }

        bool fixedUpdated = false;
        bool lastFreeFall = true;
        int fixedFrames = 0;
        float delta = 0.1f;
        internal void FixedUpdate() // Sync with physics --------------------------------------
        {
            if (Initialized == false) return;

            float finalBlend = FinalRagdollBlend;
            OutConfigurableSpring = ConfigurableSpring;
            fixedUpdated = true;
            if (UpdatePhysics) delta = Time.fixedDeltaTime;

            if (TryAnimatePelvis == EAnimatePelvis.V2)
            {
                if (!FreeFallRagdoll) OutConfigurableSpring *= 2f;
            }

            if (AnimatePhysics) Calibration();

            if (StartAfterTPose)
            {
                if (fixedFrames < 4)
                {
                    fixedFrames += 1;
                    if (fixedFrames == 4) CaptureAnimation();
                    return;
                }
            }
            else
            {
                if (fixedFrames < 1)
                {
                    fixedFrames += 1;
                    if (fixedFrames == 1) CaptureAnimation();
                    return;
                }
            }

            if (FreeFallRagdoll) // Fall on floor
            {
                if (posingPelvis.rigidbody.isKinematic == true) posingPelvis.rigidbody.isKinematic = false;
            }
            else // Nonfreefall - connected with animator
            {
                if (posingPelvis.rigidbody.isKinematic == false) posingPelvis.rigidbody.isKinematic = true;
                RagdollDummyBase.position = ObjectWithAnimator.transform.position;
                RagdollDummyBase.rotation = ObjectWithAnimator.transform.rotation;
            }

            PosingBone c = posingPelvis;
            if (TryAnimatePelvis == EAnimatePelvis.V2) latestHipsBlend = 1f;
            else
            {
                latestHipsBlend = (posingPelvis.user_internalRagdollBlend * finalBlend);
                if (BlendOnCollision) latestHipsBlend = Mathf.Max(latestHipsBlend, minSpineAnim);
            }
            //ReposeHipsCalculations();

            #region Limbs individual force update

            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                posingHead.internalForceMultiplier = HeadForce;
                posingSpineStart.internalForceMultiplier = SpineForce;
                if (posingChest != null) posingChest.internalForceMultiplier = SpineForce;

                posingRightUpperArm.internalForceMultiplier = RightArmForce;
                posingRightForeArm.internalForceMultiplier = RightArmForce;

                posingLeftUpperArm.internalForceMultiplier = LeftArmForce;
                posingLeftForeArm.internalForceMultiplier = LeftArmForce;

                posingLeftUpperLeg.internalForceMultiplier = LeftLegForce;
                posingLeftLowerLeg.internalForceMultiplier = LeftLegForce;

                posingRightUpperLeg.internalForceMultiplier = RightLegForce;
                posingRightLowerLeg.internalForceMultiplier = RightLegForce;
            }
            else if (BonesSetupMode == EBonesSetupMode.CustomLimbs)
            {
                for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                {
                    var chain = CustomLimbsBonesChains[i];
                    for (int b = 0; b < chain.BoneSetups.Count; b++)
                    {
                        var bone = chain.BoneSetups[b];
                        bone.Posing.internalForceMultiplier = chain.MusclesForce * bone.MuscleForceMultiplier;
                    }
                }
            }

            #endregion

            c.PositionAlign = false;

            if (!HipsPin)
            {
                if (!FreeFallRagdoll)
                {
                    posingPelvis.rigidbody.useGravity = false;
                    if (posingPelvis.rigidbody.isKinematic == false) posingPelvis.rigidbody.isKinematic = true;
                }
            }
            else // Hips pin
            {
                if (c.ConfigurableJoint && HipsPin)
                {

                    if (FreeFallRagdoll == false)
                    {
                        posingPelvis.rigidbody.useGravity = false;
                        if (posingPelvis.rigidbody.isKinematic == true) posingPelvis.rigidbody.isKinematic = false;
                        c.PositionAlign = HipsPin;
                        c.FixedUpdate();
                    }
                    else
                    {
                        var rootConf = c.ConfigurableJoint.xDrive;
                        rootConf.positionSpring = HipsPinSpring;
                        c.ConfigurableJoint.xDrive = rootConf;

                        if (c.ConfigurableJoint.xDrive.positionSpring > 0f)
                        {
                            var pdr = c.ConfigurableJoint.xDrive;
                            pdr.positionSpring = 0;
                            pdr.positionDamper = 0;
                            c.ConfigurableJoint.xDrive = pdr;
                            c.ConfigurableJoint.yDrive = pdr;
                            c.ConfigurableJoint.zDrive = pdr;
                            c.ConfigurableJoint.angularXDrive = pdr;
                            c.ConfigurableJoint.angularYZDrive = pdr;
                        }

                        posingPelvis.rigidbody.useGravity = true;
                        if (posingPelvis.rigidbody.isKinematic == true) posingPelvis.rigidbody.isKinematic = false;
                    }
                }
            }

            ReposeHipsCalculations();

            c = c.child;
            while (c != null)
            {
                c.FixedUpdate();
                c = c.child;
            }

        }



        public Vector3 User_GetRagdollBonesBoundsCenterBottom(bool fast = true)
        {
            Bounds b = User_GetRagdollBonesStateBounds(fast);
            Vector3 bott = b.center;
            bott.y = b.min.y;
            return bott;
        }


        void UpdateReposing()
        {
            if (FreeFallRagdoll == false) return;

            float finalBlend = FinalRagdollBlend;
            if (ReposeOnlyOnFullRagdoll) if (finalBlend < 0.99f) return;

            Vector3 targetPosition = BaseTransform.position;

            if (ReposeMode == EBaseTransformRepose.BonesBoundsBottomCenter)
            {
                targetPosition = User_GetRagdollBonesBoundsCenterBottom();
                if (finalBlend < 0.9f) targetPosition = Vector3.LerpUnclamped(GetStoredHipsRootOffset(), targetPosition, finalBlend);
            }
            else if (ReposeMode == EBaseTransformRepose.HipsToFootPosition)
            {
                targetPosition = GetStoredHipsRootOffset();
                targetPosition.y = User_GetRagdollBonesStateBounds().min.y;
            }

            if (finalBlend < 0.9f)
            {
                targetPosition = Vector3.LerpUnclamped(BaseTransform.position, targetPosition, finalBlend);
            }

            BaseTransform.position = targetPosition;
        }



        private Vector3 hipsToRootLocal = Vector3.zero;
        private Quaternion hipsToRootLocalRot = Quaternion.identity;
        public Vector3 GetStoredHipsRootOffset()
        {
            if (hipsToRootLocal == Vector3.zero) return BaseTransform.position;
            return GetRagdolledPelvis().transform.TransformPoint(hipsToRootLocal);
        }

        public Quaternion GetStoredHipsRootOffsetRot()
        {
            if (hipsToRootLocalRot == Quaternion.identity) return BaseTransform.rotation;
            return FEngineering.QToWorld(GetRagdolledPelvis().transform.rotation, hipsToRootLocalRot);
        }

        public void StoreHipsRootOffset()
        {
            hipsToRootLocal = GetAnimatorPelvis().transform.InverseTransformPoint(BaseTransform.position);
            hipsToRootLocalRot = FEngineering.QToLocal(GetAnimatorPelvis().transform.rotation, BaseTransform.rotation);
        }

        public void SwitchFreeFallRagdoll(bool freeFall, float delay = 0f)
        {
            if (freeFall == FreeFallRagdoll) return;

            if (freeFall == true)
            {
                _transitionNonFreeFall_OverrideMaxBlend = 0f;
                ResetLatestFreeFallStartTime();
                StoreHipsRootOffset();
            }

            FreeFallRagdoll = freeFall;
        }

        public void ResetLatestFreeFallStartTime()
        {
            LastestFreeFallStartTime = Time.time;
        }
    }
}