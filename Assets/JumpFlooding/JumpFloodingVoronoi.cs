using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JumpFloodingVoronoi : MonoBehaviour {

    public int _N = 256;
    public int nx = 1920;
    public int ny = 1080;

    [Range(1, 20)] public int stage = 1;

    public bool colorize;
    public Gradient gradient;

    public bool useCamera;
    public RenderTexture cameraTex;

    public Shader dotPlotterShader;
    Material dotPlotterMat;

    struct Data {
        public Vector2 pos;
        public Vector3 color;
        public float dist;
    };

    RenderTexture inputTexture;
    RenderTexture tmpTexture;
    RenderTexture render;

    public ComputeShader VoronoiCS;
    ComputeBuffer voronoiData;
    ComputeBuffer pingPong;

	void Start () {

        if (!useCamera) {
            CreateAndInitRenderTexture(ref inputTexture);
            CreateAndInitRenderTexture(ref tmpTexture);
        }
        
        CreateAndInitRenderTexture(ref render);
        

        voronoiData = new ComputeBuffer(nx * ny, Marshal.SizeOf(typeof(Data)));
        pingPong = new ComputeBuffer(nx * ny, Marshal.SizeOf(typeof(Data)));

        dotPlotterMat = new Material(dotPlotterShader);

    }

    void CreateAndInitRenderTexture(ref RenderTexture tex) {
        tex = new RenderTexture(nx, ny, 0, RenderTextureFormat.ARGB32);
        tex.filterMode = FilterMode.Point;
        tex.enableRandomWrite = true;
        tex.Create();

        Graphics.SetRenderTarget(tex);
        GL.Clear(false, true, new Color(0, 0, 0, 1));
        Graphics.SetRenderTarget(null);
    }
	

	void Update () {

        if (!useCamera) {
            if (Input.GetMouseButtonUp(0)) {
                dotPlotterMat.SetVector("_Mouse", Input.mousePosition);
                dotPlotterMat.SetColor("_RandColor", gradient.Evaluate(Random.value));
                Graphics.Blit(inputTexture, tmpTexture, dotPlotterMat, 0);
                SwapBuffer(ref inputTexture, ref tmpTexture);
            }
        }
        


        VoronoiCS.SetInt("_N", _N);
        VoronoiCS.SetBool("_Colorize", colorize);


        // inputTextureが結果を保持している
        int kernel = VoronoiCS.FindKernel("Init");
       
        if (useCamera) {
            VoronoiCS.SetTexture(kernel, "_InputPoints", cameraTex);
        } else {
            VoronoiCS.SetTexture(kernel, "_InputPoints", inputTexture);
        }
        VoronoiCS.SetBuffer(kernel, "_DataWrite", voronoiData);
        VoronoiCS.Dispatch(kernel, _N / 32, _N / 32, 1);

        // カラー情報を保持したバッファ
        // 位置を保持したバッファ
        for (int i = 1; i < stage; i++) {
            
            int stepWidth = (int)(_N / Mathf.Pow(2, i));
            if (stepWidth == 0) break;

            kernel = VoronoiCS.FindKernel("JFA");
            VoronoiCS.SetInt("_StepWidth", stepWidth);
            VoronoiCS.SetBuffer(kernel, "_DataRead", voronoiData);
            VoronoiCS.SetBuffer(kernel, "_DataWrite", pingPong);
            VoronoiCS.Dispatch(kernel, _N / 32, _N / 32, 1);
            SwapBuffer(ref voronoiData, ref pingPong);
        }

        kernel = VoronoiCS.FindKernel("RenderToTexture");
        VoronoiCS.SetTexture(kernel, "_Result", render);
        VoronoiCS.SetBuffer(kernel, "_DataRead", voronoiData);
        VoronoiCS.Dispatch(kernel, _N / 32, _N / 32, 1);
    }

    private void OnDestroy() {
        inputTexture.Release();
        tmpTexture.Release();
        voronoiData.Release();
    }

    private void OnGUI() {

        if (useCamera) {
            GUI.DrawTexture(new Rect(0, 0, 256, 256), cameraTex);
        } else {
            GUI.DrawTexture(new Rect(0, 0, 256, 256), inputTexture);
        }

        GUI.DrawTexture(new Rect(256, 0, 256, 256), render);
    }

    void SwapBuffer<T>(ref T ping, ref T pong) where T : class {
        T temp = ping;
        ping = pong;
        pong = temp;
    }

    
}
