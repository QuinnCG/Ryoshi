using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace Quinn
{
	public class CameraController : MonoBehaviour
	{
		public static CameraController Instance { get; private set; }

		[SerializeField, Required]
		private CinemachineCamera VCam;

		private Collider2D _startingCollider;

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			_startingCollider = VCam.GetComponent<CinemachineConfiner2D>().BoundingShape2D;
		}

		public void SetConfiner(Collider2D confiner)
		{
			VCam.GetComponent<CinemachineConfiner2D>().BoundingShape2D = confiner;
		}

		public void ResetConfiner()
		{
			SetConfiner(_startingCollider);
		}
	}
}
