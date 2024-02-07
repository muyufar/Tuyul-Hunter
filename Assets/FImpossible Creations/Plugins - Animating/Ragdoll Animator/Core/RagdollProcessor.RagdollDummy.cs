using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollProcessor
    {
        internal Behaviour animator = null;
        internal Animator mecanim = null;
        internal Animation legacyAnim = null;

        [HideInInspector] public Transform ObjectWithAnimator = null;
        /// <summary> Bottom most ragdoll dummy container </summary>
        /*[HideInInspector]*/
        public Transform RagdollDummyBase = null;
        /// <summary> Ragdoll dummy with origin in foot point, it's child od dummy base </summary>
        [HideInInspector] public Transform RagdollDummyRoot = null;
        //internal Transform RagdollDummyRootParent;
        /// <summary> First ragdoll dummy skeleton bone generated out of original skeleton </summary>
        [HideInInspector] public Transform RagdollDummySkeleton = null;
        [HideInInspector] public Transform RootInParent = null;
        [HideInInspector] public Transform PelvisInDummy = null;
        [HideInInspector] public Transform RagdollDummyAnimator = null;
        [SerializeField][HideInInspector] private string _helpChestName = "";

        /// <summary> Component types you want to be preset on the radoll dummy with colliders 
        /// Component which are not on this list, will remain on the non-ragdolled skeleton </summary>
        public List<System.Type> moveCompsToDummy = new List<System.Type>();

        private void PrepareRagdollDummy(Transform objectWithAnimator, Transform rootBone)
        {
            //Vector3 startScale = objectWithAnimator.localScale;
            //moveCompsToDummy.Add(typeof(CustomBoneComp));

            ObjectWithAnimator = objectWithAnimator;
            animator = objectWithAnimator.GetComponent<Animator>();

            if (animator == null)
            {
                animator = objectWithAnimator.GetComponent<Animation>();
            }

            if (animator != null)
            {
                if (animator is Animator) mecanim = animator as Animator;
                else if (animator is Animation) legacyAnim = animator as Animation;
            }

            // allow removing when it is dummy generated runtime or right now it is pre generating dummy
            bool allowRemoving = true;

            if (RagdollDummyBase) // not allowing removing only for generating
#if UNITY_EDITOR
                if (Application.isPlaying) // pre generating only in edit mode 
#endif
                    allowRemoving = false;


            RagdollDummyAnimator = objectWithAnimator;

            if (RagdollDummyBase == null)
            {
                GameObject ragdollReference = new GameObject(objectWithAnimator.name + "-Ragdoll-SkeletonOrigin");
                GameObject ragdollBase = new GameObject(objectWithAnimator.name + "-Ragdoll");
                RagdollDummyBase = ragdollBase.transform;
                RagdollDummyBase.localScale = ObjectWithAnimator.lossyScale;
                if (IsPreGeneratedDummy) RagdollDummyBase.SetParent(ObjectWithAnimator, true);
                RagdollDummyBase.position = ObjectWithAnimator.position;
                RagdollDummyBase.rotation = ObjectWithAnimator.rotation;

                ragdollReference.transform.SetParent(RagdollDummyBase, true);
                ragdollReference.transform.position = objectWithAnimator.position;
                ragdollReference.transform.rotation = objectWithAnimator.rotation;
                ragdollReference.transform.localScale = Vector3.one;

                if (IsPreGeneratedDummy == false)
                {
                    if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                    {
                        if (SpineStart != null)
                            if (SpineStart.GetComponentInChildren<ConfigurableJoint>() == null)
                            {
                                if (SpineStart.GetComponentInChildren<CharacterJoint>() == null)
                                {
                                    var ragdollGen = new RagdollGeneratorPlayer();
                                    ragdollGen.SetAllBoneReferences(this);
                                    ragdollGen.UpdateOrGenerateRagdoll(this, GeneratorParameters, false, 0f, 0f, 0f, false, 90f, 0.05f, false, false);
                                }
                            }
                    }
                    else if (BonesSetupMode == EBonesSetupMode.CustomLimbs)
                    {
                        if (Pelvis.GetComponent<Rigidbody>() == null && Pelvis.GetComponent<Rigidbody>() == null)
                            if (CustomLimbsBonesChains[0].BoneSetups[0].t.GetComponent<Rigidbody>() == null)
                            {
                                var ragdollGen = new RagdollGeneratorPlayer();
                                ragdollGen.SetAllBoneReferences(this);
                                ragdollGen.UpdateOrGenerateRagdoll(this, GeneratorParameters, false, 0f, 0f, 0f, false, 90f, 0.05f, false, false);
                            }
                    }
                }

                if (allowRemoving)
                    foreach (Transform t in objectWithAnimator.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.GetComponent<ConfigurableJoint>())
                        { CharacterJoint cj = t.GetComponent<CharacterJoint>(); if (cj) { DestroyObj(cj); } }
                    }

                RagdollDummyRoot = ragdollReference.transform;

                Transform rootSkelBone = rootBone;

                if (rootSkelBone == null)
                {
                    // Get main skinned mesh
                    SkinnedMeshRenderer[] meshes = ObjectWithAnimator.GetComponentsInChildren<SkinnedMeshRenderer>();

                    if (meshes.Length == 0)
                    {
                        if (ObjectWithAnimator.childCount > 0)
                            for (int i = 0; i < ObjectWithAnimator.childCount; i++)
                            {
                                meshes = ObjectWithAnimator.GetChild(i).GetComponentsInChildren<SkinnedMeshRenderer>();
                                if (meshes.Length > 0) break;
                            }
                    }

                    if (meshes.Length == 0)
                    {
                        UnityEngine.Debug.Log("[Ragdoll Animator] NOT FOUND SKINNED MESH RENDERERS IN TARGET MODEL! Skinned meshes are required by the component!");
                        UnityEngine.Debug.LogError("[Ragdoll Animator] NOT FOUND SKINNED MESH RENDERERS IN TARGET MODEL! Skinned meshes are required by the component!");
                        return;
                    }

                    SkinnedMeshRenderer mesh = meshes[0]; // Mesh with root nearest to animator
                    int mainC = int.MaxValue;
                    for (int m = 0; m < meshes.Length; m++)
                    {
                        if (meshes[m].bones.Length > mainC)
                        {
                            mesh = meshes[m];
                            mainC = meshes[m].bones.Length;
                        }
                    }

                    rootSkelBone = mesh.rootBone;


                    // Check if root is parent of some ragdoll bone
                    if (SpineStart)
                    {
                        if (!AnimationTools.SkeletonRecognize.IsChildOf(SpineStart, rootSkelBone))
                        {
                            Transform testGet = SpineStart;

                            while (testGet.parent != null && testGet.parent != BaseTransform)
                            {
                                testGet = testGet.parent;
                            }

                            string nme = "NOT FOUND! ";
                            if (testGet) nme = testGet.name;

                            //if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                            UnityEngine.Debug.Log("[Ragdoll Animator] <" + BaseTransform.name + "> : Root bone (" + rootSkelBone + ") is not parent of skeleton bones! Please try adjusting it manually. Automatically choosed '" + nme + "' bone by guessing.");

                            if (testGet) rootSkelBone = testGet;
                        }
                    }

                }

                if (rootSkelBone == null)
                {
                    UnityEngine.Debug.Log("[Ragdoll Animator] Root bone not found! Try assigning 'Root Bone' field manually (first skeleton bone)");
                }

                Transform rootInParent = FindChildByNameInDepth(rootSkelBone.name, objectWithAnimator, true);

                if (rootInParent == BaseTransform)
                {
                    if (rootBone)
                        rootInParent = rootBone;
                    else
                    {
                        UnityEngine.Debug.Log("[Ragdoll Animator] There will be probably something wrong with the setup. Try assigning 'Root Bone' field manually (first skeleton bone)");
                    }
                }




                //if (ragdollReference)
                //    if (rootInParent.parent)
                if (rootInParent.parent != objectWithAnimator)
                {
                    ragdollReference.transform.rotation = rootInParent.parent.rotation;
                }

                // Removing ragdoll components from source skeleton
                //foreach (Transform t in rootInParent.GetComponentsInChildren<Transform>(true))
                //{
                //    Collider cl = t.GetComponent<Collider>();
                //    if (cl) cl.enabled = false;
                //}

                GameObject skeleton = GameObject.Instantiate(rootInParent.gameObject, RagdollDummyBase);
                RootInParent = rootInParent;
                //posingRootSkelBone = new PosingBone(skeleton.transform, this, false);

                RagdollDummySkeleton = skeleton.transform;
                skeleton.name = rootInParent.name;
                //skeleton.transform.localScale = rootInParent.transform.lossyScale;
                skeleton.transform.SetParent(ragdollReference.transform, true);
                skeleton.transform.position = rootInParent.transform.position;
                skeleton.transform.rotation = rootInParent.transform.rotation;
                skeleton.transform.localScale = rootInParent.transform.localScale;
                Transform sTransform = skeleton.transform;

                if (allowRemoving)
                    foreach (Transform t in skeleton.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.GetComponent<ConfigurableJoint>())
                        { CharacterJoint cj = t.GetComponent<CharacterJoint>(); if (cj) { DestroyObj(cj); } }
                    }

                PelvisInDummy = FindChildByNameInDepth(Pelvis.name, sTransform);

                if (PelvisInDummy == null)
                {
                    if (animator is Animator)
                    {
                        Animator anim = animator as Animator;
                        if (anim.isHuman) PelvisInDummy = anim.GetBoneTransform(HumanBodyBones.Hips);
                    }

                    if (PelvisInDummy == null)
                    {
                        if (Pelvis)
                            if (Pelvis.parent)
                            {
                                Transform chSearch = Pelvis;
                                while (chSearch.parent != null && chSearch.parent != BaseTransform)
                                {
                                    chSearch = chSearch.parent;
                                }

                                if (chSearch != null)
                                {
                                    if (chSearch.parent != null)
                                    {
                                        if (chSearch.parent == BaseTransform)
                                        {
                                            PelvisInDummy = chSearch;
                                            UnityEngine.Debug.LogWarning("[Ragdoll Animator] There may be something wrong with the setup! You an also try assigning 'Root Bone' field manually (first bone of the skeleton)");
                                        }
                                    }

                                }
                            }
                    }

                    if (PelvisInDummy == null)
                    {
                        UnityEngine.Debug.Log("[Ragdoll Animator] Pelvis Bone not found! Probably some bone object naming issue. You an also try assigning 'Root Bone' field manually (first bone of the skeleton)");
                    }
                }

                Rigidbody rootRig = PelvisInDummy.transform.parent.gameObject.GetComponent<Rigidbody>();
                if (!rootRig) rootRig = PelvisInDummy.transform.parent.gameObject.AddComponent<Rigidbody>();
                rootRig.isKinematic = true;
                Rigidbody hipsRig = PelvisInDummy.GetComponent<Rigidbody>();
                if (hipsRig) rootRig.interpolation = hipsRig.interpolation;


                ConfigurableJoint pelvConf = PelvisInDummy.GetComponent<ConfigurableJoint>();

                if (!pelvConf)
                    if (HipsPin)
                    {
                        pelvConf = PelvisInDummy.gameObject.AddComponent<ConfigurableJoint>();
                        pelvConf.angularXMotion = ConfigurableJointMotion.Free;
                        pelvConf.angularYMotion = ConfigurableJointMotion.Free;
                        pelvConf.angularZMotion = ConfigurableJointMotion.Free;

                        pelvConf.xMotion = ConfigurableJointMotion.Free;
                        pelvConf.yMotion = ConfigurableJointMotion.Free;
                        pelvConf.zMotion = ConfigurableJointMotion.Free;

                        Vector3 pelvLocalForward = PelvisInDummy.InverseTransformDirection(BaseTransform.right);
                        Vector3 pelvLocalRight = PelvisInDummy.InverseTransformDirection(BaseTransform.up);

                        pelvConf.axis = FVectorMethods.ChooseDominantAxis(pelvLocalForward);
                        pelvConf.secondaryAxis = FVectorMethods.ChooseDominantAxis(pelvLocalRight);

                    }

                if (pelvConf) pelvConf.connectedBody = rootRig;

                _helpChestName = "";
                if (Chest) _helpChestName = Chest.name;
            }

            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                Transform lShould = null;
                Transform rShould = null;
                Transform chestBone = null;

                string prefix = "";

