using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [UnityEditor.CustomEditor(typeof(RagdollAnimator))]
    public partial class RagdollAnimatorEditor : UnityEditor.Editor
    {
        public RagdollAnimator Get { get { if (_get == null) _get = (RagdollAnimator)target; return _get; } }
        private RagdollAnimator _get;

        private SerializedProperty sp_RagProcessor;
        private SerializedProperty sp_ObjectWithAnimator;

        private RagdollGenerator generator;
        public static Texture2D Tex_Rag { get { if (__texRag != null) return __texRag; __texRag = Resources.Load<Texture2D>("Ragdoll Animator/Ragdoll"); return __texRag; } }
        private static Texture2D __texRag = null;

        bool drawAdditionalSettings = true;
        bool drawCorrectionsSettings = false;
        bool triggerGenerateRagd = false;

        protected float lastViewWidth = 400;

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }


        private void OnEnable()
        {
            sp_RagProcessor = serializedObject.FindProperty("Processor");
            sp_ObjectWithAnimator = serializedObject.FindProperty("ObjectWithAnimator");

            if (Application.isPlaying)
            {
                if ( Get.Parameters._EditorCategory == RagdollProcessor.EViewCategory.Setup)
                {
                    Get.Parameters._EditorCategory = RagdollProcessor.EViewCategory.Play;
                }
            }
        }


        public override bool UseDefaultMargins()
        {
            return false;
        }


        public override void OnInspectorGUI()
        {
            lastViewWidth = EditorGUIUtility.currentViewWidth;

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);


            #region Commented but can be helpful later

            //if (Application.isPlaying)
            //{
            //    Animator a = Get.transform.GetComponentInChildren<Animator>();
            //    if (a)
            //    {
            //        if (a.enabled == false)
            //        {
            //            GUILayout.Space(4);
            //            EditorGUILayout.HelpBox("Unity Animator disabled - Ragdoll Animator will apply it's algorithms", MessageType.None);
            //            if (GUILayout.Button("Test Enable Animator")) { Get.User_SwitchAnimator(null, true); Get.User_SetAllKinematic(); }
            //            GUILayout.Space(4);
            //        }
            //        else
            //        {
            //            GUILayout.Space(4);
            //            EditorGUILayout.HelpBox("! Unity Animator enabled - Ragdoll Animator overrided - not doing anything", MessageType.None);
            //            if (GUILayout.Button("Test Disable Animator")) { Get.User_SwitchAnimator(); Get.User_SetAllKinematic(false); }
            //            GUILayout.Space(4);
            //        }
            //    }
            //}

            #endregion

            serializedObject.Update();

            Editor_DrawTweakFullGUI(sp_RagProcessor, Get.Parameters);

            GUILayout.Space(2);

            if (Get.Parameters._EditorCategory == RagdollProcessor.EViewCategory.Setup)
            {
                FGUI_Inspector.DrawUILine(0.4f, 0.3f, 1, 8);

                EditorGUILayout.PropertyField(sp_ObjectWithAnimator);
                var sp = sp_ObjectWithAnimator.Copy(); sp.Next(false);
                GUI.color = new Color(0.85f, 1f, 0.85f, Application.isPlaying ? 0.5f : 1f);
                GUI.backgroundColor = new Color(0.85f, 1f, 0.85f, 1f);
                EditorGUILayout.PropertyField(sp); //sp.Next(false);
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
                //EditorGUILayout.PropertyField(sp); sp.Next(false);
                //EditorGUILayout.PropertyField(sp); sp.Next(false);
            }


            Undo.RecordObject(target, "RagdollAnimator");

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();


            if (triggerGenerateRagd)
            {
                if (Get.PreGenerateDummy)
                {
                    Get.Parameters.PreGenerateDummy(Get.ObjectWithAnimator, Get.RootBone);
                }
                else
                {
                    Get.Parameters.RemovePreGeneratedDummy();
                }

                EditorUtility.SetDirty(Get);
                triggerGenerateRagd = false;
            }

            GUILayout.Space(-4);

            Get._Editor_Perf_Update.Editor_Display("Update", true, true, -12f, 0f);

            if (Get._Editor_Perf_Update._foldout)
            {
                Get._Editor_Perf_LateUpdate.Editor_DisplayAlways("Late Update");
                Get._Editor_Perf_FixedUpdate.Editor_DisplayAlways("Fixed Update");
            }
        }


        public void Editor_DrawTweakFullGUI(SerializedProperty sp_ragProcessor, RagdollProcessor proc)
        {
            Color bg = GUI.backgroundColor;

            EditorGUILayout.BeginHorizontal();

            Texture2D catIcon = FGUI_Resources.Tex_GearSetup;
            if (proc._EditorCategory == RagdollProcessor.EViewCategory.Play) catIcon = FGUI_Resources.Tex_Tweaks;
            else if (proc._EditorCategory == RagdollProcessor.EViewCategory.Extra) catIcon = FGUI_Resources.Tex_Extension;

            if (GUILayout.Button(catIcon, FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(28), GUILayout.Height(24) }))
            {
                proc._EditorCategory += 1;
                if ((int)proc._EditorCategory > 2) proc._EditorCategory = RagdollProcessor.EViewCategory.Setup;
            }

            GUI.backgroundColor = bg;

            EditorGUILayout.BeginHorizontal(FGUI_Resources.ViewBoxStyle);

            GUILayout.Space(6);
            EditorGUILayout.PropertyField(sp_ragProcessor.FindPropertyRelative("_EditorCategory"), GUIContent.none, GUILayout.Width(68));
            GUILayout.Space(4);

            string catInfo = "Setup Ragdoll Bones";
            if (proc._EditorCategory == RagdollProcessor.EViewCategory.Play) catInfo = "Ragdoll Factors";
            else if (proc._EditorCategory == RagdollProcessor.EViewCategory.Extra) catInfo = "Extra Helper Parameters";

            EditorGUILayout.LabelField(catInfo, FGUI_Resources.HeaderStyle);
            GUILayout.Space(1);

            EditorGUILayout.EndVertical();

            //if (proc._EditorCategory == RagdollProcessor.EViewCategory.Play)

            if (lastViewWidth < 400)
            {
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Tutorials, "Open Ragdoll Animator tutorial video on the youtube"), FGUI_Resources.ButtonStyle, GUILayout.Height(20), GUILayout.Width(24)))
                    OpenTutorialURL();
            }
            else
            {
                if (GUILayout.Button(new GUIContent(" Tutorial", FGUI_Resources.Tex_Tutorials), FGUI_Resources.ButtonStyle, GUILayout.Height(20), GUILayout.Width(90)))
                    OpenTutorialURL();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(7);

            if (proc._EditorCategory == RagdollProcessor.EViewCategory.Setup)
            {
                Tabs_DrawSetup(sp_ragProcessor, proc);
            }
            else if (proc._EditorCategory == RagdollProcessor.EViewCategory.Play)
            {
                Tabs_DrawTweaking(sp_ragProcessor, proc);
            }
            else if (proc._EditorCategory == RagdollProcessor.EViewCategory.Extra)
            {
                Tabs_DrawExtra(sp_ragProcessor, proc);
            }



            if (Get.Parameters.Pelvis != null)
            {
                bool layerWarn = false;
                if (Get.gameObject.layer == Get.Parameters.Pelvis.gameObject.layer) layerWarn = true;
                if (Get.Parameters.SpineStart) if (Get.gameObject.layer == Get.Parameters.SpineStart.gameObject.layer) layerWarn = true;
                if (Get.Parameters.LeftUpperArm) if (Get.gameObject.layer == Get.Parameters.LeftUpperArm.gameObject.layer) layerWarn = true;

                if (layerWarn)
                {
                    GUILayout.Space(7);
                    EditorGUILayout.HelpBox("WARNING! It seams your main object have the same layer as bone transforms! You should create layer with ignored collision between character model and skeleton bones!", MessageType.Warning);
                    GUILayout.Space(7);
                }
            }

        }

        void OpenTutorialURL()
        {
            Application.OpenURL("https://youtu.be/dC5h-kVR650");
        }


        void DisableOnPlay(bool disable = true)
        {
            if (Application.isPlaying)
            {
                if (disable)
                    GUI.color = new Color(.9f, .9f, .9f, .5f);
                else
                    GUI.color = Color.white;
            }
        }


        private void OnSceneGUI()
        {
            if (generator == null) return;
            generator.OnSceneGUI(Get.Parameters);
        }


    }
}
