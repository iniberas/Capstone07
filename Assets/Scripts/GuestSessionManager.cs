using UnityEngine;
using System;

public class GuestSessionManager : MonoBehaviour
{
    private const string GUEST_ID_KEY = "GuestID";

    public static string GetGuestId()
    {
        if (PlayerPrefs.HasKey(GUEST_ID_KEY))
        {
            return PlayerPrefs.GetString(GUEST_ID_KEY);
        }
        else
        {
            string newGuestId = Guid.NewGuid().ToString();
            
            PlayerPrefs.SetString(GUEST_ID_KEY, newGuestId);
            PlayerPrefs.Save();
            
            return newGuestId;
        }
    }
}