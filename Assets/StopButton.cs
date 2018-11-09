using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class StopButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Button btnNonLocation = GameObject.Find("Canvas/StopButton").GetComponent<Button>();
		btnNonLocation.onClick.AddListener(StopLocalization);
	}
	
	public void StopLocalization()
	{
		GameObject Camera = GameObject.Find("ARCamera");
		ARCamera TakePhoto = (ARCamera) Camera.GetComponent(typeof(ARCamera));
		TakePhoto.OnStopClick();
		GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text = "Take a photo for localization";
	}

}
