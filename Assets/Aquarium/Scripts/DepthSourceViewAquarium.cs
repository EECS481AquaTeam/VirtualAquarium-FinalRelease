using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System;

public enum DepthViewModeAquarium
{
	SeparateSourceReaders,
	MultiSourceReader,
}

public class DepthSourceViewAquarium : MonoBehaviour
{
	public DepthViewModeAquarium ViewMode = DepthViewModeAquarium.SeparateSourceReaders;
	public GameObject ColorSourceManager;
	public GameObject DepthSourceManager;
	public GameObject MultiSourceManager;
	
	public float threshold = 10.0f; //determine if input is registered (NEEDS TO BE MODIFIED BEFOREHAND)
	public float deltaz = 0.0f; 	//difference in z 
	private Vector3[] initialCalibration;
	private static bool isCalibrated = false;
	private static int calibrationTime = 0;
	public static float maxDepth;
	public static Vector2 maxDepthCoor;

	public static Vector2 topLeftCalibration;
	public static Vector2 bottomRightCalibration;
	public static float minDepthCalibration;
	
	private KinectSensor _Sensor;
	private CoordinateMapper _Mapper;
	private Mesh _Mesh;
	private Vector3[] _Vertices;
	private Vector2[] _UV;
	private int[] _Triangles;
	
	// Only works at 4 right now
	private const int _DownsampleSize = 4;
	private const double _DepthScale = 0.1f;
	private const int _Speed = 50;
	
	private MultiSourceManager _MultiManager;
	private ColorSourceManager _ColorManager;
	private DepthSourceManager _DepthManager;
	
	void Start()
	{
		Utility.kinectClickedOn = true;
		Debug.Log ("start");

		_Sensor = KinectSensor.GetDefault();
		if (_Sensor != null)
		{
			_Mapper = _Sensor.CoordinateMapper;
			var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
			
			// Downsample to lower resolution
			CreateMesh(frameDesc.Width / _DownsampleSize, frameDesc.Height / _DownsampleSize);
			
			if (!_Sensor.IsOpen)
			{
				_Sensor.Open();
			}
		}
	}
	
