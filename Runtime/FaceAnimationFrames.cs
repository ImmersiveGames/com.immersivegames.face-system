using System;
using UnityEngine;

namespace ImmersiveGames.FaceSystem
{
    public sealed partial class FaceAnimation
    {
        [Serializable]
        public struct StateFrame
        {
            [SerializeField]
            [HideInInspector]
            private string frameId;

            [SerializeField]
            private float duration;

            public string FrameId => frameId;
            public float Duration => duration;
        }

        [Serializable]
        public struct SpriteFrame
        {
            [SerializeField]
            [HideInInspector]
            private string spriteId;

            [SerializeField]
            private float duration;

            public string SpriteId => spriteId;
            public float Duration => duration;
        }
    }

}
