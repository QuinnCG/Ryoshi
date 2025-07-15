using QFSW.QC;
using Quinn.CombatSystem;
using Quinn.DamageSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quinn
{
	[RequireComponent(typeof(PlayableAnimator))]
	[RequireComponent(typeof(PlayerMovement))]
	[RequireComponent(typeof(PlayerCombat))]
	[RequireComponent(typeof(Health))]
	public class Player : MonoBehaviour
	{
		public float FacingDirection => _movement.FacingDirection;
		// HACK: Make all references to this point to the underlying _movement.FacingDirection.

		private PlayableAnimator _animator;
		private PlayerMovement _movement;
		private PlayerCombat _combat;
		private Health _health;

		private void Awake()
		{
			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();
			_combat = GetComponent<PlayerCombat>();
			_health = GetComponent<Health>();

			_health.OnDeath += OnDeath;
		}

		private void Update()
		{
			if (ConsoleManager.IsOpen)
				return;

			if (Input.GetMouseButtonDown(0))
			{
				_combat.Attack();
			}
			else if (Input.GetMouseButtonUp(0))
			{
				_combat.ReleaseAttack();
			}

			if (!_movement.IsDashing && (!_combat.IsAttacking || _combat.IsRecovering) && !_combat.IsStaggered)
			{
				var inputDir = Input.GetAxisRaw("Horizontal");
				_movement.Move(inputDir);

				_movement.SetFacingDir(inputDir);
			}

			if (!_combat.IsAttacking || _combat.IsRecovering)
			{
				if (Input.GetMouseButton(1))
				{
					_combat.Block();
				}
				else
				{
					_combat.Unblock();
				}
			}

			if (!_combat.IsAttacking && !_movement.IsDashing && !_combat.IsStaggered)
			{
				if (Input.GetKeyDown(KeyCode.Space))
				{
					_movement.Jump();
				}
				else if (Input.GetKeyUp(KeyCode.Space))
				{
					_movement.StopJump();
				}

				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
				{
					_movement.Crouch();
				}
				else
				{
					_movement.Uncrouch();
				}

				if (Input.GetKeyDown(KeyCode.LeftShift))
				{
					_movement.Dash();
				}
			}
		}

		private async void OnDeath()
		{
			Log.Notice("Player Death!");
			await SceneManager.LoadSceneAsync(0);
		}

		[Command("hurt", "Hurts the player.")]
		protected void Hurt_Cmd(int damage = 1)
		{
			_health.TakeDamage(new()
			{
				Damage = damage,
				Direction = Vector2.up,
				TeamType = TeamType.Environment
			});
		}

		[Command("kill", "Kills the player.")]
		protected void Kill_Cmd()
		{
			_health.TakeDamage(new()
			{
				Damage = _health.Current + 1f,
				Direction = Vector2.up,
				TeamType = TeamType.Environment
			});
		}

		[Command("heal", "Heals the player.")]
		protected void Heal_Cmd(int? health = null)
		{
			if (health.HasValue)
			{
				_health.Heal(health.Value);
			}
			else
			{
				_health.FullHeal();
			}
		}
	}
}