#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    if (IsPreGeneratedDummy) if (AddNamePrefix) prefix = "R_";

                if (LeftShoulder) lShould = FindChildByNameInDepth(prefix + LeftShoulder.name, RagdollDummySkeleton);
                if (RightShoulder) rShould = FindChildByNameInDepth(prefix + RightShoulder.name, RagdollDummySkeleton);
                if (Chest) chestBone = FindChildByNameInDepth(prefix + Chest.name, RagdollDummySkeleton);

                SetRagdollTargetBones(
                    RagdollDummySkeleton,
                        PelvisInDummy,
                        FindChildByNameInDepth(prefix + SpineStart.name, RagdollDummySkeleton),
                        chestBone,
                        FindChildByNameInDepth(prefix + Head.name, RagdollDummySkeleton),

                        FindChildByNameInDepth(prefix + LeftUpperArm.name, RagdollDummySkeleton),
                        FindChildByNameInDepth(prefix + LeftForeArm.name, RagdollDummySkeleton),
                        FindChildByNameInDepth(prefix + RightUpperArm.name, RagdollDummySkeleton),
                        FindChildByNameInDepth(prefix + RightForeArm.name, RagdollDummySkeleton),

                        FindChildByNameInDepth(prefix + LeftUpperLeg.name, RagdollDummySkeleton),
                        FindChildByNameInDepth(prefix + LeftLowerLeg.name, RagdollDummySkeleton),
                        FindChildByNameInDepth(prefix + RightUpperLeg.name, RagdollDummySkeleton),
                        FindChildByNameInDepth(prefix + RightLowerLeg.name, RagdollDummySkeleton),
                        lShould, rShould
                    );


                SetAnimationPoseBones
                    (
                        Pelvis,
                        SpineStart,
                        Chest,
                        Head,

                        LeftUpperArm,
                        LeftForeArm,
                        RightUpperArm,
                        RightForeArm,

                        LeftUpperLeg,
                        LeftLowerLeg,
                        RightUpperLeg,
                        RightLowerLeg
                        );

                if (posingLeftShoulder != null) posingLeftShoulder.SetVisibleBone(LeftShoulder);
                if (posingRightShoulder != null) posingRightShoulder.SetVisibleBone(RightShoulder);

                if (posingLeftFist != null) { posingLeftFist.SetVisibleBone(LeftFist); }
                if (posingRightFist != null) { posingRightFist.SetVisibleBone(RightFist); }
                if (posingLeftFoot != null) { posingLeftFoot.SetVisibleBone(LeftFoot); }
                if (posingRightFoot != null) { posingRightFoot.SetVisibleBone(RightFoot); }

            }
            else if (BonesSetupMode == EBonesSetupMode.CustomLimbs)
            {
                posingPelvis = new PosingBone(PelvisInDummy, this);
                posingPelvis.SetVisibleBone(Pelvis);

                for (int i = 0; i < CustomLimbsBonesChains.Count; i++)
                {
                    var chain = CustomLimbsBonesChains[i];
                    chain.InitializeForDummy(this, RagdollDummySkeleton);
                    chain.DetachBoneChainsIfDetachEnabled();
                }

                // Set posing bones update child-chain hierarchy
                posingPelvis.child = CustomLimbsBonesChains[0].BoneSetups[0].Posing;

                for (int i = 0; i < CustomLimbsBonesChains.Count - 1; i++)
                {
                    var chain = CustomLimbsBonesChains[i];
                    var nchain = CustomLimbsBonesChains[i + 1];
                    chain.BoneSetups[chain.BoneSetups.Count - 1].Posing.child =
                    nchain.BoneSetups[0].Posing;
                }

                // Find the helper posing head
                PosingBone ch = posingPelvis;
                while (ch != null)
                {
                    if (ch.visibleBone == Head) { posingHead = ch; break; }
                    ch = ch.child;
                }

            }

            if (allowRemoving)
            {

                if (RemoveOnlyTargetRagdollCollidersOnAnimator)
                {
                    // Removing ragdoll components from targe ragdoll bones on source
                    if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                    {
                        // Check all humanoid limbs

                        PosingBone ch = posingPelvis;
                        while (ch != null)
                        {
                            var bone = ch;
                            Transform t = bone.AnimatorBone;
                            if (t == null) { ch = ch.child; continue; }

                            ConfigurableJoint cfj = t.GetComponent<ConfigurableJoint>(); if (cfj) { DestroyObj(cfj); }
                            CharacterJoint chj = t.GetComponent<CharacterJoint>(); if (chj) { DestroyObj(chj); }

                            if (KeepCollidersOnAnimator == false)
                            { Collider cl = t.GetComponent<Collider>(); if (cl) { DestroyObj(cl); } }
                           
                            Rigidbody rg = t.GetComponent<Rigidbody>(); if (rg) { DestroyObj(rg); }
                            ch = ch.child;
                        }

                    }
                    else // Check all custom limbs
                    {
                        for (int ch = 0; ch < CustomLimbsBonesChains.Count; ch++)
                        {
                            var chain = CustomLimbsBonesChains[ch];
                            for (int b = 0; b < chain.BoneSetups.Count; b++)
                            {
                                var bone = chain.BoneSetups[b];
                                Transform t = bone.t;
                                if (t == null) continue;

                                ConfigurableJoint cfj = t.GetComponent<ConfigurableJoint>(); if (cfj) { DestroyObj(cfj); }
                                CharacterJoint chj = t.GetComponent<CharacterJoint>(); if (chj) { DestroyObj(chj); }

                                if (KeepCollidersOnAnimator == false)
                                { Collider cl = t.GetComponent<Collider>(); if (cl) { DestroyObj(cl); } }
                               
                                Rigidbody rg = t.GetComponent<Rigidbody>(); if (rg) { DestroyObj(rg); }
                            }
                        }
                    }
                }
                else
                {
                    // Removing ragdoll components from source skeleton
                    foreach (Transform t in RootInParent.GetComponentsInChildren<Transform>(true))
                    {
                        ConfigurableJoint cc = t.GetComponent<ConfigurableJoint>();
                        if (cc) { DestroyObj(cc); }
                        CharacterJoint cj = t.GetComponent<CharacterJoint>();
                        if (cj) { DestroyObj(cj); }

                        if (KeepCollidersOnAnimator == false)
                        {
                            Collider cl = t.GetComponent<Collider>();
                            if (cl) { DestroyObj(cl); }
                        }

                        Rigidbody r = t.GetComponent<Rigidbody>();
                        if (r) { DestroyObj(r); }
                    }
                }


                if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
                {
                    // Optimizing ragdoll hierarchy
                    if (posingLeftFist == null)
                        DestroyChildren(posingLeftForeArm.transform);
                    else
                        DestroyChildren(posingLeftFist.transform);

                    if (posingRightFist == null)
                        DestroyChildren(posingRightForeArm.transform);
                    else
                        DestroyChildren(posingRightFist.transform);

                    DestroyChildren(posingHead.transform);
                }

            }


            #region Collecting bones without rigidbodies which should be animated to keep ragdoll in sync with animator pose with higher precision

            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                List<Transform> toReAnimate = new List<Transform>();

                Transform p = posingHead.transform.parent;

                while (p != null)
                {
                    Joint j = p.GetComponent<Joint>();
                    if (j == null) if (toReAnimate.Contains(p) == false) toReAnimate.Add(p);
                    p = p.parent; if (p == posingPelvis.transform) break;
                }

                p = posingLeftUpperArm.transform;
                while (p != null)
                {
                    Joint j = p.GetComponent<Joint>();
                    if (j == null) if (toReAnimate.Contains(p) == false) toReAnimate.Add(p);
                    p = p.parent; if (p == posingPelvis.transform) break;
                }

                p = posingRightUpperArm.transform;
                while (p != null)
                {
                    Joint j = p.GetComponent<Joint>();
                    if (j == null) if (toReAnimate.Contains(p) == false) toReAnimate.Add(p);
                    p = p.parent; if (p == posingPelvis.transform) break;
                }

                for (int t = 0; t < toReAnimate.Count; t++)
                {
                    Transform ragBone = toReAnimate[t];
                    Transform animBone = FindChildByNameInDepth(ragBone.name, objectWithAnimator.transform);

                    if (animBone != null)
                    {
                        PosingBone childB = null;

                        if (animBone.childCount > 0)
                        {
                            Transform childT = animBone.GetChild(0);

                            PosingBone pb = posingPelvis;
                            while (pb != null)
                            {
                                if (pb.visibleBone == childT) { childB = pb; break; }
                                pb = pb.child;
                            }

                            if (childB != null)
                                toReanimateBones.Add(new ToAnimateBone(animBone, ragBone, childB));
                        }
                    }
                }
            }

            #endregion


            // Restoring colliders collision on the dummy skeleton
            PosingBone c = posingPelvis;
            while (c != null)
            {
                if (c.collider != null) c.collider.enabled = true;
                c = c.child;
            }


            if (allowRemoving)
            {
                // Removing additional MonoBehaviours from the dummy
                c = posingPelvis;
                while (c != null)
                {
                    foreach (MonoBehaviour beh in c.visibleBone.GetComponents<MonoBehaviour>())
                    {
                        bool remove = false;
                        for (int i = 0; i < moveCompsToDummy.Count; i++) if (beh.GetType() == moveCompsToDummy[i]) remove = true;
                        if (remove) DestroyObj(beh);
                    }

                    foreach (MonoBehaviour beh in c.transform.GetComponentsInChildren<MonoBehaviour>())
                    {
                        bool remove = true;
                        for (int i = 0; i < moveCompsToDummy.Count; i++) if (beh.GetType() == moveCompsToDummy[i]) remove = false;
                        if (remove) DestroyObj(beh);
                    }

                    c = c.child;
                }
            }

            if (AddNamePrefix)
                foreach (Transform t in RagdollDummyBase.transform.GetComponentsInChildren<Transform>())
                {
                    t.name = "R_" + t.name;
                }

            IgnoreCollisionsBetweenRagdollBones();


            // Ignore collision of the ragdoll and the animator colliders

            if (IgnoreAnimatorToRagdollCollision)
            {
                if (RootInParent)
                {
                    var allAnimatorTr = RootInParent.GetComponentsInChildren<Transform>();
                    //foreach (Transform t in rootInParent.GetComponentsInChildren<Transform>(true))
                    //{
                    //    Collider cl = t.GetComponent<Collider>();
                    //    if (cl) cl.enabled = false;
                    //}

                    c = posingPelvis;
                    while (c != null)
                    {
                        foreach (var t in allAnimatorTr)
                        {
                            Collider coll = t.gameObject.GetComponent<Collider>();
                            if (coll) Ragdoll_IgnoreCollision(c.collider, coll);
                        }

                        c = c.child;
                    }
                }
            }


            if ( RotationMode == RotationDriveMode.XYAndZ)
            {
                c = posingPelvis;
                while (c != null)
                {
                    if (c.ConfigurableJoint)
                    {
                        c.ConfigurableJoint.rotationDriveMode = RotationMode;
                    }

                    c = c.child;
                }
            }


            if (containerForDummy) RagdollDummyBase.SetParent(containerForDummy, true);
            else if (IsPreGeneratedDummy) RagdollDummyBase.SetParent(null, true);
