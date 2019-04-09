using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Scrollbar))]
public class GvrPagedScrollBar : MonoBehaviour {
  [SerializeField]
  private GvrPagedScrollRect pagedScrollRect;

  private Scrollbar scrollbar;

  private const float kLerpSpeed = 12.0f;

  void Awake() {
    scrollbar = GetComponent<Scrollbar>();
  }

  void Update() {
    if (pagedScrollRect == null) {
      return;
    }

    if (scrollbar.interactable) {
      Debug.LogWarning("The Scrollbar associated with a GvrPagedScrollBar must not be interactable.");
      scrollbar.interactable = false;
    }

    // Update the size of the handle in case the
    // PageCount has changed.
    float size = 1.0f / pagedScrollRect.PageCount;
    scrollbar.size = size;

    // Calculate the desired a value of the scrollbar.
    float desiredValue = (float)pagedScrollRect.ActivePageIndex / (pagedScrollRect.PageCount - 1);

    // Animate towards the desired value.
    scrollbar.value = Mathf.Lerp(scrollbar.value, desiredValue, Time.deltaTime * kLerpSpeed);
  }
}
