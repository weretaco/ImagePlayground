using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Collections;
using System.IO;

public class Utils {



    public static GameObject FindObjectByName(string name) {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];

        for (int i = 0; i < objs.Length; i++) {
            if (objs[i].name == name && objs[i].hideFlags == HideFlags.None) {
                return objs[i].gameObject;
            }
        }

        return null;
    }

    public static IEnumerator SetSpriteFromURL(GameObject obj, string url, Vector2 screenSize) {
        Debug.Log($"IMAGE URL: {url}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        Debug.Log("Making request");

        yield return request.SendWebRequest();

        Debug.Log("Made request");

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError(request.error);
        } else {
            Debug.Log("WEB REQUEST SUCCESSFUL");

            DownloadHandlerTexture handler = (DownloadHandlerTexture) request.downloadHandler;

            if (handler == null) {
                Debug.Log("THE HANDLER IS NULL");
            }

            Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;

            Debug.Log("Made the texture");

            if (texture == null) {
                Debug.Log("THE TEXTURE IS NULL");
                yield break;
            }

            SetSpriteFromTexture(obj, texture, screenSize);
        }
    }

    public static void SetSpriteFromTexture(GameObject obj, Texture2D texture, Vector2 screenSize) {
        Debug.Log($"Texture size: {texture.width}x{texture.height}");

        float textureAspectRatio = (float) texture.width / texture.height;

        float screenAspectRatio = (float) screenSize.x / screenSize.y;

        /*
        on a screen height of 1080 with orthographic size of 5, each world space unit will take up 108 pixels (1080 / (5*2)).
        It's 5 * 2 because orthographic size specifies the size going from the center of the screen to the top.
        */

        // We have guaranteed a constant resolution of 1080x1920 in the middle of the screen
        // Effectively, we need to determine how many units are taken up by 960 vertical pixels or by 980 horizontal ones
        // The PPU is constant though, so we can use either. Basically, we need to get the canvas rect transform height

        Vector2 realScreenSize = FindObjectByName("Canvas").GetComponent<RectTransform>().sizeDelta;
        float screenPPU = realScreenSize.y / (FindObjectByName("Main Camera").GetComponent<Camera>().orthographicSize * 2);

        Debug.Log($"REAL SCREEN SIZE: {realScreenSize}");
        Debug.Log($"SCREEN PPU: {screenPPU}");

        float texturePPU = 0f;

        if (textureAspectRatio > screenAspectRatio) {
            Debug.Log("WIDE");
            Debug.Log($"{texture.width}");
            Debug.Log($"{5f * screenAspectRatio}");
            texturePPU = texture.width / screenSize.x;
            texturePPU = texture.width / (5f * screenAspectRatio);
        } else {
            Debug.Log("TALL");
            Debug.Log($"{texture.height}");
            Debug.Log($"{5f}");
            texturePPU = texture.height / screenSize.y;
            texturePPU = texture.height / 5f;
        }

        Debug.Log($"{texturePPU}");

        //texturePPU = 675f / 5;

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, 0f), texturePPU);

        Debug.Log("Made the sprite");

        Debug.Log($"Sprite size: {sprite.textureRect}");

        obj.GetComponent<SpriteRenderer>().sprite = sprite;

        Debug.Log("Set the sprite");
    }

    public static void SaveTextureToFile(Texture2D texture, string filename) {
        Debug.Log("Saving image to ???");

        byte[] bytes = texture.EncodeToPNG();
        
        FileStream fileStream = File.Open(Application.dataPath + "/" + filename, FileMode.Create);

        BinaryWriter binary = new BinaryWriter(fileStream);
        binary.Write(bytes);

        fileStream.Close();
    }

}
