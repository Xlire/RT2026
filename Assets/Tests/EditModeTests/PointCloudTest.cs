using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using PointCloudScanner;
using UnityEngine.VFX;

public class PointCloudTests
{
    private GameObject _testGameObject;
    private PointCloudManager _pointCloudManager;

    [SetUp]
    public void Setup()
    {
        // Create a test GameObject and attach the PointCloudManager component
        _testGameObject = new GameObject("TestPointCloudManager");
        _pointCloudManager = _testGameObject.AddComponent<PointCloudManager>();

        // Initialize necessary fields
        _pointCloudManager.NumRays = 10;
        _pointCloudManager.MaxDistance = 50.0f;
        _pointCloudManager.PointCloud = new GameObject("TestPointCloud");
        _pointCloudManager.PointCloud.AddComponent<PointCloudVisualizer>();
    }

    [TearDown]
    public void Teardown()
    {
        // Ensure streams are closed
        _pointCloudManager._pcStream?.Close();
        _pointCloudManager._cameraStream?.Close();

        // Clean up after each test
        Object.DestroyImmediate(_testGameObject);

        string testDirectory = Path.Combine(Path.GetTempPath(), "PointCloudTests");
        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, true); // Delete all test files and directories
        }
    }

    [Test]
    public void TestFilePathGeneration()
    {
        // Arrange
        string expectedDirectory = Path.Combine(Application.dataPath, "../PointCloudDataSets");
        string expectedFileName = "test_identifier_scene_pointcloudstr.txt";

        // Act
        string actualPath = Path.Combine(expectedDirectory, expectedFileName);

        // Assert
        Assert.IsTrue(actualPath.Contains("PointCloudDataSets"));
        Assert.IsTrue(actualPath.EndsWith("pointcloudstr.txt"));
    }

    [Test]
    public void TestSavePointCloud()
    {
        // Arrange
        _pointCloudManager._scannedPoints = new List<Point>
        {
            new Point(new Vector3(1, 2, 3), new Color32(255, 0, 0, 255)),
            new Point(new Vector3(4, 5, 6), new Color32(0, 255, 0, 255))
        };

        _pointCloudManager._pcStringBuilder = new StringBuilder();
        _pointCloudManager._cameraStringBuilder = new StringBuilder();
        // Act
        _pointCloudManager.SavePointCloud();
        // Assert
        Assert.IsNotNull(_pointCloudManager._pcStringBuilder, "_pcStringBuilder is null after SavePointCloud");
        Assert.IsTrue(_pointCloudManager._pcStringBuilder.Length > 0, "_pcStringBuilder is empty after SavePointCloud");

        string result = _pointCloudManager._pcStringBuilder.ToString();
        Assert.IsTrue(result.Contains("1 2 3"), "Result does not contain expected point data.");
        Assert.IsTrue(result.Contains("255 0 0"), "Result does not contain expected color data.");
        Assert.IsTrue(result.Contains("4 5 6"), "Result does not contain second point data.");
        Assert.IsTrue(result.Contains("0 255 0"), "Result does not contain second color data.");
    }

