/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"Char.cs"
 * 
 *	This is the base class for both NPCs and the Player.
 *	It contains the functions needed for animation and movement.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if SalsaIsPresent
using CrazyMinnow.SALSA;
#endif

namespace AC
{
	
	/**
	 * The base class for both NPCs and the Player.
	 * It contains the functions needed for animation and movement.
	 * Note that, for performance, the StateHandler calls the various Update/FixedUpdate/LateUpdate functions directly to it's
	 * own list of characters found when GatherObjects is called.  So, if you manually enable/disable characters in a scene, be sure to call
	 * StateHandler's GatherObjects function afterwards.
	 */
	[RequireComponent (typeof (Paths))]
	public class Char : MonoBehaviour
	{
		
		/** The character's state (Idle, Move, Decelarate, Custom) */
		public CharState charState;
		
		private AnimEngine animEngine;
		
		/** The animation engine enum (SpritesUnity, SpritesUnityComplex, Sprites2DToolkit, Legacy, Mecanim, Custom) */
		public AnimationEngine animationEngine = AnimationEngine.SpritesUnity;
		/** The class name of the AnimEngine ScriptableObject that animates the character, if animationEngine = AnimationEngine.Custom */
		public string customAnimationClass = "";
		/** How motion is controlled, if animationEngine = AnimationEngine.Custom (Automatic, JustTurning, Manual) */
		public MotionControl motionControl = MotionControl.Automatic;
		/** How talking animations are handled (Standard, CustomFace) */
		public TalkingAnimation talkingAnimation = TalkingAnimation.Standard;
		
		/** The character's display name when speaking */
		public string speechLabel = "";
		/** The colour of the character's speech text */
		public Color speechColor = Color.white;
		/** The character's portrait graphic, used in MenuGraphic elements when speaking */
		public CursorIconBase portraitIcon = new CursorIconBase ();
		/** If True, speech text can use expression tokens to change the character's expression */
		public bool useExpressions;
		/** The character's available expressions, if useExpressions = True */
		public List<Expression> expressions = new List<Expression>();
		/** The character's active Expression ID number */
		private Expression currentExpression = null;
		
		protected Quaternion newRotation;
		private float prevHeight;
		private float prevHeight2;
		private float heightChange;
		
		// Lip sync variables
		
		private Shapeable shapeable = null;
		private LipSyncTexture lipSyncTexture = null;
		private List<LipSyncShape> lipSyncShapes = new List<LipSyncShape>();
		
		/** If True, the character is currently lip-syncing */
		public bool isLipSyncing = false;
		/** The name of the Mecanim integer parameter set to the lip-syncing phoneme integer, if using AnimEngine_SpritesUnityComplex */
		public string phonemeParameter = "";
		/** The Shapeable group ID that controls phoneme blendshapes, if using AnimEngine_Legacy / AnimEngine_Mecanim */
		public int lipSyncGroupID;
		
		#if SalsaIsPresent
		private Salsa2D salsa2D;
		#endif
		
		// 3D variables
		
		/** The Transform to parent objects held in the character's left hand to */
		public Transform leftHandBone;
		/** The Transform to parent objects held in the character's right hand to */
		public Transform rightHandBone;
		private GameObject leftHandHeldObject;
		private GameObject rightHandHeldObject;
		
		// Legacy variables
		
		/** The "Idle" animation, if using AnimEngine_Legacy */
		public AnimationClip idleAnim;
		/** The "Walk" animation, if using AnimEngine_Legacy */
		public AnimationClip walkAnim;
		/** The "Run" animation, if using AnimEngine_Legacy */
		public AnimationClip runAnim;
		/** The "Talk" animation, if using AnimEngine_Legacy */
		public AnimationClip talkAnim;
		/** The "Turn left" animation, if using AnimEngine_Legacy */
		public AnimationClip turnLeftAnim;
		/** The "Turn right" animation, if using AnimEngine_Legacy */
		public AnimationClip turnRightAnim;
		/** The "Look left" animation, if using AnimEngine_Legacy */
		public AnimationClip headLookLeftAnim;
		/** The "Look right" animation, if using AnimEngine_Legacy */
		public AnimationClip headLookRightAnim;
		/** The "Look up" animation, if using AnimEngine_Legacy */
		public AnimationClip headLookUpAnim;
		/** The "Look down" animation, if using AnimEngine_Legacy */
		public AnimationClip headLookDownAnim;
		
		private Animation _animation;
		
		/** The "Upper body" bone Transform, used to isolate animations if using AnimEngine_Legacy */
		public Transform upperBodyBone;
		/** The "Left arm" bone Transform, used to isolate animations if using AnimEngine_Legacy */
		public Transform leftArmBone;
		/** The "Right arm" bone Transform, used to isolate animations if using AnimEngine_Legacy */
		public Transform rightArmBone;
		/** The "Neck" bone Transform, used to isolate animations if using AnimEngine_Legacy */
		public Transform neckBone;
		
		/** The default duration, in seconds, when crossfading standard animations, if using AnimEngine_Legacy */
		public float animCrossfadeSpeed = 0.2f;
		
		private Vector3 exactDestination = Vector3.zero;
		private bool doExactLerp = false;
		
		// Mecanim variables
		
		/** The name of the Mecanim float parameter set to the movement speed, if using AnimEngine_Mecanim */
		public string moveSpeedParameter = "Speed";
		/** The name of the Mecanim float parameter set to the vertical movement speed, if using AnimEngine_Mecanim */
		public string verticalMovementParameter = "";
		/** The name of the Mecanim float parameter set to the turning direction, if using AnimEngine_Mecanim */
		public string turnParameter = "";
		/** The name of the Mecanim bool parameter set to True while talking, if using AnimEngine_Mecanim */
		public string talkParameter = "IsTalking";
		/** The name of the Mecanim integer parameter set to the sprite direction (set by GetSpriteDirectionInt()), if using AnimEngine_SpritesUnityComplex */
		public string directionParameter = "Direction";
		/** The name of the Mecanim float parameter set to the facing angle, if using AnimEngine_SpritesUnityComplex */
		public string angleParameter = "Angle";
		/** The name of the Mecanim float parameter set to the head yaw, if using AnimEngine_Mecanim */
		public string headYawParameter = "";
		/** The name of the Mecanim float parameter set to the head pitch, if using AnimEngine_Mecanim */
		public string headPitchParameter = "";
		/** The name of the Mecanim integer parameter set to the active Expression ID number */
		public string expressionParameter = "";
		/** The factor by which the job of turning is left to Mecanim root motion, if using AnimEngine_Mecanim */
		public float rootTurningFactor = 0f;
		/** The Mecanim layer used to play head animations while talking, by AnimEngine_Mecanim / AnimEngine_SpritesUnity */
		public int headLayer = 1;
		/** The Mecanim layer used to play mouth animations while talking, by AnimEngine_Mecanim / AnimEngine_SpritesUnity */
		public int mouthLayer = 2;
		
		// 2D variables
		
		private Animator _animator;
		
		/** The sprite Transform, that's a child GameObject, used by AnimEngine_SpritesUnity / AnimEngine_SpritesUnityComplex / AnimEngine_Sprites2DToolkit */
		public Transform spriteChild;
		/** The method by which a sprite-based character should face the camera in 3D games (CameraRotation, CameraPosition) */
		public RotateSprite3D rotateSprite3D = RotateSprite3D.CameraFacingDirection;
		
		/** The name of the "Idle" animation(s), without suffix, used by AnimEngine_SpritesUnity */
		public string idleAnimSprite = "idle";
		/** The name of the "Walk" animation(s), without suffix, used by AnimEngine_SpritesUnity */
		public string walkAnimSprite = "walk";
		/** The name of the "Run" animation(s), without suffix, used by AnimEngine_SpritesUnity */
		public string runAnimSprite = "run";
		/** The name of the "Talk" animation(s), without suffix, used by AnimEngine_SpritesUnity */
		public string talkAnimSprite = "talk";
		
		/** If True, sprite-based characters will not resize along a SortingMap */
		public bool lockScale = false;
		/** A sprite-based character's scale, if lockScale = True */
		public float spriteScale = 1f;
		/** If True, sprite-based characters will be locked to face a set direction */
		public bool lockDirection = false;
		/** The directional suffix to face (e.g. "L" for "Left"), if lockDirection = True */
		public string spriteDirection = "D";
		
		private float spriteAngle = 0f;	
		
		/** If True, sprite-based characters will play different animations depending on which direction they are facing */
		public bool doDirections = true;
		/** If True, characters will crossfade between standard animations, if using AnimEngine_SpritesUnity */
		public bool crossfadeAnims = false;
		/** If True, sprite-based characters will be able to face 8 directions instead of 4, if doDirections = True */
		public bool doDiagonals = false;
		/** If True, the character is talking */
		public bool isTalking = false;
		/** The type of frame-flipping to use on sprite-based characters (None, LeftMirrorsRight, RightMirrorsLeft) */
		public AC_2DFrameFlipping frameFlipping = AC_2DFrameFlipping.None;
		/** If True, and frameFlipping != AC_2DFrameFlipping.None, then custom animations will also be flipped */
		public bool flipCustomAnims = false;
		
		private Vector3 originalScale;
		private bool flipFrames = false;
		
		// Movement variables
		
		/** The movement speed when walking */
		public float walkSpeedScale = 2f;
		/** The movement speed when runing */
		public float runSpeedScale = 6f;
		/** The turn speed */
		public float turnSpeed = 7f;
		/** The acceleration factor */
		public float acceleration = 6f;
		/** The deceleration factor */
		public float deceleration = 0f;
		/** If True, the character will turn on the spot to face their destination before moving */
		public bool turnBeforeWalking = false;
		/** The minimum distance between the character and its destination for running to be possible */
		public float runDistanceThreshold = 1f;
		/** If True, then sprite-based characters will only move when their sprite frame changes */
		public bool antiGlideMode = false;

		protected float pathfindUpdateTime = 0f;
		protected bool isJumping = false;
		
		private float sortingMapScale = 1f;
		private bool isReversing = false;
		private float turnFloat = 0f;
		private string currentSpriteName = "";
		private SpriteRenderer _spriteRenderer;
		private bool isTurningBeforeWalking;
		
