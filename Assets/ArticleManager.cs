using UnityEngine;
using System.Collections;

public class ArticleManager : MonoBehaviour {

    public int iArticleNumber;
    public Sprite newsImage;

    void OnEnable()
    {
        GetArticleSprite();
        //Setup Sprite to Use
        StartCoroutine(CheckComponetSetImage());
    }
    


    public ArticleContentManager contentManager
    {
        get
        {
            return GetComponentInChildren<ArticleContentManager>();
        }
    }


    public UnityEngine.UI.RawImage headerImage
    {
        get
        {
            return transform.GetComponentInChildren<UnityEngine.UI.RawImage>();
        }
    }
    public UnityEngine.UI.Image headerSprite
    {
        get
        {
            if (_headerSprite == null)
                _headerSprite = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
            return _headerSprite;
        }
    }

    public UnityEngine.UI.Image _headerSprite;

    public void GetArticleSprite()
    {
        if (iArticleNumber > -1)
        {
            if (NewsFeedParser.instance != null)
                if (NewsFeedParser.instance.articleImages != null)
                {
                    if (NewsFeedParser.instance.articleImages.Length > 0)
                    {
                        if (newsImage != NewsFeedParser.instance.articleImages[iArticleNumber] && NewsFeedParser.instance.articleImages[iArticleNumber] != null)
                            newsImage = NewsFeedParser.instance.articleImages[iArticleNumber];
                    }
                }
        }
    }

    public IEnumerator CheckComponetSetImage()
    {
        if (headerImage != null)
        {
            GameObject headerObject = headerImage.gameObject;
            Destroy(headerImage);
            yield return new WaitForEndOfFrame();
            headerObject.AddComponent<UnityEngine.UI.Image>();
            yield return null;
        }
        if(newsImage != null)
            headerSprite.sprite = newsImage;
    }

    IEnumerator SetupImageComponent()
    {
        GameObject headerObject = headerImage.gameObject;
        Destroy(headerImage);
        yield return new WaitForEndOfFrame();
        headerObject.AddComponent<UnityEngine.UI.Image>();
        yield return null;
    }

    public void SetHeaderImage(Sprite image)
    {
        newsImage = image;
        if(gameObject.activeInHierarchy)
            StartCoroutine(CheckComponetSetImage());
    }
}
