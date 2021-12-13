using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    private string filename;

    void Awake()
    {
        if (Instance = null)
        {
            Instance = this;
        }
        else
        {
            Instance = null;
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ImageHandle()
    {
        Firebase.Storage.StorageReference storageReference =
        Firebase.Storage.FirebaseStorage.DefaultInstance.GetReferenceFromUrl("storage_url");

        storageReference.Child("resource_name").GetBytesAsync(1024 * 1024).
            ContinueWith((System.Threading.Tasks.Task<byte[]> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
                }
                else
                {
                    byte[] fileContents = task.Result;
                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(fileContents);
            //if you need sprite for SpriteRenderer or Image
            Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width,
                    texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    Debug.Log("Finished downloading!");
                }
            });
    }

    private IEnumerator DownloadImage(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Instead of accessing the texture
                //var texture = DownloadHandlerTexture.GetContent(uwr);

                // you can directly get the bytes
                //byte[] bytes = DownloadHandlerTexture.GetData(uwr);

                // and write them to File e.g. using
                //using (FileStream fileStream = File.Open(filename, FileMode.OpenOrCreate))
                //{
                //    fileStream.WriteAsync(bytes, 0, bytes.Length);
                //}
            }
        }
    }
}
