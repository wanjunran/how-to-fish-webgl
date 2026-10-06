using UnityEngine;

public class ExtraLava : MonoBehaviour
{
	[SerializeField]
	private MainLava _mainLava;

	private void OnCollisionStay(Collision col)
	{
		_mainLava.TouchLava(col.collider);
	}
}
