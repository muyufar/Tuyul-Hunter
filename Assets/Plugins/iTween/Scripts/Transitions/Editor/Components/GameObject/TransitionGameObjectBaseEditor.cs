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

using UnityEditor;
using BeautifulTransitions.Scripts.Transitions.Components.GameObject.AbstractClasses;
using BeautifulTransitions.Scripts.Transitions.Editor.AbstractClasses;
using UnityEngine;

namespace BeautifulTransitions.Scripts.Transitions.Editor.Components.GameObject
{
    [CustomEditor(typeof(TransitionGameObjectBase), true)]
    public class TransitionGameObjectBaseEditor : TransitionBaseEditor
    {
        //SerializedProperty _targetTypeProperty;
        SerializedProperty _targetProperty;

        protected override void OnEnable()
        {
            //_targetTypeProperty = serializedObject.FindProperty("TargetType");
            _targetProperty = serializedObject.FindProperty("Target");
            base.OnEnable();
        }

        /// <summary>
        /// Override in subclasses to show common GUI elements.
        /// </summary>
        /// <returns></returns>
        protected override void ShowCommonGUI() {
            //EditorGUILayout.PropertyField(_targetTypeProperty, new GUIContent("Target", _targetTypeProperty.tooltip));
            //if (_targetTypeProperty.enumValueIndex == 1)
            //    EditorGUILayout.PropertyField(_targetProperty, new GUIContent(" ", _targetProperty.tooltip));
            EditorGUILayout.PropertyField(_targetProperty);
        }
    }
}
