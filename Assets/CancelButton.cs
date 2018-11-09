using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class CancelButton : MonoBehaviour {
	public GameObject PopUpPanel;
	public GameObject EmptyGameObject;
	// Use this for initialization
	void Start () {
		EmptyGameObject = GameObject.Find("EmptyObject");
		Button btnCancel = GameObject.Find("Canvas/PopUpPanel/Cancel").GetComponent<Button>();
		btnCancel.onClick.AddListener(Cancel);
	}
	
	// Update is called once per frame
	public void Cancel()
	{
		PopUpPanel = GameObject.Find ("Canvas/PopUpPanel");
		PopUpPanel.transform.localScale = new Vector3 (0, 0, 0);
		EmptyGameObject.transform.localScale = new Vector3 (1, 1, 1);
	}
}
