#if UNITY_EDITOR
using System.Diagnostics;
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;
#endif

namespace FIMSpace
{
    /// <summary> Simple class for performance measurement. 
    /// After making build, all body of this class disappears to make it weightless. 
    /// It's not inside editor directory to give possibility to use it in main assembly. </summary>
    public class FDebug_PerformanceTest
    {
#if UNITY_EDITOR
        Stopwatch watch = null;
        GameObject parent;
        long lastTicks = 0;
        long lastMS = 0;
#endif

        public FDebug_PerformanceTest()
        {
#if UNITY_EDITOR
            _foldout = false;
#endif
        }

        public void Start(UnityEngine.GameObject owner, bool onlyIfSelected = true)
        {
#if UNITY_EDITOR

            //if (_foldout == false) return;
            if (watch == null) watch = new Stopwatch();
            parent = owner;


            if (owner != null) if (onlyIfSelected) if (Selection.activeGameObject != parent) return;
            watch.Reset();
            watch.Start();
#endif
        }

        public void Pause()
        {
#if UNITY_EDITOR
            //if (_foldout == false) return;
            if (watch.IsRunning) watch.Stop();
#endif
        }

        public void Continue()
        {
#if UNITY_EDITOR
            //if (_foldout == false) return;
            if (!watch.IsRunning) watch.Start();
#endif
        }


        public void Finish(bool onlyIfSelected = true)
        {
#if UNITY_EDITOR
            //if (_foldout == false) return;
            if (watch.IsRunning == false) return;
            if (onlyIfSelected) if (Selection.activeGameObject != parent) return;
            watch.Stop();
            lastTicks = watch.ElapsedTicks;
            lastMS = watch.ElapsedMilliseconds;
            AddCurrentToAverage();
#endif
        }


#if UNITY_EDITOR

        const int AVERAGES_COUNT = 16;
        int currId = 0;
        long[] averageTicks = new long[AVERAGES_COUNT];

        void AddCurrentToAverage()
        {
            averageTicks[currId] = watch.ElapsedTicks;
            currId += 1;
            if (currId >= AVERAGES_COUNT) currId = 0;
        }

        long GetAverage(long[] list)
        {
            long averageSum = 0;
            int averageReads = 0;
            long max = long.MinValue; // remembering max value

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] <= 0) continue;
                averageSum += list[i];
                averageReads += 1;
                if (list[i] > max) { max = list[i]; }
            }

            averageSum -= max; // Remove extremum value to avoid processor peak values
            averageReads -= 1;

            if (averageReads <= 0) return 0;

            return averageSum / (long)averageReads;
        }

        public bool _foldout { get; private set; }

        public long AverageTicks { get { return GetAverage(averageTicks); } }

        public double TicksToMs(long ticks)
        {
            if (ticks <= 0) return 0;
            return 1000.0 * (double)ticks / Stopwatch.Frequency;
        }

        public double AverageMS { get { return TicksToMs(AverageTicks); } }

#endif


#if UNITY_EDITOR

        public void Editor_DisplayFoldoutButton(float yOffset = -20f, float xOffset = 4f)
        {
            var rct = GUILayoutUtility.GetLastRect();
            rct.width = 12;
            rct.height = 12;
            rct.position = new Vector2(rct.position.x + xOffset, rct.position.y + yOffset);

            Color preC = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            if (GUI.Button(rct, FGUI_Resources.Tex_Debug, EditorStyles.label)) { _foldout = true; }
            GUI.color = preC;
        }

        public void Editor_Display(string prefix = "", bool onlyPlaymode = true, bool drawAverages = true, float buttonYOffset = -20f, float buttonXOffset = 4f)
        {
            if (onlyPlaymode) if (!Application.isPlaying) return;

            if (!_foldout)
            {
                Editor_DisplayFoldoutButton(buttonYOffset, buttonXOffset);
                return;
            }

            Editor_DisplayAlways(prefix, false, drawAverages);
        }

        public void Editor_DisplayAlways(string prefix = "", bool onlyPlaymode = true, bool drawAverages = true)
        {
            if (onlyPlaymode) if (!Application.isPlaying) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (!drawAverages)
                EditorGUILayout.LabelField(prefix + "Elapsed Ticks: " + lastTicks + "  " + TicksToMs(lastTicks) + "ms");
            else
                EditorGUILayout.LabelField(prefix + "(average) Elapsed Ticks: " + AverageTicks + "  " + AverageMS + "ms");

            EditorGUILayout.EndVertical();
            var rect = GUILayoutUtility.GetLastRect();
            if (GUI.Button(rect, GUIContent.none, EditorStyles.label)) { _foldout = false; }
        }

#endif


    }
}
