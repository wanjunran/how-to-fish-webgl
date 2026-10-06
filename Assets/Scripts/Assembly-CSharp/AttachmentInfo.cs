using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Attachment", menuName = "How to Fish/Attachment")]
public class AttachmentInfo : ScriptableObject
{
	[SerializeField]
	private LocalizedString _nameLocalized;

	[SerializeField]
	private string _name;

	[SerializeField]
	private LocalizedString _descriptionLocalized;

	[SerializeField]
	private string _description;

	public string NameLocalized => _nameLocalized.GetLocalizedString();

	public string DescriptionLocalized => _descriptionLocalized.GetLocalizedString();
}
