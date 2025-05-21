using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class FaceLandmarkTracker : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public int leftCheekIndex = 234;
    public int rightCheekIndex = 454;
    public int leftMouthIndex = 61;
    public int rightMouthIndex = 291;

    private ARFace arFace;
    private float leftBaseDist = -1f;
    private float rightBaseDist = -1f;
    private bool cheekPuffDetected = false;
    private bool baseSetAnnounced = false;

    void Awake()
    {
        arFace = GetComponent<ARFace>();
        if (instructionText == null)
            instructionText = FindObjectOfType<TextMeshProUGUI>();
    }

    void Update()
    {
        if (arFace == null || arFace.vertices == null ||
            arFace.vertices.Length <= Mathf.Max(leftCheekIndex, rightCheekIndex, leftMouthIndex, rightMouthIndex))
            return;

        Vector3 leftCheekPos = arFace.transform.TransformPoint(arFace.vertices[leftCheekIndex]);
        Vector3 leftMouthPos = arFace.transform.TransformPoint(arFace.vertices[leftMouthIndex]);
        float leftDist = Vector3.Distance(leftCheekPos, leftMouthPos);

        Vector3 rightCheekPos = arFace.transform.TransformPoint(arFace.vertices[rightCheekIndex]);
        Vector3 rightMouthPos = arFace.transform.TransformPoint(arFace.vertices[rightMouthIndex]);
        float rightDist = Vector3.Distance(rightCheekPos, rightMouthPos);

        // 기준점 안내
        if (leftBaseDist < 0f || rightBaseDist < 0f)
        {
            if (instructionText != null)
                instructionText.text = "입을 다문 평소 얼굴을 유지해 주세요. 기준점을 측정합니다.";
            leftBaseDist = leftDist;
            rightBaseDist = rightDist;
            Debug.Log($"Base cheek distances set: left={leftBaseDist}, right={rightBaseDist}");
            return;
        }
        else if (!baseSetAnnounced)
        {
            if (instructionText != null)
                instructionText.text = "기준점 측정 완료! 이제 볼을 부풀려 보세요.";
            baseSetAnnounced = true;
        }

        // 볼 부풀림 감지 (기준값의 1.2배 이상이면 감지)
        bool leftPuffed = leftDist > leftBaseDist * 1.2f;
        bool rightPuffed = rightDist > rightBaseDist * 1.2f;

        if (leftPuffed && rightPuffed && !cheekPuffDetected)
        {
            cheekPuffDetected = true;
            if (instructionText != null)
                instructionText.text = "볼을 부풀렸습니다!";
            Debug.Log("Cheek puff detected!");
        }
        else if (!(leftPuffed && rightPuffed) && cheekPuffDetected)
        {
            cheekPuffDetected = false;
            if (instructionText != null)
                instructionText.text = "볼을 다시 평소로 돌렸습니다.";
            Debug.Log("Cheek puff released.");
        }
    }
}
