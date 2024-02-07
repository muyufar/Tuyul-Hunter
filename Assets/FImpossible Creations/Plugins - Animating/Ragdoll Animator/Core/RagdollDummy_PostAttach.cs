using System.Collections;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// This component will help attaching correct ragdoll dummy bone reference in playmode
    /// </summary>
    [AddComponentMenu("FImpossible Creations/Ragdoll Dummy Post Attach")]
    public class RagdollDummy_PostAttach : MonoBehaviour
    {
        public Joint joint;
        public RagdollAnimator ragdoll;

        private Transform tr;

        private void Reset()
        {
            joint = GetComponent<Joint>();
        }

        void Awake()
        {
            if (joint == null) joint = GetComponentInChildren<Joint>();

            if (joint.connectedBody != null)
                tr = joint.connectedBody.transform;
        }

        void Start()
        {
            if ( ragdoll == null)
            {
                UnityEngine.Debug.Log("[Ragdoll Animator] Post attach script don't have assigned target Ragdoll Animator! ("+name+")");
                Destroy(this);
                return;
            }

            StartCoroutine(DelayedStart());
        }


        IEnumerator DelayedStart()
        {
            yield return null;
            if (ragdoll != null)
            {
                Transform bone = ragdoll.Parameters.GetRagdollDummyBoneByAnimatorBone(tr);
                if (bone != null)
                {
                    Rigidbody rigidBody = bone.GetComponent<Rigidbody>();
                    joint.connectedBody = rigidBody;
                }
            }
        }

    }


    #region Editor Class
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(RagdollDummy_PostAttach))]
    public class RagdollDummy_PostAttachEditor : UnityEditor.Editor
    {
        public RagdollDummy_PostAttach Get { get { if (_get == null) _get = (RagdollDummy_PostAttach)target; return _get; } }
        private RagdollDummy_PostAttach _get;

        public override void OnInspectorGUI()
        {
            if (Get.ragdoll) if (Get.ragdoll.PreGenerateDummy)
                {
                    UnityEditor.EditorGUILayout.HelpBox("Your Ragdoll Animator is using Pre-Generate Ragdoll Dummy so you don't need Post Attach component!", UnityEditor.MessageType.Info);
                }
 
            UnityEditor.EditorGUILayout.HelpBox("This component will help attaching correct ragdoll dummy bone reference in playmode", UnityEditor.MessageType.None);
            serializedObject.Update();
            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion

}
