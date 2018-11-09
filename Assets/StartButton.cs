using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class StartButton : MonoBehaviour{

	void Start()
	{
		Button btnLocation = GameObject.Find("Canvas/StartButton").GetComponent<Button>();
		btnLocation.onClick.AddListener(StartLocalization);
	}

	public void StartLocalization()
	{
		GameObject Camera = GameObject.Find("ARCamera");
		ARCamera TakePhoto = (ARCamera) Camera.GetComponent(typeof(ARCamera));
		TakePhoto.OnStartClick();
	}

}