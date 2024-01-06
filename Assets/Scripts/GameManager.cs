using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

using Google;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.Services;

using System;
using System.Collections.Generic;
using System.Net;

// TODO: Apply PixelArtShader with a static pixelation value, not using the animator
// Show an animation once the final value is set
// determjine how to make the animation run once

// Also, would be nice to let the user download the final result as an image to their phone

// At the end, make the Generate button disappear and show a Download button instead

// Write up installation instructions

public class GameManager : MonoBehaviour {

    public GameObject image;
    public TMP_InputField searchInput;
    public Slider pixelSlider;
    public TMP_Text pixelateButtonText;
    public Material pixelationMat;

    public GameObject slider;
    public GameObject pixelateButton;

    const int MAX_RESULTS = 1; // This is part of the Google search api
    const bool MOCK_GOOGLE_API_CALL = false;

    private bool resetImage = false;

    void Start() {
        pixelationMat.SetFloat("_PixelSample", 5000);
    }

    public void PickImageFromDevice(BaseEventData data) {
        Debug.Log("Picking image");

        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) => {
		    Debug.Log( "Image path: " + path );

		    if (path != null) {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, -1);
                if (texture == null) {
                    Debug.Log($"Couldn't load texture from {path}");
                    return;
                }

                Utils.SetSpriteFromTexture(image, texture, new Vector2(1080, 960));
                
                image.SetActive(true);
                slider.SetActive(true);
                pixelateButton.SetActive(true);
            } else {
                Debug.Log("NO IMAGE CHOSEN");
            }
        });

        Debug.Log("Permission result: " + permission);
    }

    public void SearchForImage(BaseEventData data) {
        Debug.Log("Searching for image");

        string apiKey = Config.GetApiKey();
        string cx = Config.GetCx();
        string query = searchInput.text; // "Enemies from Diablo 4"

        CustomSearchAPIService service   = new CustomSearchAPIService(new BaseClientService.Initializer {ApiKey = apiKey});
        CseResource.ListRequest request   = service.Cse.List();

        request.Q = query;
        request.Cx = cx;
        request.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;

        IList<Result> foundItems = new List<Result>();

        //foundItems = GetImageSearchResults(searchInput.text);

        try {

            IList<Result> paging = new List<Result>();
            var count = 0;

            while (paging != null) {

                request.Start = count + 1;
                request.Num = 10;

                if ((request.Start + request.Num) > MAX_RESULTS) {
                    
                    request.Num = MAX_RESULTS - Convert.ToInt32(request.Start) + 1;
                }

                if (request.Num < 1) {
                    Debug.Log("Got the max number of results");

                    break;
                }

                if (!MOCK_GOOGLE_API_CALL) {
                    Search result = request.Execute();
                    
                    Debug.Log($"Total results: {result.SearchInformation.TotalResults}");
                    Debug.Log($"Start: {request.Start}");
                    
                    paging = result.Items;
                } else {
                    paging = new List<Result>() {
                        new Result(),
                        new Result(),
                        new Result()
                    };

                    paging[0].Title = "Vulnerable Status Effect in Diablo 4 - Icy Veins";
                    paging[0].Link = "https://static.icy-veins.com/wp/wp-content/uploads/2023/07/Vulnerable1.jpg";

                    paging[1].Title = "Character Stats";
                    paging[1].Link = "https://imageio.forbes.com/specials-images/imageserve/64808017d792b03c5157b386/0x0.jpg?format=jpg&height=900&width=1600&fit=bounds";

                    paging[2].Title = "Diablo 4 Ashava World Boss spawn times, location - Polygon";
                    paging[2].Link = "https://cdn.vox-cdn.com/thumbor/2gQIK9Hk_uII3d5joRpFliE8Afk=/0x0:1600x900/1200x0/filters:focal(0x0:1600x900):no_upscale()/cdn.vox-cdn.com/uploads/chorus_asset/file/24525994/CrucibleLocation.png";
                }
            
                Debug.Log($"Num returned items: {paging.Count}");

                if (paging != null) {
                    foreach (Result item in paging) {
                        foundItems.Add(item);
                    }

                    count += paging.Count;
                }
            }

        } catch(GoogleApiException gae) {
            if (gae.HttpStatusCode == HttpStatusCode.TooManyRequests) {
                Debug.Log("Api request limit exceeded for the day. Try again tomorrow.");

                return;
            }

            Debug.Log($"Error: {gae.HttpStatusCode}");
            Debug.Log($"Error: {gae.Message}");
        }

        foreach (Result item in foundItems) {
            Debug.Log($"Title: {item.Title}");
            Debug.Log($"Link: {item.Link}");
        }

        StartCoroutine(Utils.SetSpriteFromURL(image, foundItems[0].Link, new Vector2(1080, 960)));

        image.SetActive(true);
        slider.SetActive(true);
        pixelateButton.SetActive(true);
    }

    public void PixelateImage(BaseEventData data) {
        Debug.Log("Clicked");

        if (resetImage) {
            Debug.Log("Resetting image");

            pixelationMat.SetFloat("_PixelSample", 5000);

            pixelateButtonText.text = "Pixelate";
        } else {
            int val = Convert.ToInt32(pixelSlider.value);

            Debug.Log($"Pixelating image with a pixel value of {val}");

            pixelationMat.SetFloat("_PixelSample", val);

            pixelateButtonText.text = "Reset Image";
        }

        resetImage = !resetImage;
    }

    public void SaveImageToDisk(BaseEventData data) {
        Utils.SaveTextureToFile(image.GetComponent<SpriteRenderer>().sprite.texture, "pixelated.png");
    }

    private IList<Result> GetImageSearchResults(string query) {
        string apiKey = "AIzaSyCCO43iRGu9LtVWznpNKD4Hww1xeN86MmY";
        string cx = "b7056aeeaab3d4581";

        CustomSearchAPIService service = new CustomSearchAPIService(new BaseClientService.Initializer {ApiKey = apiKey});
        CseResource.ListRequest request = service.Cse.List();

        request.Q = query; // "Enemies from Diablo 4"
        request.Cx = cx;
        request.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;

        return new List<Result>();
    }
}