using UnityEngine;
using UnityEngine.UI;

namespace RandomFortress.Common.Utils
{
    public static class ImageUtils
    {
        // 지정된 크기안에 최대크기의 Image 조정
        public static void ImageSizeToFit(int width, int height, ref Image image)
        {
            float imageWidth = image.sprite.rect.width;
            float imageHeight = image.sprite.rect.height;

            // Sprite의 가로 세로 비율 계산
            float widthRatio = width / imageWidth;
            float heightRatio = height / imageHeight;

            // 비율에 맞게 크기 조정
            float scaleRatio = Mathf.Min(widthRatio, heightRatio);
            Vector3 newScale = new Vector3(scaleRatio, scaleRatio, 1f);
            image.rectTransform.sizeDelta = new Vector2(imageWidth * scaleRatio, imageHeight * scaleRatio);
        }
    }
}