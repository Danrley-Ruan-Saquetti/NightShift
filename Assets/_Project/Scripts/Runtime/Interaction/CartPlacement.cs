using UnityEngine;

namespace NightShift
{
    public static class CartPlacement
    {
        public static Pose ToRigSpace(Pose cart, Pose rig)
        {
            Quaternion toRig = Quaternion.Inverse(rig.rotation);
            return new Pose(toRig * (cart.position - rig.position), toRig * cart.rotation);
        }

        public static Pose FromRigSpace(Pose relative, Pose rig, float height)
        {
            Vector3 position = rig.position + rig.rotation * relative.position;
            position.y = height;
            return new Pose(position, rig.rotation * relative.rotation);
        }
    }
}
