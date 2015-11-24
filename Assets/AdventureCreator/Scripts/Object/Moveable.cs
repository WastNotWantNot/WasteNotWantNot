/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Moveable.cs"
 * 
 *	This script is attached to any gameObject that is to be transformed
 *	during gameplay via the action ActionTransform.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This script provides functions to move or transform the GameObject it is attached to.
	 * It is used by the "Object: Transform" Action to move objects without scripting.
	 */
	public class Moveable : MonoBehaviour
	{

		/** True if the script is currently moving it's GameObject */
		[HideInInspector] public bool isMoving;
		
		private bool doEulerRotation = false;
		
		private float moveChangeTime;
		private float moveStartTime;
		private AnimationCurve timeCurve;
		
		private MoveMethod moveMethod;
		private TransformType transformType;
		
		private	Vector3 startPosition;
		private Vector3 endPosition;
		
		private Vector3 startScale;
		private Vector3 endScale;
		
		private Vector3 startEulerRotation;
		private Vector3 endEulerRotation;
		
		private Quaternion startRotation;
		private Quaternion endRotation;
		

		/**
		 * Halts the GameObject, if it is being moved by this script.
		 */
		public void StopMoving ()
		{
			isMoving = false;
			StopCoroutine ("_UpdateMovement");
		}


		/**
		 * Halts the GameObject, and sets it's Transform to it's target values, if it is being moved by this script.
		 */
		public void EndMovement ()
		{
			StopMoving ();

			if (transformType == TransformType.Translate || transformType == TransformType.CopyMarker)
			{
				transform.localPosition = endPosition;
			}
			if (transformType == TransformType.Rotate)
			{
				transform.localEulerAngles = endEulerRotation;
			}
			if (transformType == TransformType.CopyMarker)
			{
				transform.rotation = endRotation;
			}
			if (transformType == TransformType.Scale || transformType == TransformType.CopyMarker)
			{
				transform.localScale = endScale;
			}
		}

		
		private IEnumerator _UpdateMovement ()
		{
			while (isMoving)
			{
				if (Time.time < moveStartTime + moveChangeTime)
				{
					if (transformType == TransformType.Translate || transformType == TransformType.CopyMarker)
					{
						if (moveMethod == MoveMethod.Curved)
						{
							transform.localPosition = Vector3.Slerp (startPosition, endPosition, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
						else
						{
							transform.localPosition = AdvGame.Lerp (startPosition, endPosition, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
					}
					
					if (transformType == TransformType.Rotate || transformType == TransformType.CopyMarker)
					{
						if (doEulerRotation)
						{
							if (moveMethod == MoveMethod.Curved)
							{
								transform.localEulerAngles = Vector3.Slerp (startEulerRotation, endEulerRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
							else
							{
								transform.localEulerAngles = AdvGame.Lerp (startEulerRotation, endEulerRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
						}
						else
						{
							if (moveMethod == MoveMethod.Curved)
							{
								transform.localRotation = Quaternion.Slerp (startRotation, endRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
							else
							{
								transform.localRotation = AdvGame.Lerp (startRotation, endRotation, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
							}
						}
					}
					
					if (transformType == TransformType.Scale || transformType == TransformType.CopyMarker)
					{
						if (moveMethod == MoveMethod.Curved)
						{
							transform.localScale = Vector3.Slerp (startScale, endScale, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
						else
						{
							transform.localScale = AdvGame.Lerp (startScale, endScale, AdvGame.Interpolate (moveStartTime, moveChangeTime, moveMethod, timeCurve)); 
						}
					}
				}
				else
				{
					EndMovement ();
				}
				
				yield return new WaitForFixedUpdate ();
			}
			
			StopCoroutine ("_UpdateMovement");
		}
		

		/**
		 * <summary>Moves the GameObject by referencing a Vector3 as it's target Transform.</summary>
		 * <param name = "_newVector">The target values of either the GameObject's position, rotation or scale</param>
		 * <param name = "_moveMethod">The interpolation method by which the GameObject moves (Linear, Smooth, Curved, EaseIn, EaseOut, CustomCurve)</param>
		 * <param name = "_transitionTime">The time, in seconds, that the movement should take place over</param>
		 * <param name = "_transformType">The way in which the GameObject should be transformed (Translate, Rotate, Scale)</param>
		 * <param name = "_doEulerRotation">If True, then the GameObject's eulerAngles will be directly manipulated. Otherwise, the rotation as a Quaternion will be affected.</param>
		 * <param name = "_timeCurve">If _moveMethod = MoveMethod.CustomCurve, then the movement speed will follow the shape of the supplied AnimationCurve. This curve can exceed "1" in the Y-scale, allowing for overshoot effects.</param>
		 */
		public void Move (Vector3 _newVector, MoveMethod _moveMethod, float _transitionTime, TransformType _transformType, bool _doEulerRotation, AnimationCurve _timeCurve)
		{
			StopCoroutine ("_UpdateMovement");
			
			if (GetComponent <Rigidbody>() && !GetComponent <Rigidbody>().isKinematic)
			{
				GetComponent <Rigidbody>().velocity = GetComponent <Rigidbody>().angularVelocity = Vector3.zero;
			}
			
			if (_transitionTime == 0f)
			{
				isMoving = false;
				
				if (_transformType == TransformType.Translate)
				{
					transform.localPosition = _newVector;
				}
				else if (_transformType == TransformType.Rotate)
				{
					transform.localEulerAngles = _newVector;
				}
				else if (_transformType == TransformType.Scale)
				{
					transform.localScale = _newVector;
				}
			}
			else
			{
				isMoving = true;
				
				doEulerRotation = _doEulerRotation;
				moveMethod = _moveMethod;
				transformType = _transformType;
				
				startPosition = endPosition = transform.localPosition;
				startEulerRotation = endEulerRotation = transform.localEulerAngles;
				startRotation = endRotation = transform.localRotation;
				startScale = endScale = transform.localScale;
				
				if (_transformType == TransformType.Translate)
				{
					endPosition = _newVector;
				}
				else if (_transformType == TransformType.Rotate)
				{
					endRotation = Quaternion.Euler (_newVector);
					endEulerRotation = _newVector;
				}
				else if (_transformType == TransformType.Scale)
				{
					endScale = _newVector;
				}
				
				moveChangeTime = _transitionTime;
				moveStartTime = Time.time;
				
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = _timeCurve;
				}
				else
				{
					timeCurve = null;
				}
				
				StartCoroutine ("_UpdateMovement");
			}
		}
		

		/**
		 * <summary>Moves the GameObject by referencing a Marker component as it's target Transform.</summary>
		 * <param name = "_marker">A Marker whose position, rotation and scale will be the target values of the GameObject</param>
		 * <param name = "_moveMethod">The interpolation method by which the GameObject moves (Linear, Smooth, Curved, EaseIn, EaseOut, CustomCurve)</param>
		 * <param name = "_transitionTime">The time, in seconds, that the movement should take place over</param>
		 * <param name = "_timeCurve">If _moveMethod = MoveMethod.CustomCurve, then the movement speed will follow the shape of the supplied AnimationCurve. This curve can exceed "1" in the Y-scale, allowing for overshoot effects.</param>
		 */
		public void Move (Marker _marker, MoveMethod _moveMethod, float _transitionTime, AnimationCurve _timeCurve)
		{
			if (GetComponent <Rigidbody>() && !GetComponent <Rigidbody>().isKinematic)
			{
				GetComponent <Rigidbody>().velocity = GetComponent <Rigidbody>().angularVelocity = Vector3.zero;
			}
			
			StopCoroutine ("_UpdateMovement");
			transformType = TransformType.CopyMarker;
			
			if (_transitionTime == 0f)
			{
				isMoving = false;
				
				transform.localPosition = _marker.transform.localPosition;
				transform.localEulerAngles = _marker.transform.localEulerAngles;
				transform.localScale = _marker.transform.localScale;
			}
			else
			{
				isMoving = true;
				
				doEulerRotation = false;
				moveMethod = _moveMethod;
				
				startPosition = transform.localPosition;
				startRotation = transform.localRotation;
				startScale = transform.localScale;
				
				endPosition = _marker.transform.localPosition;
				endRotation = _marker.transform.localRotation;
				endScale = _marker.transform.localScale;
				
				moveChangeTime = _transitionTime;
				moveStartTime = Time.time;
				
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = _timeCurve;
				}
				else
				{
					timeCurve = null;
				}
				
				StartCoroutine ("_UpdateMovement");
			}
		}
		

		/**
		 * An alias of StopMoving, for easy use in the "Object: Send message" Action.
		 */
		public void Kill ()
		{
			StopMoving ();
		}
		
	}
	
}