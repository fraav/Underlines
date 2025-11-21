using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageGallery : MonoBehaviour
{
    public List<Sprite> images;        // Lista de imágenes que asignas desde el Editor
    public Image displayImage;         // La imagen grande del Canvas

    private int currentIndex = 0;

    void Start()
    {
        if (images.Count > 0)
            displayImage.sprite = images[currentIndex];
    }

    public void NextImage()
    {
        if (images.Count == 0) return;

        currentIndex++;
        if (currentIndex >= images.Count)
            currentIndex = 0;   // Si quieres que vuelva al inicio (carrusel)

        displayImage.sprite = images[currentIndex];
    }

    public void PreviousImage()
    {
        if (images.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = images.Count - 1; // Vuelve al final

        displayImage.sprite = images[currentIndex];
    }
}
