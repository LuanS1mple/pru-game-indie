    using System.Collections;
    using UnityEngine;

    public class MiniBossAttack : MonoBehaviour
    {
        private AudioManager audioManager;

        [Header("Attack Settings")]
        public float attackCooldown = 2f;
        private float nextAttackTime;
        private bool isAttacking;

        [Header("Movement Settings")]
        public float moveSpeed = 2f;
        public float stopDistance = 1.2f;

        [Header("Animator Settings")]
        public GameObject animatorObject;
        private Animator animator;
        public string punchTrigger = "PunchTrigger";
        public string kickTrigger = "KickTrigger";
        public string crouchKickTrigger = "CrouchKickTrigger";
        public string isWalkingParam = "IsWalking";

        [Header("Player Target")]
        public Transform player;

        [Header("Attack Colliders")]
        public PolygonCollider2D punchCollider;
        public PolygonCollider2D kickCollider;
        public PolygonCollider2D crouchKickCollider;

        void Start()
        {
            if (animatorObject != null)
            {
                animator = animatorObject.GetComponent<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError("❌ [MiniBossAttack] Animator null! Gán animatorObject chưa đúng?");
            }
            else
            {
                Debug.Log("✅ [MiniBossAttack] Animator đã được gán: " + animatorObject.name);

                // ✅ Kiểm tra sự tồn tại của Trigger Parameter trong Animator
                CheckTriggerExists(punchTrigger);
                CheckTriggerExists(kickTrigger);
                CheckTriggerExists(crouchKickTrigger);
            }

            audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
            DisableAllColliders();
        }

        void Update()
        {
            if (player == null)
            {
                if (animator != null) animator.SetBool(isWalkingParam, false);
                return;
            }

            float dist = Vector2.Distance(transform.position, player.position);

            if (!isAttacking && dist > stopDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                if (animator != null) animator.SetBool(isWalkingParam, true);
            }
            else
            {
                if (animator != null) animator.SetBool(isWalkingParam, false);

                if (Time.time >= nextAttackTime && !isAttacking && dist <= stopDistance)
                {
                    StartCoroutine(AttackRoutine());
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }

        public void StartAttack(Transform target)
        {
            player = target;
            nextAttackTime = Time.time;
            Debug.Log("👀 [MiniBossAttack] Player detected.");
        }

        public void StopAttack()
        {
            player = null;
            if (animator != null) animator.SetBool(isWalkingParam, false);
            Debug.Log("👋 [MiniBossAttack] Player left.");
        }

        IEnumerator AttackRoutine()
        {
            isAttacking = true;
            if (animator != null) animator.SetBool(isWalkingParam, false);

            int attackType = Random.Range(0, 3);
            Debug.Log($"🎯 [MiniBossAttack] Attack type: {attackType}");

            yield return new WaitForSeconds(0.05f);

            switch (attackType)
            {
                case 0:
                    TriggerAnimation(punchTrigger, punchCollider);
                    break;
                case 1:
                    TriggerAnimation(kickTrigger, kickCollider);
                    break;
                case 2:
                    TriggerAnimation(crouchKickTrigger, crouchKickCollider);
                    break;
            }

            yield return new WaitForSeconds(0.9f);
            DisableAllColliders();
            isAttacking = false;

            if (animator != null)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"🏁 [MiniBossAttack] Current State after attack: {state.shortNameHash}");
            }
        }

        private void TriggerAnimation(string triggerName, PolygonCollider2D collider)
        {
            if (animator == null)
            {
                Debug.LogError("❌ [MiniBossAttack] Animator null khi set trigger!");
                return;
            }

            Debug.Log($"🕹 [MiniBossAttack] SetTrigger({triggerName})");

            animator.SetTrigger(triggerName);

            //Âm thanh quái tung chiêu
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.monsterAttack);
            }

        // ✅ kiểm tra trạng thái animator sau 1 frame
        StartCoroutine(CheckAnimationStateNextFrame(triggerName));

            EnableAttackColliderDelayed(collider, 0.15f);
        }

        IEnumerator CheckAnimationStateNextFrame(string triggerName)
        {
            yield return null;
            var state = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"ℹ️ [Animator Debug] Sau trigger {triggerName} → state: {state.shortNameHash}");
        }

        void EnableAttackColliderDelayed(PolygonCollider2D col, float delay)
        {
            StartCoroutine(EnableColCO(col, delay));
        }

        IEnumerator EnableColCO(PolygonCollider2D col, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (col == null)
            {
                Debug.LogWarning($"⚠️ [MiniBossAttack] Collider null khi bật!");
            }
            else
            {
                col.enabled = true;
                Debug.Log($"✅ [MiniBossAttack] Bật collider: {col.gameObject.name}");
            }
        }

        void DisableAllColliders()
        {
            if (punchCollider != null) punchCollider.enabled = false;
            if (kickCollider != null) kickCollider.enabled = false;
            if (crouchKickCollider != null) crouchKickCollider.enabled = false;
        }

        private void CheckTriggerExists(string triggerName)
        {
            if (animator == null) return;
            bool exists = false;
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                Debug.LogError($"❌ [MiniBossAttack] Trigger '{triggerName}' không tồn tại trong Animator!");
            }
            else
            {
                Debug.Log($"✅ [MiniBossAttack] Trigger '{triggerName}' OK.");
            }
        }
    }
