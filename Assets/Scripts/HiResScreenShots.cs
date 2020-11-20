﻿using UnityEngine;
using System.Collections;

public class HiResScreenShots : MonoBehaviour
{
    public int resWidth = 1920;
    public int resHeight = 1080;
    [SerializeField] private HandManager handManager = null;
    private Camera theCam;

    private void Awake()
    {
        theCam = gameObject.GetComponent<Camera>();
    }
    private bool takeHiResShot = false;

    public static string ScreenShotName(int width, int height)
    {
        return string.Format("U:/Pictures/Unity/screen_{1}x{2}_{3}.png",
                             Application.dataPath,
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    void LateUpdate()
    {
        takeHiResShot |= (Input.GetKeyDown("k") || handManager.Left.buttonSecondary.isPressed());
        if (takeHiResShot)
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            theCam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            theCam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            theCam.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = false;
        }
    }
}