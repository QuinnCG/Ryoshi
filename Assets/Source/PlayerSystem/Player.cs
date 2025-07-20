using FMODUnity;
using QFSW.QC;
using Quinn.AI;
using Quinn.CombatSystem;
using Quinn.DamageSystem;
using Quinn.RoomManagement;
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
		public static Player Instance { get; private set; }

		[SerializeField]
		private EventReference AreaMusic;
		[SerializeField]
		private AnimationClip DeathAnim;

		public float FacingDirection => _movement.FacingDirection;
		// HACK: Make all references to this point to the underlying _movement.FacingDirection.

		public Health Health { get; private set; }

		public bool InLockedRoom { get; set; }

		private PlayableAnimator _animator;
		private PlayerMovement _movement;
		private PlayerCombat _combat;

		private void Awake()
		{
			Instance = this;

			_animator = GetComponent<PlayableAnimator>();
			_movement = GetComponent<PlayerMovement>();
			_combat = GetComponent<PlayerCombat>();
			Health = GetComponent<Health>();

			Health.OnDeath += OnDeath;
		}

		private async void Start()
		{
			MusicManager.Instance.PlayAreaMusic(AreaMusic);
			await TransitionManager.Instance.FadeFromBlackAsync(1f);
		}

		private void Update()
		{
			if (ConsoleManager.IsOpen || RoomManager.Instance.IsLoading)
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

				if (InLockedRoom && LockedRoom.Instance.Boss != null)
				{
					Vector2 pos = LockedRoom.Instance.Boss.transform.position;
					float dir = transform.position.DirectionTo(pos).x;
					_movement.SetFacingDir(dir);
				}
				else
				{
					_movement.SetFacingDir(inputDir);
				}

				_movement.Move(inputDir);
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

			_movement.StopDash();
			_movement.StopJump();
			_animator.Stop();

			enabled = false;
			Health.enabled = false;
			_movement.enabled = false;
			_combat.enabled = false;

			MusicManager.Instance.StopAllMusic();

			_animator.PlayOnce(DeathAnim, true);

			await TransitionManager.Instance.FadeToBlackAsync(2f);
			await SceneManager.LoadSceneAsync(0);
		}

		[Command("hurt", "Hurts the player.")]
		protected void Hurt_Cmd(float damage = 10f)
		{
			Health.TakeDamage(new()
			{
				Damage = damage,
				Direction = Vector2.up,
				TeamType = TeamType.Environment
			}, out bool _);
		}

		[Command("kill", "Kills the player.")]
		protected void Kill_Cmd()
		{
			Health.TakeDamage(new()
			{
				Damage = Health.Current + 1f,
				Direction = Vector2.up,
				TeamType = TeamType.Environment
			}, out bool _);
		}

		[Command("heal", "Heals the player.")]
		protected void Heal_Cmd(int? health = null)
		{
			if (health.HasValue)
			{
				Health.Heal(health.Value);
			}
			else
			{
				Health.FullHeal();
			}
		}
	}
}
