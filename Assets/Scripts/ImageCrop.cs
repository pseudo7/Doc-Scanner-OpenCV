﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity;

public class ImageCrop : MonoBehaviour
{
    public RawImage targetImage;
    public RawImage warpedImage;
    public Material lineMat;
    public GameObject controlPanel;

    [Header("Point Transforms")]
    public Transform leftTop;
    public Transform rightTop;
    public Transform leftBottom;
    public Transform rightBottom;
    public Slider valSlider;
    public Slider brightnessSlider;
    public Slider contrastSlider;


    static Camera mainCam;


    CropHandle[] cropHandles;
    //LineRenderer line;

    void Awake()
    {
        cropHandles = FindObjectsOfType<CropHandle>();
        //line = gameObject.AddComponent<LineRenderer>();
        //line.endWidth = line.startWidth = .1f;
        //line.numCornerVertices = line.numCapVertices = 90;
        //line.positionCount = 0;
        //lineMat.color = Color.cyan;
        //line.loop = true;
        //line.material = lineMat;
        if (!mainCam) mainCam = Camera.main;
    }

    private void Start()
    {
        CropTexture();
    }

    public void CropTexture()
    {
        StartCoroutine(Warping());
    }

    IEnumerator Warping()
    {
        float minValX, minValY, maxValX, maxValY;
        minValX = minValY = float.MaxValue;
        maxValX = maxValY = float.MinValue;

        foreach (CropHandle handle in cropHandles)
        {
            Vector3 handlePos = handle.transform.position;

            if (handlePos.x < minValX) minValX = handlePos.x;
            if (handlePos.y < minValY) minValY = handlePos.y;

            if (handlePos.x > maxValX) maxValX = handlePos.x;
            if (handlePos.y > maxValY) maxValY = handlePos.y;
        }

        //float width = maxValX - minValX;
        //float height = maxValY - minValY;

        //UnityEngine.Rect cropRect = new UnityEngine.Rect(minValX, minValY, maxValX, maxValY);

        //Texture2D croppedTexture = new Texture2D((int)(maxValX - minValX), (int)(maxValY - minValY), TextureFormat.RGB24, false);

        //StartCoroutine(Capture(croppedTexture, cropRect));

        //UnityEngine.Rect cropRect = new UnityEngine.Rect(0, 0, Screen.width, Screen.height);

        //Texture2D croppedTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        //yield return StartCoroutine(Capture(croppedTexture, cropRect));

        Texture2D croppingTexture = DDOL_Navigation.SavedTexture;

        targetImage.texture = croppingTexture;

        Mat mainMat = new Mat(Screen.height, Screen.width, CvType.CV_8UC3);

        Utils.texture2DToMat(croppingTexture, mainMat);

        List<Point> srcPoints = new List<Point>
        {
            new Point(leftBottom.position.x, Screen.height - leftBottom.position.y),
            new Point(rightBottom.position.x, Screen.height - rightBottom.position.y),
            new Point(rightTop.position.x, Screen.height - rightTop.position.y),
            new Point(leftTop.position.x, Screen.height - leftTop.position.y),
        };

        Mat srcPointsMat = Converters.vector_Point_to_Mat(srcPoints, CvType.CV_32F);

        List<Point> dstPoints = new List<Point>
        {
            new Point(0, 0),
            new Point(Screen.width, 0),
            new Point(Screen.width, Screen.height),
            new Point(0, Screen.height),
        };

        Mat dstPointsMat = Converters.vector_Point_to_Mat(dstPoints, CvType.CV_32F);

        Mat M = Imgproc.getPerspectiveTransform(srcPointsMat, dstPointsMat);

        Mat warpedMat = new Mat(mainMat.size(), CvType.CV_8UC3);

        yield return new WaitForEndOfFrame();

        Imgproc.warpPerspective(mainMat, warpedMat, M, new Size(Screen.width, Screen.height));

        Imgproc.cvtColor(warpedMat, warpedMat, Imgproc.COLOR_BGR2GRAY);

        //Imgproc.GaussianBlur(warpedMat, warpedMat, new Size(-50, -50), 0);

        //warpedMat *= contrastSlider.value;
        //warpedMat += Scalar.all((int)brightnessSlider.value);

        //Imgproc.threshold(warpedMat, warpedMat, (int)valSlider.value, 255, Imgproc.THRESH_BINARY);

        Imgproc.adaptiveThreshold(warpedMat, warpedMat, 255, Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C, Imgproc.THRESH_BINARY, 11, 2);

        //warpedMat.convertTo(warpedMat, CvType.CV_8UC3);

        Texture2D finalTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        Utils.matToTexture2D(warpedMat, finalTexture);

        yield return new WaitForEndOfFrame();

        warpedImage.texture = finalTexture;
        warpedImage.SetNativeSize();
        warpedImage.material.mainTexture = finalTexture;

        warpedImage.gameObject.SetActive(true);

        Debug.LogFormat("Min X: {0}, Min Y: {1}\nMax X: {2}, Max Y: {3}\nWidth: {4}, Height: {5}", minValX, minValY, maxValX, maxValY, maxValX - minValX, maxValY - minValY);
    }

    //public void DrawCropSection()
    //{
    //    line.positionCount = 0;

    //    List<Vector3> points = new List<Vector3>();
    //    if (cropHandles.Length > 1)
    //    {
    //        foreach (CropHandle handle in cropHandles)
    //        {
    //            points.Add(mainCam.ScreenToWorldPoint(handle.transform.position) - mainCam.transform.position);
    //        }
    //    }
    //    line.positionCount = points.Count;
    //    line.SetPositions(points.ToArray());
    //}

    IEnumerator Capture(Texture2D capturedTexture, UnityEngine.Rect rect)
    {
        controlPanel.SetActive(false);
        yield return new WaitForEndOfFrame();
        capturedTexture.ReadPixels(rect, 0, 0);
        capturedTexture.Apply();
        controlPanel.SetActive(true);
    }

}
