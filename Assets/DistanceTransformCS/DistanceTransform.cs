using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceTransform : MonoBehaviour {

    public ComputeShader DistanceTransformCS;

    public RenderTexture sourceTexture;
    public RenderTexture resultTexture;

    private void Start() {
        resultTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0);
        resultTexture.filterMode = FilterMode.Point;
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();
    }

    // Use this for initialization
    void Update () {

        int kernel = DistanceTransformCS.FindKernel("DistanceTransform");
        DistanceTransformCS.SetTexture(kernel, "Source", sourceTexture);
        DistanceTransformCS.SetTexture(kernel, "Result", resultTexture);
        DistanceTransformCS.SetFloat("_Width", sourceTexture.width);
        DistanceTransformCS.SetFloat("_Height", sourceTexture.height);
        DistanceTransformCS.Dispatch(kernel, sourceTexture.width / 32, sourceTexture.height / 32, 1 / 1);
	}
	
	// Update is called once per frame
	//void Update () {
		
	//}

    private void OnGUI() {
        GUI.DrawTexture(new Rect(0, 0, 256, 256), sourceTexture);
        GUI.DrawTexture(new Rect(256, 0, 256, 256), resultTexture);
    }

    private void OnDestroy() {
        resultTexture.Release();
    }
}
