using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class ReserveController : MonoBehaviour {
	public GameObject PopUpPanel;
	public GameObject[] toggles;
	public string RoomName;
	public string[] Reserves;
	public string Time;
	// Use this for initialization
	void Start () {
		RoomName = "A208";
		PopUpPanel = GameObject.Find("Canvas/PopUpPanel");
		toggles = GameObject.FindGameObjectsWithTag ("Reserve");
		StartCoroutine ("PostForm");
	}

	public IEnumerator PostForm(){
		Debug.Log ("Post Form is started");
		WWWForm Form = new WWWForm();
		Form.AddField ("RoomName", RoomName);
		UnityWebRequest submit = UnityWebRequest.Post("http://localhost/arroomreservation/ReservationForRoom.php", Form);
		yield return submit.SendWebRequest ();
		if (submit.isNetworkError || submit.isHttpError) {
			Debug.Log ("Error: " + submit.downloadHandler.text);
		} else {
			//Debug.Log ("Success: " + submit.downloadHandler.text);
			string[] ReserveStatus = submit.downloadHandler.text.Split(',');
			for (int i = 0; i < ReserveStatus.Length-1; i++){
				//Debug.Log (ReserveStatus [i].Substring(0,2));
				Time = ReserveStatus [i].Substring(0,2);
				//Debug.Log (Time);
				string[] mStatus = ReserveStatus [i].Split (' ');
				//Status[i] = mStatus [1];
				Debug.Log (mStatus [1]);
				foreach (GameObject Reservetoggle in toggles) {
					Toggle m_Toggle = Reservetoggle.GetComponent<Toggle> ();
					string ReservationTime = m_Toggle.name.Substring (1, 2);
					if (ReservationTime.Equals (Time) && mStatus [1].Equals ("true")) {
						m_Toggle.isOn = true; 
					}else if(ReservationTime.Equals (Time) && mStatus [1].Equals ("false")) {
						m_Toggle.isOn = false; 
					}
				}
			}
		}
	}
		
}
