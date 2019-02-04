﻿using Firebase.Auth;
using Firebase.Database;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using System.Collections;

namespace Clanbutton.Builders
{
    public class UserAccount
    {
        public string UserId { get; set; }
        public string SteamId { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        private ArrayList PastGameSearches = new ArrayList();

        public string CurrentGameSearch { get; set; }

        public UserAccount() { }

        public UserAccount(string userid, string steamid, string email)
        {
            UserId = userid;
            SteamId = steamid;
            Email = email;
            Username = "Guest";
        }

        public async void Update(FirebaseClient firebase)
        {
            //Delete current account
            string userid = FirebaseAuth.Instance.CurrentUser.Uid;
            var accounts = await firebase.Child("accounts").OnceAsync<UserAccount>();
            string key = "";

            foreach(var acc in accounts)
            {
                if (acc.Object.UserId == userid)
                {
                    key = acc.Key;
                }
            }
            await firebase.Child("accounts").Child(key).PutAsync(this);

            //Add 'new' account.
        }
    }
}