using LiteNetLib;
using LiteNetLibManager;
using StandardAssets.Characters.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(OpenCharacterController))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class SwimRigidBodyEntityMovement : BaseEntityMovement
    {
        // Buffer to avoid character fall underground when teleport
        public const float GROUND_BUFFER = 0.16f;

        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public ApplyJumpForceMode applyJumpForceMode = ApplyJumpForceMode.ApplyImmediately;
        public float applyJumpForceFixedDuration;
        public float backwardMoveSpeedRate = 0.75f;
        public float gravity = 9.81f;
        public float maxFallVelocity = 40f;
        [Tooltip("Delay before character change from grounded state to airborne")]
        public float airborneDelay = 0.01f;
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionWhileNotMoving;

        public Animator CacheAnimator { get; private set; }
        public LiteNetLibTransform CacheNetTransform { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public CapsuleCollider CacheCapsuleCollider { get; private set; }
        public OpenCharacterController CacheOpenCharacterController { get; private set; }

        public Collider waterSurfaceCollider;

        private bool isFlying;
        public bool canFly;
        public float flySpeed = 1.5f;
        public float swimSpeed = 1f;

        public bool IsFlying
        {
            get
            {
                return isFlying;
            }
        }

        public bool IsUnderWater
        {
            get
            {
                return isUnderWater;
            }
        }
        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public Queue<Vector3> navPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }

        // Movement codes
        private PhysicFunctions physicFunctions;
        private float airborneElapsed;
        private bool isUnderWater;
        private bool isJumping;
        private bool applyingJumpForce;
        private float applyJumpForceCountDown;
        private Collider waterCollider;
        private float yRotation;

        // Optimize garbage collector
        private MovementState tempMovementState;
        private Vector3 tempInputDirection;
        private Vector3 tempMoveDirection;
        private Vector3 tempHorizontalMoveDirection;
        private Vector3 tempMoveVelocity;
        private Vector3 tempTargetPosition;
        private Vector3 tempCurrentPosition;
        private Vector3 tempPredictPosition;
        private float tempVerticalVelocity;
        private float tempSqrMagnitude;
        private float tempPredictSqrMagnitude;
        private float tempTargetDistance;
        private float tempEntityMoveSpeed;
        private float tempCurrentMoveSpeed;
        private CollisionFlags collisionFlags;

        private float submergence = 0f;

        private bool shouldDive = false;

        public override void EntityAwake()
        {
            physicFunctions = new PhysicFunctions(30);
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            // Prepare network transform component
            CacheNetTransform = gameObject.GetOrAddComponent<LiteNetLibTransform>();
            // Prepare rigidbody component
            CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            // Prepare collider component
            CacheCapsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();
            // Prepare open character controller
            float radius = CacheCapsuleCollider.radius;
            float height = CacheCapsuleCollider.height;
            Vector3 center = CacheCapsuleCollider.center;
            CacheOpenCharacterController = gameObject.GetOrAddComponent<OpenCharacterController>((comp) =>
            {
                comp.SetRadiusHeightAndCenter(radius, height, center, true, true);
            });
            // Setup
            StopMove();
        }

        public override void EntityStart()
        {
            yRotation = CacheTransform.eulerAngles.y;
            tempCurrentPosition = CacheTransform.position;
            tempCurrentPosition.y += GROUND_BUFFER;
            CacheOpenCharacterController.SetPosition(tempCurrentPosition, true);
            tempVerticalVelocity = 0;
        }

        public override void EntityLateUpdate()
        {
            base.EntityLateUpdate();
            // Setup network components
            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheNetTransform.ownerClientCanSendTransform = false;
                    CacheNetTransform.ownerClientNotInterpolate = false;
                    break;
                case MovementSecure.NotSecure:
                    CacheNetTransform.ownerClientCanSendTransform = true;
                    CacheNetTransform.ownerClientNotInterpolate = true;
                    break;
            }
        }

        public override void ComponentOnEnable()
        {
            CacheNetTransform.enabled = true;
            CacheOpenCharacterController.enabled = true;
            CacheOpenCharacterController.SetPosition(CacheTransform.position, true);
            tempVerticalVelocity = 0;
            CacheNetTransform.onTeleport += OnTeleport;
        }

        public override void ComponentOnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheOpenCharacterController.enabled = false;
            CacheNetTransform.onTeleport -= OnTeleport;
        }

        protected void OnTeleport(Vector3 position, Quaternion rotation)
        {
            CacheOpenCharacterController.SetPosition(position, true);
            tempVerticalVelocity = 0;
        }

        protected void OnAnimatorMove()
        {
            if (!CacheAnimator)
                return;

            if (useRootMotionWhileNotMoving &&
                !CacheEntity.MovementState.HasFlag(MovementState.Forward) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Backward) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Left) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Right) &&
                !CacheEntity.MovementState.HasFlag(MovementState.IsJump))
            {
                // No movement, apply root motion position / rotation
                CacheAnimator.ApplyBuiltinRootMotion();
                return;
            }

            if (CacheEntity.MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
            if (!CacheEntity.MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForAirMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Register Network functions
            RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            RegisterNetFunction<DirectionVector3, MovementState>(NetFuncKeyMovement);
            RegisterNetFunction<short>(NetFuncUpdateYRotation);
            RegisterNetFunction(StopMove);
        }

        protected void NetFuncKeyMovement(DirectionVector3 inputDirection, MovementState movementState)
        {
            if (!CacheEntity.CanMove())
                return;
            tempInputDirection = inputDirection;
            tempMovementState = movementState;
            if (tempInputDirection.sqrMagnitude > 0)
                navPaths = null;
            if (canFly && !CacheOpenCharacterController.isGrounded && !isUnderWater && tempMovementState.HasFlag(MovementState.IsJump))
            {
                isFlying = true;
                isJumping = false;
                applyingJumpForce = false;
            }
            if (!isJumping && !applyingJumpForce)
                isJumping = (CacheOpenCharacterController.isGrounded || isUnderWater) && tempMovementState.HasFlag(MovementState.IsJump);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;
            tempMovementState = MovementState.Forward;
            SetMovePaths(position, true);
        }

        protected void NetFuncUpdateYRotation(short yRotation)
        {
            if (!CacheEntity.CanMove())
                return;
            if (!HasNavPaths)
            {
                this.yRotation = yRotation;
                UpdateRotation();
            }
        }

        public override void StopMove()
        {
            navPaths = null;
            if (IsOwnerClient && !IsServer)
                CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public override void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CallNetFunction(NetFuncKeyMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, new DirectionVector3(moveDirection), movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    tempMovementState = movementState;
                    if (tempInputDirection.sqrMagnitude > 0)
                        navPaths = null;
                    if (canFly && !CacheOpenCharacterController.isGrounded && !isUnderWater && tempMovementState.HasFlag(MovementState.IsJump))
                    {
                        isFlying = true;
                        isJumping = false;
                        applyingJumpForce = false;
                    }
                    if (!isJumping && !applyingJumpForce)
                        isJumping = (isUnderWater || CacheOpenCharacterController.isGrounded) && tempMovementState.HasFlag(MovementState.IsJump);
                    break;
            }
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    tempMovementState = MovementState.Forward;
                    SetMovePaths(position, true);
                    break;
            }
        }

        public override void SetLookRotation(Quaternion rotation)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Cast to short to reduce packet size
                    CallNetFunction(NetFuncUpdateYRotation, DeliveryMethod.Sequenced, FunctionReceivers.Server, (short)rotation.eulerAngles.y);
                    break;
                case MovementSecure.NotSecure:
                    if (!HasNavPaths)
                        yRotation = rotation.eulerAngles.y;
                    break;
            }
        }

        public override Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, yRotation, 0f);
        }

        public override void Teleport(Vector3 position)
        {
            CacheNetTransform.Teleport(position + (Vector3.up * GROUND_BUFFER), Quaternion.Euler(0, CacheEntity.MovementTransform.eulerAngles.y, 0));
        }

        public override bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            int foundCount = physicFunctions.RaycastDown(fromPosition, Physics.DefaultRaycastLayers, findDistance, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < foundCount; ++i)
            {
                if (physicFunctions.GetRaycastTransform(i).root == CacheTransform.root)
                    continue;
                result = physicFunctions.GetRaycastPoint(i);
                return true;
            }
            return false;
        }

        public override void EntityUpdate()
        {
            if ((CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer) ||
                (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient))
                return;

            UpdateMovement(Time.deltaTime);

            tempMovementState = tempMoveDirection.sqrMagnitude > 0f ? tempMovementState : MovementState.None;
            if (isUnderWater)
                tempMovementState |= MovementState.IsUnderWater;
            if (CacheOpenCharacterController.isGrounded || airborneElapsed < airborneDelay)
                tempMovementState |= MovementState.IsGrounded;
            CacheEntity.SetMovement(tempMovementState);
        }


        private void UpdateMovement(float deltaTime)
        {
            tempMoveVelocity = Vector3.zero;
            tempMoveDirection = Vector3.zero;
            tempTargetDistance = 0f;

            // Update airborne elasped
            if (CacheOpenCharacterController.isGrounded)
                airborneElapsed = 0f;
            else
                airborneElapsed += deltaTime;

            bool isGrounded = CacheOpenCharacterController.isGrounded || airborneElapsed < airborneDelay;
            if (isGrounded || !canFly)
                isFlying = false;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = navPaths.Peek();
                tempCurrentPosition = CacheTransform.position;
                tempTargetPosition.y = 0;
                tempCurrentPosition.y = 0;
                tempMoveDirection = tempTargetPosition - tempCurrentPosition;
                tempMoveDirection.Normalize();
                tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                if (tempTargetDistance < StoppingDistance)
                {
                    navPaths.Dequeue();
                    if (!HasNavPaths)
                        StopMove();
                }
                else
                {
                    // Turn character to destination
                    yRotation = Quaternion.LookRotation(tempMoveDirection).eulerAngles.y;
                }
            }

            // If move by WASD keys, set move direction to input direction
            if (tempInputDirection.sqrMagnitude > 0f)
            {
                tempMoveDirection = tempInputDirection;
                tempMoveDirection.Normalize();
            }

            if (!CacheEntity.CanMove())
            {
                tempMoveDirection = Vector3.zero;
                isJumping = false;
                applyingJumpForce = false;
            }

            // Prepare movement speed
            tempEntityMoveSpeed = applyingJumpForce ? 0f : CacheEntity.GetMoveSpeed();
            if (isUnderWater)
                isFlying = false;
            tempCurrentMoveSpeed = tempEntityMoveSpeed * (isFlying ? flySpeed : isUnderWater ? swimSpeed : 1f);
            // Calculate vertical velocity by gravity
            if (!isGrounded && !isFlying)
            {
                if (!isUnderWater || submergence <= underWaterThreshold)
                {
                    if (!useRootMotionForFall)
                        tempVerticalVelocity = Mathf.MoveTowards(tempVerticalVelocity, -maxFallVelocity, gravity * deltaTime);
                    else
                        tempVerticalVelocity = 0f;
                }
                else
                    tempVerticalVelocity = 0f;
            }
            else
                tempVerticalVelocity = 0f;

            // Jumping 
            if (!isFlying && (isUnderWater || isGrounded) && isJumping)
            {
                if (!isUnderWater || submergence <= underWaterThreshold)
                {
                    airborneElapsed = airborneDelay;
                    CacheEntity.CallAllPlayJumpAnimation();
                    applyingJumpForce = true;
                    applyJumpForceCountDown = 0f;
                    switch (applyJumpForceMode)
                    {
                        case ApplyJumpForceMode.ApplyAfterFixedDuration:
                            applyJumpForceCountDown = applyJumpForceFixedDuration;
                            break;
                        case ApplyJumpForceMode.ApplyAfterJumpDuration:
                            if (CacheEntity.Model is IJumppableModel)
                                applyJumpForceCountDown = (CacheEntity.Model as IJumppableModel).GetJumpAnimationDuration();
                            break;
                    }
                }
            }

            if (applyingJumpForce)
            {
                applyJumpForceCountDown -= Time.deltaTime;
                if (applyJumpForceCountDown <= 0f)
                {
                    applyingJumpForce = false;
                    if (!useRootMotionForJump)
                        tempVerticalVelocity = CalculateJumpVerticalSpeed();
                }
            }
            // Updating horizontal movement (WASD inputs)
            if (tempMoveDirection.sqrMagnitude > 0f)
            {
                // Calculate only horizontal move direction
                tempHorizontalMoveDirection = tempMoveDirection;
                tempHorizontalMoveDirection.y = 0;
                tempHorizontalMoveDirection.Normalize();

                // If character move backward
                if (Vector3.Angle(tempHorizontalMoveDirection, CacheTransform.forward) > 120)
                    tempCurrentMoveSpeed *= backwardMoveSpeedRate;

                if (HasNavPaths)
                {
                    // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (tempHorizontalMoveDirection * tempCurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                    tempMoveVelocity = tempHorizontalMoveDirection * tempCurrentMoveSpeed;
                }
                else
                {
                    // Move with wasd keys so it does not have to adjust speed
                    tempMoveVelocity = tempHorizontalMoveDirection * tempCurrentMoveSpeed;
                }
            }

            // Updating vertical movement (Fall, WASD inputs under water)
            if (isUnderWater || isFlying)
            {
                if (submergence >= underWaterThreshold || shouldDive || isFlying)
                {
                    tempMoveVelocity.y = tempMoveDirection.y * tempCurrentMoveSpeed;
                    shouldDive = false;
                }
                else
                {
                    float distanceFromThreshold = underWaterThreshold - submergence;
                    float bouyantVelocity = 0.01f;
                    if (distanceFromThreshold > 0.01)
                        bouyantVelocity = -(distanceFromThreshold / deltaTime / tempCurrentMoveSpeed);
                    tempMoveVelocity.y = bouyantVelocity + (tempVerticalVelocity > 0 ? tempVerticalVelocity : 0f);
                }
                if (InputManager.GetButton("Crouch") || tempMoveDirection.y < -0.8)
                    shouldDive = true;
            }
            else
            {
                // Update velocity while not under water
                tempMoveVelocity.y = tempVerticalVelocity;
            }

            collisionFlags = CacheOpenCharacterController.Move(tempMoveVelocity * deltaTime);
            if ((collisionFlags & CollisionFlags.CollidedBelow) == CollisionFlags.CollidedBelow ||
                (collisionFlags & CollisionFlags.CollidedAbove) == CollisionFlags.CollidedAbove)
            {
                // Hit something below or above, falling in next frame
                tempVerticalVelocity = 0f;
            }

            UpdateRotation();
            isJumping = false;
        }

        protected void UpdateRotation()
        {
            CacheTransform.eulerAngles = new Vector3(0f, yRotation, 0f);
        }

        protected void SetMovePaths(Vector3 position, bool useNavMesh)
        {
            if (useNavMesh)
            {
                NavMeshPath navPath = new NavMeshPath();
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(position, out navHit, 5f, NavMesh.AllAreas) &&
                    NavMesh.CalculatePath(CacheTransform.position, navHit.position, NavMesh.AllAreas, navPath))
                {
                    navPaths = new Queue<Vector3>(navPath.corners);
                    // Dequeue first path it's not require for future movement
                    navPaths.Dequeue();
                }
            }
            else
            {
                // If not use nav mesh, just move to position by direction
                navPaths = new Queue<Vector3>();
                navPaths.Enqueue(position);
            }
        }

        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * jumpHeight * gravity);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                waterCollider = other;
                float footToSurfaceDist = waterCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y;
                float currentThreshold = footToSurfaceDist / (CacheCapsuleCollider.bounds.size.y * underWaterThreshold);
                submergence = footToSurfaceDist / CacheCapsuleCollider.bounds.size.y;
                isUnderWater = currentThreshold >= underWaterThreshold;
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                waterCollider = other;
                float footToSurfaceDist = waterCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y;
                float currentThreshold = footToSurfaceDist / (CacheCapsuleCollider.bounds.size.y * underWaterThreshold);
                submergence = footToSurfaceDist / CacheCapsuleCollider.bounds.size.y;
                isUnderWater = currentThreshold >= underWaterThreshold;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other == waterCollider)
            {
                waterCollider = null;
                submergence = 1f;
                isUnderWater = false;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Applies Collider Settings To Controller")]
        public void AppliesColliderSettingsToController()
        {
            CapsuleCollider collider = gameObject.GetOrAddComponent<CapsuleCollider>();
            float radius = collider.radius;
            float height = collider.height;
            Vector3 center = collider.center;
            // Prepare open character controller
            OpenCharacterController controller = gameObject.GetOrAddComponent<OpenCharacterController>();
            controller.SetRadiusHeightAndCenter(radius, height, center, true, true);
        }
#endif
    }
}