		// Rigidbody variables
		
		/** If True, the character will ignore the effects of gravity */
		public bool ignoreGravity = false;
		/** If True, the character's Rigidbody will be frozen in place when idle. This is to help slipping when on surfaces */
		public bool freezeRigidbodyWhenIdle = false;
		
		protected Rigidbody _rigidbody = null;
		private Rigidbody2D _rigidbody2D = null;
		protected Collider _collider = null;
		protected CharacterController _characterController;
		
		// Wall detection variables
		
		/** If True, then characters will slow down when walking into walls, if using AnimEngine_Mecanim */
		public bool doWallReduction = false;
		/** The layer that walls are expected to be placed on, if doWallReduction = True */
		public string wallLayer = "Default";
		/** The distance to keep away from walls, if doWallReduction = True */
		public float wallDistance = 0.5f;
		
		private float wallReduction = 1f;
		private Vector3 wallRayOrigin = Vector3.zero;
		private float wallRayForward = 0f;
		
		// Sound variables
		
		/** The sound to play when walking */
		public AudioClip walkSound;
		/** The sound to play when running */
		public AudioClip runSound;
		/** The sound to play when the character's speech text is scrolling */
		public AudioClip textScrollClip;
		/** The Sound child to play all non-speech AudioClips on. Speech AudioClips will be placed on the root GameObject */
		public Sound soundChild;
		
		protected AudioSource audioSource;
		public AudioSource speechAudioSource;
		
		protected Paths activePath = null;
		
		/** If True, the character will run when moving */
		public bool isRunning { get; set; }
		
		protected bool isUFPSPlayer = false;
		protected float moveSpeed;
		protected Vector3 moveDirection; 
		
		protected int targetNode = 0;
		protected bool pausePath = false;
		
		protected Vector3 lookDirection;
		private float pausePathTime;
		private ActionList nodeActionList;
		private int prevNode = 0;
		
		// Resume path
		private int lastPathPrevNode = 0;
		private int lastPathTargetNode = 0;
		private Paths lastPathActivePath = null;
		
		protected bool tankTurning = false;
		
		private Vector2 targetHeadAngles;
		private Vector2 actualHeadAngles;
		private float headTurnWeight = 0f;
		
		/** The point in World Space that the character is turning its head towards, if headFacing != HeadFacing.None */
		public Vector3 headTurnTarget;
		/** The type of head-turning effect that is currently active (None, Hotspot, Manual) */
		public HeadFacing headFacing = HeadFacing.None;
		/** If True, then inverse-kinematics will be used to turn the character's head dynamically, rather than playing pre-made animations */
		public bool ikHeadTurning = false;
		
		
		protected void _Awake ()
		{
			if (GetComponent <CharacterController>())
			{
				_characterController = GetComponent <CharacterController>();
				wallRayOrigin = _characterController.center;
				wallRayForward = _characterController.radius;
			}
			else if (GetComponent <CapsuleCollider>())
			{
				CapsuleCollider capsuleCollider = GetComponent <CapsuleCollider>();
				wallRayOrigin = capsuleCollider.center;
				wallRayForward = capsuleCollider.radius;
			}
			else if (GetComponent <CircleCollider2D>())
			{
				CircleCollider2D circleCollider = GetComponent <CircleCollider2D>();
				#if !UNITY_5
				wallRayOrigin = circleCollider.center;
				#else
				wallRayOrigin = circleCollider.offset;
				#endif
				wallRayForward = circleCollider.radius;
			}
			else if (GetComponent <BoxCollider2D>())
			{
				BoxCollider2D boxCollider = GetComponent <BoxCollider2D>();
				wallRayOrigin = boxCollider.bounds.center;
				wallRayForward = boxCollider.bounds.size.x / 2f;
			}
			
			if (GetComponentInChildren <FollowSortingMap>())
			{
				transform.localScale = Vector3.one;
			}
			originalScale = transform.localScale;
			charState = CharState.Idle;
			shapeable = GetShapeable ();
			if (GetComponent <LipSyncTexture>())
			{
				lipSyncTexture = GetComponent <LipSyncTexture>();
			}
			
			ResetAnimationEngine ();
			ResetBaseClips ();
			
			_animator = GetAnimator ();
			_animation = GetAnimation ();
			SetAntiGlideState ();
			
			if (spriteChild && spriteChild.gameObject.GetComponent <SpriteRenderer>())
			{
				_spriteRenderer = spriteChild.gameObject.GetComponent <SpriteRenderer>();
				if (spriteChild.localPosition != Vector3.zero)
				{
					if (!(gameObject.name == "Bird" && spriteChild.gameObject.name == "Bird_Sprite"))
					{
						// You found the dirtest hack in AC!
						ACDebug.LogWarning ("The sprite child of '" + gameObject.name + "' is not positioned at (0,0,0) - is this correct?");
					}
				}
			}
			
			if (speechAudioSource == null && GetComponent <AudioSource>())
			{
				speechAudioSource = GetComponent <AudioSource>();
			}
			
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}
			
			if (GetComponent <Rigidbody>())
			{
				_rigidbody = GetComponent <Rigidbody>();
			}
			else if (GetComponent <Rigidbody2D>())
			{
				_rigidbody2D = GetComponent <Rigidbody2D>();
			}
			
			if (GetComponent <Collider>())
			{
				_collider = GetComponent <Collider>();
			}
			
