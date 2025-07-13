using Quinn.CombatSystem;
using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(PlayerMovement))]
	[RequireComponent(typeof(PlayerCombat))]
	public class Player : MonoBehaviour
	{
		public float FacingDirection { get; private set; } = 1f;

		private PlayableAnimator _animator;
		private PlayerMovement _movement;
		private PlayerCombat _combat;

		private void Awake()
		{
			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();
			_combat = GetComponent<PlayerCombat>();
		}

		private void Update()
		{
			if (!_combat.IsAttacking)
			{
				var inputDir = Input.GetAxisRaw("Horizontal");
				_movement.Move(inputDir);

				if (inputDir != 0f)
				{
					FacingDirection = Mathf.Sign(inputDir);
				}

				if (Input.GetKeyDown(KeyCode.Space))
				{
					_movement.Jump();
				}
				else if (Input.GetKeyUp(KeyCode.Space))
				{
					_movement.StopJump();
				}

				if (Input.GetMouseButtonDown(0))
				{
					_combat.Attack();
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				_combat.ReleaseAttack();
			}
		}
	}
}
