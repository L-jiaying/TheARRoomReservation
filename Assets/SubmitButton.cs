using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class SubmitButton : MonoBehaviour {
	public GameObject PopUpPanel;
	public GameObject EmptyGameObject;
	public GameObject[] toggles;
	public string RoomName;
	public string ReservationStatus;
	public string ReservationTime;
	public string Now;
	void Start()
	{
		EmptyGameObject = GameObject.Find("EmptyObject");
		//RoomName = "A208";
		toggles = GameObject.FindGameObjectsWithTag ("Reserve");
		Button btnSubmit = GameObject.Find("Canvas/PopUpPanel/Submit").GetComponent<Button>();
		btnSubmit.onClick.AddListener(SubmitForm);
	}

	public void SubmitForm()
	{
		RoomName = GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text;
		foreach (GameObject Reservetoggle in toggles) {
			Toggle m_Toggle = Reservetoggle.GetComponent<Toggle>();
			ReservationTime = m_Toggle.name.Substring(1,2);
			ReservationTime += ":00:00"; 
			if (m_Toggle.isOn) {
				//Debug.Log ("Reserve" + m_Toggle.name);
				ReservationStatus = "true";
			} else {
				//Debug.Log ("Not reserve" + m_Toggle.name);
				ReservationStatus = "false";
			}
			StartCoroutine ("PostForm");
		}
		PopUpPanel = GameObject.Find ("Canvas/PopUpPanel");
		PopUpPanel.transform.localScale = new Vector3 (0, 0, 0);
		EmptyGameObject.transform.localScale = new Vector3 (1, 1, 1);
		GameObject Camera = GameObject.Find("ARCamera");
		ARCamera TakePhoto = (ARCamera) Camera.GetComponent(typeof(ARCamera));
		TakePhoto.OnStartClick();
	}

	public IEnumerator PostForm(){
		Debug.Log ("Post Form is started");
		WWWForm Form = new WWWForm();
		Form.AddField ("RoomName", RoomName);
		Form.AddField ("ReservationStatus", ReservationStatus);
		Form.AddField("ReservationTime", ReservationTime);
		UnityWebRequest submit = UnityWebRequest.Post("http://192.168.43.238:80/arroomreservation/RequestedReservation.php", Form);
		yield return submit.SendWebRequest ();
		if (submit.isNetworkError || submit.isHttpError) {
			GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text = "Error " + submit.downloadHandler.text;
		} else {
			GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text += "\n" + submit.downloadHandler.text;
		}
	}

}