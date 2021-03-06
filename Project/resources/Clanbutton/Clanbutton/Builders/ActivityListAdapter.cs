﻿using System.Collections.Generic;

using Android.Views;
using Android.Widget;

using Java.Lang;

using Clanbutton.Activities;
using Clanbutton.Core;
using System.Net;

using Android.App;
using Android.Support.V7.App;

namespace Clanbutton.Builders
{
    [Activity]
    internal class ActivityListAdapter : BaseAdapter
    {
        private MainActivity mainActivity;
        public List<UserActivity> lstActivity;

        public ActivityListAdapter(MainActivity MainActivity, List<UserActivity> lstActivity)
        {
            this.mainActivity = MainActivity;
            this.lstActivity = lstActivity;
        }

        public override int Count
        {
            get
            {
                return lstActivity.Count;
            }
        }

        public override Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            LayoutInflater inflater = (LayoutInflater)mainActivity.BaseContext.GetSystemService(Android.Content.Context.LayoutInflaterService);
            View itemView = inflater.Inflate(Resource.Layout.Activity_ListItem, null);

            TextView activity_user, activity_content;
            ImageView profile_picture;
            RelativeLayout activity_layout;
            activity_user = itemView.FindViewById<TextView>(Resource.Id.activity_user);
            activity_content = itemView.FindViewById<TextView>(Resource.Id.activity_message);
            profile_picture = itemView.FindViewById<ImageView>(Resource.Id.activity_avatar);
            activity_layout = itemView.FindViewById<RelativeLayout>(Resource.Id.activity_layout);
            
            ExtensionMethods extensionMethods = new ExtensionMethods();
            extensionMethods.DownloadPicture(lstActivity[position].ProfilePicture, profile_picture);

            string time_since = extensionMethods.GetTimeSince(lstActivity[position].CreationDate);
            activity_user.Text = lstActivity[position].Username;
            activity_content.Text = $"{lstActivity[position].ActivityMessage} ({time_since})";

            profile_picture.Click += delegate
            {
                OpenProfile(lstActivity[position].UserId);
                mainActivity.activities_reference.RemoveEventListener(mainActivity);
            };

            activity_layout.Click += delegate
            {
                SearchActivity.SearchStarter = lstActivity[position].GameName;
                mainActivity.StartActivity(new Android.Content.Intent(mainActivity, typeof(SearchActivity)).SetFlags(Android.Content.ActivityFlags.NoAnimation));
            };

            return itemView;
        }

        public async void OpenProfile(string userId)
        {
            DatabaseHandler firebase_database = new DatabaseHandler();
            UserAccount account = await firebase_database.GetAccountAsync(userId);
            ExtensionMethods.OpenUserProfile(account, mainActivity);
        }
    }
}