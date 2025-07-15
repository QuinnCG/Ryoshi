using FMODUnity;
using QFSW.QC;
using Quinn.CombatSystem;
using Quinn.DamageSystem;
using Sirenix.OdinInspector;
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
		[SerializeField, Required, Tooltip("Not a prefab."), ChildGameObjectsOnly]
		private ParticleSystem BlockDamageVFX;
		[SerializeField]
		private EventReference BlockDamageSound;

		public float FacingDirection { get; private set; } = 1f;

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
			_health.AllowDamage = AllowDamage;
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

			if (!_movement.IsDashing && (!_combat.IsAttacking || _combat.IsRecovering))
			{
				var inputDir = Input.GetAxisRaw("Horizontal");
				_movement.Move(inputDir);

				if (inputDir != 0f)
				{
					FacingDirection = Mathf.Sign(inputDir);
				}
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

			if (!_combat.IsAttacking && !_movement.IsDashing)
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

		private bool AllowDamage(DamageInfo info)
		{
			// Ignore damage if we are blocking in the opposing direction.
			if (_combat.IsBlocking)
			{
				bool blockingDmg = false;

				if (info.Direction.x > 0f && FacingDirection < 0f)
					blockingDmg = true;

				if (info.Direction.x < 0f && FacingDirection > 0f)
					blockingDmg = true;

				if (blockingDmg)
				{
					Vector2 dir = info.Direction;
					dir.y += 1f;
					dir.Normalize();
					float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

					BlockDamageVFX.transform.rotation = Quaternion.AngleAxis(angle - 45f, Vector3.forward);
					BlockDamageVFX.Play();

					Audio.Play(BlockDamageSound);

					// Do not allow damage.
					return false;
				}
			}

			return true;
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