			AdvGame.AssignMixerGroup (speechAudioSource, SoundType.Other, true);
			AdvGame.AssignMixerGroup (audioSource, SoundType.SFX);
		}
		
		
		/**
		 * The character's "Update" function, called by StateHandler.
		 */
		public virtual void _Update ()
		{
			UpdateHeadTurnAngle ();
			CalcHeightChange ();
			
			if (!antiGlideMode && spriteChild != null && KickStarter.settingsManager != null)
			{
				UpdateSpriteChild (KickStarter.settingsManager.IsTopDown (), KickStarter.settingsManager.IsUnity2D ());
			}
			
			AnimUpdate ();
			SpeedUpdate ();
		}
		
		
		private void OnAnimatorIK (int layerIndex)
		{
			if (ikHeadTurning)
			{
				if (headTurnWeight > 0f)
				{
					Quaternion rot = Quaternion.Euler (0f, actualHeadAngles.x, 0f);
					Vector3 position = rot * transform.forward;
					
					position.y += actualHeadAngles.y;
					
					if (neckBone != null)
					{
						position += neckBone.position;
					}
					else
					{
						position += transform.position;
						
						if (_collider is CapsuleCollider)
						{
							CapsuleCollider capsuleCollder = (CapsuleCollider) _collider;
							position += new Vector3 (0f, capsuleCollder.height * transform.localScale.y * 0.8f, 0f);
						}
					}
					
					if (headFacing != AC.HeadFacing.None)
					{
						_animator.SetLookAtPosition (position);
					}
				}
				
				_animator.SetLookAtWeight (headTurnWeight);
			}
		}
		
		
		/**
		 * The character's "FixedUpdate" function, called by StateHandler.
		 */
		public virtual void _FixedUpdate ()
		{
			PathUpdate ();
			PhysicsUpdate ();
			
			if (!antiGlideMode)
			{
				MoveUpdate ();
			}
		}
		
		
		/**
		 * The character's "LateUpdate" function, called by StateHandler.
		 */
		public void _LateUpdate ()
		{
			if (antiGlideMode)
			{
				MoveUpdate (false);
				
				if (spriteChild && KickStarter.settingsManager != null)
				{
					UpdateSpriteChild (KickStarter.settingsManager.IsTopDown (), KickStarter.settingsManager.IsUnity2D ());
				}
			}
		}
		
		
		protected void PathUpdate ()
		{
			if (activePath && activePath.nodes.Count > 0)
			{
				if (pausePath)
				{
					if (nodeActionList != null)
					{
						if (!KickStarter.actionListManager.IsListRunning (nodeActionList))
						{
							SetNextNodes (true);
						}
					}
					else if (Time.time > pausePathTime)
					{
						SetNextNodes (true);
					}
					return;
				}
				else
				{
					if (pathfindUpdateTime > 0f)
					{
						pathfindUpdateTime -= Time.deltaTime;
						if (pathfindUpdateTime <= 0f)
						{
							pathfindUpdateTime = 0f;
							if (activePath.nodes.Count > targetNode)
							{
								Vector3 targetPosition = activePath.nodes [activePath.nodes.Count-1];
								MoveToPoint (targetPosition, false, true);
							}
						}
					}
					
					Vector3 direction = activePath.nodes[targetNode] - transform.position;
					Vector3 lookDir = new Vector3 (direction.x, 0f, direction.z);
					
					if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
					{
						direction.z = 0f;
						SetMoveDirection (direction);
						lookDir = new Vector3 (direction.x, 0f, direction.y);
						SetLookDirection (lookDir, false);
					}
					else if (activePath.affectY)
					{
						SetMoveDirection (direction);
						SetLookDirection (lookDir, false);
					}
					else
					{
						SetLookDirection (lookDir, false);
						SetMoveDirectionAsForward ();
					}
					
					if (isRunning && direction.magnitude > 0 && direction.magnitude < runDistanceThreshold)
					{
						if (targetNode == (activePath.nodes.Count - 1) && activePath.pathType == AC_PathType.ForwardOnly)
						{
							isRunning = false;
						}
					}

					float nodeThreshold = 1.1f - KickStarter.settingsManager.destinationAccuracy;

					if (isRunning && GetMotionControl () == MotionControl.Automatic)
					{
						float multiplier = (1 - (runSpeedScale / walkSpeedScale)) / 20f;
						multiplier *= Mathf.Clamp (GetDeceleration (), 1f, 20f);
						multiplier += (runSpeedScale / walkSpeedScale);

						nodeThreshold *= multiplier;
					}

					if ((KickStarter.settingsManager.IsUnity2D () && direction.magnitude < nodeThreshold) ||
					    (activePath.affectY && direction.magnitude < nodeThreshold) ||
					    (!activePath.affectY && lookDir.magnitude < nodeThreshold))
					{
						if (targetNode == 0 && prevNode == 0)
						{
							SetNextNodes ();
						}
						else if (activePath.nodeCommands.Count > targetNode)
						{
							if (activePath.commandSource == ActionListSource.InScene && activePath.nodeCommands [targetNode].cutscene != null)
							{
								PausePath (activePath.nodePause, activePath.nodeCommands [targetNode].cutscene, activePath.nodeCommands [targetNode].parameterID);
							}
							else if (activePath.commandSource == ActionListSource.AssetFile && activePath.nodeCommands [targetNode].actionListAsset != null)
							{
								PausePath (activePath.nodePause, activePath.nodeCommands [targetNode].actionListAsset, activePath.nodeCommands [targetNode].parameterID);
							}
							else if (activePath.nodePause > 0f)
							{
								PausePath (activePath.nodePause);
							}
							else
							{
								SetNextNodes ();
							}
						}
						else if (activePath.nodePause > 0f)
						{
							PausePath (activePath.nodePause);
						}
						else
						{
							SetNextNodes ();
						}
					}
				}
			}
		}
		
		
		private void SpeedUpdate ()
		{
			if (charState == CharState.Move)
			{
				Accelerate ();
			}
			else if (charState == CharState.Decelerate || charState == CharState.Custom)
			{
				Decelerate ();
			}
			else if (charState == CharState.Idle && moveSpeed > 0f)
			{
				moveSpeed = 0f;
			}
		}
		
		
		private void PhysicsUpdate ()
		{
			if (_rigidbody)
			{
				if (ignoreGravity)
				{
					_rigidbody.useGravity = false;
				}
				
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					_rigidbody.useGravity = false;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						_rigidbody.useGravity = false;
					}
					else
					{
						_rigidbody.useGravity = true;
					}
				}
			}
			else if (_rigidbody2D)
			{
				if (ignoreGravity)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					_rigidbody2D.gravityScale = 0f;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						_rigidbody2D.gravityScale = 0f;
					}
					else
					{
						_rigidbody2D.gravityScale = 1f;
					}
				}
			}
		}
		
		
		private void AnimUpdate ()
		{
			if (isTalking)
			{
				ProcessLipSync ();
			}
			
			if (isJumping)
			{
				animEngine.PlayJump ();
				StopStandardAudio ();
			}
			else
			{
				if (charState == CharState.Idle || charState == CharState.Decelerate)
				{
					if (IsTurning ())
					{
						if (turnFloat < 0f)
						{
							animEngine.PlayTurnLeft ();
						}
						else
						{
							animEngine.PlayTurnRight ();
						}
					}
					else
					{
						if (isTalking && (talkingAnimation == TalkingAnimation.Standard || animationEngine == AnimationEngine.Custom))
						{
							animEngine.PlayTalk ();
						}
						else
						{
							animEngine.PlayIdle ();
						}
					}
					
					StopStandardAudio ();
				}
				else if (charState == CharState.Move)
				{
					if (isRunning)
					{
						animEngine.PlayRun ();
					}
					else
					{
						animEngine.PlayWalk ();
					}
					
					PlayStandardAudio ();
				}
				else
				{
					StopStandardAudio ();
				}
				
				animEngine.PlayVertical ();
			}
		}
		
		
		private void MoveUpdate (bool deltaTime = true)
		{
			if (doExactLerp)
			{
				ExactLerp ();
				return;
			}
			
			if (animEngine)
			{
				if (GetMotionControl () == MotionControl.Automatic)
				{
					RootMotionType rootMotionType = GetRootMotionType ();
					
					if (moveSpeed > 0.01f && rootMotionType != RootMotionType.ThreeD)
					{
						Vector3 newVel;
						newVel = moveDirection * moveSpeed * walkSpeedScale * sortingMapScale;
						
						if (KickStarter.settingsManager)
						{
							if (KickStarter.settingsManager.IsTopDown ())
							{
								float upAmount = Mathf.Abs (Vector3.Dot (newVel.normalized, Vector3.forward));
								float mag = (newVel.magnitude * (1f - upAmount)) + (newVel.magnitude * KickStarter.sceneSettings.GetVerticalReductionFactor () * upAmount);
								newVel *= mag / newVel.magnitude;
							}
							else if (KickStarter.settingsManager.IsUnity2D ())
							{
								newVel.z = 0f;
								float upAmount = Mathf.Abs (Vector3.Dot (newVel.normalized, Vector3.up));
								float mag = (newVel.magnitude * (1f - upAmount)) + (newVel.magnitude * KickStarter.sceneSettings.GetVerticalReductionFactor () * upAmount);
								newVel *= mag / newVel.magnitude;
							}
							newVel *= KickStarter.playerInput.GetDragMovementSlowDown ();
						}
						
						bool noMove = false;
						if (rootMotionType == RootMotionType.TwoD)
						{
							if (spriteDirection == "L" || spriteDirection == "R")
							{
								newVel.x = 0f;
							}
							else if (spriteDirection == "U" || spriteDirection == "D")
							{
								newVel.y = 0f;
							}
							else
							{
								newVel = Vector3.zero;
							}
						}
						else if (antiGlideMode)
						{
							string newSpriteName = _spriteRenderer.sprite.name;
							if (newSpriteName == currentSpriteName)
							{
								noMove = true;
							}
							else
							{
								currentSpriteName = newSpriteName;
							}
						}
						
						if (!noMove)
						{
							if (DoRigidbodyMovement ())
							{
								_rigidbody.MovePosition (_rigidbody.position + newVel * ((deltaTime) ? Time.deltaTime : 0.025f));
							}
							else if (_characterController)
							{
								if (!_characterController.isGrounded && !ignoreGravity)
								{
									newVel += Physics.gravity;
								}
								_characterController.Move (newVel * ((deltaTime) ? Time.deltaTime : 0.025f));
							}
							else
							{
								transform.position += (newVel * ((deltaTime) ? Time.deltaTime : 0.025f));
							}
						}
					}
					else
					{
						if (_characterController)
						{
							if (this is Player && KickStarter.settingsManager.movementMethod == MovementMethod.UltimateFPS)
							{}
							else
							{
								if (!_characterController.isGrounded && !ignoreGravity)
								{
									_characterController.Move (Physics.gravity * Time.deltaTime);
								}
							}
						}
					}
				}
			}
			
			//			if (animEngine && animEngine.turningStyle == TurningStyle.Linear && moveSpeed > 0.01f && !IsUFPSPlayer ())
			//			{
			//				Turn (true);
			//			}
			//			else
			//			{
			Turn (false);
			//			}
			
			DoTurn ();
			
			if (_rigidbody)
			{
				if (freezeRigidbodyWhenIdle && !isJumping && (charState == CharState.Custom || charState == CharState.Idle))
				{
					_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				}
				else
				{
					_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				}
			}
		}
		
		
		protected virtual void Accelerate ()
		{
			float targetSpeed;
			
			if (GetComponent <Animator>())
			{
				if (isRunning)
				{
					targetSpeed = runSpeedScale;
				}
				else
				{
					targetSpeed = walkSpeedScale;
				}
			}
			else
			{
				if (isRunning)
				{
					targetSpeed = moveDirection.magnitude * runSpeedScale / walkSpeedScale;
				}
				else
				{
					targetSpeed = moveDirection.magnitude;
				}
			}
			
			moveSpeed = Mathf.Lerp (moveSpeed, targetSpeed, Time.deltaTime * acceleration);
		}
		
		
		private void Decelerate ()
		{
			if (doExactLerp)
			{
				moveSpeed = 0;
				return;
			}
			
			else if (animEngine != null && animEngine.turningStyle == TurningStyle.Linear)
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * GetDeceleration () * 3f);
			}
			
			else
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * GetDeceleration ());
			}
			
			if (moveSpeed < 0.01f)
			{
				moveSpeed = 0f;
				
				if (charState != CharState.Custom)
				{
					charState = CharState.Idle;
				}
			}
		}
		
		
		private void ExactLerp ()
		{
			if (!doExactLerp)
			{
				return;
			}
			
			if (charState != CharState.Decelerate || moveSpeed > 0f)
			{
				doExactLerp = false;
				return;
			}
			
			moveSpeed = 0f;
			Vector3 smartPosition = exactDestination;
			
			if (KickStarter.settingsManager.IsUnity2D ())
			{
				smartPosition = new Vector3 (exactDestination.x, exactDestination.y, transform.position.z);
			}
			else
			{
				smartPosition = new Vector3 (exactDestination.x, transform.position.y, exactDestination.z);
			}
			
			if (isUFPSPlayer)
			{
				UltimateFPSIntegration.Teleport (smartPosition);
				doExactLerp = false;
				charState = CharState.Idle;
				return;
			}
			
			if (smartPosition == transform.position)
			{
				doExactLerp = false;
				if (charState == CharState.Decelerate)
				{
					charState = CharState.Idle;
				}
			}
			else
			{
				float mag = (transform.position - smartPosition).magnitude * 10f;
				transform.position = Vector3.Lerp (transform.position, smartPosition, Time.deltaTime * GetDeceleration () / mag);
			}
		}
		
		
		private float GetDeceleration ()
		{
			if (deceleration <= 0f)
			{
				return acceleration;
			}
			return deceleration;
		}
		
		
		/**
		 * <summary>Teleports the Player</summary>
		 * <param name = "_position">The point to teleport to</param>
		 */
		public void Teleport (Vector3 _position)
		{
			doExactLerp = false;
			
			if (isUFPSPlayer)
			{
				UltimateFPSIntegration.Teleport (_position);
			}
			else
			{
				transform.position = _position;
			}
		}
		
		
		/**
		 * <summary>Instantly rotates the Player</summary>
		 * <param name = "_rotation">The new rotation</param>
		 */
		public void SetRotation (Quaternion _rotation)
		{
			transform.rotation = _rotation;
			SetLookDirection (transform.forward, true);
		}
		
		
		/**
		 * <summary>Instantly rotates the Player</summary>
		 * <param name = "angle">The angle, in degrees, around the Y-axis to rotate to</param>
		 */
		public void SetRotation (float angle)
		{
			transform.rotation = Quaternion.AngleAxis (angle, Vector3.up);
			SetLookDirection (transform.forward, true);
		}
		
		
		private void Turn (bool isInstant)
		{
			if (lookDirection == Vector3.zero)
			{
				return;
			}
			
			if (isInstant)
			{
				turnFloat = 0f;
				
				if (isUFPSPlayer)
				{
					UltimateFPSIntegration.SetRotation (lookDirection); // GetTargetRotation ().eulerAngles??
					transform.forward = lookDirection.normalized;
				}
				/*else if (DoRigidbodyMovement ())
				{
					_rigidbody.rotation = GetTargetRotation ();
				}*/
				else
				{
					transform.rotation = GetTargetRotation ();
				}
				newRotation = transform.rotation;
				
				if (KickStarter.settingsManager != null && spriteChild)
				{
					UpdateSpriteChild (KickStarter.settingsManager.IsTopDown (), KickStarter.settingsManager.IsUnity2D ());
				}
				return;
			}
			
			float targetAngle = Mathf.Atan2 (lookDirection.x, lookDirection.z);
			float currentAngle = Mathf.Atan2 (transform.forward.x, transform.forward.z);
			
			float angleDiff = targetAngle - currentAngle;
			if (angleDiff < -Mathf.PI)
			{
				targetAngle += Mathf.PI * 2f;
				angleDiff += Mathf.PI * 2f;
			}
			else if (angleDiff > Mathf.PI)
			{
				targetAngle -= Mathf.PI * 2f;
				angleDiff -= Mathf.PI * 2f;
			}
			
			turnFloat = Mathf.Lerp (turnFloat, turnSpeed * Mathf.Min (angleDiff/2f, 1f), turnSpeed * Time.deltaTime); // Min prevents turn direction flipping when slowing down
			float newAngle = currentAngle;
			
			if (isUFPSPlayer && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				lookDirection = transform.forward;
				return;
			}
			if (animEngine && animEngine.turningStyle == TurningStyle.Linear && !isUFPSPlayer)
			{
				if (DoRigidbodyMovement ())
				{
					newRotation = Quaternion.RotateTowards (_rigidbody.rotation, GetTargetRotation (), turnSpeed);
				}
				else
				{
					newRotation = Quaternion.RotateTowards (transform.rotation, GetTargetRotation (), turnSpeed);
				}
			}
			else
			{
				if ((tankTurning || moveSpeed == 0f) && animEngine && animEngine.turningStyle == AC.TurningStyle.Script)
				{
					newAngle = Mathf.Lerp (currentAngle, targetAngle, turnSpeed * Time.deltaTime * GetScriptTurningFactor () / 2f);
				}
				else
				{
					newAngle = Mathf.Lerp (currentAngle, targetAngle, turnSpeed * Time.deltaTime * GetScriptTurningFactor ());
				}
				newRotation = Quaternion.AngleAxis (newAngle * Mathf.Rad2Deg, Vector3.up);
			}
		}
		
		
		private float GetScriptTurningFactor ()
		{
			if (_animator != null && _animator.applyRootMotion)
			{
				return (1f - rootTurningFactor);
			}
			return 1f;
		}
		
		
		private void DoTurn ()
		{
			if (GetMotionControl () == MotionControl.Manual || isUFPSPlayer || lookDirection == Vector3.zero)
			{
				return;
			}
			
			if (DoRigidbodyMovement ())
			{
				_rigidbody.MoveRotation (newRotation);
			}
			else
			{
				transform.rotation = newRotation;
			}
		}
		
		
		/**
		 * <summary>Sets the intended direction to face.</summary>
		 * <param name = "_direction">The relative direction to face</param>
		 * <param name = "isInstant">If True, the Player will instantly turn to face the direction</param>
		 */
		public void SetLookDirection (Vector3 _direction, bool isInstant)
		{
			lookDirection = new Vector3 (_direction.x, 0f, _direction.z);
			Turn (isInstant);
			
			if (isUFPSPlayer)
			{
				transform.forward = lookDirection.normalized;
			}
		}
		
		
		/**
		 * <summary>Moves the character in a particular direction.</summary>
		 * <param name = "_direction">The direction to move in</param>
		 */
		public void SetMoveDirection (Vector3 _direction)
		{
			doExactLerp = false;
			if (_direction != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation (_direction, Vector3.up);
				moveDirection = targetRotation * Vector3.forward;
				moveDirection.Normalize ();
			}
		}
		
		
		/**
		 * Moves the character forward.
		 */
		public void SetMoveDirectionAsForward ()
		{
			doExactLerp = false;
			isReversing = false;
			moveDirection = transform.forward;
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				moveDirection = new Vector3 (moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize ();
		}
		
		
		/**
		 * Moves the character backward.
		 */
		public void SetMoveDirectionAsBackward ()
		{
			doExactLerp = false;
			isReversing = true;
			moveDirection = -transform.forward;
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				moveDirection = new Vector3 (moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize ();
		}
		
		
		private void SetAntiGlideState ()
		{
			if (!animEngine.isSpriteBased || GetRootMotionType () != RootMotionType.None)
			{
				antiGlideMode = false;
			}
		}
		
		
		/**
		 * <summary>Gets the character's Animation component.</summary>
		 * <returns>The character's Animation component</returns>
		 */
		public Animation GetAnimation ()
		{
			if (_animation == null)
			{
				_animation = GetComponent <Animation>();
			}
			return _animation;
		}
		
		
		/**
		 * <summary>Gets the character's Animator component. An Animator placed on the spriteChild GameObject will be given priority.</summary>
		 * <returns>The character's Animator component</returns>
		 */
		public Animator GetAnimator ()
		{
			if (_animator == null)
			{
				if (spriteChild && spriteChild.GetComponent <Animator>())
				{
					_animator = spriteChild.GetComponent <Animator>();
				}
				else if (GetComponent <Animator>())
				{
					_animator = GetComponent <Animator>();
				}
			}
			return _animator;
		}
		
		
		private RootMotionType GetRootMotionType ()
		{
			if (_animator == null || !_animator.applyRootMotion)
			{
				return RootMotionType.None;
			}
			if (animEngine.isSpriteBased)
			{
				return RootMotionType.TwoD;
			}
			return RootMotionType.ThreeD;
		}
		
		
		/**
		 * <summary>Gets the direction that the character is moving in.</summary>
		 * <returns>The direction that the character is moving in</summary>
		 */
		public Vector3 GetMoveDirection ()
		{
			return moveDirection;	
		}
		
		
		private void SetNextNodes (bool justResumedPath = false)
		{
			pausePath = false;
			nodeActionList = null;
			
			int tempPrev = targetNode;
			
			if (this is Player && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, true);
			}
			else
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, false);
			}
			
			prevNode = tempPrev;
			
			if (targetNode == 0 && activePath.pathType == AC_PathType.Loop && activePath.teleportToStart)
			{
				Teleport (activePath.transform.position);
				
				// Set rotation if there is more than one node
				if (activePath.nodes.Count > 1)
				{
					SetLookDirection (activePath.nodes[1] - activePath.nodes[0], true);
				}
				SetNextNodes ();
				return;
			}
			
			if (targetNode == -1)
			{
				EndPath (false);
				return;
			}
			
			if (justResumedPath && turnBeforeWalking)
			{
				TurnBeforeWalking ();
			}
		}
		
		
		/**
		 * <summary>Stops the character from moving along the current Paths object.</summary>
		 * <param name = "stopLerpToo">If True, then the lerp effect used to ensure pinpoint accuracy will also be cancelled</param>
		 */
		public void EndPath (bool stopLerpToo = true)
		{
			if (GetComponent <Paths>() && activePath == GetComponent <Paths>())
			{
				activePath.nodes.Clear ();
			}
			else
			{
				lastPathPrevNode = prevNode;
				lastPathTargetNode = targetNode;
				lastPathActivePath = activePath;
			}
			
			activePath = null;
			targetNode = 0;
			pathfindUpdateTime = 0f;
			
			if (charState == CharState.Move)
			{
				charState = CharState.Decelerate;
				
				if (AccurateDestination () && !stopLerpToo)
				{
					moveSpeed = 0f;
					doExactLerp = true;
				}
			}
			
			if (stopLerpToo)
			{
				doExactLerp = false;
			}
		}
		
		
		/**
		 * Resumes moving along the character's last Paths object, if there was one.
		 */
		public void ResumeLastPath ()
		{
			if (lastPathActivePath != null)
			{
				SetPath (lastPathActivePath, lastPathTargetNode, lastPathPrevNode);
			}
		}
		
		
		protected void SetLastPath (Paths _lastPathActivePath, int _lastPathTargetNode, int _lastPathPrevNode)
		{
			lastPathActivePath = _lastPathActivePath;
			lastPathTargetNode = _lastPathTargetNode;
			lastPathPrevNode = _lastPathPrevNode;
		}
		
		
		/**
		 * Stops the character moving.
		 */
		public void Halt ()
		{
			if (GetComponent <Paths>() && activePath == GetComponent <Paths>()) {}
			else
			{
				lastPathPrevNode = prevNode;
				lastPathTargetNode = targetNode;
				lastPathActivePath = activePath;
			}
			
			activePath = null;
			targetNode = 0;
			moveSpeed = 0f;
			
			if (charState == CharState.Move || charState == CharState.Decelerate)
			{
				charState = CharState.Idle;
			}
		}
		
		
		/**
		 * Forces the character into an idle state.
		 */
		public void ForceIdle ()
		{
			charState = CharState.Idle;
		}
		
		
		protected void ReverseDirection ()
		{
			int tempPrev = targetNode;
			targetNode = prevNode;
			prevNode = tempPrev;
		}
		
		
		private void PausePath (float pauseTime)
		{
			charState = CharState.Decelerate;
			pausePath = true;
			pausePathTime = Time.time + pauseTime;
			nodeActionList = null;
		}
		
		
		private void PausePath (float pauseTime, Cutscene pauseCutscene, int parameterID)
		{
			charState = CharState.Decelerate;
			pausePath = true;
			
			if (pauseCutscene.useParameters && parameterID >= 0 && pauseCutscene.parameters.Count > parameterID)
			{
				pauseCutscene.parameters [parameterID].SetValue (this.gameObject);
			}
			
			if (pauseTime > 0f)
			{
				pausePathTime = Time.time + pauseTime + 1f;
				StartCoroutine (DelayPathCutscene (pauseTime, pauseCutscene));
			}
			else
			{
				pausePathTime = 0f;
				pauseCutscene.Interact ();
				nodeActionList = pauseCutscene;
			}
		}
		
		
		private IEnumerator DelayPathCutscene (float pauseTime, Cutscene pauseCutscene)
		{
			yield return new WaitForSeconds (pauseTime);
			
			pausePathTime = 0f;
			pauseCutscene.Interact ();
			nodeActionList = pauseCutscene;
		}
		
		
		private void PausePath (float pauseTime, ActionListAsset pauseAsset, int parameterID)
		{
			charState = CharState.Decelerate;
			pausePath = true;
			
			if (pauseAsset.useParameters && parameterID >= 0 && pauseAsset.parameters.Count > parameterID)
			{
				int idToSend = 0;
				if (this.gameObject.GetComponent <ConstantID>())
				{
					idToSend = this.gameObject.GetComponent <ConstantID>().constantID;
				}
				else
				{
					ACDebug.LogWarning (this.gameObject.name + " requires a ConstantID script component!");
				}
				pauseAsset.parameters [parameterID].SetValue (idToSend);
			}
			
			if (pauseTime > 0f)
			{
				pausePathTime = Time.time + pauseTime + 1f;
				StartCoroutine (DelayPathActionList (pauseTime, pauseAsset));
			}
			else
			{
				pausePathTime = 0f;
				nodeActionList = AdvGame.RunActionListAsset (pauseAsset);
			}
		}
		
		
		private IEnumerator DelayPathActionList (float pauseTime, ActionListAsset pauseAsset)
		{
			yield return new WaitForSeconds (pauseTime);
			
			pausePathTime = 0f;
			nodeActionList = AdvGame.RunActionListAsset (pauseAsset);
		}
		
		
		private void TurnBeforeWalking ()
		{
			Vector3 direction = activePath.nodes[1] - transform.position;
			if (KickStarter.settingsManager && KickStarter.settingsManager.IsUnity2D ())
			{
				SetLookDirection (new Vector3 (direction.x, 0f, direction.y), false);
			}
			else
			{
				SetLookDirection (new Vector3 (direction.x, 0f, direction.z), false);
			}
			isTurningBeforeWalking = true;
			Turn (false);
		}
		
		
		protected bool IsTurningBeforeWalking ()
		{
			if (Mathf.Abs (turnFloat) > 0.3f && isTurningBeforeWalking)
			{
				return true;
			}
			isTurningBeforeWalking = false;
			return false;
		}
		
		
		private bool CanTurnBeforeMoving ()
		{
			if (turnBeforeWalking && activePath == GetComponent <Paths>() && targetNode <= 1 && activePath.nodes.Count > 1)
			{
				return true;
			}
			return false;
		}
		
		
		private void SetPath (Paths pathOb, PathSpeed _speed, int _targetNode, int _prevNode)
		{
			activePath = pathOb;
			targetNode = _targetNode;
			prevNode = _prevNode;
			
			doExactLerp = false;
			exactDestination = pathOb.nodes [pathOb.nodes.Count-1];
			
			if (CanTurnBeforeMoving ())
			{
				TurnBeforeWalking ();
			}
			
			if (pathOb)
			{
				if (_speed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}
			
			if (charState == CharState.Custom)
			{
				charState = CharState.Idle;
			}
			
			pathfindUpdateTime = 0f;
		}
		
		
		/**
		 * <summary>Assigns the character to a Paths object to move along. This is not the same as pathfinding - this assumes a path has already been defined.</summary>
		 * <param name = "pathOb">The Paths object to move along</param>
		 * <param name = "_speed">The speed to move along the path (Walk, Run)</param>
		 */
		public void SetPath (Paths pathOb, PathSpeed _speed)
		{
			SetPath (pathOb, _speed, 0, 0);
		}
		
		
		/**
		 * <summary>Assigns the character to a Paths object to move along. This is not the same as pathfinding - this assumes a path has already been defined.</summary>
		 * <param name = "pathOb">The Paths object to move along</param>
		 */
		public void SetPath (Paths pathOb)
		{
			SetPath (pathOb, pathOb.pathSpeed, 0, 0);
		}
		
		
		/**
		 * <summary>Assigns the character to a Paths object to move along. This is not the same as pathfinding - this assumes a path has already been defined.</summary>
		 * <param name = "pathOb">The Paths object to move along</param>
		 * <param name = "_targetNode">The index number of the first node to move to</param>
		 * <param name = "_prevNode">The index number of the node moving away from</param>
		 */
		public void SetPath (Paths pathOb, int _targetNode, int _prevNode)
		{
			SetPath (pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
		}
		
		
		/**
		 * <summary>Assigns the character to a Paths object to move along. This is not the same as pathfinding - this assumes a path has already been defined.</summary>
		 * <param name = "pathOb">The Paths object to move along</param>
		 * <param name = "_targetNode">The index number of the first node to move to</param>
		 * <param name = "_prevNode">The index number of the node moving away from</param>
		 * <param name = "affectY">If True, then the character will account for the "Y" position of nodes</param>
		 */
		public void SetPath (Paths pathOb, int _targetNode, int _prevNode, bool affectY)
		{
			if (pathOb)
			{
				SetPath (pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
				activePath.affectY = affectY;
			}
		}
		
		
		protected void CheckIfStuck ()
		{
			// Check for null movement error: if not moving on a path, end the path
			
			/*if (_rigidbody)
			{
				Vector3 newPosition = _rigidbody.position;
				if (oldPosition == newPosition)
				{
					ACDebug.Log ("Stuck in active path - removing");
					EndPath (false);
				}
				
				oldPosition = newPosition;
			}  */
		}
		
		
		/**
		 * <summary>Gets the current Paths object that the character is moving along. If the character is pathfinding, the Paths component on the character's root will be returned.</summary>
		 * <returns>The current Paths object that the character is moving along.</returns>
		 */
		public Paths GetPath ()
		{
			return activePath;
		}
		
		
		protected int GetTargetNode ()
		{
			return targetNode;
		}
		
		
		protected int GetPrevNode ()
		{
			return prevNode;
		}
		
		
		protected Paths GetLastPath ()
		{
			return lastPathActivePath;
		}
		
		
		protected int GetLastTargetNode ()
		{
			return lastPathTargetNode;
		}
		
		
		protected int GetLastPrevNode ()
		{
			return lastPathPrevNode;
		}
		
		
		/**
		 * <summary>Moves towards a point in the scene.</summary>
		 * <param name = "point">The point to move to</param>
		 * <param name = "run">If True, the character will run</param>
		 * <param name = "usePathfinding">If True, the character will pathfind using the scene's chosen navigation algorithm</param>
		 */
		public void MoveToPoint (Vector3 point, bool run = false, bool usePathfinding = false)
		{
			if (usePathfinding)
			{
				if (KickStarter.navigationManager)
				{
					Vector3[] pointArray = null;
					pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (transform.position, point, this);
					MoveAlongPoints (pointArray, false);
				}
				else
				{
					MoveToPoint (point, run, false);
				}
			}
			else
			{
				List<Vector3> pointData = new List<Vector3>();
				pointData.Add (point);
				MoveAlongPoints (pointData.ToArray (), run);
			}
		}


		/**
		 * <summary>Moves along a series of points in the scene.</summary>
		 * <param name = "pointData">The array of points to move along</param>
		 * <param name = "run">If True, the character will run</param>
		 */
		public void MoveAlongPoints (Vector3[] pointData, bool run)
		{
			Paths path = GetComponent <Paths>();
			if (path)
			{
				path.BuildNavPath (pointData);
				
				if (run)
				{
					SetPath (path, PathSpeed.Run);
				}
				else
				{
					SetPath (path, PathSpeed.Walk);
				}
				
				pathfindUpdateTime = Mathf.Max (0f, KickStarter.settingsManager.pathfindUpdateFrequency);
			}
			else
			{
				ACDebug.LogWarning (this.name + " cannot pathfind without a Paths component");
			}
		}
		
		
		/**
		 * Removes all AnimationClips from the Animation component that are not "standard", e.g. Idle, Walk, Run and Talk.
		 */
		public void ResetBaseClips ()
		{
			// Remove all animations except Idle, Walk, Run and Talk
			
			if (spriteChild && spriteChild.GetComponent <Animation>())
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in spriteChild.GetComponent <Animation>())
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					spriteChild.GetComponent <Animation>().RemoveClip (_clip);
				}
			}
			
			if (_animation)
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in _animation)
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					_animation.RemoveClip (_clip);
				}
			}
			
		}
		
		
		/**
		 * <summary>Gets the angle that a 2D character's sprite is facing, relative to the root.</summary>
		 * <returns>The angle that a 2D character's sprite is facing, relative to the root</returns>
		 */
		public float GetSpriteAngle ()
		{
			return spriteAngle;
		}
		
		
		/**
		 * <summary>Gets the suffix to add to sprite animations that account for the facing direction - e.g. "Walk" -> "Walk_DR".</summary>
		 * <returns>Gets the suffix to add to sprite animations that account for the facing direction - e.g. "Walk" -> "Walk_DR".</returns>
		 */
		public string GetSpriteDirection ()
		{
			return ("_" + spriteDirection);
		}
		
		
		/**
		 * <summary>Gets the direction that a 2D character is facing (Down = 0, Left = 1, Right = 2, Up = 3, DownLeft = 4, DownRight = 5, UpLeft = 6, UpRight = 7)</summary>
		 * <returns>The direction that a 2D character is facing (Down = 0, Left = 1, Right = 2, Up = 3, DownLeft = 4, DownRight = 5, UpLeft = 6, UpRight = 7)</returns>
		 */
		public int GetSpriteDirectionInt ()
		{
			if (spriteDirection == "D")
			{
				return 0;
			}
			if (spriteDirection == "L")
			{
				return 1;
			}
			if (spriteDirection == "R")
			{
				return 2;
			}
			if (spriteDirection == "U")
			{
				return 3;
			}
			if (spriteDirection == "DL")
			{
				return 4;
			}
			if (spriteDirection == "DR")
			{
				return 5;
			}
			if (spriteDirection == "UL")
			{
				return 6;
			}
			if (spriteDirection == "UR")
			{
				return 7;
			}
			
			return 0;
		}
		
		
		/**
		 * <summary>Locks a 2D character's sprite to face a particular direction.</summary>
		 * <param name = "direction">The direction to face (Down, Left, Right, Up, DownLeft, DownRight, UpLeft, UpRight)</param>
		 */
		public void SetSpriteDirection (CharDirection direction)
		{
			lockDirection = true;
			
			if (direction == CharDirection.Down)
			{
				spriteDirection = "D";
			}
			else if (direction == CharDirection.Left)
			{
				spriteDirection = "L";
			}
			else if (direction == CharDirection.Right)
			{
				spriteDirection = "R";
			}
			else if (direction == CharDirection.Up)
			{
				spriteDirection = "U";
			}
			else if (direction == CharDirection.DownLeft)
			{
				spriteDirection = "DL";
			}
			else if (direction == CharDirection.DownRight)
			{
				spriteDirection = "DR";
			}
			else if (direction == CharDirection.UpLeft)
			{
				spriteDirection = "UL";
			}
			else if (direction == CharDirection.UpRight)
			{
				spriteDirection = "UR";
			}
		}
		
		
		private string SetSpriteDirection (float rightAmount, float forwardAmount)
		{
			float angle = Vector2.Angle (new Vector2 (1f, 0f), new Vector2 (rightAmount, forwardAmount));
			
			if (doDiagonals)
			{
				if (forwardAmount > 0f)
				{
					if (angle > 22.5f && angle < 67.5f)
					{
						return "UR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "UL";
					}
				}
				else
				{
					if (angle > 22.5f && angle < 67.55f)
					{
						return "DR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "DL";
					}
				}
			}
			
			if (forwardAmount > 0f)
			{
				if (angle > 45f && angle < 135f)
				{
					return "U";
				}
			}
			else
			{
				if (angle > 45f && angle < 135f)
				{
					return "D";
				}
			}
			
			if (rightAmount > 0f)
			{
				return "R";
			}
			
			return "L";
		}
		
		
		private void CalcHeightChange ()
		{
			float currentHeight = transform.position.y;
			
			if (currentHeight != prevHeight && currentHeight != prevHeight2 && prevHeight != prevHeight2)
			{
				// Is changing height, but not teleporting
				heightChange = currentHeight - prevHeight;
			}
			else
			{
				heightChange = 0f;
			}
			
			prevHeight2 = prevHeight;
			prevHeight = currentHeight;
		}
		
		
		protected void StopStandardAudio ()
		{
			if (audioSource && audioSource.isPlaying)
			{
				if ((runSound && audioSource.clip == runSound) || (walkSound && audioSource.clip == walkSound))
				{
					audioSource.Stop ();
				}
			}
		}
		
		
		protected void PlayStandardAudio ()
		{
			if (audioSource)
			{
				if (isRunning && runSound)
				{
					if (audioSource.isPlaying && audioSource.clip == runSound)
					{
						return;
					}
					
					audioSource.loop = false;
					audioSource.clip = runSound;
					audioSource.Play ();
				}
				
				else if (walkSound)
				{
					if (audioSource.isPlaying && audioSource.clip == walkSound)
					{
						return;
					}
					
					audioSource.loop = false;
					audioSource.clip = walkSound;
					audioSource.Play ();
				}
			}
		}
		
		
		private void ResetAnimationEngine ()
		{
			string className = "AnimEngine";
			
			if (animationEngine == AnimationEngine.Custom)
			{
				if (customAnimationClass.Length > 0)
				{
					className = customAnimationClass;
				}
			}
			else
			{
				className += "_" + animationEngine.ToString ();
			}
			
			if (animEngine == null || animEngine.ToString () != className)
			{
				try
				{
					animEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
					if (animEngine != null)
					{
						animEngine.Declare (this);
					}
				} catch {}
			}
		}
		
		
		protected void UpdateSpriteChild (bool isTopDown, bool isUnity2D)
		{
			float forwardAmount = 0f;
			float rightAmount = 0f;
			
			if (isTopDown || isUnity2D)
			{
				forwardAmount = Vector3.Dot (Vector3.forward, transform.forward.normalized);
				rightAmount = Vector3.Dot (Vector3.right, transform.forward.normalized);
			}
			else
			{
				forwardAmount = Vector3.Dot (KickStarter.mainCamera.ForwardVector ().normalized, transform.forward.normalized);
				rightAmount = Vector3.Dot (KickStarter.mainCamera.RightVector ().normalized, transform.forward.normalized);
			}
			
			spriteAngle = Mathf.Atan (rightAmount / forwardAmount) * Mathf.Rad2Deg;
			if (forwardAmount > 0f) spriteAngle += 180f;
			else if (rightAmount > 0f) spriteAngle += 360f;
			
			if (charState == CharState.Custom && !flipCustomAnims)
			{
				flipFrames = false;
			}
			else
			{
				if (!lockDirection)
				{
					spriteDirection = SetSpriteDirection (rightAmount, forwardAmount);
				}
				
				if (!doDirections)
				{
					flipFrames = false;
				}
				else if (frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight && spriteDirection.Contains ("L"))
				{
					spriteDirection = spriteDirection.Replace ("L", "R");
					flipFrames = true;
				}
				else if (frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft && spriteDirection.Contains ("R"))
				{
					spriteDirection = spriteDirection.Replace ("R", "L");
					flipFrames = true;
				}
				else
				{
					flipFrames = false;
				}
			}
			
			if (frameFlipping != AC_2DFrameFlipping.None)
			{
				if ((flipFrames && spriteChild.localScale.x > 0f) || (!flipFrames && spriteChild.localScale.x < 0f))
				{
					spriteChild.localScale = new Vector3 (-spriteChild.localScale.x, spriteChild.localScale.y, spriteChild.localScale.z);
				}
			}
			
			if (isTopDown)
			{
				if (animEngine && !animEngine.isSpriteBased)
				{
					spriteChild.rotation = transform.rotation;
					spriteChild.RotateAround (transform.position, Vector3.right, 90f);
				}
				else
				{
					spriteChild.rotation = Quaternion.Euler (90f, 0, 0);
				}
			}
			else if (isUnity2D)
			{
				spriteChild.rotation = Quaternion.Euler (0f, 0f, 0f);
			}
			else
			{
				if (rotateSprite3D == RotateSprite3D.RelativePositionToCamera)
				{
					Vector3 relative = (transform.position - KickStarter.mainCamera.transform.position).normalized;
					spriteChild.forward = relative;
				}
				else
				{
					spriteChild.rotation = Quaternion.Euler (spriteChild.rotation.eulerAngles.x, KickStarter.mainCamera.transform.rotation.eulerAngles.y, spriteChild.rotation.eulerAngles.z);
				}
			}
			
			if (spriteChild.GetComponent <FollowSortingMap>())
			{	
				if (!lockScale)
				{
					spriteScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalScale ();
				}
				
				if (spriteScale != 0f)
				{
					transform.localScale = originalScale * spriteScale;
					
					if (lockScale)
					{
						sortingMapScale = spriteScale;
					}
					else
					{
						sortingMapScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalSpeed ();
					}
				}
			}
		}
		
		
		/**
		 * <summary>Locks the Renderer's sortingOrder.</summary>
		 * <param name = "order">The sorting order to lock the Renderer to</param>
		 */
		public void SetSorting (int order)
		{
			if (spriteChild)
			{
				if (spriteChild.GetComponent <FollowSortingMap>())
				{
					spriteChild.GetComponent <FollowSortingMap>().LockSortingOrder (order);
				}
				else
				{
					spriteChild.GetComponent <Renderer>().sortingOrder = order;
				}
			}
			
			if (GetComponent <Renderer>())
			{
				if (GetComponent <FollowSortingMap>())
				{
					GetComponent <FollowSortingMap>().LockSortingOrder (order);
				}
				else
				{
					GetComponent <Renderer>().sortingOrder = order;
				}
			}
		}
		
		
		/**
		 * <summary>Locks the Renderer's sorting layer.</summary>
		 * <param name = "layer">The sorting layer to lock the Renderer to</param>
		 */
		public void SetSorting (string layer)
		{
			if (spriteChild)
			{
				if (spriteChild.GetComponent <FollowSortingMap>())
				{
					spriteChild.GetComponent <FollowSortingMap>().LockSortingLayer (layer);
				}
				else
				{
					spriteChild.GetComponent <Renderer>().sortingLayerName = layer;
				}
			}
			
			if (GetComponent <Renderer>())
			{
				if (GetComponent <FollowSortingMap>())
				{
					GetComponent <FollowSortingMap>().LockSortingLayer (layer);
				}
				else
				{
					GetComponent <Renderer>().sortingLayerName = layer;
				}
			}
		}
		
		
		/**
		 * Unlocks any FollowSortingMap that was locked to a sorting order/layer by SetSorting().
		 */
		public void ReleaseSorting ()
		{
			if (spriteChild && spriteChild.GetComponent <Renderer>() && spriteChild.GetComponent <FollowSortingMap>())
			{
				spriteChild.GetComponent <FollowSortingMap>().lockSorting = false;
			}
			
			if (GetComponent <Renderer>() && GetComponent <FollowSortingMap>())
			{
				GetComponent <FollowSortingMap>().lockSorting = false;
			}
		}
		
		
		/**
		 * <summary>Gets the current movement speed.</summary>
		 * <returns>The current movement speed</returns>
		 */
		public float GetMoveSpeed ()
		{
			if (doWallReduction)
			{
				if (KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					Vector2 forwardVector = transform.forward;
					if (KickStarter.settingsManager.IsUnity2D ())
					{
						forwardVector = new Vector2 (transform.forward.x, transform.forward.z);
					}
					
					Vector2 origin = (Vector2) transform.position + (Vector2) wallRayOrigin + (forwardVector * wallRayForward);
					RaycastHit2D hit = Physics2D.Raycast (origin, forwardVector, wallDistance, 1 << LayerMask.NameToLayer (wallLayer));
					if (hit.collider != null)
					{
						wallReduction = Mathf.Lerp (wallReduction, (hit.point - origin).magnitude / wallDistance, Time.deltaTime * 10f);
					}
					else
					{
						wallReduction = Mathf.Lerp (wallReduction, 1f, Time.deltaTime * 10f);
					}
					return moveSpeed * wallReduction;
				}
				else
				{
					Vector3 origin = transform.position + wallRayOrigin + (transform.forward * wallRayForward);
					RaycastHit hit;
					if (Physics.Raycast (origin, transform.forward, out hit, wallDistance, 1 << LayerMask.NameToLayer (wallLayer)))
					{
						wallReduction = Mathf.Lerp (wallReduction, (hit.point - origin).magnitude / wallDistance, Time.deltaTime * 10f);
					}
					else
					{
						wallReduction = Mathf.Lerp (wallReduction, 1f, Time.deltaTime * 10f);
					}
					return moveSpeed * wallReduction;
				}
			}
			
			return moveSpeed;
		}
		
		
		/**
		 * <summary>Controls the head-facing position.</summary>
		 * <param name = "position">The point in World Space to face</param>
		 * <param name = "isInstant">If True, the head will turn instantly</param>
		 * <param name = "_headFacing">What the head should face (Manual, Hotspot, None)</param>
		 */
		virtual public void SetHeadTurnTarget (Vector3 position, bool isInstant, HeadFacing _headFacing = HeadFacing.Manual)
		{
			if (_headFacing == HeadFacing.Hotspot && headFacing == HeadFacing.Manual)
			{
				// Don't look at Hotspots if manually-set
				return;
			}
			
			headTurnTarget = position;
			headFacing = _headFacing;
			
			if (isInstant)
			{
				CalculateHeadTurn ();
				SnapHeadMovement ();
			}
		}
		
		
		/**
		 * <summary>Ceases a particular type of head-facing.</summary>
		 * <param name = "_headFacing">The type of head-facing to cease (Hotspot, Manual)</param>
		 * <param name = "isInstant">If True, the head will return to normal instantly</param>
		 */
		public void ClearHeadTurnTarget (bool isInstant, HeadFacing _headFacing)
		{
			if (headFacing == _headFacing)
			{
				ClearHeadTurnTarget (isInstant);
			}
		}
		
		
		/**
		 * <summary>Stops the head from facing a fixed point.</summary>
		 * <param name = "isInstant">If True, the head will return to normal instantly</param>
		 */
		public void ClearHeadTurnTarget (bool isInstant)
		{
			headFacing = HeadFacing.None;
			
			if (isInstant)
			{
				targetHeadAngles = Vector2.zero;
				SnapHeadMovement ();
			}
		}
		
		
		/**
		 * Snaps the head so that it faces the fixed point it's been told to, instead of rotating towards it over time.
		 */
		public void SnapHeadMovement ()
		{
			actualHeadAngles = targetHeadAngles;
			AnimateHeadTurn ();
		}
		
		
		/**
		 * <summary>Checks if the head is rotating to face it's target, but has not yet reached it.</summary>
		 * <returns>True if the head is rotating to face it's target, but has not yet reached it.</returns>
		 */
		public bool IsMovingHead ()
		{
			if (actualHeadAngles != targetHeadAngles)
			{
				return true;
			}
			return false;
		}
		
		
		/**
		 * <summary>Gets the Shapeable script component attached to the GameObject's root or child.</summary>
		 * <returns>The Shapeable script component attached to the GameObject's root or child.</returns>
		 */
		public Shapeable GetShapeable ()
		{
			Shapeable shapeable = GetComponent <Shapeable> ();
			if (shapeable == null)
			{
				shapeable = GetComponentInChildren <Shapeable>();
			}
			return shapeable;
		}
		
		
		private void UpdateHeadTurnAngle ()
		{
			CalculateHeadTurn ();
			
			if (IsMovingHead ())
			{
				AnimateHeadTurn ();
			}
		}
		
		
		private void CalculateHeadTurn ()
		{
			if (headFacing == HeadFacing.None)
			{
				targetHeadAngles = Vector2.Lerp (targetHeadAngles, Vector2.zero, Time.deltaTime * 4f);
				headTurnWeight = Mathf.Lerp (headTurnWeight, 0f, Time.deltaTime * 4f);
			}
			else
			{
				headTurnWeight = Mathf.Lerp (headTurnWeight, 1f, Time.deltaTime * 4f);
				
				// Horizontal
				Vector3 pointForward = headTurnTarget - transform.position;
				pointForward.y = 0f;
				targetHeadAngles.x = Vector3.Angle (transform.forward, pointForward);
				targetHeadAngles.x = Mathf.Min (targetHeadAngles.x, 60f);
				
				Vector3 crossProduct = Vector3.Cross (transform.forward, pointForward);
				float sideOn = Vector3.Dot (crossProduct, Vector2.up);
				
				if (sideOn < 0f)
				{
					targetHeadAngles.x *= -1f;
				}
				
				// Vertical
				Vector3 pointPitch = headTurnTarget;
				if (neckBone != null)
				{
					pointPitch -= neckBone.position;
				}
				else
				{
					pointPitch -= transform.position;
					if (_collider is CapsuleCollider)
					{
						CapsuleCollider capsuleCollder = (CapsuleCollider) _collider;
						pointPitch -= new Vector3 (0f, capsuleCollder.height * transform.localScale.y * 0.8f, 0f);
					}
				}
				
				targetHeadAngles.y = Vector3.Angle (pointPitch, pointForward);
				targetHeadAngles.y = Mathf.Min (targetHeadAngles.y, 30f);
				
				if (pointPitch.y < pointForward.y)
				{
					targetHeadAngles.y *= -1f;
				}
				
				targetHeadAngles.y *= (Vector3.Dot (transform.forward, pointForward.normalized) / 2f) + 0.5f;
				
				
				if (!ikHeadTurning)
				{
					targetHeadAngles.x /= 60f;
					targetHeadAngles.y /= 30f;
				}
				else
				{
					targetHeadAngles.y /= 60f;
				}
			}
		}
		
		
		private void AnimateHeadTurn ()
		{
			if (targetHeadAngles.x == 0f && KickStarter.stateHandler != null && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				actualHeadAngles = Vector2.Lerp (actualHeadAngles, targetHeadAngles, Time.deltaTime * 3f);
			}
			else
			{
				actualHeadAngles = Vector2.Lerp (actualHeadAngles, targetHeadAngles, Time.deltaTime * 5f);
			}
			
			if (!ikHeadTurning)
			{
				animEngine.TurnHead (actualHeadAngles);
			}
		}
		
		
		/**
		 * <summary>Parents an object to the character's hand</summary>
		 * <param name = "objectToHold">The object to hold</param>
		 * <param name = "hand">Which hand to parent the object to (Left, Right)</param>
		 * <returns>True if the parenting was sucessful</returns>
		 */
		public bool HoldObject (GameObject objectToHold, Hand hand)
		{
			if (objectToHold == null)
			{
				return false;
			}
			
			Transform handTransform;
			if (hand == Hand.Left)
			{
				handTransform = leftHandBone;
				leftHandHeldObject = objectToHold;
			}
			else
			{
				handTransform = rightHandBone;
				rightHandHeldObject = objectToHold;
			}
			
			if (handTransform)
			{
				objectToHold.transform.parent = handTransform;
				objectToHold.transform.localPosition = Vector3.zero;
				return true;
			}
			
			ACDebug.Log ("Cannot parent object - no hand bone found.");
			return false;
		}
		
		
		/**
		 * Drops any objects held in the character's hands.
		 */
		public void ReleaseHeldObjects ()
		{
			if (leftHandHeldObject != null && leftHandHeldObject.transform.IsChildOf (transform))
			{
				leftHandHeldObject.transform.parent = null;
			}
			
			if (rightHandHeldObject != null && rightHandHeldObject.transform.IsChildOf (transform))
			{
				rightHandHeldObject.transform.parent = null;
			}
		}
		
		
		/**
		 * <summary>Gets the position of the top of the character, in screen-space.</summary>
		 * <returns>The position of the top of the character, in screen-space.</returns>
		 */
		public Vector2 GetScreenCentre ()
		{
			Vector3 worldPosition = transform.position;
			
			if (_collider && _collider is CapsuleCollider)
			{
				CapsuleCollider capsuleCollder = (CapsuleCollider) _collider;
				float addedHeight = capsuleCollder.height * transform.localScale.y;
				
				if (_spriteRenderer != null)
				{
					addedHeight *= spriteChild.localScale.y;
				}
				
				if (KickStarter.settingsManager && KickStarter.settingsManager.IsTopDown ())
				{
					worldPosition.z += addedHeight;
				}
				else
				{
					worldPosition.y += addedHeight;
				}
			}
			else
			{
				if (spriteChild != null)
				{
					if (_spriteRenderer != null)
					{
						worldPosition.y = _spriteRenderer.bounds.extents.y + _spriteRenderer.bounds.center.y;
					}
					else if (spriteChild.GetComponent <Renderer>())
					{
						worldPosition.y = spriteChild.GetComponent <Renderer>().bounds.extents.y + spriteChild.GetComponent <Renderer>().bounds.center.y;
					}
				}
			}
			
			Vector3 screenPosition = Camera.main.WorldToViewportPoint (worldPosition);
			return (new Vector2 (screenPosition.x, 1 - screenPosition.y));
		}
		
		
		private bool DoRigidbodyMovement ()
		{
			if (_rigidbody && (spriteChild == null || spriteChild.GetComponent <FollowSortingMap>() == null))
			{
				// Don't use Rigidbody's MovePosition etc if the localScale is being set - Unity bug 
				return true;
			}
			return false;
		}
		
		
		private float GetTargetDistance ()
		{
			if (activePath != null && activePath.nodes.Count > targetNode)
			{
				return (activePath.nodes[targetNode] - transform.position).magnitude;
			}
			return 0f;
		}
		

		/**
		 * <summary>Checks if the character should attempt to be as accurate as possible when moving to a destination.</summary>
		 * <returns>True if the character should attempt to be as accurate as possible when moving to a destination.</returns>
		 */
		public bool AccurateDestination ()
		{
			if (KickStarter.settingsManager.experimentalAccuracy &&
			    KickStarter.settingsManager.destinationAccuracy == 1f &&
			    (this is NPC || KickStarter.settingsManager.movementMethod != MovementMethod.StraightToCursor))
			{
				return true;
			}
			return false;
		}
		
		
		private void OnLevelWasLoaded ()
		{
			headFacing = HeadFacing.None;
			lockDirection = false;
			lockScale = false;
			isLipSyncing = false;
			lipSyncShapes.Clear ();
			ReleaseSorting ();
		}
		
		
		/**
		 * <summary>Begins a lip-syncing animation based on a series of LipSyncShapes.</summary>
		 * <param name = "_lipSyncShapes">The LipSyncShapes to use as the basis for the animation</param>
		 */
		public void StartLipSync (List<LipSyncShape> _lipSyncShapes)
		{
			#if SalsaIsPresent
			if (KickStarter.speechManager.lipSyncMode == LipSyncMode.Salsa2D)
			{
				salsa2D = GetComponent <Salsa2D>();
				if (salsa2D == null)
				{
					ACDebug.LogWarning ("To perform Salsa 2D lipsyncing, Character GameObjects must have the 'Salsa2D' component attached.");
				}
			}
			else
			{
				salsa2D = null;
			}
			#endif
			
			lipSyncShapes = _lipSyncShapes;
			isLipSyncing = true;
		}
		
		
		/**
		 * <summary>Gets the current lip-sync frame.</summary>
		 * <returns>The current lip-sync frame</returns>
		 */
		public int GetLipSyncFrame ()
		{
			#if SalsaIsPresent
			if (isTalking && salsa2D != null)
			{
				return salsa2D.sayIndex;
			}
			return 0;
			#else
			if (isTalking && lipSyncShapes.Count > 0)
			{
				return lipSyncShapes[0].frame;
			}
			return 0;
			#endif
		}
		
		
		/**
		 * <summary>Gets the current lip-sync frame, as a fraction of the total number of phoneme frames.</summary>
		 * <returns>The current lip-sync frame, as a fraction of the total number of phoneme frames</returns>
		 */
		public float GetLipSyncNormalised ()
		{
			#if SalsaIsPresent
			if (salsa2D != null)
			{
				return ((float) salsa2D.sayIndex / (float) (KickStarter.speechManager.phonemes.Count - 1));
			}
			#endif
			
			if (lipSyncShapes.Count > 0)
			{
				return ((float) lipSyncShapes[0].frame / (float) (KickStarter.speechManager.phonemes.Count - 1));
			}
			return 0f;
		}
		
		
		/**
		 * <summary>Checks if the character is playing a lip-sync animation that affects the GameObject.</summary>
		 * <returns>True if the character is playing a lip-sync animation that affects the GameObject</returns>
		 */
		public bool LipSyncGameObject ()
		{
			if (isLipSyncing && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
			{
				return true;
			}
			return false;
		}
		
		
		private void ProcessLipSync ()
		{
			if (lipSyncShapes.Count > 0)
			{
				#if SalsaIsPresent
				if (salsa2D != null)
				{
					if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture && lipSyncTexture)
					{
						lipSyncTexture.SetFrame (GetLipSyncFrame ());
					}
					return;
				}
				#endif
				
				if (Time.time > lipSyncShapes[0].timeIndex)
				{
					if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject && shapeable)
					{
						if (lipSyncShapes.Count > 1)
						{
							float moveTime = lipSyncShapes[1].timeIndex - lipSyncShapes[0].timeIndex;
							shapeable.SetActiveKey (lipSyncGroupID, lipSyncShapes[1].frame, 100f, moveTime, MoveMethod.Smooth, null);
						}
						else
						{
							shapeable.SetActiveKey (lipSyncGroupID, 0, 100f, 0.2f, MoveMethod.Smooth, null);
						}
					}
					else if (KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture && lipSyncTexture)
					{
						lipSyncTexture.SetFrame (lipSyncShapes[0].frame);
					}
					
					lipSyncShapes.RemoveAt (0);
				}
			}
		}
		
		
		/**
		 * Stops the character speaking.
		 */
		public void StopSpeaking ()
		{
			isTalking = false;
			
			if (_animation && !isLipSyncing)
			{
				foreach (AnimationState state in _animation)
				{
					if (state.layer == (int) AnimLayer.Mouth)
					{
						state.normalizedTime = 1f;
						state.weight = 0f;
					}
				}
			}
			
			if (shapeable != null && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
			{
				shapeable.DisableAllKeys (lipSyncGroupID, 0.1f, MoveMethod.Curved, null);
			}
			else if (lipSyncTexture != null && KickStarter.speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture)
			{
				lipSyncTexture.SetFrame (0);
			}
			if (KickStarter.speechManager.lipSyncMode == LipSyncMode.RogoLipSync)
			{
				RogoLipSyncIntegration.Stop (this);
			}
			
			lipSyncShapes.Clear ();
			
			if (speechAudioSource)
			{
				speechAudioSource.Stop ();
			}
		}
		
		
		/**
		 * <summary>Gets the ID number of the Expression that has a specific label.</summary>
		 * <param name = "expressionLabel">The label of the Expression to return the ID number of.</param>
		 * <returns>The ID number of the Expression if found, or -1 if not found</returns>
		 */
		public int GetExpressionID (string expressionLabel)
		{
			foreach (Expression expression in expressions)
			{
				if (expression.label == expressionLabel)
				{
					return expression.ID;
				}
			}
			return -1;
		}
		
		
		/**
		 * <summary>Gets the ID number of the currently-active Expression.</summary>
		 * <returns>The ID number of the currently-active Expression</returns>
		 */
		public int GetExpressionID ()
		{
			if (currentExpression != null)
			{
				return currentExpression.ID;
			}
			return 0;
		}
		
		
		/**
		 * Clears the current Expression so that the default portrait icon is used.
		 */
		public void ClearExpression ()
		{
			currentExpression = null;
			
			if (portraitIcon != null)
			{
				portraitIcon.Reset ();
			}
			
			foreach (Expression expression in expressions)
			{
				if (expression.portraitIcon != null)
				{
					expression.portraitIcon.Reset ();
				}
			}
		}
		
		
		/**
		 * <summary>Changes the active Expression.</summary>
		 * <param name = "ID">The ID number of the Expression, in expressions, to make active.</param>
		 */
		public void SetExpression (int ID)
		{
			currentExpression = null;
			foreach (Expression expression in expressions)
			{
				if (expression.ID == ID)
				{
					currentExpression = expression;
					return;
				}
			}
			ACDebug.LogWarning ("Cannot find expression with ID=" + ID.ToString () + " on character " + gameObject.name);
		}
		
		
		/**
		 * <summary>Gets the active portrait that's displayed in a MenuGraphic element when the character speaks.
		 * The portrait can change if an Expression has been defined.</summary>
		 */
		public CursorIconBase GetPortrait ()
		{
			if (useExpressions && currentExpression != null && currentExpression.portraitIcon.texture != null)
			{
				return currentExpression.portraitIcon;
			}
			return portraitIcon;
		}
		
		
		/**
		 * <summary>Checks if the character can be controlled directly at this time.</summary>
		 * <returns>True if the character can be controlled directly at this time</returns>
		 */
		public virtual bool CanBeDirectControlled ()
		{
			return false;
		}
		
		
		/**
		 * <summary>Gets the extent to which motion is controlled by the character script (Automatic, JustTurning, Manual).</summary>
		 * <returns>The extent to which motion is controlled by the character script (Automatic, JustTurning, Manual).</returns>
		 */
		public MotionControl GetMotionControl ()
		{
			if (animationEngine == AnimationEngine.Custom)
			{
				return motionControl;
			}
			return MotionControl.Automatic;
		}
		
		
		/**
		 * <summary>Gets the character's destination when walking along a path.</summary>
		 * <returns>The character's destination when walking along a path</returns>
		 */
		public Vector3 GetTargetPosition ()
		{
			if (activePath && targetNode >= 0 && activePath.nodes.Count > targetNode)
			{
				return activePath.nodes[targetNode];
			}
			return transform.position;
		}
		
		
		/**
		 * <summary>Gets the character's target rotation.</summary>
		 * <returns>The character's target rotation</returns>
		 */
		public Quaternion GetTargetRotation ()
		{
			return Quaternion.LookRotation (lookDirection, Vector3.up);
		}
		
		
		/**
		 * <summary>Gets the character's AnimEngine ScriptableObject.</summary>
		 * <returns>The character's AnimEngine ScriptableObject</returns>
		 */
		public AnimEngine GetAnimEngine ()
		{
			if (animEngine == null || !animEngine.ToString ().EndsWith (animationEngine.ToString () + ")"))
			{
				ResetAnimationEngine ();
			}
			return animEngine;
		}
		
		
		/**
		 * <summary>Gets the character's change in height over the last frame.</summary>
		 * <returns>The character's change in height over the last frame</summary>
		 */
		public float GetHeightChange ()
		{
			return heightChange;
		}
		
		
		/**
		 * <summary>Checks if the character is moving backwards.</summary>
		 * <returns>True if the character is moving backwards</returns>
		 */
		public bool IsReversing ()
		{
			return isReversing;
		}
		
		
		/**
		 * <summary>Gets the rate at which the character is turning (negative values used when turning anti-clockwise).</summary>
		 * <returns>The rate at which the character is turning (negative values used when turninng anti-clockwise)</returns>
		 */
		public float GetTurnFloat ()
		{
			return turnFloat;
		}
		
		
		/**
		 * <summary>Checks if the character is turning.</summary>
		 * <returns>True if the character is turning</returns>
		 */
		public bool IsTurning ()
		{
			if (lookDirection == Vector3.zero || Quaternion.Angle (Quaternion.LookRotation (lookDirection), transform.rotation) < 4f)
			{
				return false;
			}
			return true;
		}
		
		
		/**
		 * <summary>Checks if the character is moving along a path.</summary>
		 * <returns>True if the character is moving along a path</returns>
		 */
		public bool IsMovingAlongPath ()
		{
			if (activePath != null && !pausePath)
			{
				return true;
			}
			return false;
		}

	}
}