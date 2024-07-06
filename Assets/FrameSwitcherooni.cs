using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using uWindowCapture;
using System.IO;
using Valve.VR;
using UnityEditor;
using SimpleFileBrowser;
using System;
public class FrameSwitcherooni : MonoBehaviour

{
    
    [SerializeField]
    private Color lockColor = new Color(1f,1f,1f);
    public Color readColor ;
    private Texture2D texture2D2;
    private Renderer myRenderer;
    [SerializeField]
    private GameObject headsetObject ;
    [SerializeField]
    private GameObject leftContObject ;
    [SerializeField]
    private GameObject rightContObject ;

    private string path;
    // Start is called before the first frame update
    void Start()
    {
        FileBrowser.SetFilters( false, new FileBrowser.Filter( "orient.json", ".json" ));
        myRenderer = GetComponent<Renderer>();
        FileBrowser.SetDefaultFilter( "orient.json" );
        StartCoroutine( ShowLoadDialogCoroutine() );    

    }
    IEnumerator ShowLoadDialogCoroutine()
	{
        yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, false, "..\\", "orient.json", "Find \'orient.json\' in the SMVR Mod Folder", "Select" ); //EditorUtility.OpenFilePanel("Find \'Orient.json\' in the SMVR Mod Folder", "", "json");
        if( FileBrowser.Success ) {
			path = FileBrowser.Result[0];
            print(path);
        }
	}
    // Update is called once per frame
    void Update()
    {
        if (UwcManager.Find("Scrap Mechanic") == null){
            return;
        }
        // Material screen = renderer.material;
        UwcWindow window = UwcManager.Find("Scrap Mechanic");
        // Texture texture = material.GetTexture("_MainTex");
        Texture2D texture2D = window.texture;


        if (texture2D != null){
            //Color frameColor = texture2D.GetPixel(((46)/1920)*texture2D.width,((1080-128)/1080)*texture2D.height);
            
            Color frameColor = ReadPixel(texture2D,(int)(((46.0f)/1920.0f)*texture2D.width),(int)(((130.0f)/1080.0f)*texture2D.height));
            
            
            if (GetColorDistance(lockColor,frameColor) <= 0.1f){    //if (lockColor == frameColor){ //
                // Set texture2D as the active texture
                RenderTexture renderTexture = new RenderTexture(texture2D.width, texture2D.height, 24);
        
                // Set the active render texture
                RenderTexture.active = renderTexture;
                
                // Copy the source texture to the render texture
                Graphics.Blit(texture2D, renderTexture);

                // Create a new Texture2D instancse and copy pixels from the original texture
                texture2D2 = new Texture2D(texture2D.width, texture2D.height, texture2D.format, false);
                texture2D2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                texture2D2.Apply();


                // Destroy the existing texture of the material to avoid memory leaks
                if (myRenderer.material.mainTexture != null)
                {
                    Destroy(myRenderer.material.mainTexture);
                }

                // Explicitly set the new texture to the material
                myRenderer.material.mainTexture = texture2D2;
                RenderTexture.active = null;
                Destroy(renderTexture);
            }

            

        }
        Vector3 headsetRot = headsetObject.transform.localRotation.eulerAngles;
        Vector3 headsetPos = headsetObject.transform.position;
        Vector3 leftContRot = leftContObject.transform.localRotation.eulerAngles;
        Vector3 leftContPos = leftContObject.transform.position;
        Vector3 rightContRot = rightContObject.transform.localRotation.eulerAngles;
        Vector3 rightContPos = rightContObject.transform.position;
        Quaternion headsetRotQuat = headsetObject.transform.rotation;
        Vector3[] orientData = {headsetRot, headsetPos,leftContRot,leftContPos,rightContRot,rightContPos};
        string orientString = "[";
        if (path != null){
            for (int i = 0; i <= 5; i++){
                orientString = orientString + "[" + orientData[i].x.ToString()+","+ orientData[i].y.ToString()+","+ orientData[i].z.ToString() + "]"+ ",";
                
            }
            orientString =  orientString + "[" + headsetRotQuat.x.ToString()+","+ headsetRotQuat.y.ToString()+","+ headsetRotQuat.z.ToString() +","  +headsetRotQuat.w.ToString()+ "]" + "]";
            WriteString(orientString,path);
        }
    }
    // Function to read a pixel from the given texture at specified coordinates

    public static Color ReadPixel(Texture2D texture, int x, int y)
    {
        // Create a new RenderTexture with the same dimensions as the source texture
        RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 24);
        
        // Set the active render texture
        RenderTexture.active = renderTexture;
        
        // Copy the source texture to the render texture
        Graphics.Blit(texture, renderTexture);
        
        // Create a new Texture2D to read the pixel from the render texture
        Texture2D tempTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        tempTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        tempTexture.Apply();
        
        // Read the pixel at the specified coordinates
        Color pixelColor = tempTexture.GetPixel(x, y);
        
        // Clean up
        RenderTexture.active = null;
        Destroy(renderTexture);
        Destroy(tempTexture);
        
        return pixelColor;
    }
        public static float GetColorDistance(Color color1, Color color2)
    {
        // Convert colors to Vector3 for easier distance calculation (ignoring alpha)
        Vector3 vec1 = new Vector3(color1.r, color1.g, color1.b);
        Vector3 vec2 = new Vector3(color2.r, color2.g, color2.b);

        // Calculate the Euclidean distance between the two color vectors
        float distance = Vector3.Distance(vec1, vec2);

        // Normalize the distance to a range of 0 to 1
        float maxDistance = Mathf.Sqrt(3); // sqrt(1^2 + 1^2 + 1^2)
        float normalizedDistance = distance / maxDistance;

        return normalizedDistance;
    }
    static void WriteString(string string2, string path2)
        {

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path2, false);
        writer.WriteLine(string2);
        writer.Close();
    }


}
