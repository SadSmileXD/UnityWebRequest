# UnityWebRequest

## 구조: 3대 핵심 요소를 이해하자
UnityWebRequest는 내부적으로 역할이 나뉘어 있습니다.
- UnityWebRequest (메인 관리자): 요청 URL, HTTP 메서드(GET, POST 등), 헤더, 타임아웃을 관리합니다.
- UploadHandler **(보내는 데이터)**: 서버로 전송할 데이터를 바이트 배열이나 폼 데이터로 변환하여 실어 보냅니다. (UploadHandlerRaw, UploadHandlerRaw 등)
- DownloadHandler **(받는 데이터)**: 서버로부터 돌아온 응답 데이터를 처리합니다.
    - DownloadHandlerBuffer: 일반 텍스트, JSON, 바이트 데이터를 메모리에 저장

    - DownloadHandlerTexture: 이미지(PNG/JPG)를 Unity Texture2D로 바로 변환

    - DownloadHandlerAudioClip: 오디오 파일(.mp3, .wav 등)을 AudioClip으로 변환

    - DownloadHandlerFile: 대용량 파일을 메모리를 거치지 않고 바로 디스크에 저장

# 2) 비동기 처리 방식
네트워크 요청은 시간이 걸리므로 메인 스레드를 멈추지 않기 위해 비동기로 처리합니다.

- 코루틴(Coroutine): yield return request.SendWebRequest(); 방식 (기본적이면서 널리 쓰임)

- async / await (UniTask 권장): 최신 Unity 개발 트렌드로, 코루틴보다 코드 흐름을 깔끔하게 작성 가능

# 3)주요 HTTP 메서드
GET: 서버에서 데이터(JSON, 파일 등)를 조회/다운로드할 때

POST: 서버에 새로운 데이터를 전송/등록할 때 (JSON payload, Form data 등)

PUT / DELETE: 기존 데이터 수정 / 삭제할 때

----
# 1.GET 요청 (JSON/텍스트 데이터 가져오기)
가장 기본이 되는 데이터 조회 코드입니다.
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestGetExample : MonoBehaviour
{
    private const string TargetUrl = "https://jsonplaceholder.typicode.com/todos/1";

    private void Start()
    {
        StartCoroutine(GetRequest(TargetUrl));
    }

    private IEnumerator GetRequest(string url)
    {
        // UnityWebRequest 객체 생성
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 서버에 요청을 보내고 응답이 올 때까지 대기
            yield return request.SendWebRequest();

            // 에러 체크 (Unity 2020.1 이상 권장 방식)
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[GET Error] {request.error}");
            }
            else
            {
                // 성공적인 응답 데이터 출력
                string jsonResult = request.downloadHandler.text;
                Debug.Log($"[GET Success]\n{jsonResult}");
            }
        } // using 문을 나가면 UnityWebRequest 및 메모리가 자동 해제(Dispose)됩니다.
    }
}
```
# 2.POST 요청 (JSON 데이터 전송하기)
서버에 JSON 형태의 데이터를 전송할 때는 UploadHandlerRaw와  
 Content-Type 헤더를 올바르게 설정하는 것이 핵심입니다.
```csharp
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class UserData
{
    public string name;
    public int score;
}

public class WebRequestPostExample : MonoBehaviour
{
    private const string TargetUrl = "https://jsonplaceholder.typicode.com/posts";

    private void Start()
    {
        UserData myData = new UserData { name = "Player1", score = 1500 };
        StartCoroutine(PostJsonRequest(TargetUrl, myData));
    }

    private IEnumerator PostJsonRequest(string url, UserData data)
    {
        // 1. 객체를 JSON 문자열로 변환
        string jsonString = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        // 2. UnityWebRequest 객체 생성 및 Upload/Download Handler 수동 세팅
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // JSON 요청 시 필수로 헤더 설정!
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[POST Error] {request.error}");
            }
            else
            {
                Debug.Log($"[POST Success] Response: {request.downloadHandler.text}");
            }
        }
    }
}
```
# 3. 이미지(텍스처) 다운로드
웹상의 이미지 URL을 통해 Unity의 RawImage나 UI에 적용할 Texture2D로 가져오는 패턴입니다.

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WebRequestTextureExample : MonoBehaviour
{
    [SerializeField] private RawImage targetRawImage;
    private const string ImageUrl = "https://via.placeholder.com/150";

    private void Start()
    {
        StartCoroutine(DownloadImage(ImageUrl));
    }

    private IEnumerator DownloadImage(string url)
    {
        // UnityWebRequestTexture.GetTexture 메서드 사용
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Texture Error] {request.error}");
            }
            else
            {
                // DownloadHandlerTexture에서 바로 Texture2D 추출
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                targetRawImage.texture = texture;
                Debug.Log("[Texture Download Success]");
            }
        }
    }
}
```
# 학습 시 반드시 알아야 할 주의사항 & 팁  
- using 구문 필수 사용:

    - UnityWebRequest는 C++ 네이티브 메모리를 사용합니다. using 구문 없이 사용하거나 메모리 해제(request.Dispose())를 하지 않으면 메모리 누수(Memory Leak)가 발생합니다.

- 에러 판단 방식 (result 속성):

    - 과거에는 request.isNetworkError 등을 썼지만, Unity 2020.1 이후부터는 request.result enum을 사용해야 합니다.

    - Result.ConnectionError: 인터넷 연결 없음, DNS 실패 등

    - Result.ProtocolError: 서버 응답 에러 (404 Not Found, 500 Internal Server Error 등)

     - Result.DataProcessingError: 응답 데이터 파싱 실패

- HTTP 인증 / 헤더 추가:

    - JWT 토큰이나 API Key 전달 시 request.SetRequestHeader("Authorization", "Bearer " + token); 처럼 SendWebRequest() 호출 이전에 작성해야 합니다.
- WebGL 주의사항:

    - WebGL 빌드 시 브라우저 보안 규정인 CORS (Cross-Origin Resource Sharing) 이슈가 자주 발생합니다. 서버 측에서 CORS 헤더(Access-Control-Allow-Origin)를 허용해주어야 통신이 가능합니다.