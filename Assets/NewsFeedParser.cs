using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

public class NewsFeedParser : MonoBehaviour {

    public static NewsFeedParser instance;
    private string newsFeedFilterLong = "MajorLeagueBaseball";
    public string urlNewsFeed
    {
        get
        {
            if (TeamData.ReturnTeamAbv(newsFeedFilterLong) != null)
                return "http://mlb.mlb.com/gen/content/tag/v1/" + TeamData.ReturnTeamAbv(newsFeedFilterLong) + "/news/android_newsreader.json";
            else
            {
                return "http://mlb.mlb.com/gen/content/tag/v1/mlb/news/android_newsreader.json";
            }
        }
    }
    string jsonString;
    public int currentArticle = 0;
    public DateTime dateTestDisplay;

    public VRArticleManager articlesToPopulate;

    [SerializeField]
    public NewsItemModel[] newsArticles;
    public Sprite[] articleImages;
    [SerializeField]
    public Texture2D[] articleImagesRawPart1;
    [SerializeField]
    public Texture2D[] articleImagesRawPart2;
    [SerializeField]
    Texture2D atlas1;   // Can hold 25 320x180 textures 8000x4500
    [SerializeField]
    Rect[] atlas1Rects;
    [SerializeField]
    Texture2D atlas2;   // Can hold 15 320x180 textures 4800x2750
    [SerializeField]
    Rect[] atlas2Rects;
    Dictionary<string, object> feedParsed
    {
        get
        {
            if (_feedParsed == null)
                _feedParsed = MiniJSON.MiniJson.Deserialize(urlNewsFeed) as Dictionary<string, object>;
            return _feedParsed;
        }

    }
    Dictionary<string, object> _feedParsed;

    void OnEnable()
    {
        //TODO Abstract out to encapsulate news feed (to handle multiple windows)
        instance = this;
    }

    void OnDisable()
    {
        instance = null;
        // If the thread is still running, we should shut it down,
        // otherwise it can prevent the game from exiting correctly.
        if (_threadRunning)
        {
            // This forces the while loop in the ThreadedWork function to abort.
            _threadRunning = false;

            // This waits until the thread exits,
            // ensuring any cleanup we do after this is safe. 
            _thread.Join();
        }

        // Thread is guaranteed no longer running. Do other cleanup tasks.
    }

    #region Initialization
    void Start()
    {
        articleImagesRawPart1 = new Texture2D[40];

        //TODO Have a master Panel Manager that if detects News Reader Prefab is instantiated/Active then to Load News Feed
        StartCoroutine(LoadNewsFeed());
    }

    IEnumerator TestDateParse()
    {
        Debug.Log(urlNewsFeed);
        List<object> articlesRaw = feedParsed["list"] as List<object>;
        newsArticles = new NewsItemModel[articlesRaw.Count];
        articleImages = new Sprite[articlesRaw.Count];
        for (int i = 0; i < articlesRaw.Count-1; i++)
        {
            newsArticles[i] = new NewsItemModel(articlesRaw[i] as Dictionary<string, object>);
            if (newsArticles[i].getNewsItemPhotoDictionary().ContainsKey("320x180"))
                yield return GetImageAsSprite(i, string.Join("", new string[2] { "http://mlb.mlb.com/", newsArticles[i].getNewsItemPhotoDictionary()["320x180"].getSrc() }));
            else
                Debug.Log("No image content for Article " + i);
            if(i < articlesToPopulate.articleObjects.Length)
                SetupArticle(i);
        }
        //atlas1 = new Texture2D(8050, 4550);
        //atlas2 = new Texture2D(4850, 2750);
        //PackAtlas();
        //yield return new WaitForSeconds(.5f);
        //atlas2Rects = atlas2.PackTextures(articleImagesRawPart2, 0);
        //yield return new WaitForSeconds(.5f);
        //yield return SetSpritesToAtlas();
        //PopulateArticleContent();
    }

    public void LoadNewsFeedByFilter(string filter)
    {
        StopAllCoroutines();
        newsFeedFilterLong = filter;
        StartCoroutine(LoadNewsFeed());
    }

    private IEnumerator LoadNewsFeed()
    {
        yield return DownloadNewsfeed();
        yield return TestDateParse();
    }

    public void PackAtlas()
    {
        atlas1Rects = atlas1.PackTextures(articleImagesRawPart1, 0);
    }

