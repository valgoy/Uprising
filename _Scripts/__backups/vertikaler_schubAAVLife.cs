using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vertikaler_schubAAVLife : FlyingLife {
	
	public float MinHeight = 7f;
	public float MaxHeight = 50f;
	
	public bool  WippingUp = true;
	public float WippingFactor = 1f;
	public float MaxWippingFactor = 1f;
	
	public float AccelerationUp = 2f;
	public float AccelerationForward = 20f;
	
	public float MaxAccelerationUp = 70f;
	
	public Vector3 Destination; 
	public Vector3 StartOfFalling = Vector3.zero;
	
	public SimpleLifeform myTarget;
	
	public Vector3 Force = Vector3.zero;
	public Vector3   GravitationForce = Vector3.zero;
	
	public Vector3 GainHeightWaypoint;
	
	public bool VerticalStart = false;
	
	protected RaycastHit _heightHitter;
	protected Ray _rayDownToTheGround;
	
	public Vector3 F_helper = Vector3.zero;
	
	private float currentVelocity;
	private FlyingUnitState _flyToEnemyState;
	
	public override void Awake ()
	{
		base.Awake ();
		
	}
	
	public override void Start ()
	{
		base.Start ();
		_selectedRegions = new Region[8];
		_selectedRegionsHash = new System.Guid[8];
		_rayDownToTheGround = new Ray(transform.position, -transform.up);
		Force = Vector3.zero;
		
		GravitationForce.y = -15f;
		Destination = transform.position;
		Destination.y += 15f;
		Destination.z -= 1;
	}
	
	public override void initFSM ()
	{
		_stateMachine = ScriptableObject.CreateInstance<LifeStateMachine>(); //(this);
		_stateMachine.setOwner(this);
		
		_flyToEnemyState = ScriptableObject.CreateInstance<FlyingUnitState>();
		
		_stateMachine.AddTransition(Transition.Idle,         _flyToEnemyState);
		_stateMachine.AddTransition(Transition.Patrol,       _flyToEnemyState);
		_stateMachine.AddTransition(Transition.FollowFriend, FollowFriendState.Instance);
		
		List<AbstractState<BasicLifeform>> F_stateListChase = new List<AbstractState<BasicLifeform>>();
		F_stateListChase.Add(CanSeeTargetState.Instance);
		F_stateListChase.Add(WeaponChangeState.Instance);
		F_stateListChase.Add(AimAtTarget.Instance);
		F_stateListChase.Add(_flyToEnemyState);
		F_stateListChase.Add(FireWeaponState.Instance);
		F_stateListChase.Add(RememberTargetLastPosition.Instance);
		
		CompoundState F_chaseEnemy = ScriptableObject.CreateInstance<CompoundState>();
		F_chaseEnemy.setStateList(F_stateListChase.ToArray());
		
		_stateMachine.AddTransition(Transition.ChaseEnemy,    F_chaseEnemy);
		
		
		List<AbstractState<BasicLifeform>> F_listLost = new List<AbstractState<BasicLifeform>>();
		F_listLost.Add(CannotSeeTargetState.Instance);
		CompoundState F_lostTargetState = ScriptableObject.CreateInstance<CompoundState>();
		F_lostTargetState.setStateList(F_listLost.ToArray());
		
		_stateMachine.AddTransition(Transition.LostTarget,    F_lostTargetState);
		_stateMachine.AddTransition(Transition.SearchTarget,  SearchTargetState.Instance);
		_stateMachine.AddTransition(Transition.GoHome,        GoHomeState.Instance);
		_stateMachine.AddTransition(Transition.Flee,          FleeSimpleState.Instance);
		
		
		_stateMachine.setCurrentState(Transition.Idle);
	}
	
//	protected void FixedUpdate ()
//	{
//		F_helper = transform.position;
//		F_helper.z -= 1;
//		Debug.DrawLine(F_helper, Destination, Color.cyan);
//		if (GainHeightWaypoint != Vector3.zero) {
//			Destination = GainHeightWaypoint;
//			if ((GainHeightWaypoint - transform.position).sqrMagnitude <= 1.5f) {
//				if (Vector3.Dot(transform.forward, (myTarget.transform.position - transform.position).normalized) < 0f) {
//					GainHeightWaypoint = GainHeightWaypoint + (transform.right * 30f);
//				} else {
//					GainHeightWaypoint = Vector3.zero;
//				}
//			}
//		} else {
//			Destination = myTarget.transform.position;
//		}
//	
//		
//		
//			//beware, rigidbody.velocity has a gravitation calculation right before fixedupdate
//			//means there is already an applied force down the gravitation, that we have to outsmart with speed against gravitation
//			Vector3 velocity = GravitationForce;
//			if (StartOfFalling != Vector3.zero) {
//				
//			}
//			
//			Debug.DrawRay(transform.position, Vector3.up * GravitationForce.y, Color.red);
//			
//			float F_groundHeight = 0f;
//			if (Physics.Raycast(transform.position, -Vector3.up, out _heightHitter, MaxHeight, LayerUtils.GroundMask)) {
//				F_groundHeight = _heightHitter.distance;
//			}
//			
//			if ((transform.position.y-WippingFactor) > Destination.y) {
//				Force.y -= AccelerationUp * Time.deltaTime * 0.5f;
//				Vector3 beforeChange = velocity +  Force;
//				if ((transform.position.y + beforeChange.y) < Destination.y) {
//					Force.y = Mathf.SmoothDamp(Force.y, -GravitationForce.y, ref currentVelocity, Time.deltaTime * AccelerationUp);
//				}
//	//			if (WippingUp) {
//	//				WippingUp = false;
//	//				WippingFactor = -MaxWippingFactor;
//	//			}
//			} else if ((transform.position.y-WippingFactor) < Destination.y) {
//				Force.y += AccelerationUp * Time.deltaTime;
//				Vector3 beforeChange = velocity +  Force;
//				if ((transform.position.y + beforeChange.y) > Destination.y) {
//					Force.y = Mathf.SmoothDamp(Force.y, -GravitationForce.y, ref currentVelocity, Time.deltaTime * AccelerationUp);
//				} 
//	//			if (!WippingUp) {
//	//				WippingUp = true;
//	//				WippingFactor = MaxWippingFactor;
//	//			}
//			}
//			
//	//		if ((Destination - transform.position).sqrMagnitude <= 1.2f) {
//				Destination = myTarget.transform.position;
//	//		}
//			
//			Debug.DrawRay(transform.position, Vector3.up * Force.y, Color.yellow);
//			velocity.y += Force.y;
//			
//			if ((F_groundHeight-(Lifesize.y*0.5f)) <= 0.1f && velocity.y <= 0f) {
//				velocity.y = 0f;
//			}
//			rigidbody.velocity = velocity;
//			
//			if (velocity.y < 0f) {
//				if (StartOfFalling == Vector3.zero) {
//					StartOfFalling = transform.position;
//				}
//			} else {
//				StartOfFalling = Vector3.zero;
//			}
//	}
	
    
    public override void OnChildTriggerEnter(ChildCollider P_child, Collider P_other) {
		if (P_child.ColliderID == 0) { //main collider
		
		} else if (P_child.ColliderID == 1) { //Vision-Collider
			if (P_other.gameObject.tag == "floor") {
				FlyingUnitState.Instance.GainHeight(this);
				
			} else {
				BasicLifeform F_life = GameObjectHelper.getLifeform(P_other.gameObject.transform);
				FlyingUnitState.Instance.AvoidObstacle(this, F_life, P_other);
				
			
			}
		}
    }
    
    public override void OnChildTriggerExit(ChildCollider P_child, Collider P_other) {
    
    }
}