	void CreateMesh(int width, int height)
	{
		_Mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = _Mesh;
		
		_Vertices = new Vector3[width * height];
		_UV = new Vector2[width * height];
		_Triangles = new int[6 * ((width - 1) * (height - 1))];
		
		int triangleIndex = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int index = (y * width) + x;
				
				_Vertices[index] = new Vector3(x, -y, 0);
				_UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));
				
				// Skip the last row/col
				if (x != (width - 1) && y != (height - 1))
				{
					int topLeft = index;
					int topRight = topLeft + 1;
					int bottomLeft = topLeft + width;
					int bottomRight = bottomLeft + 1;
					
					_Triangles[triangleIndex++] = topLeft;
					_Triangles[triangleIndex++] = topRight;
					_Triangles[triangleIndex++] = bottomLeft;
					_Triangles[triangleIndex++] = bottomLeft;
					_Triangles[triangleIndex++] = topRight;
					_Triangles[triangleIndex++] = bottomRight;
				}
			}
		}
		
		_Mesh.vertices = _Vertices;
		initialCalibration = _Vertices;
		_Mesh.uv = _UV;
		_Mesh.triangles = _Triangles;
		_Mesh.RecalculateNormals();
	}
	
	void OnGUI()
	{
		GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
		GUI.TextField(new Rect(Screen.width - 250 , 10, 250, 20), "DepthMode: " + ViewMode.ToString());
		GUI.EndGroup();
	}
	
	void Update()
	{
		if (_Sensor == null)
		{
			return;
		}
		
		if (Input.GetButtonDown("Fire1"))
		{
			if(ViewMode == DepthViewModeAquarium.MultiSourceReader)
			{
				ViewMode = DepthViewModeAquarium.SeparateSourceReaders;
			}
			else
			{
				ViewMode = DepthViewModeAquarium.MultiSourceReader;
			}
		}
		
		float yVal = Input.GetAxis("Horizontal");
		float xVal = -Input.GetAxis("Vertical");
		
		transform.Rotate(
			(xVal * Time.deltaTime * _Speed), 
			(yVal * Time.deltaTime * _Speed), 
			0, 
			Space.Self);
		
		if (ViewMode == DepthViewModeAquarium.SeparateSourceReaders)
		{
			if (ColorSourceManager == null)
			{
				return;
			}
			
			_ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
			if (_ColorManager == null)
			{
				return;
			}
			
			if (DepthSourceManager == null)
			{
				return;
			}
			
			_DepthManager = DepthSourceManager.GetComponent<DepthSourceManager>();
			if (_DepthManager == null)
			{
				return;
			}
			
			gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture();
			RefreshData(_DepthManager.GetData(),
			            _ColorManager.ColorWidth,
			            _ColorManager.ColorHeight);
		}
		else
		{
			if (MultiSourceManager == null)
			{
				return;
			}
			
			_MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
			if (_MultiManager == null)
			{
				return;
			}
			
			gameObject.GetComponent<Renderer>().material.mainTexture = _MultiManager.GetColorTexture();
			
			RefreshData(_MultiManager.GetDepthData(),
			            _MultiManager.ColorWidth,
			            _MultiManager.ColorHeight);
		}
	}
	
	private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
	{
		
		var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
		
		ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
		_Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
		
		for (int y = 0; y < frameDesc.Height; y += _DownsampleSize)
		{
			for (int x = 0; x < frameDesc.Width; x += _DownsampleSize)
			{
				int indexX = x / _DownsampleSize;
				int indexY = y / _DownsampleSize;
				int smallIndex = (indexY * (frameDesc.Width / _DownsampleSize)) + indexX;
				
				double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);
				
				avg = avg * _DepthScale;
				
				_Vertices[smallIndex].z = (float)avg;
				
				// Update UV mapping with CDRP
				var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
				_UV[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
			}
		}
		
		
		//kevin
		//Debug.Log (_Vertices [100]);
		Vector3 max_vert = new Vector3(999.0f, 999.0f, 999.0f);
		//Vector3 max_vert;
		
		for (int i = 0; i < _Vertices.Length; i++) {
			if (_Vertices[i].z < max_vert.z) {
				max_vert = _Vertices[i];
				//deltaz = initialCalibration[i].z - max_vert.z;
			}
		}
		
		//Debug.Log ("INITIAL" + initialCalibration [12]);
		//Debug.Log (max_vert.z + " < " + threshold);
		
		if ((max_vert.z + 2.5f) < minDepthCalibration) {
			Utility.pushingOnKinectOn = true;
			Utility.locationOfPush = scale(max_vert);
			//Debug.Log ("MAX_VERT.Z PASSES THE THRESHOLD" + Utility.locationOfPush + " < " + minDepthCalibration);

			//Debug.Log("scale: " + Utility.locationOfPush); 
		} else {
			//Debug.Log ("NO CLICK REGISTERED" + max_vert);
			Utility.pushingOnKinectOn = false;
		}
		
		//if (isCalibrated == false) { 
		//Debug.Log (max_vert);

		maxDepth = max_vert.z;
		maxDepthCoor.x = max_vert.x;
		maxDepthCoor.y = max_vert.y;
		
		//Environment.Exit (0);
		
		_Mesh.vertices = _Vertices;
		_Mesh.uv = _UV;
		_Mesh.triangles = _Triangles;
		_Mesh.RecalculateNormals();
		
		/*if (isCalibrated == false && calibrationTime < 26) {
			calibrationTime++;
		}
		if (isCalibrated == false && calibrationTime > 25) {
			Debug.Log ("Initial Calibration");
			initialCalibration = _Vertices;
			isCalibrated = true;*/
		/*for (int i = 0; i < _Vertices.Length; i++) {
				if (initialCalibration[i].z < 300.0f) {
					Debug.Log ("Calibration Complete at: " + i);
					isCalibrated = true;
					initialCalibration = _Vertices;
					Debug.Log (i + " : " + initialCalibration[i]);
					break;
				}
			};*/
		//}
	}
	
	private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
	{
		double sum = 0.0;
		
		for (int y1 = y; y1 < y + 4; y1++)
		{
			for (int x1 = x; x1 < x + 4; x1++)
			{
				int fullIndex = (y1 * width) + x1;
				
				if (depthData[fullIndex] == 0)
					sum += 4500;
				else
					sum += depthData[fullIndex];
				
			}
		}
		
		return sum / 16;
	}
	
	void OnApplicationQuit()
	{
		if (_Mapper != null)
		{
			_Mapper = null;
		}
		
		if (_Sensor != null)
		{
			if (_Sensor.IsOpen)
			{
				_Sensor.Close();
			}
			
			_Sensor = null;
		}
	}
	
	private Vector2 scale(Vector3 max_vert_input)
	{
		//Debug.Log ("scale max_vert_input" + max_vert_input);
		float x = (topLeftCalibration.x - max_vert_input.x) / (topLeftCalibration.x - bottomRightCalibration.x);
		x *= 20;
		float y = (max_vert_input.y - bottomRightCalibration.y) / (topLeftCalibration.y - bottomRightCalibration.y);
		//	bottomRightCalibration.y - max_vert_input.y) / (bottomRightCalibration.y - topLeftCalibration.y);
		y *= 10;

		return new Vector2(x, y); //need to scale
	}
}
