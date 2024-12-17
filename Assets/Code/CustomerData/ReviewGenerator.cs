using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Review
{
    public string name;
    public string review;
    public int stars;
}

[System.Serializable]
public class ReviewList
{
    public Review[] reviews;
}

public static class ReviewGenerator
{
    private static string reviewFilePath = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            "reviews.json"
            );

    public static void UpdateStars(List<VisualElement> stars, int starsAchieved)
    {
        float timeTaken = Time.time - GameManager.Instance.startTime;

        foreach (var (index, star) in stars.Select((value, i) => (i, value)))
        {
            if (index >= starsAchieved)
                return;

            star.AddToClassList("star-filled");
            star.RemoveFromClassList("star-stroke");
        }
    }

    public static Review GenerateReview(int starsAchieved)
    {
        string rawJson = File.ReadAllText(reviewFilePath);
        ReviewList reviews = JsonUtility.FromJson<ReviewList>(rawJson);

        // get a random review with the same amount of stars as passed
        Review[] filteredReviews = reviews.reviews.Where(r => r.stars == starsAchieved).ToArray();
        return filteredReviews[Random.Range(0, filteredReviews.Count())];
    }
}
