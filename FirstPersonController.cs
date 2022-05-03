using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using System.Collections;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {

        
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_StartRunSpeed;
        [SerializeField] private float initialRunSpeed;

        public Ray cursorDir;
        private float curRunSpeed;
        [SerializeField] private float RunStrafeFactor;
        [SerializeField] private float m_MaxRunSpeed;
        public float backwardsMoveSpeedFactor;
        [SerializeField] private float m_TimeToMaxRunSpeed;
        private float curTimeToMaxRunSpeed;
        [SerializeField] private AnimationCurve runRampUpCurve;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private float storeVert;
        private Camera m_Camera;
        public bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        public CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        private ControllerColliderHit curPlayerColliderHit;

        //to prevent phyisics objects from moving while being graplled over
        private GameObject ledgeHoldTarget;
        private Pickupable lhtarget;
        public float maxVelocityOfRigidToLedgeGrab;
        //tap to sprint
        private float curTapTime = -1f;
        [SerializeField] private float tapTime = 1f;

        /*
        //VaultStepVars
        [SerializeField] private float m_VaultStepDist = 0.5f;
        [SerializeField] private float m_VaultStepTopOffset = 1.08f;
        [SerializeField] private float m_VaultStepForwardDetectDist = 1.2f;
        [SerializeField] private float m_VaultStepTime = 0.15f;
        [SerializeField] private int m_VaultStepNumForwardRays = 4;
        */
        //JumpVars
        [SerializeField] private float airControlFactor;
        //erializeField] private float RunSpeedContributingToUpWardMotionFactor;
        private Vector3 airDir;

        //modifying to be complatible with new hold jump code
        private bool holdJump;
        [SerializeField] private float holdJumpTime;
        private float curHoldJumpTime;
        [SerializeField] private float holdJumpSpeed;
        [SerializeField] private AnimationCurve holdJumpSpeedCurve;


        [SerializeField] private float timeToFullJumpPower;
        [SerializeField] private float minJumpPower;
        private float curJumpPower;
        private float prevJumpPower;





        //momentum or speed relatvie slide on stop
        private Vector3 curMomentumDir;
        [SerializeField] private float momentumFactor;
        [SerializeField] private float momentumDec;
        [SerializeField] private bool disableMomentumOnWalk;
        [SerializeField] private float momentumRampUpTime;
        private float curMomentumRampUpTime;
        [SerializeField] private AnimationCurve momentumRampUpCurve;


        //jump buffer vars, let player jump after just entering air
        [SerializeField] private float jumpBufferTime = 0.3f;
        private float curJumpBufferTime;
        private bool jumpBuffer;

        //speedBleed
        private float prevRotation;
        [SerializeField] private float speedBleedMaxAngle;
        [SerializeField] private float speedBleedRecoveryFactor;
        private Quaternion qPrevRotation;
        [SerializeField] private float speedBleedDecFactor;
        [SerializeField] private float speedLossReaquire;
        private float speedBleedAngleDifSum;

        //crouch vars
        [SerializeField] private bool crouchToggle;
        [SerializeField] public float crouchHeight;
        [SerializeField] private float camDownDist;
        public float standHeight;
        private float standCamHeight;

        [SerializeField] private float crouchTime;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float unCrouchHeightAmount;
        [SerializeField] private float unCrouchDampenFactorReposInAir;

        private float curCrouchTime;
        private bool isCrouch;
        public bool tryCrouch;
        private bool tryUncrouch;




        //sliding vars
        //use crouch heights for controller, lower camera
        [SerializeField] private float slideCamPosY;
        [SerializeField] private float slideDownTime;
        private float curSlideDownTime;
        [SerializeField] private float slideSpeedDec;
        private bool isSliding;
        [SerializeField] private float slideSpeedTerminate; //slide time proportionaly to time spend running?
        private float curSlideTime;

        private Vector3 slideDir;
        private float curSlideSpeed;
        [SerializeField] private float slideSlopeAccelFactor;
        private bool cancelSlideIntoCrouch;
        [SerializeField] private float slideExtraSpeed;
        [SerializeField] private float slideStrafeSpeed;
        private bool cancelSlide;

        private IEnumerator coroutine;


        //lean vars
        private bool isLeanLeft;
        private bool isLeanRight;
        private float leanDirection;
        [SerializeField] private float leanDistSide;
        [SerializeField] private float leanDistDown;
        [SerializeField] private float leanTime;
        [SerializeField] private float leanCamRotation;
        [SerializeField] private float leanClipBuffer;
        private float curLeanTime;
        private bool tryLean;
        private bool isLean;
        private float curLeanDir;

        private GameObject pivot;
        private int playerMask;


        //vars to improve sprinting/slideing of buildings and falling (remove bhoping)
        private bool hasLeftGround;
        private float timeInAir;
        [SerializeField] private float FallSpeedTimeDecFactor;

        private float initSprintTime;

        [SerializeField] private float timeToRecover;
        private float curTimeToRecover;



        //long fall Recovery
        [SerializeField] private float moveSpeedDownToFallRecovery;
        [SerializeField] private Vector3 camPosDuringFallAnim;
        public AudioClip fallSound;
        private bool triggerFallRecover;
        private bool isFallRecovery;
        private float prevAirSpeedDown;
        private Vector3 prevRot;



        //Player controller now has an animator ftw!! can make really immersive sequences with this(handles conflicting feedback effortlessly)!
        [SerializeField] private Animator fpsAnimator;

        //smallStepUp Functionality
        //when near small stepup level and grounded, jumping steps you up the object.
        [SerializeField] private float m_StepUpDist = 0.5f;
        [SerializeField] private float m_StepUpTopOffset = 1.08f;
        [SerializeField] private float m_StepUpForwardDetectDist = 1.2f;
        // [SerializeField] private float m_StepUpTime = 0.15f;
        public float stepUpSpeed;
        [SerializeField] private int m_StepUpNumForwardRays = 3;
        [SerializeField] private float m_StepUpRayStepValue = 0.15f;
        [SerializeField] private float m_StepUpTransformOffset = 0.6f;
        [SerializeField] private String m_StepUpObjectTag = "ledge";


        //LedgeGrab
        [SerializeField] private float m_LedgeGrabDist = 0.5f;
        [SerializeField] private float m_LedgeGrabTopOffset = 1.08f;
        [SerializeField] private float m_LedgeGrabForwardDetectDist = 1.2f;
        //[SerializeField] private float m_LedgeGrabTime = 0.35f;
        //public float ledgeGrabSpeed;
        [SerializeField] private int m_LedgeGrabNumForwardRays = 6;
        [SerializeField] private float m_LedgeGrabRayStepValue = 0.15f;
        [SerializeField] private float m_LedgeGrabTransformOffset = 0.6f;
        [SerializeField] private String m_LedgeGrabObjectTag = "ledge";

        private bool needNewKeyPressToLedgeGrab;

        public float m_LedgeEndPointCrouchOffset;

        private bool ledgeCancelPhase;
        private float curLedgeCancelPhaseTime;
        public float ledgeCancelPhaseTime;
        public float ledgeGrabSlowUpSpeed;
        public float ledgeGrabCompletionSpeed;

        // private float m_CurLedgeGrabTime = 0;
        private bool m_TryLedgeGrab = false;
        private bool m_IsLedgeGrab = false;
        private Vector3 m_LedgeGrabEndPoint = Vector3.zero;
        private Vector3 m_LedgeGrabStartPoint = Vector3.zero;

        //forgot what these do
        public GameObject LedgeGrabGameObject;
        //public float slopeLedgeVal;
        public float hitNormalMaxDiffToBeALedge;

        [SerializeField] private Transform camAngleRawLocal;
        //private float m_CurStepUpTime;
        private bool m_TryStepUp;
        private bool m_IsStepUp;
        private Vector3 m_StepUpEndPoint;
        private Vector3 m_StepUpStartPoint;

        //interaction functinonality for climbing. disables movement
        public bool hold;
        private Interact playerInteract;

        public float forceToHitObjMult;


       

        // Use this for initialization
        private void Start()
        {
         
            needNewKeyPressToLedgeGrab = false;
            playerInteract = GetComponent<Interact>();
            hold = false;
            fpsAnimator = GetComponent<Animator>();
            initSprintTime = ((initialRunSpeed - m_StartRunSpeed) / (m_MaxRunSpeed - m_StartRunSpeed)) * m_TimeToMaxRunSpeed;
            //Debug.Log (initSprintTime);
            pivot = transform.Find("LeanPivot").gameObject;
            playerMask = (1 << 9) + (1 << 2) + (1 << 10); //had problem where invis trigger colliders were triggering raycasts for leedge vaulting. im moving them to igrnore raycast layer and using player mask as this

            playerMask = ~playerMask;

            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
            m_MouseLook.Init(transform, m_Camera.transform);
            curRunSpeed = m_StartRunSpeed;
            jumpBuffer = false;
            curJumpBufferTime = 0f;
            isCrouch = false;
            standHeight = m_CharacterController.height;
            standCamHeight = m_Camera.transform.localPosition.y;
            isLean = false;
            curJumpPower = timeToFullJumpPower;

 
        }


        // Update is called once per frame
        private void Update()
        {
            if (!CrossPlatformInputManager.GetButton("Jump")) {
                needNewKeyPressToLedgeGrab = false;
            }

            if (curTimeToRecover == 0)
            {
                RotateView();


            }
            // Debug.Log(camAngleRawLocal.eulerAngles);
            leanDirection = CrossPlatformInputManager.GetAxis("Lean");
            if (leanDirection != 0) {
                //tryLean = true;
                if (!isLean) {
                    //intialize things
                    curLeanTime = 0;
                    curLeanDir = leanDirection;
                    isLean = true;
                    //Debug.Log ("active");
                } else {

                    //do nothing?
                }

                //on key press, force the transition to occur


            }

            //do not allow jumping during fall recovery anim/ any anim we add here
            if (curTimeToRecover == 0) {
                // the jump state needs to read here to make sure it is not missed
                if (!m_Jump)
                {
                    m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                }

                if (m_Jump)
                {
                    /*
                    if (hold) {
                        hold = false;
                        //playerInteract.checkHold = false;
                    }
                    */
                    //detect if user holding jump down(high jump)
                    holdJump = true;
                }

                if (!CrossPlatformInputManager.GetButton("Jump"))
                {
                    holdJump = false; //holdjump can only be reenabled by 
                }

                if (CrossPlatformInputManager.GetButton("Jump") && (m_MoveDir.y >= -2 && m_Jumping)) //disable the step up if you arnt jumping because gamefeel wise feels wrong -> force ledge grab
                {
                    if (!needNewKeyPressToLedgeGrab)
                        m_TryStepUp = true;
                }



                if (CrossPlatformInputManager.GetButton("Jump")) {
                    if(!needNewKeyPressToLedgeGrab)
                        m_TryLedgeGrab = true;
                }
            }

            //Detect toggle crouch input
            if (CrossPlatformInputManager.GetButtonDown("Crouch")) {
                if (isCrouch) {
                    //cant directly alter crouch from here, to many possible conflicts
                    tryUncrouch = true;

                } else {
                    tryCrouch = true;
                }

            }



            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
         
            float speed;
            GetInput(out speed);
           // Debug.Log(transform.position);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x * ((!m_IsWalking) ? RunStrafeFactor : 1);
            desiredMove = desiredMove.normalized;


            //stepUp Rays
            Ray[] store = new Ray[m_StepUpNumForwardRays];
            Vector3 forward = transform.TransformDirection(Vector3.forward) * m_StepUpForwardDetectDist;

            for (int i = 0; i < m_StepUpNumForwardRays; i++)
            {
                Debug.DrawRay(transform.position + new Vector3(0, i * m_StepUpRayStepValue + m_StepUpTransformOffset, 0), forward);
                store[i] = new Ray(transform.position + new Vector3(0, i * m_StepUpRayStepValue + m_StepUpTransformOffset, 0), forward);
            }

            RaycastHit hit;
            Ray downRay;
            RaycastHit downhit;
            Vector3 downward = Vector3.down * m_StepUpDist;

            RaycastHit slopeHit;
            Ray slopeRay;
            //

            //ledge grab

            Ray[] ledgeStore = new Ray[m_LedgeGrabNumForwardRays];
            Vector3 ledgeforward = transform.TransformDirection(Vector3.forward) * m_LedgeGrabForwardDetectDist;

            for (int i = 0; i < m_LedgeGrabNumForwardRays; i++)
            {
                Debug.DrawRay(transform.position + new Vector3(0, i * m_LedgeGrabRayStepValue + m_LedgeGrabTransformOffset, 0), ledgeforward, Color.green);
                ledgeStore[i] = new Ray(transform.position + new Vector3(0, i * m_LedgeGrabRayStepValue + m_LedgeGrabTransformOffset, 0), ledgeforward);
            }

            RaycastHit ledgehit;
            Ray ledgedownRay;
            RaycastHit ledgedownhit;
            Vector3 ledgedownward = Vector3.down * m_LedgeGrabDist;

            RaycastHit ledgeslopeHit;
            Ray ledgeslopeRay;
            //

            //ledgegrab
            for (int i = ledgeStore.Length - 1; i >= 0; i--) {
                if (Physics.Raycast(ledgeStore[i], out ledgehit, m_LedgeGrabForwardDetectDist, playerMask)) {
                    if (ledgehit.collider.tag.Equals(m_LedgeGrabObjectTag)) {
                        ledgedownRay = new Ray(new Vector3(ledgehit.point.x, ledgehit.point.y + m_LedgeGrabDist, ledgehit.point.z) + transform.TransformDirection(Vector3.forward) * 0.01f, ledgedownward);
                        Debug.DrawRay(new Vector3(ledgehit.point.x, ledgehit.point.y + m_LedgeGrabDist, ledgehit.point.z) + transform.TransformDirection(Vector3.forward) * 0.01f, ledgedownward, Color.green);
                        if (Physics.Raycast(ledgedownRay, out ledgedownhit, m_LedgeGrabDist,playerMask)) {
                            if (m_TryLedgeGrab && !m_IsStepUp && !m_IsLedgeGrab) {
                                if (ledgehit.collider.Equals(ledgedownhit.collider)) {
                                    //Debug.Log(Mathf.Abs((hit.normal + downhit.normal).magnitude - 1.414f));
                                    //How to actually solve if it's a ledge or a slope, use the normals!!! the difference of this should be zero for a perfect ledge
                                    if (Mathf.Abs((ledgehit.normal + ledgedownhit.normal).magnitude - 1.414f) < hitNormalMaxDiffToBeALedge){

                                       
                                        
                                        //Debug.Log ("SUCCESS" + i);
                                        //is there enough room to ledge grab
                                        //need a ray origin that starts at top of collider,
                                        if (Physics.Raycast(ledgedownhit.point, Vector3.up, standHeight + 0.1f, playerMask)) {
                                            //we cant ledge grap like this
                                            //can we crouch?
                                            //does crouch heigh up hit?
                                            if (Physics.Raycast(ledgedownhit.point, Vector3.up, crouchHeight + 0.1f, playerMask)){
                                                //Debug.Log ("not enough space to make it, even crouching");
                                                //m_TryLedgeGrab = false;
                                                break;
                                            } else {
                                                //Debug.Log("gap can be made if crouch occurs");
                                                //in this case
                                                tryCrouch = true;
                                            }
                                        }

                                        m_LedgeGrabEndPoint = ledgedownhit.point + Vector3.up * m_LedgeGrabTopOffset;
                                        m_LedgeGrabStartPoint = transform.position;
                                        playerInteract.handRayEndPoint = ledgedownhit.point;
                                        //is there enough local room to ledge grab(ie spherecast at placement)
                                        RaycastHit SphereCheckRoom;
                                        Collider[] sphereCheckRoom = Physics.OverlapSphere(ledgedownhit.point + Vector3.up * m_LedgeGrabTopOffset, 0.3f, playerMask);
                                        for (int z = 0; z < sphereCheckRoom.Length; z++) {
                                            //Debug.Log(sphereCheckRoom[z]);
                                        }
                                        //Debug.Log(sphereCheckRoom.Length);
                                        if (sphereCheckRoom.Length != 0)
                                        {
                                            
                                            //no room, dont set ledge. DO SET LEDGE HOLD THOUGH
                                            Debug.Log("No room");
                                            /*
                                            m_IsLedgeGrab = false;
                                            hold = true;
                                            playerInteract.checkHold = true;
                                            ledgeCancelPhase = false;
                                            */
                                            m_IsLedgeGrab = true;
                                            fpsAnimator.SetTrigger("PullUp");
                                            ledgeCancelPhase = true;
                                            curLedgeCancelPhaseTime = 0;
                                            
                                            m_LedgeGrabEndPoint = ledgedownhit.point + Vector3.up * m_LedgeGrabTopOffset - 0.5f * transform.forward;

                                        }
                                        else {
                                            m_IsLedgeGrab = true;
                                            fpsAnimator.SetTrigger("PullUp");
                                            ledgeCancelPhase = true;
                                            curLedgeCancelPhaseTime = 0;
                                            //Debug.Log("Room exists");
                                        }
                                        playerInteract.holdPos = playerInteract.GetHoldPos(ledgedownhit, ledgehit);//ledgehit.point - Vector3.up * 0.8f + ledgehit.normal * 0.5f;
                                        needNewKeyPressToLedgeGrab = true;

                                        // this code stores the object being ledge grabed
                                        if (ledgehit.collider.gameObject.transform.childCount > 0)
                                        {
                                            ledgeHoldTarget = ledgehit.collider.gameObject.transform.GetChild(0).gameObject;
                                            lhtarget = ledgeHoldTarget.GetComponent<Pickupable>();
                                            //send it a message if not null
                                            if (lhtarget != null)
                                            {
                                                ledgeHoldTarget.SendMessage("freeze", true);
                                                ledgeCancelPhase = false; //prevent 
                                            }
                                        }
                                        else
                                        {
                                            ledgeHoldTarget = null;
                                            lhtarget = null;
                                        }
                                        //if not enough room for hold. we actually set ledge canel phase = to false

                                        RaycastHit checkForRoomHit;
                                        if (Physics.SphereCast(playerInteract.holdPos, 0.3f, Vector3.down, out checkForRoomHit, IsCrouched() ? 0.1f : 0.5f, playerMask))
                                        {
                                            //if it hits something, then there's a collider in the way of the hold
                                            //Debug.Log("No can do flapper jack");
                                            playerInteract.checkHold = false;
                                            //trigger a ledge grab at this position instead, but with no hold potential.
                                            //TriggerLedgeGrab(checkHit.point);
                                            ledgeCancelPhase = false;


                                        }
                                        else
                                        {
                                           // Debug.Log("That'l do nicely");
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    } else {
                        //Debug.Log ("FAIL" + i);
                    }
                }
            }
        
            //stepup let this code run seperately as well
            for (int i = store.Length - 1; i >= 0; i--){
                if (Physics.Raycast(store[i], out hit, m_StepUpForwardDetectDist,playerMask)){
                    
                    if (hit.collider.tag.Equals(m_StepUpObjectTag)){
                        downRay = new Ray(new Vector3(hit.point.x, hit.point.y + m_StepUpDist, hit.point.z) + transform.TransformDirection(Vector3.forward) * 0.01f, downward);
                        Debug.DrawRay(new Vector3(hit.point.x, hit.point.y + m_StepUpDist, hit.point.z) + transform.TransformDirection(Vector3.forward) * 0.01f, downward);
                        if (Physics.Raycast(downRay, out downhit, m_StepUpDist, playerMask)){
                            if (hit.collider.Equals(downhit.collider)){ 
                               // Debug.Log(Mathf.Abs((hit.normal + downhit.normal).magnitude - 1.414f));
                                //How to actually solve if it's a ledge or a slope, use the normals!!! the difference of this should be zero for a perfect ledge
                                if (Mathf.Abs((hit.normal + downhit.normal).magnitude - 1.414f) < hitNormalMaxDiffToBeALedge) {
                                    //code in here used to be outside of this else statement, malfunctinon might occur now
                                    if (m_TryStepUp && !m_IsStepUp && !m_IsLedgeGrab) {
                                        //m_TryStepUp = false;
                                        //Debug.Log ("SUCCESS" + i);
                                        //is there enough room to stepUp?
                                        //need a ray origin that starts at top of collider,
                                        if (Physics.Raycast(downhit.point, Vector3.up, standHeight + 0.1f, playerMask))
                                        {
                                            //we cant stepUpLike this
                                            //can we crouch?
                                            //does crouch heigh up hit?
                                            if (Physics.Raycast(downhit.point, Vector3.up, crouchHeight + 0.1f, playerMask))
                                            {
                                                //Debug.Log ("not enough space to make it, even crouching"); 
                                                break; //exit forloop

                                            }
                                            else
                                            {
                                                //Debug.Log("gap can be made if crouch occurs");
                                                //in this case
                                                tryCrouch = true;
                                            }
                                        }
                                        fpsAnimator.SetTrigger("stepUp");
                                        m_IsStepUp = true;
                                        m_Jump = false;
                                        m_StepUpEndPoint = downhit.point + Vector3.up * m_StepUpTopOffset;
                                        m_StepUpStartPoint = transform.position;
                                        needNewKeyPressToLedgeGrab = true;
                                        // this code stores the object being ledge grabed
                                        if (hit.collider.gameObject.transform.childCount > 0)
                                        {
                                            ledgeHoldTarget = hit.collider.gameObject.transform.GetChild(0).gameObject;
                                            lhtarget = ledgeHoldTarget.GetComponent<Pickupable>();
                                            //send it a message if not null
                                            if (lhtarget != null)
                                            {
                                                ledgeHoldTarget.SendMessage("freeze", true);
                                            }
                                        }
                                        else {
                                            ledgeHoldTarget = null;
                                            lhtarget = null;
                                        }
                                        

                                        // m_CurStepUpTime = m_StepUpTime;
                                    }

                                    break;
                                }

                               
                            }

                        }
                    }
                }
            }



            

            float angleDetermine = 0;
            //conflict resoloution
            if (m_Camera.transform.eulerAngles.x > 91)
            {
                angleDetermine = m_Camera.transform.eulerAngles.x - 360;

            }
            else {
                angleDetermine = m_Camera.transform.eulerAngles.x;
            }

            if (m_TryLedgeGrab && angleDetermine > 45) {
                m_IsLedgeGrab = false;
            }

            m_TryLedgeGrab = false;
            m_TryStepUp = false;


            if (m_IsLedgeGrab) {
                m_UseHeadBob = false;
				if ( transform.position == m_LedgeGrabEndPoint + ((isCrouch) ? (Vector3.down * m_LedgeEndPointCrouchOffset) : Vector3.zero)) {
					m_IsLedgeGrab = false;
					transform.position = m_LedgeGrabEndPoint;
                    //dont send out this if it wasnt initalized, should have been a method, oh well
                    if (ledgeHoldTarget != null)
                        ledgeHoldTarget.SendMessage("freeze", false);

                }
                else{
                    //2 states for this motion. 
                    //intialcancel phase
                    //slower, upward movement only
                    //if jump key released. we cancel and trigger a ledge hold at a specific point
                    //completion phase, quick, jump can be released
                    if (ledgeCancelPhase)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, m_LedgeGrabStartPoint + Vector3.up * 0.5f, Time.fixedDeltaTime * ((!m_IsWalking)?speed:ledgeGrabSlowUpSpeed));
                        curLedgeCancelPhaseTime += Time.fixedDeltaTime;
                        if (curLedgeCancelPhaseTime > ((!m_IsWalking)?(ledgeCancelPhaseTime/2.5f):ledgeCancelPhaseTime)) {
                            ledgeCancelPhase = false;
                            
                            
                        }

                        if (!CrossPlatformInputManager.GetButton("Jump")) {
                            m_IsLedgeGrab = false;
                            hold = true;
                            playerInteract.checkHold = true;
                        }

                    }
                    else {
                        transform.position = Vector3.MoveTowards(transform.position, m_LedgeGrabEndPoint + ((isCrouch)?(Vector3.down * m_LedgeEndPointCrouchOffset) :Vector3.zero), Time.fixedDeltaTime * ((!m_IsWalking)?speed:ledgeGrabCompletionSpeed));
                        
                    }
                    

                    
				}
			}

            if (m_IsStepUp)
            {
                m_UseHeadBob = false;
                if (storeVert == -1)
                {
                    m_IsStepUp = false;
                    if (ledgeHoldTarget != null)
                        ledgeHoldTarget.SendMessage("freeze", false);

                }
                if (/*m_CurStepUpTime < 0*/ transform.position == m_StepUpEndPoint)
                {
                    m_UseHeadBob = true;
                    m_IsStepUp = false;
                    transform.position = m_StepUpEndPoint;
                    if (ledgeHoldTarget != null)
                        ledgeHoldTarget.SendMessage("freeze", true);

                }
                else
                {
                    //m_CurStepUpTime -= Time.fixedDeltaTime;
                    //transform.position = Vector3.Lerp(m_StepUpStartPoint, m_StepUpEndPoint, 1 - (m_CurStepUpTime / m_StepUpTime)); Use movetowards, makes more sense
                    transform.position = Vector3.MoveTowards(transform.position, m_StepUpEndPoint, Time.fixedDeltaTime * ((!m_IsWalking) ? speed : stepUpSpeed));

                }
            }
         
            //Debug.Log (m_CharacterController.collisionFlags);

          
			//*************************************************************************************************************************************************************************************
			//crouch code
			if (!cancelSlide) {
                

				if(tryCrouch && isSliding){
					//disable the slide into crouch
					isSliding = false;
					isCrouch = true;
					curCrouchTime = crouchTime;
					m_UseHeadBob = false;
					cancelSlide = true;
					cancelSlideIntoCrouch = true;
					curSlideDownTime = slideDownTime;

				} else if (tryCrouch && m_IsWalking) {
					isCrouch = true;
					tryCrouch = false;
					//initialize vars
					curCrouchTime = 0;
					m_IsWalking = true;
					//disable and reenable head bob during interpolation crouch
					m_UseHeadBob = false;

				} else if (tryCrouch && !m_IsWalking && speed <= slideSpeedTerminate + 0.5f) {
					//fail slide
					isCrouch = true;
					tryCrouch = false;
					//initialize vars
					curCrouchTime = 0;
					m_IsWalking = true;
					//disable and reenable head bob during interpolation crouch
					m_UseHeadBob = false;


				} else if (tryCrouch && !m_IsWalking && speed > slideSpeedTerminate + 0.5f) {

					//successful slide
					tryCrouch = false;
					isSliding = true;
					m_UseHeadBob = false;
					slideDir = m_MoveDir.normalized;
					curSlideSpeed = speed +slideExtraSpeed;
					//curSlideTime = 0;
					curSlideDownTime = 0;
				}

				//IF sliding inherit motion. but speed decreases


				if (tryUncrouch) {
					//test if enough room to uncrouch


					//Debug.Log()
					//currently not masking this. ie, weird things may prevent crouching ATM
					if (Physics.Raycast (transform.position, Vector3.up, unCrouchHeightAmount, (1<<8))) {  //layer mask 8. player colliders here
						//dont uncrouch
						tryUncrouch = false;

					} else {
						//empty space above, uncrouch
						tryUncrouch = false;
						isCrouch = false;
						curCrouchTime = crouchTime;
						//Debug.Log ("True");
						m_UseHeadBob = false;
                        
                        
					}


				}
			
			} else {
				tryCrouch = false;
				tryUncrouch = false;
			}

			//Debug.Log (m_Camera.transform.localPosition);

			if (isCrouch && (curCrouchTime < crouchTime)) {
				//interpolate the height to crouch height
				m_Camera.transform.localPosition = new Vector3(0,Mathf.Lerp(standCamHeight, camDownDist, (curCrouchTime/crouchTime)),0);
				m_CharacterController.height = Mathf.Lerp(standHeight, crouchHeight, (curCrouchTime/crouchTime));
				curCrouchTime += Time.fixedDeltaTime * (m_IsWalking?1f:9000f);

                //modify hold pos
                
			} else if (isCrouch) {
				m_CharacterController.height = crouchHeight;
				m_UseHeadBob = true;
				m_OriginalCameraPosition = new Vector3 (0,camDownDist, 0);
				m_Camera.transform.localPosition = new Vector3 (0, camDownDist, 0);
				m_HeadBob.changeOrigPos (camDownDist); //i created a method for headbob that lets me change the head bob "center" where it resets to
			}

			//Debug.Log (curCrouchTime);

			if (!isCrouch && curCrouchTime > 0) {
                
                m_Camera.transform.localPosition =  new Vector3(0,Mathf.Lerp(camDownDist,standCamHeight, 1- (curCrouchTime/crouchTime)),0);
				float diff = Mathf.Lerp (crouchHeight, standHeight, 1 - (curCrouchTime / crouchTime)) - m_CharacterController.height;
				m_CharacterController.height = Mathf.Lerp (crouchHeight, standHeight, 1 - (curCrouchTime / crouchTime));
				m_CharacterController.transform.position = m_CharacterController.transform.position + new Vector3 (0, diff * ((m_CharacterController.isGrounded)?1:unCrouchDampenFactorReposInAir), 0);
				curCrouchTime -= Time.fixedDeltaTime;
			} else if (!isCrouch) {

				m_UseHeadBob = true;
				m_OriginalCameraPosition = new Vector3 (0, standCamHeight, 0);
				m_Camera.transform.localPosition = new Vector3 (0, standCamHeight, 0);
				m_HeadBob.changeOrigPos (standCamHeight);
			}

			if (isCrouch) {
				curRunSpeed = m_StartRunSpeed;

			}

			//end of crouch code
			//*************************************************************************************************************************************************************************************
			
			//Commmented out because it caused edge sliding************************************************************
            // get a normal for the surface that is being touched to move along it
            //RaycastHit hitInfo;
            //Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                             //  m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            //desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;

			//Debug.Log (desiredMove.magnitude);
			if (desiredMove.magnitude == 0 && curMomentumDir.magnitude > (Time.fixedDeltaTime*momentumDec)) {
				curMomentumDir = curMomentumDir.normalized * (curMomentumDir.magnitude - Time.fixedDeltaTime*momentumDec);
				m_MoveDir.x = curMomentumDir.x;
				m_MoveDir.z = curMomentumDir.z;

			} else if(desiredMove.magnitude == 0){
				curMomentumDir = new Vector3 (0, 0, 0);
		
			}

			if (!m_CharacterController.isGrounded) {

                
               
               airDir.x += desiredMove.x * speed * airControlFactor;
               airDir.z += desiredMove.z * speed * airControlFactor; //dont add more to transform.forward dir...!!
                

                //this speed clamp is critical
                //Debug.Log(curRunSpeed);
               
                if (m_IsWalking)
                {
                    if (airDir.magnitude >= m_WalkSpeed)
                    {
                        airDir = airDir.normalized * m_WalkSpeed;
                    }
                }
                else {
                    if (airDir.magnitude >= curRunSpeed) {
                        airDir = airDir.normalized * curRunSpeed;
                    }

                }
                
                
                m_MoveDir.x = airDir.x;//+ desiredMove.x*speed*airControlFactor;
                m_MoveDir.z = airDir.z; //+ desiredMove.z*speed*airControlFactor;

                //hmm, air control is letting us jump to far.
                
				//But, we want the jump to inherit momentum
				//sol1 we make airDir effected by desired Move

                

			}
            //Debug.Log(airDir);

            
			if(m_Jumping && holdJump && (curHoldJumpTime < holdJumpTime)){
				m_MoveDir.y += holdJumpSpeed * holdJumpSpeedCurve.Evaluate(curHoldJumpTime/holdJumpTime) /*  prevJumpPower*/;
				curHoldJumpTime += Time.fixedDeltaTime;

				//We might want to use a curve to increase the jump speed at the right time(might feel weird atm)
			}
            
			//momentum code, ramp up from zero for fine motor strafing uninfluceneced by slip
			if (desiredMove.magnitude == 0) {
				curMomentumRampUpTime = 0f;
			}


			//if sustained input to move, we increase the counter
			if (desiredMove.magnitude != 0) {
				//curMomentumRampUpTime += Time.fixedDeltaTime;
                //we've disabled momentum for walking entirely, rampup is nearly pointless atm
				if (!m_IsWalking) {
					//running disables the counter for gaurenteed slip
					curMomentumRampUpTime = momentumRampUpTime;
				}
			}


			//ensures momentum inherits the movement of player
			if (desiredMove.magnitude != 0 || !m_CharacterController.isGrounded) 
			{
				//in the air exclusively we inherit momentum
				curMomentumDir = m_MoveDir * momentumFactor;

				if (desiredMove.magnitude != 0) {
					//this is the rampup code
					if (disableMomentumOnWalk) {
						if (!m_IsWalking) {
							curMomentumDir = m_MoveDir * momentumFactor;
						}

					} else {
						curMomentumDir = m_MoveDir * momentumFactor * (Mathf.Lerp (0, 1, momentumRampUpCurve.Evaluate (curMomentumRampUpTime / momentumRampUpTime)));
					}

				}

			}

		

            if (m_CharacterController.isGrounded)
            {
				if (curJumpPower < timeToFullJumpPower) 
				{
					curJumpPower += Time.fixedDeltaTime;
				} else {
					curJumpPower = timeToFullJumpPower;
				}



				airDir = m_MoveDir;
				jumpBuffer = true;
				curJumpBufferTime = jumpBufferTime;

                if (!m_IsStepUp && !m_IsLedgeGrab)
                {
                    m_MoveDir.y = -m_StickToGroundForce;
                }
               
				
               
				curHoldJumpTime = 0;


                
            }
            else
            {
				if (jumpBuffer && curJumpBufferTime > 0)
                {
					curJumpBufferTime -= Time.fixedDeltaTime;
				} else {
					jumpBuffer = false;
				}

                if (!m_IsStepUp && !m_IsLedgeGrab)
                {
                    m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
                }

                if (hold) {
                    airDir = Vector3.zero;
                }

            }

            if (isSliding && m_Jump)
            {
                isSliding = false;
                cancelSlide = true;
                curSlideDownTime = slideDownTime;
               // m_Jump = false; eNABLE JUMP out of slide
               //airDir = 
            }



            if (m_Jump && (jumpBuffer || hold))
            {
                /*
				if(isSliding){
					isSliding = false;
					cancelSlide = true;
					curSlideDownTime = slideDownTime;
				}*/
                prevJumpPower = (curJumpPower / timeToFullJumpPower);
                m_MoveDir.y = m_JumpSpeed;//* /prevJumpPower;
                airDir = m_MoveDir;
                if (isCrouch)
                {
                    //tryUncrouch = true; //not sure about this. prey and dishonoured keep jump during crouch, so will I
                }


                //Buggy way to handle ledge hold into jump. but it is working to some extent
                //we dont want to cancel hold in case where we 
                if (hold) {
                    hold = false;
                    playerInteract.checkHold = false;
                    m_MoveDir = m_MoveDir + (transform.forward * m_JumpSpeed);
                    curHoldJumpTime = 0; //we want to be able to get more air time!
                    needNewKeyPressToLedgeGrab = false; //let players smooth transition properly!!
                   airDir = m_MoveDir;
                }



                curJumpPower = minJumpPower;
                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
                jumpBuffer = false;

            }

            if (m_IsLedgeGrab) {
                airDir = Vector3.zero;
            }

            if (m_IsStepUp || m_IsLedgeGrab)
            {
                m_MoveDir = Vector3.zero;
                curJumpPower = timeToFullJumpPower;
            }
			
			//**************************************************************************************************************************************************************************
			//Slide implementation
			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
				m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			Vector3 detectSlope = Vector3.ProjectOnPlane(slideDir.normalized, hitInfo.normal).normalized;

			//Debug.Log (detectSlope.y);

			//use y component



			if(isSliding){
                //if (m_Camera.transform.localEulerAngles.x < -85 || m_Camera.transform.localEulerAngles.x > -15) {
                   // m_MouseLook.m_CameraTargetRot = Quaternion.Euler(Mathf.Clamp(m_Camera.transform.localEulerAngles.x, -85, -15), 0, 0);
                //}
				m_CharacterController.height = Mathf.Lerp(standHeight,crouchHeight,(curSlideDownTime/slideDownTime));
                m_MouseLook.MaximumX = -15;
                m_MouseLook.MinimumX = -85;
				m_Camera.transform.localPosition = new Vector3(0,Mathf.Lerp(standCamHeight,slideCamPosY,(curSlideDownTime/slideDownTime)),0);
				curSlideDownTime += Time.fixedDeltaTime;
				m_UseHeadBob = false;
				m_MoveDir.x = desiredMove.x * slideStrafeSpeed + slideDir.x * speed;
				m_MoveDir.z = desiredMove.z * slideStrafeSpeed+ slideDir.z * speed;
                //curSlideSpeed -= Time.fixedDeltaTime * slideSpeedDec;
                //curSlideSpeed -= Time.fixedDeltaTime * detectSlope.y * slideSlopeAccelFactor;
                curSlideSpeed -= Mathf.Clamp(Time.fixedDeltaTime * (slideSpeedDec + detectSlope.y * slideSlopeAccelFactor), 0, 100f);
				//Debug.Log (curSlideSpeed);
				if (curSlideSpeed < slideSpeedTerminate) {

                    isSliding = false;
                    isCrouch = true;
                    curCrouchTime = crouchTime;
                    m_UseHeadBob = false;
                    cancelSlide = true;
                    cancelSlideIntoCrouch = true;
                    curSlideDownTime = slideDownTime;
                    //players who want to go fast keep going fast
                    if (!m_IsWalking) {
                        if (Physics.Raycast(transform.position, Vector3.up, unCrouchHeightAmount, (1 << 8)))
                        {  //layer mask 8. player colliders here
                          
                        }
                        else {
                            cancelSlideIntoCrouch = false;
                            isCrouch = false;
                        }
                        

                    }
                }
			}

            /* This chunk breaks sliding using backward movement, however, now that thee's some slide control, and other ways to break slide, this is unneccessary and possibly limits input
			if (storeVert == -1 && isSliding) {

				isSliding = false;
				cancelSlide = true;
				curSlideDownTime = slideDownTime;
			}
            */

			if (cancelSlide) {
				//later on check why slide cancel occured and react appropriately. during slide cancel; prevent crouching and leaning(go to if statements)
				//back key cancels slide 
				//toggle crouch mode, pressing crouch again stops the slide and crouches player
				//if Cant emerage from slide standing, emerge crouching as an overwrite(regarldes of if player wants to stand)
				//during this , inc the vars back to standing normal
				m_UseHeadBob = false;
                m_MouseLook.MaximumX = 90;
                m_MouseLook.MinimumX = -90;
                float diff1 = Mathf.Lerp (standHeight, crouchHeight, (curSlideDownTime / slideDownTime)) - m_CharacterController.height;
				float diff2 = 0;//  Mathf.Lerp (crouchHeight, (curSlideDownTime / slideDownTime)) - m_CharacterController.height;
				transform.localPosition += new Vector3 (0,cancelSlideIntoCrouch?diff2:diff1,0);

				float diff4 = Mathf.Lerp (standCamHeight, slideCamPosY, (curSlideDownTime / slideDownTime));
				float diff5 = Mathf.Lerp (camDownDist, slideCamPosY, (curSlideDownTime / slideDownTime));
				m_CharacterController.height = cancelSlideIntoCrouch? crouchHeight : Mathf.Lerp(standHeight,crouchHeight,(curSlideDownTime/slideDownTime));

				m_Camera.transform.localPosition = new Vector3(0,cancelSlideIntoCrouch? diff5: diff4,0);
                m_MouseLook.m_CameraTargetRot *= Quaternion.Euler(100 * Time.fixedDeltaTime, 0f, 0f);

                //allows slide to inherit air momentum! handy and fun
                if (m_Jumping && curSlideDownTime == slideDownTime)
                {
                    airDir = slideDir.normalized * curSlideSpeed;
                }
                curSlideDownTime -= Time.fixedDeltaTime;
				if(curSlideDownTime < 0){
					cancelSlide = false;
					m_UseHeadBob = true;
					cancelSlideIntoCrouch = false;
				}

                

			}

			//************************************************************************************************************************************************************************			**
			if(isLean)
			{
				m_UseHeadBob = false;

				if (leanDirection == 0 || leanDirection == -1 * curLeanDir) {
					//if lean is not happening anymore start reseting back to regular	
					curLeanTime -=Time.fixedDeltaTime;
					curLeanTime = (curLeanTime < 0)? 0: curLeanTime;


					if(curLeanTime <= 0){
						curLeanTime = 0;
						isLean = false; //this allows us to try lean in diff direction
						m_UseHeadBob = true;
					}
				} else {
					//if we are leaning in destination direction, go up to maxlean dist and stop
					curLeanTime +=Time.fixedDeltaTime;
					curLeanTime = (curLeanTime > leanTime)? leanTime: curLeanTime;


				}

				Vector3 leanRayDir = transform.TransformDirection (Vector3.right) * (leanDistSide + leanClipBuffer);
				Vector3 leanRayStopOrig = m_Camera.transform.position + Vector3.down * leanDistDown;
				RaycastHit leanHit;
				Debug.DrawRay (leanRayStopOrig, leanRayDir * curLeanDir, Color.red);
				Ray leanRay = new Ray(leanRayStopOrig, leanRayDir * curLeanDir);
				Physics.Raycast (leanRay,out leanHit,leanDistSide + leanClipBuffer, playerMask );  //ignores the players collider
				if (leanHit.collider != null && leanHit.distance < Mathf.Lerp (0, leanDistSide, (curLeanTime / leanTime)) + leanClipBuffer) {
					//this means we need to lower curLeanTime to a value that doesnt violate setup
					while(leanHit.distance <= Mathf.Lerp (0, leanDistSide,/*Clamping the value fixed a game crash, outof bound lerp..*/ Mathf.Clamp(((curLeanTime / leanTime) + leanClipBuffer),0f,1f))){
						curLeanTime -= Time.fixedDeltaTime;
					}


				}

				m_Camera.transform.localPosition += curLeanDir * Mathf.Lerp(0,leanDistSide,(curLeanTime/leanTime)) * Vector3.right;
				m_Camera.transform.localPosition += Vector3.down * Mathf.Lerp(0,leanDistDown,(curLeanTime/leanTime));
				pivot.transform.localEulerAngles = -1*curLeanDir * Mathf.Lerp (0, leanCamRotation, (curLeanTime / leanTime)) * new Vector3(0,0,1);
				//assuming nothing else is using pivot to angle player
				//leaning is currently making sound of feat skewed ... not ideal
				///would also like to prevent wall clipping with lean
			}

            //fall recovery code
            //********************************************************************************************************************************************************************************
           
                //check if prev air speed big enough to trigger fall recovery
                
                if (prevAirSpeedDown <= -1*moveSpeedDownToFallRecovery)
                {
                    isFallRecovery = true;
                    triggerFallRecover = true;
                    curTimeToRecover = 0;
                   
                }
            



            if (!m_CharacterController.isGrounded)
            {
                prevAirSpeedDown = m_MoveDir.y;
               
            }
            else
            {
                prevAirSpeedDown = 0;
            }

            if (isFallRecovery)
            {
                //IVE FIGURED OUT THE BUG! THE PLAYER FALL RECOVERS.... AND IS NOT GROUNDED!!!! Thus nullfying the camera reset!
                if (triggerFallRecover && (curTimeToRecover == 0) && m_CharacterController.isGrounded)
                {
                    fpsAnimator.SetTrigger("FallRecovery");
                    m_AudioSource.PlayOneShot(fallSound);
                    triggerFallRecover = false;
                    m_IsWalking = true;
                    curTimeToMaxRunSpeed = 0;
                }
                //play the fallRecoveryState Animation
                m_MoveDir.x = 0f;
                m_MoveDir.z = 0f;
                //m_MoveDir.y = 0f;
                //m_Camera.transform.localEulerAngles = 
                //during this translate camera rotation to local 0 0 0 . standadize anim, then  near end set it back
                if (curTimeToRecover > timeToRecover && !triggerFallRecover) {
                    isFallRecovery = false;
                    curTimeToRecover = 0;
                }
                else if(!triggerFallRecover) {
                    curTimeToRecover += Time.fixedDeltaTime;
                    m_Camera.transform.localEulerAngles = Vector3.zero;
                }

            }
            //********************************************************************************************************************************************************************************

            //***************************************************************************************************************************************************************

            //Experimental code for testing ledge grab in another script


            if (hold)
            {
                m_MoveDir = Vector3.zero;

            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

			//while not touching ground. start a counter. deduct counter from slidespeed or run speed. whichever is appropriate
			if ((m_CharacterController.collisionFlags & CollisionFlags.Below) != 0) {
				//Debug.Log ("touching ground");
				if(hasLeftGround){
					hasLeftGround = false;
					if (!m_IsWalking) {
						curTimeToMaxRunSpeed -= timeInAir * FallSpeedTimeDecFactor;
						curTimeToMaxRunSpeed = (curTimeToMaxRunSpeed < 0) ? 0 : curTimeToMaxRunSpeed;
					}

					if (isSliding) {
						curSlideTime -= timeInAir * FallSpeedTimeDecFactor;
						curSlideTime = (curSlideTime < 0) ? 0 : curSlideTime;
					}

					timeInAir = 0f;
				}
			} else {
				//Debug.Log ("not touching ground");
				if(!hasLeftGround){
					hasLeftGround = true;
				}

				timeInAir += Time.fixedDeltaTime;
			}

			//Debug.Log (timeInAir);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);



            m_MouseLook.UpdateCursorLock();
        }

	


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
           
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            if (!isSliding)
            {
                PlayFootStepAudio();
            }
            
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
					
			if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
			{
					m_Camera.transform.localPosition =
					m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
						(speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
				newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
			}
			else
			{
				newCameraPosition = m_Camera.transform.localPosition;
				newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
			}

            
            m_Camera.transform.localPosition = newCameraPosition;
        }

	


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");
			storeVert = CrossPlatformInputManager.GetAxisRaw("Vertical");
            bool waswalking = m_IsWalking;

           // Debug.Log(CrossPlatformInputManager.GetAxis("Vertical"));

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            //m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
            //change sprint functionality to double tap forwardkey //combine w/ looser forward movement stop time like halflife2s

            bool forwardKeyDown = CrossPlatformInputManager.GetButtonDown("Vertical") && CrossPlatformInputManager.GetAxis("Vertical") > 0;

			bool doubleTapD = false;

			//*************************************************************************************************************************************************************************************
			//Sprint code
			if(forwardKeyDown)
			{
				//CLEVER WAY OF HANDLING counters I handt realised/considered
				if(Time.time < curTapTime + tapTime)
				{
					doubleTapD = true;	
				}	
				curTapTime = Time.time;
			}

			if(doubleTapD)
			{
				m_IsWalking = false;
				tryUncrouch = isCrouch? true: false;  //need crouch to break sprint, and vice versa, slide will become the sprint break
				prevRotation = transform.eulerAngles.y;
				qPrevRotation = transform.rotation;
				curTimeToMaxRunSpeed = initSprintTime;
			}


			if(vertical == 0){
				m_IsWalking = true;
			}

			//*************************************************************************************************************************************************************************************
			bool decrease = false;

			//building speed while running
			//initSprintTime
			if(!m_IsWalking){
				prevRotation = Mathf.MoveTowards(prevRotation, transform.eulerAngles.y, speedBleedRecoveryFactor * Time.fixedDeltaTime);
				qPrevRotation = Quaternion.RotateTowards(qPrevRotation, transform.rotation, speedBleedRecoveryFactor * Time.fixedDeltaTime);
				curTimeToMaxRunSpeed += (curTimeToMaxRunSpeed < m_TimeToMaxRunSpeed)? Time.fixedDeltaTime : 0;
				curRunSpeed = Mathf.Lerp(m_StartRunSpeed,m_MaxRunSpeed,runRampUpCurve.Evaluate(curTimeToMaxRunSpeed/m_TimeToMaxRunSpeed));
			}else{
				curTimeToMaxRunSpeed = 0;
				//later add a feature that builds the init speed up after being deppleted(prevent sprint spamming)
				curRunSpeed = initialRunSpeed;
			}


			if(Mathf.Abs(Mathf.DeltaAngle(prevRotation, transform.eulerAngles.y)) > speedBleedMaxAngle){
				curTimeToMaxRunSpeed -= Time.fixedDeltaTime * speedBleedDecFactor * ((Mathf.Abs(Mathf.DeltaAngle(prevRotation, transform.eulerAngles.y)) - speedBleedMaxAngle));
				//Debug.Log(((Mathf.Abs(Mathf.DeltaAngle(prevRotation, transform.eulerAngles.y)) - speedBleedMaxAngle)));
				decrease = true;
				prevRotation = Mathf.MoveTowards(prevRotation, transform.eulerAngles.y, Time.fixedDeltaTime * speedLossReaquire * ((Mathf.Abs(Mathf.DeltaAngle(prevRotation, transform.eulerAngles.y)) - speedBleedMaxAngle)));
				qPrevRotation = Quaternion.RotateTowards(qPrevRotation, transform.rotation, Time.fixedDeltaTime * speedLossReaquire * ((Mathf.Abs(Mathf.DeltaAngle(prevRotation, transform.eulerAngles.y)) - speedBleedMaxAngle)));
				//on decrease increase reaquire rate proportionally to speed lost
				if(curTimeToMaxRunSpeed < 0){
					curTimeToMaxRunSpeed = 0;
					prevRotation = transform.eulerAngles.y;
					qPrevRotation = transform.rotation;

				}

			}

			Debug.DrawRay(transform.position, qPrevRotation * Vector3.forward * 15, (!decrease)?Color.red:Color.blue);
			//*************************************************************************************************************************************************************************************
			//Debug.Log(curRunSpeed);

		



#endif

            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : curRunSpeed;

            
            

            if (isCrouch) {//handle crouch speed overwrite
				speed = crouchSpeed;
			}



            if (m_IsWalking)
            {
                if (Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal")) > Mathf.Abs(CrossPlatformInputManager.GetAxis("Vertical")))
                {
                    speed = speed * Mathf.Abs(CrossPlatformInputManager.GetAxis("Horizontal"));
                }
                else
                {
                    speed = speed * Mathf.Abs(CrossPlatformInputManager.GetAxis("Vertical"));
                }
            }

            if (storeVert == -1)
            {
                speed *= backwardsMoveSpeedFactor;
            }

            if (isSliding) 
			{
				speed = curSlideSpeed;
			}
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }
     


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);

        }

        public void TriggerLedgeGrab(Vector3 endPos) {
            m_IsLedgeGrab = true;
            m_LedgeGrabEndPoint = endPos + Vector3.up * m_LedgeGrabTopOffset;
            ledgeCancelPhase = false;

        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            curPlayerColliderHit = hit;

            if (hit.collider.gameObject.Equals(LedgeGrabGameObject)) {
                hold = true;
            }
            else
            {
                hold = false;
            }

			if ((m_CollisionFlags & CollisionFlags.Sides) != 0) {
				//did we collide directly with obstacle?
				//Debug.Log(hit.collider);

				//if hit normal has a y component  greater then .5 disaqualify

				if (hit.normal.y < 0.2) {
					Vector3 moveDirNoY = new Vector3 (m_MoveDir.x, 0, m_MoveDir.y);
					Vector3 collideWall = hit.normal.normalized + moveDirNoY.normalized;
					//Debug.Log(hit.collider + "collision normal: " + hit.normal + " move Dir: " + moveDirNoY.normalized + "collidewall vect:" + collideWall + " magnitude" + collideWall.magnitude);
					//Debug.Log(collideWall.magnitude);
					//Debug.Log ("side Collision");
					if(collideWall.magnitude < .95){
						if(!m_IsWalking){
							curTimeToMaxRunSpeed = 0;
							//collison rebuff on the player(normal force)
							//maybe in future a full speed decrease sends player to feet(recovery anim, simlar to falling effects of collision)
						}


						if (isSliding) {
                            isSliding = false;
                            isCrouch = true;
                            curCrouchTime = crouchTime;
                            m_UseHeadBob = false;
                            cancelSlide = true;
                            cancelSlideIntoCrouch = true;
                            curSlideDownTime = slideDownTime;
                        }
					}

				}

			}

            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*forceToHitObjMult, hit.point, ForceMode.Impulse);
        }

        public bool IsCrouched() {
            return isCrouch;
        }

        public ControllerColliderHit GetControllerColliderHit() {
            return curPlayerColliderHit;
        }

        public bool IsLedgeGrab() {
            return m_IsLedgeGrab;
        }

    }
}
