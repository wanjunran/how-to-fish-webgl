using UnityEngine;

public class Attachment : MonoBehaviour
{
	[SerializeField]
	private AttachmentInfo _info;

	[SerializeField]
	private int _cost;

	public AttachmentInfo Info => _info;

	public int Cost => _cost;
}
