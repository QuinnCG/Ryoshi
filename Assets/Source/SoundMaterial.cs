using UnityEngine;

namespace Quinn
{
	public class SoundMaterial : MonoBehaviour
	{
		[field: SerializeField]
		public SoundMaterialType Material { get; private set; }
	}
}
