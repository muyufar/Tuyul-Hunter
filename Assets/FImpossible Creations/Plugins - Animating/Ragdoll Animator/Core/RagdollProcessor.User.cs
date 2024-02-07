using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollProcessor
    {
        /// <summary>
        /// Setting all ragdoll limbs rigidbodies kinematic or non kinematic
        /// </summary>
        public void User_SetAllKinematic(bool kinematic = true)
        {
            PosingBone c = posingPelvis.child;
            while (c != null)
            {
                if (c.rigidbody) c.rigidbody.isKinematic = kinematic;
                c = c.child;
            }
        }


        /// <summary>
        /// Setting all ragdoll limbs rigidbodies angular speed limit (by default unity restricts it very tightly)
        /// </summary>
        public void User_SetAllAngularSpeedLimit(float angularSpeedLimit)
        {
            PosingBone c = posingPelvis.child;
            while (c != null)
            {
                if (c.rigidbody) c.rigidbody.maxAngularVelocity = angularSpeedLimit;
                c = c.child;
            }
        }


        /// <summary>
        /// Setting all ragdoll limbs rigidbodies interpolation mode
        /// </summary>
        public void User_SetAllIterpolation(RigidbodyInterpolation interpolation)
        {
            foreach (var r in RagdollLimbs)
            {
                r.interpolation = interpolation;
            }
        }

        /// <summary>
        /// Adding physical push impact to single rigidbody limb
        /// </summary>
        /// <param name="limb"> Access 'Parameters' for ragdoll limb </param>
        /// <param name="powerDirection"> World space direction vector </param>
        /// <param name="duration"> Time in seconds </param>
        public static IEnumerator User_SetPhysicalImpact(Rigidbody limb, Vector3 powerDirection, float duration, ForceMode forceMode = ForceMode.Impulse)
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            while (elapsed < duration)
            {
                ApplyLimbImpact(limb, powerDirection, forceMode);
                elapsed += Time.fixedDeltaTime;
                yield return fixedWait;
            }

            yield break;
        }

        /// <summary> Default impulse impact on the rigidbody </summary>
        internal static void ApplyLimbImpact(Rigidbody limb, Vector3 powerDirection, ForceMode forceMode = ForceMode.Impulse)
        {
            limb.AddForce(powerDirection, forceMode);
        }

        /// <summary>
        /// Adding physical push impact to single rigidbody limb
        /// </summary>
        /// <param name="limb"> Access 'Parameters' for ragdoll limb </param>
        /// <param name="powerDirection"> World space direction vector </param>
        /// <param name="duration"> Time in seconds </param>
        public IEnumerator User_SetLimbImpact(Rigidbody limb, Vector3 powerDirection, float duration, ForceMode forceMode = ForceMode.Impulse)
        {
            yield return User_SetPhysicalImpact(limb, powerDirection, duration, forceMode);
        }


        /// <summary>
        /// Adding physical push impact to all limbs of the ragdoll
        /// </summary>
        /// <param name="powerDirection"> World space direction vector </param>
        /// <param name="duration"> Time in seconds </param>
        public IEnumerator User_SetPhysicalImpactAll(Vector3 powerDirection, float duration, ForceMode forceMode = ForceMode.Impulse)
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            while (elapsed < duration)
            {
                PosingBone c = posingPelvis.child;
                while (c != null)
                {
                    if (c.rigidbody) c.rigidbody.AddForce(powerDirection, forceMode);
                    c = c.child;
                }

                elapsed += Time.fixedDeltaTime;

                yield return fixedWait;
            }

            yield break;
        }


        /// <summary>
        /// Adding physical torque impact to the core limbs
        /// </summary>
        /// <param name="rotationPower"> Rotation angles torque power </param>
        /// <param name="duration"> Time in seconds </param>
        public IEnumerator User_SetPhysicalTorque(Vector3 rotationPower, float duration, bool relativeSpace = true, ForceMode forceMode = ForceMode.Impulse)
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();


            while (elapsed < duration)
            {
                PosingBone c = posingPelvis;

                while (c != null)
                {
                    if (c.rigidbody)
                    {
                        if (relativeSpace)
                            c.rigidbody.AddRelativeTorque(rotationPower, forceMode);
                        else c.rigidbody.AddTorque(rotationPower, forceMode);
                    }

                    c = c.child;
                }

                //if (c != null) if (c.rigidbody) c.rigidbody.AddTorque(rotationPower, ForceMode.Impulse);
                elapsed += Time.fixedDeltaTime;

                yield return fixedWait;
            }

            yield break;
        }


        public IEnumerator User_SetPhysicalTorque(Rigidbody limb, Vector3 rotationPower, float duration, bool relativeSpace = true, ForceMode forceMode = ForceMode.Impulse)
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            while (elapsed < duration)
            {
                if (relativeSpace)
                    limb.AddRelativeTorque(rotationPower, forceMode);
                else
                    limb.AddTorque(rotationPower, forceMode);

                elapsed += Time.fixedDeltaTime;
                yield return fixedWait;
            }

            yield break;
        }



        /// <summary>
        /// Enable / disable animator component with delay
        /// </summary>
        public IEnumerator User_SwitchAnimator(Animator animator, bool enable, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            animator.enabled = enable;
            //CaptureAnimator = capturing;

            yield break;
        }

        /// <summary>
        /// Transitioning all rigidbody muscles power to target value
        /// </summary>
        /// <param name="forcePoseEnd"> Target muscle power </param>
        /// <param name="duration"> Transition duration </param>
        /// <param name="delay"> Delay to start transition </param>
        public IEnumerator User_FadeMuscles(float forcePoseEnd = 0f, float duration = 0.75f, float delay = 0f)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            float startPoseForce = RotateToPoseForce;
            float elapsed = -0.0001f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (elapsed > duration) elapsed = duration;
                RotateToPoseForce = Mathf.LerpUnclamped(startPoseForce, forcePoseEnd, elapsed / duration);

                yield return null;
            }

            RotateToPoseForce = forcePoseEnd;

            yield break;
        }

        /// <summary>
        /// Forcing applying rigidbody pose to the animator pose and fading out to zero smoothly
        /// </summary>
        public IEnumerator User_ForceRagdollToAnimatorFor(float duration, float forcingFullDelay = 0.2f)
        {
            for (int i = 0; i < toReanimateBones.Count; i++)
            {
                toReanimateBones[i].InternalRagdollToAnimatorOverride = 1f;
            }

            if (forcingFullDelay > 0f) yield return new WaitForSeconds(forcingFullDelay);

            if (duration > 0f)
            {
                float elapsed = -0.0001f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed > duration) elapsed = duration;
                    float progr = elapsed / duration;

                    for (int i = 0; i < toReanimateBones.Count; i++)
                    {
                        toReanimateBones[i].InternalRagdollToAnimatorOverride = 1f - progr;
                    }

                    yield return null;
                }
            }

            for (int i = 0; i < toReanimateBones.Count; i++)
            {
                toReanimateBones[i].InternalRagdollToAnimatorOverride = 0f;
            }

            yield break;
        }

        public bool IsFadeRagdollCoroutineRunning { get; private set; }

        /// <summary>
        /// Transitioning ragdoll blend value
        /// </summary>
        public IEnumerator User_FadeRagdolledBlend(float targetBlend = 0f, float duration = 0.75f, float delay = 0f)
        {
            IsFadeRagdollCoroutineRunning = true;

            if (delay > 0f) yield return new WaitForSeconds(delay);

            if (duration > 0f)
            {
                float startBlend = RagdolledBlend;
                float elapsed = -0.0001f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    if (elapsed > duration) elapsed = duration;
                    RagdolledBlend = Mathf.LerpUnclamped(startBlend, targetBlend, elapsed / duration);

                    yield return null;
                }
            }

            RagdolledBlend = targetBlend;

            IsFadeRagdollCoroutineRunning = false;
            yield break;
        }

        public void User_SetAllLimbsVelocity(Vector3 velocity)
        {
            PosingBone c = posingPelvis.child;
            while (c != null)
            {
                if (c.rigidbody) c.rigidbody.velocity = velocity;
                c = c.child;
            }
        }

        /// <summary>
        /// Computing total velocity of all ragdoll limbs
        /// </summary>
        public Vector3 User_GetAllLimbsVelocity()
        {
            Vector3 velo = Vector3.zero;

            PosingBone c = posingPelvis.child;
            while (c != null)
            {
                if (c.rigidbody) velo += c.rigidbody.velocity;
                c = c.child;
            }

            return velo;
        }


        public IEnumerator User_TransitionToNonFreeFallRagdoll(float duration, float delay = 0f)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            float elapsed = -0.0001f;
            _transitionNonFreeFall_OverrideMaxBlend = 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _transitionNonFreeFall_OverrideMaxBlend = Mathf.Min(1f, elapsed / duration);
                yield return null;
            }

            SwitchFreeFallRagdoll(false);
            _transitionNonFreeFall_OverrideMaxBlend = 0f;
        }

        public void SafetyResetAfterCouroutinesStop()
        {
            _transitionNonFreeFall_OverrideMaxBlend = 0f;
            latestHipsBlend = 0f;
        }

        /// <summary>
        /// Computing total velocity of all ragdoll limbs
        /// </summary>
        public Vector3 User_GetAllLimbsMaxVelocity()
        {
            Vector3 velo = Vector3.zero;

            PosingBone c = posingPelvis.child;
            while (c != null)
            {
                if (c.rigidbody) if (c.rigidbody.velocity.sqrMagnitude > velo.sqrMagnitude) velo = c.rigidbody.velocity;
                c = c.child;
            }

            return velo;
        }

        /// <summary>
        /// Computing velocity of ragdoll spine bones
        /// </summary>
        public Vector3 User_GetSpineLimbsVelocity(bool withChest = false)
        {
            Vector3 velo = Vector3.zero;
            velo += posingPelvis.rigidbody.velocity;
            if (withChest) velo += posingChest.rigidbody.velocity;
            return velo;
        }

        /// Computing angular velocity of ragdoll spine bones
        /// </summary>
        public Vector3 User_GetSpineLimbsAngularVelocity(bool withChest = false)
        {
            Vector3 velo = Vector3.zero;
            velo += posingPelvis.rigidbody.angularVelocity;
            if (withChest) velo += posingChest.rigidbody.angularVelocity;
            return velo;
        }

        /// <summary>
        /// Computing pelvis forward pointing direction in world space
        /// </summary>
        public Vector3 User_PelvisWorldForward()
        {
            return posingPelvis.rigidbody.rotation * PelvisLocalForward;
        }

        /// <summary>
        /// Computing pelvis up pointing direction in world space
        /// </summary>
        public Vector3 User_PelvisWorldUp()
        {
            return posingPelvis.rigidbody.rotation * PelvisLocalUp;
        }

        /// <summary>
        /// Computing pelvis right pointing direction in world space
        /// </summary>
        public Vector3 User_PelvisWorldRight()
        {
            return posingPelvis.rigidbody.rotation * PelvisLocalRight;
        }

        public enum EGetUpType
        {
            None, FromBack, FromFacedown, FromLeftSide, FromRightSide
        }


        float _getUp_latestProbeTime = -1000f;
        float _getUp_latestVeloProbeTime = -1000f;
        EGetUpType _getUp_latestGetUpType = EGetUpType.None;
        Vector3 _getUp_latestSpineLimbsVelocity = Vector3.one;
        float _getUp_latestSpineLimbsVelocityMagn = 1000f;
        float _getUp_stabilityTimer = 0f;

        public void _User_GetUpResetProbe()
        {
            _getUp_latestProbeTime = -1000f;
            _getUp_latestVeloProbeTime = -1000f;
            _getUp_latestGetUpType = EGetUpType.None;
            _getUp_latestSpineLimbsVelocity = Vector3.one;
            _getUp_latestSpineLimbsVelocityMagn = 1000f;
            _getUp_stabilityTimer = 0f;
        }

        public EGetUpType ProbeGetUpStatePer(float maxVelocityToAllow = 0.7f, float delayPerProbe = 0.1f, float minimumStabilityTime = 0.5f, Vector3? worldUp = null, bool canBeNone = true)
        {
            if (_getUp_latestGetUpType == EGetUpType.None)
            {
                if (_getUp_stabilityTimer > 0f) _getUp_stabilityTimer -= Time.deltaTime; else _getUp_stabilityTimer = 0f;
            }
            else
                _getUp_stabilityTimer += Time.deltaTime;

            float ago = Time.time - _getUp_latestVeloProbeTime;

            if (ago > delayPerProbe * 0.5f)
            {
                _getUp_latestSpineLimbsVelocity = User_GetSpineLimbsVelocity();
                _getUp_latestSpineLimbsVelocityMagn = _getUp_latestSpineLimbsVelocity.magnitude;
                _getUp_latestVeloProbeTime = Time.time;
            }

            if (_getUp_latestSpineLimbsVelocityMagn > maxVelocityToAllow) if (_getUp_stabilityTimer < minimumStabilityTime) return EGetUpType.None; else return _getUp_latestGetUpType;

            ago = Time.time - _getUp_latestProbeTime;
            if (ago < delayPerProbe) if (_getUp_stabilityTimer < minimumStabilityTime) return EGetUpType.None; else return _getUp_latestGetUpType;

            _getUp_latestProbeTime = Time.time;
            _getUp_latestGetUpType = User_CanGetUp(worldUp, canBeNone);

            if (_getUp_stabilityTimer < minimumStabilityTime) return EGetUpType.None; else return _getUp_latestGetUpType;
        }

        public bool RecognizeRagdollIsStandingOnLegs(float tolerance = 0.75f, Vector3? worldUp = null)
        {
            if (Initialized == false) return false;

            Vector3 footsMidToHips;

            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                footsMidToHips = posingLeftLowerLeg.transform.position;
                footsMidToHips = Vector3.Lerp(footsMidToHips, posingRightLowerLeg.transform.position, 0.5f);
                footsMidToHips = posingPelvis.transform.position - footsMidToHips;
            }
            else
            {
                footsMidToHips = posingPelvis.transform.position - GetStoredHipsRootOffset();
            }

            Vector3 wUp = Vector3.up;
            if (worldUp != null) wUp = worldUp.Value;
            float dot = Vector3.Dot(footsMidToHips.normalized, wUp);

            return dot > tolerance;
        }


        public Vector3 User_GetMiddleFootPos()
        {
            Vector3 footMiddle;
            if (BonesSetupMode == EBonesSetupMode.HumanoidLimbs)
            {
                footMiddle = posingLeftLowerLeg.transform.position;
            }
            else
            {
                return GetStoredHipsRootOffset();
            }

            footMiddle = Vector3.LerpUnclamped(footMiddle, posingRightLowerLeg.transform.position, 0.5f);
            return footMiddle;
        }

        /// <summary> Model scale reference length : current distance between hips and head </summary>
        public float User_ReferenceLength()
        {
            return Vector3.Distance(posingPelvis.transform.position, posingHead.transform.position);
        }

        /// <summary>
        /// Checking state for ragdoll get-up possiblity case
        /// </summary>
        /// <param name="quadroped"> IF it's animal with horizontal instead of vertical spine, it will help detecting facedown / on back </param>
        public EGetUpType User_CanGetUp(Vector3? worldUp = null, bool canBeNone = true, bool includeLeftRightSide = false, float tolerance = 0.35f, bool quadroped = false)
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            float dot;

            if (quadroped)
            {
                dot = Vector3.Dot(-User_PelvisWorldUp(), up);
            }
            else
            {
                dot = Vector3.Dot(User_PelvisWorldForward(), up);
            }

            //UnityEngine.Debug.Log(up + " vs " + User_PelvisWorldForward().ToString() +  " DOT = " + dot);

            if (canBeNone)
            {
                if (dot > tolerance) return EGetUpType.FromBack;
                else if (dot < -tolerance) return EGetUpType.FromFacedown;
                else
                {
                    if (includeLeftRightSide)
                    {
                        return User_LayingOnSide(worldUp);
                    }
                }
            }
            else
            {
                if (dot >= 0f) return EGetUpType.FromBack;
                if (dot < 0f) return EGetUpType.FromFacedown;
            }

            return EGetUpType.None;
        }

        public EGetUpType User_LayingOnSide(Vector3? worldUp = null, bool canBeNone = true, float tolerance = 0.35f)
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            float dot = Vector3.Dot(User_PelvisWorldRight(), up);

            if (canBeNone)
            {
                if (dot > tolerance) return EGetUpType.FromLeftSide;
                if (dot < -tolerance) return EGetUpType.FromRightSide;
            }
            else
            {
                if (dot >= 0f) return EGetUpType.FromLeftSide;
                if (dot < 0f) return EGetUpType.FromRightSide;
            }

            return EGetUpType.None;
        }

        /// <summary>
        /// Making pelvis kinematic and anchored to pelvis position
        /// </summary>
        public IEnumerator User_AnchorPelvis(bool anchor = true, float duration = 0f)
        {
            if (duration != 0f)
            {
                Vector3 startPos = posingPelvis.rigidbody.position;
                Quaternion startRot = posingPelvis.rigidbody.rotation;
                float elapsed = -0.0001f;

                while (elapsed < duration)
                {

                    yield return new WaitForFixedUpdate();
                    elapsed += Time.fixedDeltaTime;
                    if (elapsed > duration) elapsed = duration;

                    float blend = (1f - RagdolledBlend) * (elapsed / duration);
                    posingPelvis.rigidbody.position = Vector3.LerpUnclamped(startPos, pelvisAnimatorPosition, blend);
                    posingPelvis.rigidbody.rotation = Quaternion.LerpUnclamped(startRot, pelvisAnimatorRotation, blend);

                }
            }

            if (anchor)
            {
                posingPelvis.transform.localPosition = posingPelvis.visibleBone.localPosition;
                posingPelvis.transform.localRotation = posingPelvis.visibleBone.localRotation;
                //posingPelvis.transform.localPosition = Vector3.LerpUnclamped(posingPelvis.transform.localPosition, posingPelvis.visibleBone.localPosition, blend);
                //posingPelvis.transform.localRotation = Quaternion.LerpUnclamped(posingPelvis.transform.localRotation, posingPelvis.visibleBone.localRotation, blend);
            }

            posingPelvis.rigidbody.isKinematic = anchor;

            yield break;
        }

        /// <summary>
        /// Computing target get-up position for ragdoll controller to fit with current ragdolled position
        /// </summary>
        public Vector3 User_ComputeGetUpPosition()
        {
            Vector3 local = Vector3.LerpUnclamped(Vector3.zero, PelvisToBase, StandUpInFootPoint);
            return posingPelvis.transform.TransformPoint(local);
        }

        /// <summary>
        /// Moving ragdoll controller object to fit with current ragdolled position hips
        /// </summary>
        public void User_RepositionRoot(Transform root = null, Vector3? getUpPosition = null, Vector3? worldUp = null, EGetUpType getupType = EGetUpType.None, LayerMask? snapToGround = null)
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            if (root == null) root = BaseTransform;

            RagdollDummySkeleton.SetParent(null, true);

            // Fitting main transform in target predicted position for stand up
            if (getUpPosition == null)
            {
                root.position = User_ComputeGetUpPosition();
            }
            else
            {
                root.position = getUpPosition.Value;
            }

            if (snapToGround != null) // Place main model on ground point
            {
                RaycastHit hit;
                if (Physics.Raycast(new Ray(root.position, -up), out hit, PelvisToBase.magnitude, snapToGround.Value, QueryTriggerInteraction.Ignore))
                {
                    root.position = hit.point;
                }
            }


            //root.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((Vector3.Lerp(posingLeftLowerLeg.transform.position, posingRightLowerLeg.transform.position, 0.5f) - posingPelvis.transform.position).normalized * (getupType == EGetUpType.FromBack ? 1f : -1f), up), up);
            root.rotation = User_GetMappedRotationFor(getupType, up);
            //root.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(posingPelvis.transform.rotation * (getupType == EGetUpType.FromBack ? -PelvisLocalUp : PelvisLocalUp), up), up);

            posingPelvis.visibleBone.position = posingPelvis.transform.position;
            posingPelvis.visibleBone.rotation = posingPelvis.transform.rotation;

            if (FixRootInPelvis)
            {
                if (RagdollDummySkeleton.childCount > 0)
                {
                    Transform c = RagdollDummySkeleton.GetChild(0);
                    Vector3 preChildLoc = c.position;
                    Quaternion preChildRot = c.rotation;

                    RagdollDummySkeleton.transform.position = RootInParent.position;
                    RagdollDummySkeleton.transform.rotation = RootInParent.rotation;

                    c.position = preChildLoc;
                    c.rotation = preChildRot;
                }
            }

            RagdollDummyBase.position = root.position;
            RagdollDummyBase.rotation = root.rotation;

            RagdollDummySkeleton.SetParent(RagdollDummyRoot, true);
        }


        public RaycastHit ProbeGroundBelowHips(LayerMask mask, float distance = 10f, Vector3? worldUp = null)
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            RaycastHit result = new RaycastHit();

            Physics.Raycast(new Ray(posingPelvis.transform.position, -up), out result, distance, mask, QueryTriggerInteraction.Ignore);

            return result;
        }

        public bool User_GetUpPossible(Vector3 up)
        {
            return User_CanGetUp(up, true) != EGetUpType.None;
        }

        public bool User_GetUpPossible(EGetUpType getup)
        {
            return getup != EGetUpType.None;
        }

        public bool User_IsOnBack(Vector3 up)
        {
            return User_CanGetUp(up, false) == EGetUpType.FromBack;
        }

        public Quaternion User_GetMappedRotationHipsToHead(Vector3 up, bool checkIfOnBack = true)
        {
            if (checkIfOnBack)
            {
                if (User_IsOnBack(up))
                    return Quaternion.LookRotation(Vector3.ProjectOnPlane(-(posingHead.transform.position - posingPelvis.transform.position), up), up);
                else
                    return Quaternion.LookRotation(Vector3.ProjectOnPlane(posingHead.transform.position - posingPelvis.transform.position, up), up);
            }
            else
                return Quaternion.LookRotation(Vector3.ProjectOnPlane(posingHead.transform.position - posingPelvis.transform.position, up), up);
        }


        public Quaternion User_GetMappedRotation(Vector3 up)
        {
            float dot = Vector3.Dot(User_PelvisWorldForward(), up);

            if (dot > 0.6f) return User_GetMappedRotationHipsToHead(up);
            else if (dot < -0.6f) return User_GetMappedRotationHipsToHead(up);

            return Quaternion.LookRotation(Vector3.ProjectOnPlane(posingPelvis.transform.rotation * PelvisLocalForward, up), up);
        }

        public Quaternion User_GetMappedRotationFor(EGetUpType getupType, Vector3 up)
        {
            return Quaternion.LookRotation(Vector3.ProjectOnPlane(posingPelvis.transform.rotation * (getupType == EGetUpType.FromBack ? -PelvisLocalUp : PelvisLocalUp), up), up);
        }

        public bool User_IsPelvisKinematic()
        {
            return posingPelvis.rigidbody.isKinematic;
        }

        private void SetPosingParams(PosingBone bone, float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            bone.user_internalMusclePower = muscleAmount;
            bone.user_internalMuscleMultiplier = muscleMultiplier;
            bone.user_internalRagdollBlend = onRagdoll;
        }

        public void User_SetAllParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            User_SetArmsParams(muscleAmount, muscleMultiplier, onRagdoll);
            User_SetLegsParams(muscleAmount, muscleMultiplier, onRagdoll);
            User_SetSpineParams(muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_OverrideRagdollStateWithCurrentAnimationState()
        {
            CaptureAnimation();
        }

        public void User_SetArmsParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            User_SetLeftArmParams(muscleAmount, muscleMultiplier, onRagdoll);
            User_SetRightArmParams(muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_SetLeftArmParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            SetPosingParams(posingLeftForeArm, muscleAmount, muscleMultiplier, onRagdoll);
            SetPosingParams(posingLeftUpperArm, muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_SetRightArmParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            SetPosingParams(posingRightUpperArm, muscleAmount, muscleMultiplier, onRagdoll);
            SetPosingParams(posingRightForeArm, muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_SetLegsParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            User_SetLeftLegParams(muscleAmount, muscleMultiplier, onRagdoll);
            User_SetRightLegParams(muscleAmount, muscleMultiplier, onRagdoll);
        }

        public IEnumerator IE_User_FadeLegsParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f, float duration = 0.5f)
        {
            if (duration != 0f)
            {
                float startM = posingLeftUpperLeg.user_internalMusclePower;
                float startMul = posingLeftUpperLeg.user_internalMuscleMultiplier;
                float startBl = posingLeftUpperLeg.user_internalRagdollBlend;

                float elapsed = -0.0001f;

                while (elapsed < duration)
                {

                    yield return new WaitForFixedUpdate();
                    elapsed += Time.fixedDeltaTime;
                    if (elapsed > duration) elapsed = duration;

                    float blend = (elapsed / duration);
                    User_SetLegsParams(Mathf.Lerp(startM, muscleAmount, blend), Mathf.Lerp(startMul, muscleMultiplier, blend), Mathf.Lerp(startBl, onRagdoll, blend));

                }
            }

            User_SetLegsParams(muscleAmount, muscleMultiplier, onRagdoll);

            yield break;
        }

        public IEnumerator IE_User_FadePelvisParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f, float duration = 0.5f)
        {
            if (duration != 0f)
            {
                float startM = posingPelvis.user_internalMusclePower;
                float startMul = posingPelvis.user_internalMuscleMultiplier;
                float startBl = posingPelvis.user_internalRagdollBlend;

                float elapsed = -0.0001f;

                while (elapsed < duration)
                {

                    yield return new WaitForFixedUpdate();
                    elapsed += Time.fixedDeltaTime;
                    if (elapsed > duration) elapsed = duration;

                    float blend = (elapsed / duration);
                    User_SetPelvisParams(Mathf.Lerp(startM, muscleAmount, blend), Mathf.Lerp(startMul, muscleMultiplier, blend), Mathf.Lerp(startBl, onRagdoll, blend));

                }
            }

            User_SetPelvisParams(muscleAmount, muscleMultiplier, onRagdoll);

            yield break;
        }

        public void User_SetLeftLegParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            SetPosingParams(posingLeftUpperLeg, muscleAmount, muscleMultiplier, onRagdoll);
            SetPosingParams(posingLeftLowerLeg, muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_SetRightLegParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            SetPosingParams(posingRightUpperLeg, muscleAmount, muscleMultiplier, onRagdoll);
            SetPosingParams(posingRightLowerLeg, muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_SetSpineParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            SetPosingParams(posingChest, muscleAmount, muscleMultiplier, onRagdoll);
            SetPosingParams(posingHead, muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_SetPelvisParams(float muscleAmount = 1f, float muscleMultiplier = 1f, float onRagdoll = 1f)
        {
            SetPosingParams(posingPelvis, muscleAmount, muscleMultiplier, onRagdoll);
        }

        public void User_PoseAsInitalPose()
        {
            PosingBone c = posingPelvis.child;

            while (c != null)
            {
                c.transform.localRotation = c.initialLocalRotation;
                if (c.rigidbody) c.rigidbody.rotation = c.transform.rotation;
                c = c.child;
            }
        }

        public void User_PoseAsAnimator()
        {
            PosingBone c = posingPelvis.child;
            while (c != null)
            {
                c.transform.localRotation = c.animatorLocalRotation;
                if (c.rigidbody) c.rigidbody.rotation = c.transform.rotation;
                c = c.child;
            }
        }


        internal Bounds User_GetRagdollBonesStateBounds(bool fast = true)
        {

            float refScale = Vector3.Distance(Pelvis.position, Head.position) * 0.2f;
            Bounds b = new Bounds(posingPelvis.transform.position, new Vector3(refScale, refScale, refScale));

            if (fast && BonesSetupMode != EBonesSetupMode.CustomLimbs)
            {
                b.Encapsulate(posingLeftLowerLeg.transform.position);
                b.Encapsulate(posingRightLowerLeg.transform.position);
                b.Encapsulate(posingLeftForeArm.transform.position);
                b.Encapsulate(posingRightForeArm.transform.position);
                b.Encapsulate(posingHead.transform.position);
                if (posingLeftLowerLeg.transform.childCount > 0) b.Encapsulate(posingLeftLowerLeg.transform.GetChild(0).position);
                if (posingRightLowerLeg.transform.childCount > 0) b.Encapsulate(posingRightLowerLeg.transform.GetChild(0).position);
            }
            else
            {
                PosingBone c = posingPelvis.child;
                while (c != null)
                {
                    b.Encapsulate(c.transform.position);
                    if (c.transform.childCount > 0) b.Encapsulate(c.transform.GetChild(0).position);
                    c = c.child;
                }
            }

            return b;
        }

        bool IsSpinePosingBone(PosingBone b)
        {
            if (b == posingPelvis) return true;
            if (b == posingSpineStart) return true;
            if (b == posingChest) return true;
            if (b == posingHead) return true;
            return false;
        }

        public PosingBone User_GetPosingBoneWithRagdollDummyBone(Transform ragdollBone)
        {
            PosingBone c = posingPelvis;
            while (c != null)
            {
                if (c.transform == ragdollBone) return c;
                c = c.child;
            }

            return null;
        }

        public PosingBone User_GetPosingBoneByAnimatorBone(Transform animatorBone)
        {
            PosingBone c = posingPelvis;
            while (c != null)
            {
                if (c.AnimatorBone == animatorBone) return c;
                c = c.child;
            }

            return null;
        }


        public PosingBone User_GetNearestPosingBoneToPosition(Vector3 pos, bool fast = true, bool justSpine = false)
        {
            if (fast)
            {
                PosingBone c = posingPelvis.child;
                PosingBone nearestB = c;
                float nearestDist = float.MaxValue;

                while (c != null)
                {
                    if (justSpine) if (!IsSpinePosingBone(c)) { c = c.child; continue; }

                    if (c.rigidbody)
                    {
                        float dist = (pos - c.rigidbody.worldCenterOfMass).sqrMagnitude;
                        if (dist < nearestDist) { nearestDist = dist; nearestB = c; }
                    }

                    c = c.child;
                }

                return nearestB;
            }
            else
            {
                PosingBone c = posingPelvis.child;
                PosingBone nearestB = c;
                float nearestDist = float.MaxValue;

                while (c != null)
                {
                    if (justSpine) if (!IsSpinePosingBone(c)) { c = c.child; continue; }

                    if (c.collider)
                    {
                        float dist = (pos - c.collider.ClosestPoint(pos)).sqrMagnitude;
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearestB = c;
                        }
                    }

                    c = c.child;
                }

                return nearestB;
            }
        }

        public Collider User_GetNearestRagdollColliderToPosition(Vector3 pos, bool fast = true, bool justSpine = false)
        {
            return User_GetNearestPosingBoneToPosition(pos, fast, justSpine).collider;
        }

        public Rigidbody User_GetNearestRagdollRigidbodyToPosition(Vector3 pos, bool fast = true, bool justSpine = false)
        {
            return User_GetNearestPosingBoneToPosition(pos, fast, justSpine).rigidbody;
        }

        public Transform User_GetNearestAnimatorTransformBoneToPosition(Vector3 pos, bool fast = true, bool justSpine = false)
        {
            return User_GetNearestPosingBoneToPosition(pos, fast, justSpine).visibleBone;
        }


        /// <summary>
        /// Call it if your character died and you want to keep it in the current lying pose.
        /// This method is disabling unity's Animator! It's required to avoid playing standing animations.
        /// </summary>
        public void User_FreezeAndDestroyRagdollDummy(bool disableAnimator = true)
        {
            if (disableAnimator) if (animator) animator.enabled = false;

            PosingBone c = posingPelvis.child;

            while (c != null)
            {
                c.visibleBone.position = c.transform.position;
                c.visibleBone.rotation = c.transform.rotation;
                c = c.child;
            }

            GameObject.Destroy(RagdollDummyBase.gameObject);
        }

        /// <summary>
        /// Generates new list of Rigidbodies belonging to the physical ragdoll dummy
        /// </summary>
        public List<Rigidbody> User_GetAllRigidbodies()
        {
            List<Rigidbody> rigs = new List<Rigidbody>();
            PosingBone c = posingPelvis;

            while (c != null)
            {
                if (c.rigidbody != null) rigs.Add(c.rigidbody);
                c = c.child;
            }

            return rigs;
        }

        /// <summary>
        /// Generates new list of Rigidbodies belonging to the physical ragdoll dummy
        /// </summary>
        public List<PosingBone> User_GetAllRagdollDummyControlBones()
        {
            List<PosingBone> bones = new List<PosingBone>();
            PosingBone c = posingPelvis;
            bones.Add(c);

            while (c != null)
            {
                bones.Add(c);
                c = c.child;
            }

            return bones;
        }

    }
}