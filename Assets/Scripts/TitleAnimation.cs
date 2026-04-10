using UnityEngine;
using TMPro;

public class TitleAnimation : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobSpeed = 2f;
    public float bobAmount = 15f;

    [Header("Arc Settings")]
    public float arcHeight = 40f; // how high the centre dips below

    private TMP_Text textMesh;
    private Vector3 startPos;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Whole object bobs up and down
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.localPosition = startPos + new Vector3(0, bob, 0);

        // Arc: each character dips down toward centre
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;
        int charCount = textInfo.characterCount;

        for (int i = 0; i < charCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            Vector3[] verts = textInfo.meshInfo[meshIndex].vertices;

            // t goes from 0 to 1 across the text
            float t = charCount > 1 ? (float)i / (charCount - 1) : 0.5f;

            // Parabola: peaks at edges (0 and 1), dips at centre (0.5)
            float arc = arcHeight * (1 - (2 * t - 1) * (2 * t - 1)) - 50;

            for (int j = 0; j < 4; j++)
                verts[vertexIndex + j] += new Vector3(0, arc, 0);
        }

        // Apply changes
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Mesh mesh = textInfo.meshInfo[i].mesh;
            mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(mesh, i);
        }
    }
}