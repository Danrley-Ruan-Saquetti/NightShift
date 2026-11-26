using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

namespace NightShift
{
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class CartFollower : MonoBehaviour
    {
        private const float CastSkin = 0.02f;
        private const float ContactTolerance = 0.001f;
        private const int MaxResolveIterations = 4;
        private const int QueryCapacity = 16;

        [SerializeField]
        private XRBodyTransformer bodyTransformer;
        [SerializeField]
        private Transform rig;
        [SerializeField]
        private Transform head;
        [SerializeField]
        private PlayerState playerState;
        [SerializeField]
        private BoxCollider body;
        [SerializeField]
        private LayerMask blockingLayers;
        [SerializeField]
        private float frontDistance = 0.5f;

        private readonly RaycastHit[] hits = new RaycastHit[QueryCapacity];
        private readonly Collider[] overlaps = new Collider[QueryCapacity];

        private XRGrabInteractable grabInteractable;
        private Rigidbody cartBody;
        private Pose relativePose;
        private bool isFollowing;

        private void Awake()
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
            cartBody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            bodyTransformer.beforeApplyTransformations += OnRigMoving;
            bodyTransformer.afterApplyTransformations += OnRigMoved;
        }

        private void OnDisable()
        {
            bodyTransformer.beforeApplyTransformations -= OnRigMoving;
            bodyTransformer.afterApplyTransformations -= OnRigMoved;
        }

        private void LeaveOutside(Closet closet)
        {
            Pose spot = closet.CartSpot;
            spot.position.y = cartBody.position.y;
            grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);
            Place(spot);
        }

        private void MoveWithRig()
        {
            Pose rigPose = new(rig.position, rig.rotation);
            Pose target = CartPlacement.FromRigSpace(relativePose, rigPose, cartBody.position.y);
            target.position = StopAtObstacle(target);
            target.position = ResolveOverlaps(target);
            Place(target);
        }

        private Vector3 StopAtObstacle(Pose target)
        {
            Vector3 targetCenter = GetBodyCenter(target);
            Vector3 origin = new(head.position.x, targetCenter.y, head.position.z);
            Vector3 path = targetCenter - origin;
            float length = path.magnitude;

            if (Mathf.Approximately(length, 0f))
            {
                return target.position;
            }

            Vector3 direction = path / length;
            Vector3 halfExtents = GetBodyHalfExtents() - Vector3.one * CastSkin;
            int boxHits = Physics.BoxCastNonAlloc(
                origin, halfExtents, direction, hits, target.rotation, length, blockingLayers, QueryTriggerInteraction.Ignore);
            float reach = FindNearestObstacle(boxHits, length);
            int lineHits = Physics.RaycastNonAlloc(
                origin, direction, hits, length, blockingLayers, QueryTriggerInteraction.Ignore);
            reach = FindNearestObstacle(lineHits, reach);

            return target.position - direction * (length - reach);
        }

        private float FindNearestObstacle(int hitCount, float limit)
        {
            float nearest = limit;

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].distance > 0f && !IsOwnCollider(hits[i].collider))
                {
                    nearest = Mathf.Min(nearest, hits[i].distance);
                }
            }

            return nearest;
        }

        private Vector3 ResolveOverlaps(Pose target)
        {
            Pose resolved = target;

            for (int i = 0; i <= MaxResolveIterations; i++)
            {
                if (!TryPushOut(ref resolved))
                {
                    return IsReachable(resolved)
                        ? resolved.position
                        : PlaceInFrontOfPlayer(target.rotation, target.position.y);
                }
            }

            return PlaceInFrontOfPlayer(target.rotation, target.position.y);
        }

        private bool IsReachable(Pose cart)
        {
            Vector3 center = GetBodyCenter(cart);
            Vector3 path = center - new Vector3(head.position.x, center.y, head.position.z);
            float length = path.magnitude;
            int hitCount = Physics.RaycastNonAlloc(
                center - path, path.normalized, hits, length, blockingLayers, QueryTriggerInteraction.Ignore);
            return Mathf.Approximately(FindNearestObstacle(hitCount, length), length);
        }

        private Vector3 PlaceInFrontOfPlayer(Quaternion rotation, float height)
        {
            Pose front = new(GetPositionInFront(height), rotation);
            front.position = StopAtObstacle(front);
            TryPushOut(ref front);
            return IsReachable(front) ? front.position : GetPositionUnderPlayer(rotation, height);
        }

        private Vector3 GetPositionUnderPlayer(Quaternion rotation, float height)
        {
            Vector3 centerOffset = GetBodyCenter(new Pose(Vector3.zero, rotation));
            Vector3 position = head.position - centerOffset;
            position.y = height;
            return position;
        }

        private bool TryPushOut(ref Pose pose)
        {
            int count = Physics.OverlapBoxNonAlloc(
                GetBodyCenter(pose),
                GetBodyHalfExtents(),
                overlaps,
                pose.rotation,
                blockingLayers,
                QueryTriggerInteraction.Ignore);
            bool isOverlapping = false;

            for (int i = 0; i < count; i++)
            {
                Collider obstacle = overlaps[i];

                if (IsOwnCollider(obstacle))
                {
                    continue;
                }

                Vector3 bodyPosition = pose.position + pose.rotation * transform.InverseTransformPoint(body.transform.position);

                if (Physics.ComputePenetration(
                    body, bodyPosition, pose.rotation * body.transform.localRotation,
                    obstacle, obstacle.transform.position, obstacle.transform.rotation,
                    out Vector3 direction, out float distance)
                    && distance > ContactTolerance)
                {
                    pose.position += Vector3.ProjectOnPlane(direction * distance, Vector3.up);
                    isOverlapping = true;
                }
            }

            return isOverlapping;
        }

        private Vector3 GetBodyCenter(Pose cart)
        {
            Vector3 localCenter = transform.InverseTransformPoint(body.transform.TransformPoint(body.center));
            return cart.position + cart.rotation * localCenter;
        }

        private Vector3 GetBodyHalfExtents()
        {
            return Vector3.Scale(body.size, body.transform.lossyScale) * 0.5f;
        }

        private Vector3 GetPositionInFront(float height)
        {
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            Vector3 position = head.position + forward * frontDistance;
            position.y = height;
            return position;
        }

        private bool IsOwnCollider(Collider candidate)
        {
            return candidate.attachedRigidbody == cartBody || candidate.transform.IsChildOf(transform);
        }

        private void Place(Pose pose)
        {
            cartBody.position = pose.position;
            cartBody.rotation = pose.rotation;
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            cartBody.linearVelocity = Vector3.zero;
            cartBody.angularVelocity = Vector3.zero;
        }

        private void OnRigMoving(XRBodyTransformer _)
        {
            isFollowing = grabInteractable.isSelected;

            if (isFollowing)
            {
                relativePose = CartPlacement.ToRigSpace(
                    new Pose(cartBody.position, cartBody.rotation),
                    new Pose(rig.position, rig.rotation));
            }
        }

        private void OnRigMoved(ApplyBodyTransformationsEventArgs _)
        {
            if (!isFollowing)
            {
                return;
            }

            Closet closet = playerState.FindClosetAroundHead();

            if (closet != null)
            {
                LeaveOutside(closet);
                return;
            }

            MoveWithRig();
        }
    }
}
