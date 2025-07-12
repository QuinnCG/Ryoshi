using UnityEngine;

namespace Quinn
{
	[RequireComponent(typeof(PlayerMovement))]
	public class Player : MonoBehaviour
	{
		private PlayerMovement _movement;

		private void Awake()
		{
			_movement = GetComponent<PlayerMovement>();
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
