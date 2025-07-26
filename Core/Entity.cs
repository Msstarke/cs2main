using System;
using System.Numerics;

namespace cs2aim.Core
{
    public class Entity
    {
        public IntPtr PawnAddress { get; set; }
        public IntPtr ControllerAddress { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 view { get; set; }
        public Vector3 position { get; set; }
        public Vector3 position2D { get; set; }
        public Vector3 viewPosition2D { get; set; }
        public Vector3 head { get; set; }
        public int Health { get; set; }
        public int Team { get; set; }
        public uint LifeState { get; set; }
        public float DistanceToLocal { get; set; }
        public float distance { get; set; }
        public bool spotted { get; set; }
    }
}
 