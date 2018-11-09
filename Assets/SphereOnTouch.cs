using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class SphereOnTouch : MonoBehaviour {
	public GameObject PopUpPanel;
	public GameObject EmptyGameObject;
	public GameObject[] toggles;
	public string RoomName;
	public string[] Reserves;
	public string Time;
	public bool PopWindow = false;
	private Rect WindowRect = new Rect ((Screen.width/2)-240, (Screen.height/2)-100, 480, 200);
	// Use this for initialization
	void Start () {
		PopUpPanel = GameObject.Find("Canvas/PopUpPanel");
		EmptyGameObject = GameObject.Find("EmptyObject");
		toggles = GameObject.FindGameObjectsWithTag ("Reserve");
	}

	void OnGUI(){
		if (PopWindow == true) {
			WindowRect=GUI.Window(0,WindowRect,ShowBookWindow,"Info");
		}
	}

	public void ShowBookWindow(int windowID){
		if(GUI.Button (new Rect (180, 80, 160, 40), "Not Reservable")) {
			PopWindow = false;
			EmptyGameObject.transform.localScale = new Vector3 (1, 1, 1);
		}
	}

	// Update is called once per frame
	void Update () {
		foreach(Touch touch in Input.touches){
			if(touch.phase == TouchPhase.Began){
				var ray=Camera.main.ScreenPointToRay(touch.position);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit)){
					GameObject Camera = GameObject.Find("ARCamera");
					ARCamera TakePhoto = (ARCamera) Camera.GetComponent(typeof(ARCamera));
					TakePhoto.OnStopClick();
					//GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text =  hit.collider.name + " is on touched";
					Renderer SphereRend = GameObject.Find(hit.collider.name).GetComponent<Renderer> ();
					//string RoomName = SphereRend.material.name.Replace("(Instance)","");
					GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text =  hit.collider.name;
					RoomName = hit.collider.name;
					EmptyGameObject.transform.localScale = new Vector3 (0, 0, 0);
					StartCoroutine ("PostForm");
				}
			}
		}
	}

	public IEnumerator PostForm(){
		//Debug.Log ("Post Form is started");
		WWWForm Form = new WWWForm();
		Form.AddField ("RoomName", RoomName);
		UnityWebRequest submit = UnityWebRequest.Post("http://192.168.43.238:80/arroomreservation/ReservationForRoom.php", Form);
		yield return submit.SendWebRequest ();
		if (submit.isNetworkError || submit.isHttpError) {
			Debug.Log ("Error: " + submit.downloadHandler.text);
		} else {
			//Debug.Log ("Success: " + submit.downloadHandler.text);
			if(submit.downloadHandler.text.Equals("")){
				PopWindow = true;
			}else{
				PopUpPanel.transform.localScale = new Vector3 (1f, 1f, 1f);
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

}
