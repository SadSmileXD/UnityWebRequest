using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UnityWebRequestAllInOneTest : MonoBehaviour
{

    public Image image;
    public AudioSource audioSource;
    private void Start()
    {
        // 원하시는 테스트 메서드의 주석을 해제하고 실행해 보세요.
        //StartCoroutine(TestGET());
        //StartCoroutine(TestPOST_Form());
        // StartCoroutine(TestPOST_JSON());
        // StartCoroutine(TestPUT());
        // StartCoroutine(TestDELETE());
        // StartCoroutine(TestHEAD());
        // StartCoroutine(TestTextureDownload());
         //StartCoroutine(TestAudioDownload());
         StartCoroutine(TestDownloadWithProgress());
    }

    // 1. GET - 데이터 조회
    private IEnumerator TestGET()
    {
        string url = "https://jsonplaceholder.typicode.com/todos/1";
        Debug.Log($"[GET 시작] {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[GET 성공]\n{request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"[GET 에러] {request.error}");
            }
        }
    }

    // 2-1. POST - 폼 데이터(Form Data) 전달
    private IEnumerator TestPOST_Form()
    {
        string url = "https://httpbin.org/post";
        Debug.Log($"[POST Form 시작] {url}");

        WWWForm form = new WWWForm();
        form.AddField("username", "player1");
        form.AddField("score", "100");

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[POST Form 성공]\n{request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"[POST Form 에러] {request.error}");
            }
        }
    }

    // 2-2. POST - JSON 데이터 전달 (API에서 가장 흔히 사용하는 형태)
    private IEnumerator TestPOST_JSON()
    {
        string url = "https://httpbin.org/post";
        Debug.Log($"[POST JSON 시작] {url}");

        string jsonBody = "{\"username\":\"player1\", \"score\":100}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[POST JSON 성공]\n{request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"[POST JSON 에러] {request.error}");
            }
        }
    }

    // 3. PUT - 데이터 수정/전체 교체
    private IEnumerator TestPUT()
    {
        string url = "https://httpbin.org/put";
        Debug.Log($"[PUT 시작] {url}");

        byte[] myData = Encoding.UTF8.GetBytes("수정할 데이터 바이트 배열");

        using (UnityWebRequest request = UnityWebRequest.Put(url, myData))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[PUT 성공]\n{request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"[PUT 에러] {request.error}");
            }
        }
    }

    // 4. DELETE - 데이터 삭제
    private IEnumerator TestDELETE()
    {
        string url = "https://jsonplaceholder.typicode.com/posts/1";
        Debug.Log($"[DELETE 시작] {url}");

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            // DELETE는 기본 DownloadHandler가 연결되어 있지 않으므로 응답을 받아보려면 추가
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[DELETE 성공] 응답 코드: {request.responseCode}");
            }
            else
            {
                Debug.LogError($"[DELETE 에러] {request.error}");
            }
        }
    }

    // 5. HEAD - 메타데이터만 확인 (파일 크기, 수정일 등 본문 제외 수신)
    private IEnumerator TestHEAD()
    {
        string url = "https://httpbin.org/get";
        Debug.Log($"[HEAD 시작] {url}");

        using (UnityWebRequest request = UnityWebRequest.Head(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string contentLength = request.GetResponseHeader("Content-Length");
                string contentType = request.GetResponseHeader("Content-Type");
                Debug.Log($"[HEAD 성공] 파일 크기: {contentLength} bytes, 타입: {contentType}");
            }
            else
            {
                Debug.LogError($"[HEAD 에러] {request.error}");
            }
        }
    }

    // 6. Texture 다운로드(이미지)
    private IEnumerator TestTextureDownload()
    {
        string imageUrl = "https://i.pinimg.com/236x/20/06/79/200679226c726af2d8af546b03b2e6d5.jpg";
        Debug.Log($"[이미지 다운로드 시작] {imageUrl}");

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Debug.Log($"[이미지 다운로드 성공] 텍스처 크기: {texture.width}x{texture.height}");
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                Debug.LogError($"[이미지 다운로드 에러] {request.error}");
            }
        }
    }

    // 7. Audio 다운로드 (음원 파일)
    private IEnumerator TestAudioDownload()
    {
        // mp3 테스트용 무료 음원 샘플 링크
        string audioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3";
        Debug.Log($"[오디오 다운로드 시작] {audioUrl}");

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                Debug.Log($"[오디오 다운로드 성공] 재생 시간: {clip.length}초");
                audioSource.clip = clip;
            }
            else
            {
                Debug.LogError($"[오디오 다운로드 에러] {request.error}");
            }
        }
    }

    // 8. 진행률(Progress) 확인하며 다운로드
    private IEnumerator TestDownloadWithProgress()
    {
        string url = "https://httpbin.org/bytes/1048576"; // 1MB 크기 테스트 파일
        Debug.Log($"[진행률 다운로드 시작] {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 비동기 작업 객체 획득
            var operation = request.SendWebRequest();

            // 완료될 때까지 매 프레임 진행률 출력
            while (!operation.isDone)
            {
                Debug.Log($"다운로드 진행률: {request.downloadProgress * 100:F1}%");
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[다운로드 완료] 받은 바이트 크기: {request.downloadHandler.data.Length} bytes");
            }
            else
            {
                Debug.LogError($"[다운로드 에러] {request.error}");
            }
        }
    }
}