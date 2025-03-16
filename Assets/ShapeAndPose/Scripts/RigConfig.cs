using System;

namespace ShapeAndPose_ns
{
    [Serializable]
    public class RigConfig
    {
        public LimbConfig[] limbs;
    }

    [Serializable]
    public class LimbConfig
    {
        public string name;      // e.g., "arm_lft"
        public string[] joints;  // e.g., ["LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand"]
    }
}
