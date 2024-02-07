﻿//----------------------------------------------
// Flip Web Apps: Game Framework
// Copyright © 2016 Flip Web Apps / Mark Hewitt
//
// Please direct any bugs/comments/suggestions to http://www.flipwebapps.com
// 
// The copyright owner grants to the end user a non-exclusive, worldwide, and perpetual license to this Asset
// to integrate only as incorporated and embedded components of electronic games and interactive media and 
// distribute such electronic game and interactive media. End user may modify Assets. End user may otherwise 
// not reproduce, distribute, sublicense, rent, lease or lend the Assets. It is emphasized that the end 
// user shall not be entitled to distribute or transfer in any way (including, without, limitation by way of 
// sublicense) the Assets in any other way than as integrated components of electronic games and interactive media. 

// The above copyright notice and this permission notice must not be removed from any files.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using BeautifulTransitions.Scripts.Transitions.Components.GameObject;
using BeautifulTransitions.Scripts.Transitions.Editor.AbstractClasses;

namespace BeautifulTransitions.Scripts.Transitions.Editor.Components.GameObject
{
    [CustomEditor(typeof(TransitionMove))]
    public class TransitionMoveEditor : TransitionGameObjectBaseEditor
    {
        SerializedProperty _moveModeProperty;
        SerializedProperty _inStartPositionTypeProperty;
        SerializedProperty _inStartPositionProperty;
        SerializedProperty _outEndPositionTypeProperty;
        SerializedProperty _outEndPositionProperty;

        protected override void OnEnable()
        {
            _moveModeProperty = serializedObject.FindProperty("MoveMode");
            var inConfigProperty = serializedObject.FindProperty("InConfig");
            _inStartPositionTypeProperty = inConfigProperty.FindPropertyRelative("StartPositionType");
            _inStartPositionProperty = inConfigProperty.FindPropertyRelative("StartPosition");
            var outConfigProperty = serializedObject.FindProperty("OutConfig");
            _outEndPositionTypeProperty = outConfigProperty.FindPropertyRelative("EndPositionType");
            _outEndPositionProperty = outConfigProperty.FindPropertyRelative("EndPosition");
            base.OnEnable();
        }

        /// <summary>
        /// Override in subclasses to show common GUI elements.
        /// </summary>
        /// <returns></returns>
        protected override void ShowCommonGUI() {
            base.ShowCommonGUI();
            EditorGUILayout.PropertyField(_moveModeProperty);
        }

        /// <summary>
        /// Override in subclasses to show Transition In GUI elements.
        /// </summary>
        /// <returns></returns>
        protected override void ShowTransitionInGUI() {
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Move Specific", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_inStartPositionTypeProperty);
            EditorGUILayout.PropertyField(_inStartPositionProperty);
        }

        /// <summary>
        /// Override in subclasses to show Transition Out GUI elements.
        /// </summary>
        /// <returns></returns>
        protected override void ShowTransitionOutGUI() {
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Move Specific", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_outEndPositionTypeProperty);
            EditorGUILayout.PropertyField(_outEndPositionProperty);
        }
    }
}
