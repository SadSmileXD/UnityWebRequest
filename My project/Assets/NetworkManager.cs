using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public string url;
    private void Awake()
    {
        StartCoroutine(GetDataRoutine(url));
    }

    private IEnumerator GetDataRoutine(string url)
    {
        // using을 사용하여 통신 완료 후 객체가 자동으로 Dispose 되도록 설정
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 서버에 요청을 보내고 응답이 올 때까지 대기
            yield return request.SendWebRequest();

            // 통신 결과 확인
            if (request.result == UnityWebRequest.Result.Success)
            {
                // 성공 시 텍스트 결과 출력
                Debug.Log($"[성공] 데이터: {request.downloadHandler.text}");
            }
            else
            {
                // 실패 시 에러 내용 출력
                Debug.LogError($"[에러] {request.error}");
            }
        }
    }
}