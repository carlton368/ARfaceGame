using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

public class FaceLandmarkTracker : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public Text uiText;

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
        if (tmpText == null)
            tmpText = FindObjectOfType<TextMeshProUGUI>();
        if (uiText == null)
            uiText = FindObjectOfType<Text>();
    }

    void Update()
    {
        if (arFace == null)
            return;

        NativeArray<Vector3> verts = arFace.vertices;
        if (!verts.IsCreated)
            return;

        int maxIndex = Mathf.Max(leftCheekIndex, rightCheekIndex, leftMouthIndex, rightMouthIndex);
        if (verts.Length <= maxIndex)
            return;

        Vector3 leftCheekPos = arFace.transform.TransformPoint(verts[leftCheekIndex]);
        Vector3 leftMouthPos = arFace.transform.TransformPoint(verts[leftMouthIndex]);
        float leftDist = Vector3.Distance(leftCheekPos, leftMouthPos);

        Vector3 rightCheekPos = arFace.transform.TransformPoint(verts[rightCheekIndex]);
        Vector3 rightMouthPos = arFace.transform.TransformPoint(verts[rightMouthIndex]);
        float rightDist = Vector3.Distance(rightCheekPos, rightMouthPos);

        // Baseline message
        if (leftBaseDist < 0f || rightBaseDist < 0f)
        {
            SetInstruction("Hold a neutral face to set baseline...");
            leftBaseDist = leftDist;
            rightBaseDist = rightDist;
            return;
        }
        else if (!baseSetAnnounced)
        {
            SetInstruction("Baseline set! Now puff your cheeks.");
            baseSetAnnounced = true;
        }

        bool leftPuffed = leftDist > leftBaseDist * 1.2f;
        bool rightPuffed = rightDist > rightBaseDist * 1.2f;

        if (leftPuffed && rightPuffed && !cheekPuffDetected)
        {
            cheekPuffDetected = true;
            SetInstruction("Cheeks puffed! Great.");
        }
        else if (!(leftPuffed && rightPuffed) && cheekPuffDetected)
        {
            cheekPuffDetected = false;
            SetInstruction("Cheeks relaxed.\nTry puffing again.");
        }
    }

    private void SetInstruction(string message)
    {
        if (tmpText != null)
            tmpText.text = message;
        if (uiText != null)
            uiText.text = message;
    }
}