#if UNITY_EDITOR
            if (IsPreGeneratedDummy && allowRemoving) UnityEditor.EditorGUIUtility.PingObject(RagdollDummyBase);
#endif
            //RagdollDummyBase.localScale = objectWithAnimator.localScale;
            //objectWithAnimator.localScale = startScale;

            CheckDummyHide();
        }

        void CheckDummyHide()
        {
            if (HideDummy)
                if (RagdollDummyBase != null)
                    if (Application.isPlaying) RagdollDummyBase.hideFlags = HideFlags.HideInHierarchy;
        }

        internal void AlignRagdollBonesWithAnimatorBones()
        {
            RagdollDummyBase.position = parentCaller.transform.position;
            RagdollDummyBase.rotation = parentCaller.transform.rotation;

            PosingBone c = posingPelvis;
            c.AlignDummyBoneWithAnimatorPose(false);
            c = c.child;

            while (c != null)
            {
                c.AlignDummyBoneWithAnimatorPose(true);
                c = c.child;
            }

        }

        void IgnoreCollisionsBetweenRagdollBones()
        {

            if (BonesSetupMode == EBonesSetupMode.CustomLimbs)
            {
                // Ignore collision between colliders which are too near to each other
                var c = posingPelvis;
                while (c != null)
                {
                    var cc = posingPelvis;
                    while (cc != null)
                    {
                        if (cc == c) { cc = cc.child; continue; }

                        Bounds oBounds = cc.collider.bounds;
                        oBounds.size *= 1.1f;

                        if (c.collider.bounds.Intersects(oBounds))
                        {
                            Ragdoll_IgnoreCollision(c.collider, cc.collider);
                        }

                        cc = cc.child;
                    }

                    c = c.child;
                }

            }

            // Ignore all self colliders collisions
            if (IgnoreSelfCollision)
            {
                var c = posingPelvis;
                while (c != null)
                {
                    var cc = posingPelvis;
                    while (cc != null)
                    {
                        if (cc == c) { cc = cc.child; continue; }
                        Ragdoll_IgnoreCollision(c.collider, cc.collider);
                        cc = cc.child;
                    }

                    c = c.child;
                }
            }


            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                Ragdoll_IgnoreCollision(posingSpineStart.collider, posingPelvis.collider);

                if (Chest) Ragdoll_IgnoreCollision(posingPelvis.collider, posingChest.collider);
                if (Chest) Ragdoll_IgnoreCollision(posingSpineStart.collider, posingChest.collider);

                if (SpineStart) Ragdoll_IgnoreCollision(posingSpineStart.collider, posingRightUpperArm.collider);
                if (SpineStart) Ragdoll_IgnoreCollision(posingSpineStart.collider, posingLeftUpperArm.collider);

                if (Chest) Ragdoll_IgnoreCollision(posingChest.collider, posingHead.collider); else Ragdoll_IgnoreCollision(posingSpineStart.collider, posingHead.collider);
                if (Chest) Ragdoll_IgnoreCollision(posingChest.collider, posingLeftUpperArm.collider); else Ragdoll_IgnoreCollision(posingSpineStart.collider, posingLeftUpperArm.collider);
                if (Chest) Ragdoll_IgnoreCollision(posingChest.collider, posingRightUpperArm.collider); else Ragdoll_IgnoreCollision(posingSpineStart.collider, posingRightUpperArm.collider);

                if (LeftShoulder)
                {
                    if (Chest) Ragdoll_IgnoreCollision(posingLeftShoulder.collider, posingChest.collider);
                    Ragdoll_IgnoreCollision(posingLeftShoulder.collider, posingSpineStart.collider);
                    Ragdoll_IgnoreCollision(posingLeftShoulder.collider, posingLeftUpperArm.collider);
                }

                if (RightShoulder)
                {
                    if (Chest) Ragdoll_IgnoreCollision(posingRightShoulder.collider, posingChest.collider);
                    Ragdoll_IgnoreCollision(posingRightShoulder.collider, posingSpineStart.collider);
                    Ragdoll_IgnoreCollision(posingRightShoulder.collider, posingRightUpperArm.collider);
                }

                Ragdoll_IgnoreCollision(posingRightUpperArm.collider, posingHead.collider);
                Ragdoll_IgnoreCollision(posingRightUpperArm.collider, posingRightForeArm.collider);
                Ragdoll_IgnoreCollision(posingLeftUpperArm.collider, posingHead.collider);
                Ragdoll_IgnoreCollision(posingLeftUpperArm.collider, posingLeftForeArm.collider);
                Ragdoll_IgnoreCollision(posingPelvis.collider, posingLeftUpperLeg.collider);
                Ragdoll_IgnoreCollision(posingPelvis.collider, posingRightUpperLeg.collider);
                Ragdoll_IgnoreCollision(posingRightUpperLeg.collider, posingRightLowerLeg.collider);
                Ragdoll_IgnoreCollision(posingLeftUpperLeg.collider, posingLeftLowerLeg.collider);


                Ragdoll_IgnoreCollision(posingRightUpperLeg.collider, posingLeftUpperLeg.collider);
                Ragdoll_IgnoreCollision(posingLeftUpperLeg.collider, posingRightUpperLeg.collider);

                if (posingRightFist != null) Ragdoll_IgnoreCollision(posingRightFist.collider, posingRightForeArm.collider);
                if (posingLeftFist != null) Ragdoll_IgnoreCollision(posingLeftFist.collider, posingLeftForeArm.collider);
                if (posingRightFoot != null) Ragdoll_IgnoreCollision(posingRightFoot.collider, posingRightLowerLeg.collider);
                if (posingLeftFoot != null) Ragdoll_IgnoreCollision(posingLeftFoot.collider, posingLeftLowerLeg.collider);
            }


            if (IgnoreCollisionWith != null)
                for (int i = 0; i < IgnoreCollisionWith.Count; i++)
                {
                    if (IgnoreCollisionWith[i] == null) continue;
                    User_SetIgnoreCollisionWith(IgnoreCollisionWith[i]);
                }


            if (SetPhysicalMaterial != null) User_SetPhysicalMaterialAll(SetPhysicalMaterial);



            if (KeepCollidersOnAnimator)
            {
                // If keeping bones on the animator, let's ignore collisions between dummy bones and them to avoid glitches
                foreach (Transform t in RootInParent.GetComponentsInChildren<Transform>(true))
                {
                    Collider cl = t.GetComponent<Collider>();

                    if (cl)
                    {
                        var c = posingPelvis;
                        PosingBone myPos = null;

                        while (c != null)
                        {
                            if (c.AnimatorBone == cl.transform) myPos = c;

                            Ragdoll_IgnoreCollision(c.collider, cl);

                            if (c.extraColliders != null) for (int ec = 0; ec < c.extraColliders.Count; ec++)
                                {
                                    if (c.extraColliders[ec] == null) continue;
                                    Ragdoll_IgnoreCollision(c.extraColliders[ec], cl);
                                }

                            c = c.child;
                        }

                        cl.enabled = true;
                        cl.gameObject.layer = ChangeAnimatorCollidersLayerTo;
                        if (myPos != null) cl.gameObject.AddComponent<RagdollIndicatorHelper>().Initialize(this, myPos, true);
                    }
                }

            }

        }

        public void User_SetIgnoreCollisionWith(Collider coll)
        {
            var c = posingPelvis;
            while (c != null)
            {
                Ragdoll_IgnoreCollision(c.collider, coll);

                if (c.extraColliders != null) for (int ec = 0; ec < c.extraColliders.Count; ec++)
                    { if (c.extraColliders[ec] == null) continue; Ragdoll_IgnoreCollision(c.extraColliders[ec], coll); }

                c = c.child;
            }
        }

        public void User_SetPhysicalMaterialAll(PhysicMaterial material)
        {
            var c = posingPelvis;
            while (c != null)
            {
                c.collider.sharedMaterial = material;

                if (c.extraColliders != null) for (int ec = 0; ec < c.extraColliders.Count; ec++)
                    { if (c.extraColliders[ec] == null) continue; c.extraColliders[ec].sharedMaterial = material; }

                c = c.child;
            }
        }

        private void DestroyObj(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (Application.isPlaying) GameObject.Destroy(obj); else GameObject.DestroyImmediate(obj);
#else
            GameObject.Destroy(obj);
#endif
        }

        public void Ragdoll_IgnoreCollision(Collider a, Collider b)
        {
            if (a != null && b != null)
            {
                Physics.IgnoreCollision(a, b);
            }
        }

        private void DestroyChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroyObj(parent.GetChild(i).gameObject);

        }




        #region Pre Generated Dummy Support

        [HideInInspector]
        [SerializeField] private bool usePreGeneratedDummy = false;
        public bool IsPreGeneratedDummy { get { return usePreGeneratedDummy; } }

        public void PreGenerateDummy(Transform objectWithAnimator, Transform rootBone)
        {
            usePreGeneratedDummy = true;
            PrepareRagdollDummy(objectWithAnimator, rootBone);
        }

        public void RemovePreGeneratedDummy()
        {
            usePreGeneratedDummy = false;

            if (RagdollDummyBase)
            {
                DestroyObj(RagdollDummyBase.gameObject);
            }
        }

        #endregion

    }
}