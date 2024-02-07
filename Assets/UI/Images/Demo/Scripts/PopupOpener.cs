// Copyright (C) 2015 ricimi - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

// This class is responsible for creating and opening a popup of the given prefab and add
// it to the UI canvas of the current scene.
public class PopupOpener : MonoBehaviour
{
    public GameObject popupPrefab;

    public Canvas m_canvas;

    protected void Start()
    {
       m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    public virtual void OpenPopup()
    {
        var popup = Instantiate(popupPrefab) as GameObject;
        popup.SetActive(true);
        popup.transform.localScale = Vector3.zero;

        // BEGIN_MECANIM_HACK
        // This works around a Mecanim bug present in Unity 5.2.1 where
        // the animation does not start until a frame after the prefab
        // has been instantiated. See:
        // http://forum.unity3d.com/threads/unity-5-2-mecanim-transitions-not-working-the-same-as-5-1.353815
#if UNITY_5_2_1
        var animator = popup.GetComponent<Animator>();
        animator.Update(0.01f);
#endif
        // END_MECANIM_HACK

        popup.transform.SetParent(m_canvas.transform, false);
        popup.GetComponent<Popup>().Open();
    }
}
