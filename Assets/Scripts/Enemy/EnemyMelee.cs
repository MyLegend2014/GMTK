﻿using System;
using System.Collections.Generic;
using Collisions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class EnemyMelee : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    public Moves moveUsing = new Moves();


    [SerializeField] private List<Moves> moveList = new List<Moves>();
    public CombatStats combatStats = new CombatStats();
    public MovesManager movesInspector;
    private EntityManager entityManager;
    private Entity meleeEntity;
 
    private static readonly int CombatAction = Animator.StringToHash("CombatAction");

    void Start()
    {
        //if (!entityManager.HasComponent<EnemyMeleeMovementComponent>(meleeEntity)) return;
        if (meleeEntity == Entity.Null)
        {
            meleeEntity = GetComponent<CharacterEntityTracker>().linkedEntity;
            if (entityManager == default)
            {
                entityManager = GetComponent<CharacterEntityTracker>().entityManager;
            }

            //entityManager.AddComponentObject(meleeEntity, this);
            if(meleeEntity != Entity.Null) 
                entityManager.AddComponentObject(meleeEntity, this);

        }
        
        animator = GetComponent<Animator>();
        //var em = GetComponent<EnemyMove>();
        agent = GetComponent<NavMeshAgent>();

        if (!movesInspector) return;

        for (var i = 0; i < movesInspector.Moves.Count; i++)
        {
            var move = movesInspector.Moves[i];
            if(!move.active) continue;

            move.target = moveUsing.target; //default target assigned in system
            move.targetEntity = meleeEntity;
            moveList.Add(move);
        }

    }

    public void SelectMoveUsing()
    {
        if (moveList.Count == 0) return;
        combatAction = Random.Range(1, moveList.Count + 1);
        moveUsing = moveList[combatAction - 1];
        if (moveUsing.moveAudioSource.isActiveAndEnabled)
        {
            if (moveUsing.moveAudioSource && moveUsing.moveAudioClip &&
                !moveUsing.moveAudioSource.isPlaying)
            {
                //Debug.Log("PLAY SOUND");
                moveUsing.moveAudioSource.clip = moveUsing.moveAudioClip;
                moveUsing.moveAudioSource.PlayOneShot(moveUsing.moveAudioClip);
            }
        }

        if (moveUsing.moveParticleSystem)
        {
            moveUsing.moveParticleSystem.Play(true);
        }
        
        var animationIndex = (int)moveUsing.animationType;
        var primaryTrigger = moveUsing.triggerType;
            
        if (entityManager.HasComponent<CheckedComponent>(meleeEntity))
        {
            var defense = animationIndex == (int)AnimationType.Deflect;
            var checkedComponent = entityManager.GetComponentData<CheckedComponent>(meleeEntity);
            checkedComponent.anyDefenseStarted = defense;
            checkedComponent.primaryTrigger = primaryTrigger;
            entityManager.SetComponentData(meleeEntity, checkedComponent);
            StartMove(animationIndex);
        }
        
    }

    private int combatAction { get; set; }

    private void StartMove(int animationIndex)
    {
        animator.SetInteger(CombatAction, animationIndex);
    }



    public void StartAttackUpdateCheckComponent()//event
    {
        if (entityManager.HasComponent<CheckedComponent>(meleeEntity))
        {
            var checkedComponent = entityManager.GetComponentData<CheckedComponent>(meleeEntity);
            checkedComponent.anyAttackStarted = true;
            checkedComponent.attackFirstFrame = true;
            checkedComponent.hitTriggered = false;
            entityManager.SetComponentData(meleeEntity, checkedComponent);
            
        }
    }
    
    public void StartMotionUpdateCheckComponent()//event
    {
        if (entityManager.HasComponent<CheckedComponent>(meleeEntity))
        {
            var checkedComponent = entityManager.GetComponentData<CheckedComponent>(meleeEntity);
        }
      
    }


    public void MatchupAdjustments()//event
    {
        if (entityManager.HasComponent<MatchupComponent>(meleeEntity))
        {
            var matchupComponent = entityManager.GetComponentData<MatchupComponent>(meleeEntity);
            entityManager.SetComponentData(meleeEntity, matchupComponent);
        }
    }

    public void EndAttack()
    {
        if (entityManager.HasComponent<CheckedComponent>(meleeEntity))
        {
            var checkedComponent = entityManager.GetComponentData<CheckedComponent>(meleeEntity);
            if (checkedComponent.hitLanded == false && entityManager.HasComponent<ScoreComponent>(meleeEntity))
            {
                var score = entityManager.GetComponentData<ScoreComponent>(meleeEntity);
                score.combo = 0;
                score.streak = 0;
                entityManager.SetComponentData(meleeEntity, score);
            }

            checkedComponent.hitLanded = false;//set at end of attack only
            checkedComponent.anyDefenseStarted = false;
            checkedComponent.anyAttackStarted = false;
            checkedComponent.AttackStages = AttackStages.End;

            entityManager.SetComponentData(meleeEntity, checkedComponent);

        }
    }
    
    public void StartAgent()
    {
        agent.enabled = true;
    }

    public void StopAgent()
    {
        agent.enabled = false;
    }

    public void StartAimIK()
    {
    }

    public void StopAimIK()
    {
    }


    public void StartIK()
    {
    }

    
    
    

    public void StopIK()
    {
    }

    public void Aim()
    {
        moveUsing.target = entityManager.GetComponentData<MatchupComponent>(meleeEntity).targetZone;
    }

    public void LateUpdateSystem()
    {
        if (entityManager == default) return;
        Aim();
    }


    
}