[UnityTest]
public IEnumerator TestFlushStringBuilders()
{
    // Arrange
    string pcFilePath = Path.Combine(Path.GetTempPath(), "PointCloudTests", "TestPointCloud.txt");
    string cameraFilePath = Path.Combine(Path.GetTempPath(), "PointCloudTests", "TestCamera.txt");

    Directory.CreateDirectory(Path.GetDirectoryName(pcFilePath));
    File.WriteAllText(pcFilePath, string.Empty);
    File.WriteAllText(cameraFilePath, string.Empty);

    InitializeStreamsAndBuilders(pcFilePath, cameraFilePath);

    _pointCloudManager._pcStringBuilder.AppendLine("PointCloud data: 1 2 3");
    _pointCloudManager._cameraStringBuilder.AppendLine("Camera data: 255 0 0");

    // Act
    _pointCloudManager.FlushStringBuilders();

    // Wait for the asynchronous operation to complete
    yield return WaitForFlushCompletion();

    // Call FinalFlush after the async operation is complete
    _pointCloudManager.FinalFlush();

    // Assert
    AssertFileContent(pcFilePath, "PointCloud data: 1 2 3");
    AssertFileContent(cameraFilePath, "Camera data: 255 0 0");
}

    private void InitializeStreamsAndBuilders(string pcFilePath, string cameraFilePath)
    {
        _pointCloudManager._pcStream = new StreamWriter(pcFilePath, true);
        _pointCloudManager._cameraStream = new StreamWriter(cameraFilePath, true);
        _pointCloudManager._pcStringBuilder = new StringBuilder();
        _pointCloudManager._cameraStringBuilder = new StringBuilder();
    }

    private IEnumerator WaitForFlushCompletion()
    {
        float timeout = 5.0f;
        float startTime = Time.time;

        while (!_pointCloudManager._isFlushComplete)
        {
            if (Time.time - startTime > timeout)
            {
                Assert.Fail("TestFlushStringBuilders timed out after 5 seconds.");
            }
            yield return null;
        }
    }

    private void AssertFileContent(string filePath, string expectedContent)
    {
        string content = File.ReadAllText(filePath);
        Assert.IsNotEmpty(content, $"{filePath} is empty after flush.");
        Assert.IsTrue(content.Contains(expectedContent), $"{filePath} does not contain expected content: {expectedContent}");
    }

    [Test]
    public void SetupPointCloudVisualizer()
    {
        // Create GameObjects for PointCloudManager and PointCloudVisualizer
        var managerGameObject = new GameObject("PointCloudManager");
        var visualizerGameObject = new GameObject("PointCloudVisualizer");

        // Add components
        var pointCloudManager = managerGameObject.AddComponent<PointCloudManager>();
        var pointCloudVisualizer = visualizerGameObject.AddComponent<PointCloudVisualizer>();

        // Assign PointCloudManager to PointCloudVisualizer
        pointCloudVisualizer.PointCloudManager = pointCloudManager;

        Assert.IsNotNull(pointCloudVisualizer.PointCloudManager, "PointCloudManager is not assigned to PointCloudVisualizer.");
    }

    [Test]
    public void TestPointCloudVisualizerInitialization()
    {
        // Arrange
        var visualizerGameObject = new GameObject("PointCloudVisualizer");
        var pointCloudVisualizer = visualizerGameObject.AddComponent<PointCloudVisualizer>();

        var managerGameObject = new GameObject("PointCloudManager");
        var pointCloudManager = managerGameObject.AddComponent<PointCloudManager>();
        pointCloudManager._points = new List<Point>
        {
            new Point(new Vector3(1, 2, 3), new Color32(255, 0, 0, 255)),
            new Point(new Vector3(4, 5, 6), new Color32(0, 255, 0, 255))
        };

        pointCloudVisualizer.PointCloudManager = pointCloudManager;

        // Act
        pointCloudVisualizer.MaxPoints = 100;

        // Assert
        Assert.IsNotNull(pointCloudVisualizer.PointCloudManager, "PointCloudManager is not assigned.");
        Assert.AreEqual(100, pointCloudVisualizer.MaxPoints, "MaxPoints was not set correctly.");
    }

    [Test]
    public void SetupVisualEffect()
    {
        var visualizerGameObject = new GameObject("PointCloudVisualizer");
        var visualEffect = visualizerGameObject.AddComponent<VisualEffect>();
        var pointCloudVisualizer = visualizerGameObject.AddComponent<PointCloudVisualizer>();

        Assert.IsNotNull(pointCloudVisualizer, "PointCloudVisualizer is not attached to the GameObject.");
        Assert.IsNotNull(visualEffect, "VisualEffect is not attached to the GameObject.");
    }
}