using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private void Awake()
    {
        AdjustCameraOrthographicSize();
    }

    private void AdjustCameraOrthographicSize()
    {

        float reference_orth_size = 7f;
        float reference_aspect_ratio = 9f / 16f;
        Camera.main.orthographicSize = reference_orth_size * (reference_aspect_ratio / Camera.main.aspect);
    }

    public void DamageShake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(0.10f, 0.1f));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitute)
    {
        Vector3 starting_position = transform.position;

        float elapsed_time = 0f;
        float x_displacement;
        float y_displacement;
        float z_displacement;

        while (elapsed_time < duration)
        {
            x_displacement = Random.Range(-1, 1) * magnitute;
            y_displacement = Random.Range(-1, 1) * magnitute;
            z_displacement = Random.Range(-1, 1) * magnitute;

            transform.position = new Vector3(starting_position.x + x_displacement,
                                             starting_position.y + y_displacement,
                                             starting_position.z + z_displacement);

            elapsed_time += Time.deltaTime;
            yield return new WaitForEndOfFrame(); ;
        }

        transform.position = starting_position;
    }
}
