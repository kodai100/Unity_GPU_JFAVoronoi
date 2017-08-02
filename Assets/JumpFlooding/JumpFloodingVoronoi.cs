using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JumpFloodingVoronoi : MonoBehaviour {

    public int nx = 1920;
    public int ny = 1080;

    [Range(1, 20)] public int stage = 1;
    
    public RenderTexture cameraTex;

    struct Data {
        public Vector2 pos;
        public Vector3 color;
        public float dist;
    };
    
    RenderTexture render;

    public ComputeShader VoronoiCS;
    ComputeBuffer voronoiData;
    ComputeBuffer pingPong;

    public Material resultMat;

	void Start () {
        CreateAndInitRenderTexture(ref render);
        

        voronoiData = new ComputeBuffer(nx * ny, Marshal.SizeOf(typeof(Data)));
        pingPong = new ComputeBuffer(nx * ny, Marshal.SizeOf(typeof(Data)));

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


        VoronoiCS.SetInt("_NX", nx);
        VoronoiCS.SetInt("_NY", ny);    // TODO compute

        int kernel = VoronoiCS.FindKernel("Init");
        VoronoiCS.SetTexture(kernel, "_InputPoints", cameraTex);
        VoronoiCS.SetBuffer(kernel, "_DataWrite", voronoiData);
        VoronoiCS.Dispatch(kernel, nx / 32, ny / 32, 1);

        // カラー情報を保持したバッファ
        // 位置を保持したバッファ
        for (int i = 1; i < stage; i++) {
            
            int stepWidth = (int)(Mathf.Max(nx, ny) / Mathf.Pow(2, i));
            if (stepWidth == 0) break;

            kernel = VoronoiCS.FindKernel("JFA");
            VoronoiCS.SetInt("_StepWidth", stepWidth);
            VoronoiCS.SetBuffer(kernel, "_DataRead", voronoiData);
            VoronoiCS.SetBuffer(kernel, "_DataWrite", pingPong);
            VoronoiCS.Dispatch(kernel, nx / 32, ny / 32, 1);
            SwapBuffer(ref voronoiData, ref pingPong);
        }

        kernel = VoronoiCS.FindKernel("RenderToTexture");
        VoronoiCS.SetTexture(kernel, "_Result", render);
        VoronoiCS.SetBuffer(kernel, "_DataRead", voronoiData);
        VoronoiCS.Dispatch(kernel, nx / 32, ny / 32, 1);

        resultMat.SetTexture("_MainTex", render);
    }

    private void OnDestroy() {
        voronoiData.Release();
        render.Release();
        pingPong.Release();
    }

    private void OnGUI() {
    }

    void SwapBuffer<T>(ref T ping, ref T pong) where T : class {
        T temp = ping;
        ping = pong;
        pong = temp;
    }

    
}
