using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavMeshAgentTest : MonoBehaviour
{

    public Transform goal;
    NavMeshAgent agent;
    public Animator animator;
    int animIDSpeed, _animIDGrounded, _animIDJump, _animIDFreeFall, _animIDMotionSpeed;
    float animationBlend = 0f;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = goal.position;
        animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        animator.SetBool(_animIDGrounded, true);
        animator.SetBool(_animIDJump, false);
        animator.SetBool(_animIDFreeFall, false);
    }

    private void Update()
    {
        animationBlend = Mathf.Lerp(animationBlend, agent.speed, Time.deltaTime * agent.acceleration);
        if (animationBlend < 0.01f) animationBlend = 0f;
        agent.SetDestination(goal.position);
        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(_animIDMotionSpeed, 1);
    }

}