    public IEnumerator SetSpritesToAtlas()
    {
        yield return null;
        articleImages = new Sprite[articleImages.Length];
        for (int i = 0; i < articleImages.Length; i++)
        {
            
            articleImages[i] = Sprite.Create(atlas1, new Rect(atlas1Rects[i].x, atlas1Rects[i].y,320,180), new Vector2(.5f, .5f));
            yield return null;
            /*if (i < 25)
                articleImages[i] = Sprite.Create(atlas1, atlas1Rects[i], new Vector2(.5f, .5f));
            else
                articleImages[i] = Sprite.Create(atlas2, atlas1Rects[i-25], new Vector2(.5f, .5f));*/
        }
    }

    public void PopulateArticleContent()
    {
        for (int i = 0; i < articlesToPopulate.articleObjects.Length; i++)
        {
            articlesToPopulate.SetArticleToContent(i, newsArticles[i]);
        }
    }

    IEnumerator GetImageAsSprite(int articleNumber,string textureURL)
    {
        yield return null;
        WWW textureDownload = new WWW(textureURL);
        yield return textureDownload;
        if (textureDownload.error == null)
        {
            articleImages[articleNumber] = Sprite.Create(textureDownload.texture, new Rect(0, 0, textureDownload.texture.width, textureDownload.texture.height), new Vector2(.5f, .5f));
            articleImagesRawPart1[articleNumber] = textureDownload.texture;
            /*if (articleNumber < 25)
                articleImagesRawPart1[articleNumber] = textureDownload.texture;
            else
                articleImagesRawPart2[articleNumber-25] = textureDownload.texture;*/
        }
        else
        {
            Debug.Log("error getting sprite for article " + articleNumber);
        }
    }

    void InitializeThread()
    {
        // Begin our heavy work on a new thread.
        _thread = new Thread(ThreadedWork);
        _thread.Start();
    }

    void InitializeNewsFeedJSON()
    {
         
    }

    private IEnumerator DownloadNewsfeed()
    {
        WWW www = new WWW(urlNewsFeed);
        yield return www;
        if (www.error == null)
        {
            jsonString = www.text;
            yield return ProcessNewsFeed(www.text);
        }
        else
        {
            Debug.Log("No News Feed JSOn was Downloaded");
        }
    }

    private IEnumerator ProcessNewsFeed(string jsonString)
    {
        Debug.Log("Parsing News Feed");
        _feedParsed = MiniJSON.MiniJson.Deserialize(jsonString) as Dictionary<string, object>;
        yield return new WaitForEndOfFrame();
        Debug.Log("Parse Finished");
    }

    #endregion

    public void SetupCurrentArticles()
    {
        for (int i = 0; i < newsArticles.Length; i++)
        {
            articlesToPopulate.SetArticleToContent(i, newsArticles[i],articleImages[i]);
        }
        /*articlesToPopulate.contentManager.SetHeader(newsArticles[currentArticle].getTitle());
        articlesToPopulate.contentManager.SetBody(newsArticles[currentArticle].getBody());
        articlesToPopulate.contentManager.SetByline(newsArticles[currentArticle].getByline());
        articlesToPopulate.SetHeaderImage(String.Join("", new string[2] { "http://mlb.mlb.com/", newsArticles[currentArticle].getNewsItemPhotoDictionary()["1280x720"].getSrc()}));*/
        //Debug.Log(newsArticles[0].getNewsItemPhotoDictionary());
        //GetComponentInChildren<UnityEngine.UI.Text>().text = feedParsed["seo-headline"] as string;
    }

    public void SetupArticle(int iArticleNumber)
    {
        //Debug.Log("Setting up Article content for Article " + iArticleNumber);
        //if (iArticleNumber > 6)
        //    Debug.Log("Checking for issues");
        articlesToPopulate.SetArticleToContent(iArticleNumber, newsArticles[iArticleNumber], articleImages[iArticleNumber]);
    }

    public void NextArticle()
    {
        currentArticle = Mathf.Clamp(currentArticle + 1, 0, newsArticles.Length-1);
        SetupCurrentArticles();
    }

    public void PreviousArticle()
    {
        currentArticle = Mathf.Clamp(currentArticle - 1, 0, newsArticles.Length - 1);
        SetupCurrentArticles();
    }



    #region TheadingTest

    bool _threadRunning;
    Thread _thread;

    void ThreadedWork()
    {
        _threadRunning = true;
        bool workDone = false;

        // This pattern lets us interrupt the work at a safe point if neeeded.
        while (_threadRunning && !workDone)
        {
            // Do Work...
        }
        _threadRunning = false;
    }

    #endregion
        
}
