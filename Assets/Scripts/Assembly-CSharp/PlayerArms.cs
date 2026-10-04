using UnityEngine;

public class PlayerArms : MonoBehaviour
{
	[SerializeField]
	private IK _ikRight;

	[SerializeField]
	private IK _ikLeft;

	public void SetIKTarget(Transform right, Transform left)
	{
		if ((bool)_ikRight && (bool)_ikLeft)
		{
			_ikRight.enabled = true;
			_ikLeft.enabled = true;
			_ikRight.Target = right;
			_ikLeft.Target = left;
		}
	}
}
