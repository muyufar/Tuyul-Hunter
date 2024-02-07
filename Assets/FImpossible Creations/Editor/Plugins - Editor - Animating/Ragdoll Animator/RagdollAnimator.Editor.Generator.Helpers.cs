using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static class RagdollExtHelpers
    {
        public static Rigidbody RagdollBody(this Transform t)
        {
            return t.GetComponent<Rigidbody>();
        }

        public static CapsuleCollider RagdollCollider(this Transform t)
        {
            return t.GetComponent<CapsuleCollider>();
        }

        public static BoxCollider RagdollBCollider(this Transform t)
        {
            return t.GetComponent<BoxCollider>();
        }

    }

}
