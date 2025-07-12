using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(PlayerMovement))]
	[RequireComponent(typeof(PlayerCombat))]
	public class Player : MonoBehaviour
	{
		private PlayerMovement _movement;
		private PlayerCombat _combat;

		private void Awake()
		{
			_movement = GetComponent<PlayerMovement>();
			_combat = GetComponent<PlayerCombat>();
		}

		private void Update()
		{
			var inputDir = Input.GetAxisRaw("Horizontal");
			_movement.Move(inputDir);

			if (Input.GetKeyDown(KeyCode.Space))
			{
				_movement.Jump();
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				_movement.StopJump();
			}
		}
	}
}
