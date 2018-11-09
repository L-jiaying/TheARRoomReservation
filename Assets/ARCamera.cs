using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class ARCamera : MonoBehaviour
{
	private bool mAccessCameraImage = true;

	// The desired camera image pixel format
	// private Image.PIXEL_FORMAT mPixelFormat = Image.PIXEL_FORMAT.RGB565;// or RGBA8888, RGB888, RGB565, YUV
	// Boolean flag telling whether the pixel format has been registered

	#if UNITY_EDITOR
	private Image.PIXEL_FORMAT mPixelFormat = Vuforia.Image.PIXEL_FORMAT.GRAYSCALE;
	#elif UNITY_ANDROID
	private Image.PIXEL_FORMAT mPixelFormat =  Vuforia.Image.PIXEL_FORMAT.RGB888;
	#elif UNITY_IOS
	private Image.PIXEL_FORMAT mPixelFormat =  Vuforia.Image.PIXEL_FORMAT.RGB888;
	#endif

	private bool mFormatRegistered = false;

	public string PosX, PosY, PosZ, DirX, DirY, DirZ, RotX, RotY, RotZ, RotW;
	public float x, z, yAngle;
	public double Dx, Dz, RadianXz, AngleXz;
	public int count = 0;
	public GameObject EmptyObject, ArCamera, StartButton, StopButton, mSphere, ReservationInfo, PopUpPanel;

	private bool IsStart = false;
	private Material FreeSphereMaterial;
	private Material BookedSphereMaterial;

	public RoomInfoList RoomInfoList = new RoomInfoList ();
	public RoomReservationList RoomReservationList = new RoomReservationList ();
	//public float[] Lx, Lz;
	//public string[] Location, Booked;

	void Start()
	{
		if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true)){
			mFormatRegistered = true;
		}else{
			mFormatRegistered = false;
		}
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
		GameObject.Find("Canvas/Panel/Text").GetComponent<Text>().text = "Take a photo for localization";
		//GameObject.Find ("Canvas/StopButton").SetActive (false);
		EmptyObject = GameObject.Find("EmptyObject");
		ArCamera = GameObject.Find("ARCamera");
		StartButton = GameObject.Find ("Canvas/StartButton");
		StopButton = GameObject.Find ("Canvas/StopButton");
		PopUpPanel = GameObject.Find ("Canvas/PopUpPanel");
		StartButton.SetActive (true);
		StopButton.SetActive (false);
		PopUpPanel.transform.localScale = new Vector3 (0, 0, 0);
		FreeSphereMaterial = (Material)Resources.Load ("Free");
		BookedSphereMaterial = (Material)Resources.Load ("Booked");
		StartCoroutine ("RoomInitialize");
		//Button btnLocation = GameObject.Find("Canvas/Button").GetComponent<Button>();
		//btnLocation.onClick.AddListener(OnClick);
	}

	public IEnumerator RoomInitialize(){
		WWW RoomPosition = new WWW ("http://192.168.43.238:80/ARRoomReservation/initialize.php");
		yield return RoomPosition;
		string RoomPositionData = RoomPosition.text;
		RoomInfoList = JsonUtility.FromJson<RoomInfoList> (RoomPositionData);
	}

	private void OnApplicationFocus(bool focusStatus){
		if (focusStatus){
			mFormatRegistered = true;
		}else{
			mFormatRegistered = false;
		}
	}

	public void OnStartClick(){
		StartButton.SetActive (false);
		StopButton.SetActive (true);
		IsStart = true;
		StartCoroutine ("TakePhoto");
	}

	public void OnStopClick(){
		IsStart = false;
		//StopCoroutine ("TakePhoto");
		//StopCoroutine ("ReadPosition");
		//StopCoroutine ("RoomReservationInfo");
		StopAllCoroutines ();
		StartButton.SetActive (true);
		StopButton.SetActive (false);
		//GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text = "Take a photo for localization";
	}

	public IEnumerator TakePhoto(){
		yield return new WaitForEndOfFrame ();
		count++;
		
		// Try register camera image format
		Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);
		string FocalLength = "";
		Vuforia.CameraDevice.Instance.GetField("focal-length", out FocalLength);
		decimal fl = System.Convert.ToDecimal(FocalLength);
		fl = fl * 100;
		int Newfl = System.Convert.ToInt32(fl);
		FocalLength = Newfl.ToString();
		FocalLength = FocalLength + "/100";
		string Model = SystemInfo.deviceModel;

		if (image != null){
			Texture2D photo = new Texture2D(image.Width, image.Height);
			Texture2D flip = new Texture2D(image.Width, image.Height);
			image.CopyToTexture(photo);
			photo.Apply();

			flip = FlipTexture(photo);
			flip.Compress(false);
			byte[] bytes = flip.EncodeToJPG();

			// on Android - /Data/Data/com.companyname.gamename/Files
			System.DateTime now = new System.DateTime();
			now = System.DateTime.Now;
			string filename = string.Format("{0}{1}{2}_{3}{4}{5}.jpg", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
			string path = Application.persistentDataPath + filename;
			System.IO.File.WriteAllBytes(path, bytes);

			AndroidJavaObject ex = new AndroidJavaObject("android.media.ExifInterface", path);
			ex.Call("setAttribute", "FocalLength", FocalLength);
			ex.Call("setAttribute", "Model", Model);
			ex.Call("saveAttributes");

			byte[] imageData = System.IO.File.ReadAllBytes(path);
			WWWForm locationForm = new WWWForm();
			locationForm.AddField("buildingID", "1");
			locationForm.AddBinaryData("image", imageData);
			locationForm.AddField("useVise", "true");
			UnityWebRequest locationWww = UnityWebRequest.Post("http://crowdsensing.cs.hut.fi:5004/location/fine", locationForm);
			System.IO.File.Delete(path);
			yield return locationWww.SendWebRequest();
			if (locationWww.isNetworkError || locationWww.isHttpError){
				GameObject.Find("Canvas/Panel/Text").GetComponent<Text>().text = "Error: " + locationWww.downloadHandler.text;
			}else{
				string LocationText = locationWww.downloadHandler.text;
				string[] LocationInfomation = LocationText.Split(new char[2] { ':', ',' });

				PosX = LocationInfomation[2];
				PosY = LocationInfomation[4];
				PosZ = LocationInfomation[6].Substring(0, LocationInfomation[6].Length - 1);
		
				DirX = LocationInfomation[9];
				DirY = LocationInfomation[11];
				DirZ = LocationInfomation[13].Substring(0, LocationInfomation[13].Length - 1);

				RotX = LocationInfomation[16];
				RotY = LocationInfomation[18];
				RotZ = LocationInfomation[20];
				RotW = LocationInfomation[22].Substring(0, LocationInfomation[22].Length - 1);
				ReadPosition();
			}
		}else{
			Debug.Log("No image!");
		}
	}

	public void ReadPosition(){
		EmptyObject.transform.localEulerAngles = ArCamera.transform.localEulerAngles;
		EmptyObject.transform.position = ArCamera.transform.position;
		x = float.Parse(PosX);
		z = float.Parse(PosZ);
		Dx = double.Parse(DirX);
		Dz = double.Parse(DirZ);

		RadianXz = System.Math.Atan(System.Math.Abs(Dz) / System.Math.Abs(Dx));
		AngleXz = (180 / System.Math.PI) * RadianXz;

		if (Dx > 0 && Dz > 0){
			AngleXz = 360 - AngleXz;
		}else if (Dx > 0 && Dz < 0){
			AngleXz = AngleXz;
		}else if (Dx < 0 && Dz > 0){
			AngleXz = 180 + AngleXz;
		}else if (Dx < 0 && Dz < 0){
			AngleXz = 180 - AngleXz;
		}else{
			AngleXz = 0;
		}

		yAngle = System.Convert.ToSingle(AngleXz);
		//GameObject.Find("Canvas/Panel/Text").GetComponent<Text>().text += "\nyAngle: " + yAngle;
		EmptyObject.transform.Rotate(0f, yAngle, 0f);
		//ArCamera.transform.Rotate(0f, yAngle, 0f);
		//GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text += "\n EmptyObject Rotation: " + EmptyObject.transform.localEulerAngles;
		
		StartCoroutine ("RoomReservationInfo");
	}

	public IEnumerator RoomReservationInfo(){
		GameObject.Find("Canvas/Panel/Text").GetComponent<Text>().text = "Shows targets";
		GameObject.Find("Canvas/Panel/Text").GetComponent<Text>().text += "\n Your Position: (" + PosX + ", " + PosY + ", " + PosZ + ")";
		GameObject.Find("Canvas/Panel/Text").GetComponent<Text>().text += "\n Direction: (" + DirX + ", " + DirZ + ")";
		GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text += "\n EmptyObject Rotation: " + EmptyObject.transform.localEulerAngles;

		WWW RoomReservation = new WWW ("http://192.168.43.238:80/ARRoomReservation/initializeReservation.php");
		yield return RoomReservation;
		string RoomReservationData = RoomReservation.text;
		RoomReservationList = JsonUtility.FromJson<RoomReservationList> (RoomReservationData);	

		foreach (RoomInfo Room in RoomInfoList.RoomInfo) {
			float NewX = Room.LocationX - x;
			float NewZ = Room.LocationZ - z;
			if (count == 1) {
				mSphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				mSphere.tag = "Sphere";
				mSphere.name = Room.RoomName;
				mSphere.transform.parent = EmptyObject.transform;
				mSphere.transform.localScale = new Vector3 (3f, 3f, 3f);
				
				ReservationInfo = new GameObject ();
				ReservationInfo.transform.parent = GameObject.Find ("EmptyObject/" + Room.RoomName).transform;
				ReservationInfo.tag = "Text";
				ReservationInfo.name = Room.RoomName + "Text";
				ReservationInfo.transform.localPosition = new Vector3 (0f, 0f, 0f);

				TextMesh text = ReservationInfo.AddComponent<TextMesh> ();
				ReservationInfo.GetComponent<TextMesh> ().anchor = TextAnchor.UpperCenter;
				text.text = Room.RoomName.Replace (" ", "\n");
				text.fontSize = 10;
			} 
			mSphere = GameObject.Find ("EmptyObject/" + Room.RoomName);
			ReservationInfo = GameObject.Find ("EmptyObject/" + Room.RoomName + "/" + Room.RoomName + "Text");
			mSphere.transform.localPosition = new Vector3 (NewZ, 0f, NewX);

			mSphere.transform.rotation = Quaternion.Euler (new Vector3 (0, -yAngle, 0));

			mSphere.GetComponent<Renderer> ().material = FreeSphereMaterial;

			foreach (RoomReservationInfo Reservation in RoomReservationList.RoomReservationInfo) {
				if(Reservation.RoomName.Equals(Room.RoomName) && Reservation.ReservationTime.Substring(0,2).Equals(System.DateTime.Now.Hour.ToString())){
					if (Reservation.ReservationStatus.Equals ("true")) {
						GameObject.Find ("EmptyObject/" + Room.RoomName).GetComponent<Renderer> ().material = BookedSphereMaterial;
					}
				}
			}

			Renderer SphereRend = mSphere.GetComponent<Renderer> ();
			Renderer TextRend = ReservationInfo.GetComponent<Renderer> ();

			double Distance = System.Math.Sqrt (System.Math.Pow (NewX, 2) + System.Math.Pow (NewZ, 2));
			if (Distance > 30) {
				SphereRend.enabled = false;
				TextRend.enabled = false;
				//GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text += "\n No room nearby";
			} else {
				SphereRend.enabled = true;
				TextRend.enabled = true;
				GameObject.Find ("Canvas/Panel/Text").GetComponent<Text> ().text += "\n" + mSphere.name + ": " + mSphere.transform.localPosition.ToString ();
			}
		}

		if (IsStart == true) {
			yield return new WaitForSeconds (2f);
			StartCoroutine ("TakePhoto");
		}
	}

	Texture2D FlipTexture(Texture2D original){
		Texture2D rotated = new Texture2D(original.height, original.width);
		int xR = original.width;
		int yR = original.height;
		for (int m = 0; m < xR; m++){
			for (int n = 0; n < yR; n++){
				rotated.SetPixel(n, xR - 1 - m, original.GetPixel(m, n));
			}
		}
		rotated.Apply();

		Texture2D flipped = new Texture2D(rotated.width, rotated.height);
		int xF = rotated.width;
		int yF = rotated.height;
		for (int i = 0; i < xF; i++){
			for (int j = 0; j < yF; j++){
				flipped.SetPixel(xF - 1 - i, j, rotated.GetPixel(i, j));
			}
		}
		flipped.Apply();
		return flipped;
	}

}