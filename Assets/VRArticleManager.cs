using UnityEngine;
using System.Collections.Generic;

public class VRArticleManager : MonoBehaviour {

    public ArticleManager[] articleObjects;

    public Sprite[] articleSprites;

    public void Awake()
    {
        InitializeContent();
    }

    void InitializeContent()
    {
        articleObjects = GetAllArticles(transform);//GetComponentsInChildren<ArticleManager>();
    }

    ArticleManager[] GetAllArticles(Transform tObject)
    {
        List<ArticleManager> allMeshs = new List<ArticleManager>();
        if (tObject.GetComponent<ArticleManager>())
            allMeshs.Add(tObject.GetComponent<ArticleManager>());
        //allMeshs.AddRange(tObject.GetComponentsInChildren<ArticleManager>());
        for (int i = 0; i < tObject.childCount; i++)
        {
            allMeshs.AddRange(GetAllArticles(tObject.GetChild(i)));
        }
            /*for (int i = 0; i < tObject.childCount; i++)
            {
                allMeshs.AddRange(GetAllArticles(tObject.GetChild(i)));
            }*/
        return allMeshs.ToArray();
    }


    public void SetArticleToContent(int iArticle, NewsItemModel newsArticle)
    {
        articleObjects[iArticle].contentManager.SetHeader(newsArticle.getTitle());
        articleObjects[iArticle].contentManager.SetBody(newsArticle.getBody());
        articleObjects[iArticle].contentManager.SetByline(newsArticle.getByline());
        //articleObjects[iArticle].SetHeaderImage(string.Join("", new string[2] { "http://mlb.mlb.com/", newsArticle.getNewsItemPhotoDictionary()["1280x720"].getSrc() }));
        articleObjects[iArticle].iArticleNumber = iArticle;
        articleObjects[iArticle].CheckComponetSetImage();
    }

    public void SetArticleToContent(int iArticle, NewsItemModel newsArticle,Sprite imageTexture)
    {
        try
        {
            if (iArticle < articleObjects.Length)
            {
                articleObjects[iArticle].iArticleNumber = iArticle;
                articleObjects[iArticle].contentManager.SetHeader(newsArticle.getTitle());
                articleObjects[iArticle].contentManager.SetBody(newsArticle.getBody());
                articleObjects[iArticle].contentManager.SetByline(newsArticle.getByline());
                if(imageTexture != null)
                    articleObjects[iArticle].SetHeaderImage(imageTexture);
            }
        }
        catch
        {
            Debug.Log(iArticle + " failed to set content");
        }
    }
}
