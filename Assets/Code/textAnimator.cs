using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class textAnimator : MonoBehaviour
{
    [Header("Text Animator Properties")]
    public TextMeshProUGUI textMesh;
    public bool random = true;
    private Mesh mesh;
    private Vector3[] vertices;

    public Vector2 amplitude = Vector2.one * 4;
    public Vector2 intensity = Vector2.one * 3;

    public float textOffset = 1;
    private float offset;
    private void Awake()
    {
        if(!textMesh)
            textMesh = GetComponent<TextMeshProUGUI>();
        if (random)
            offset = Random.Range(-5000, 5000);
    }

    private void Update()
    {
        Animate();
    }
    private void OnDrawGizmosSelected()
    {
        Animate();
    }
    public void Animate()
    {
        if (!textMesh)
            return;

        textMesh.ForceMeshUpdate();
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < textMesh.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo c = textMesh.textInfo.characterInfo[i];

            int index = c.vertexIndex;

            Vector3 offset = wobble(Time.time + i * textOffset);
            vertices[index] += offset;
            vertices[index + 1] += offset;
            vertices[index + 2] += offset;
            vertices[index + 3] += offset;

        }
        mesh.vertices = vertices;
        textMesh.canvasRenderer.SetMesh(mesh);
    }
    Vector2 wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * amplitude.x + offset) * intensity.x, Mathf.Cos(time * amplitude.y + offset) * intensity.y);
    }
}
