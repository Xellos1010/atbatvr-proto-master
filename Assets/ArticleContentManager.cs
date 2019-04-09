using UnityEngine;

public class ArticleContentManager : MonoBehaviour {

    public UnityEngine.UI.Text header;
    public UnityEngine.UI.Text byline;
    public UnityEngine.UI.Text body;
    
    public void SetHeader(string headerText)
    {
        header.text = headerText;
    }

    public void SetByline(string bylineText)
    {
        byline.text = bylineText;
    }

    public void SetBody(string bodyText)
    {
        body.text = bodyText;
    }
}
