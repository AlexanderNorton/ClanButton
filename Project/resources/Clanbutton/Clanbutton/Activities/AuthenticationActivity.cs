﻿using System;
using System.Web;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.V7.App;
using Android.Gms.Tasks;

using Firebase.Auth;

using Clanbutton.Core;

using Android.Webkit;
using Android.Graphics;
using Android.Runtime;
using Android.Views;

namespace Clanbutton.Activities
{
    [Activity(Label = "Clanbutton", Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class AuthenticationActivity : AppCompatActivity, IOnCompleteListener
    {

        ulong SteamUserId;
    
        FirebaseAuth auth;
        FirebaseUser user;
        DatabaseHandler firebase_database;
        public WebView webView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Authentication Activity is started (on app open).
            base.OnCreate(savedInstanceState);
            // The view is set to the Authentication_Layout.
            SetContentView(Resource.Layout.Authentication_Layout);

            // Get the current user.
            auth = FirebaseAuth.Instance;
            user = auth.CurrentUser;

            if (user != null)
            {
                // If user already exists, they are already signed in.
                // Start the MainActivity.
                StartActivity(new Android.Content.Intent(this, typeof(MainActivity)).SetFlags(Android.Content.ActivityFlags.NoAnimation));
				Finish();
                return;
            }

            // Otherwise, continue to obtain the ImageButton btnlogin from the resources.
            ImageButton btnLogin = FindViewById<ImageButton>(Resource.Id.btnLogin);

            btnLogin.Click += delegate
            {
                // On btnLogin click, open a WebView (with the Steam URL).
                SetContentView(Resource.Layout.WebView_Layout);
                webView = FindViewById<WebView>(Resource.Id.webView);
                string steam_url = "https://steamcommunity.com/openid/login?openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select&openid.identity=http://specs.openid.net/auth/2.0/identifier_select&openid.mode=checkid_setup&openid.ns=http://specs.openid.net/auth/2.0&openid.realm=https://clanbutton&openid.return_to=https://clanbutton/signin/";
                webView.Visibility = ViewStates.Visible;
                ExtendedWebViewClient webClient = new ExtendedWebViewClient();
                webClient.steamAuthentication = this;
                webView.SetWebViewClient(webClient);
                webView.LoadUrl(steam_url);

                // Allow JavaScript to be used in the WebView.
                WebSettings webSettings = webView.Settings;
                webSettings.JavaScriptEnabled = true;
            };
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (webView != null)
            {
                if (e.Action == KeyEventActions.Down)
                {
                    if (keyCode == Keycode.Back)
                    {
                        StartActivity(new Android.Content.Intent(this, typeof(AuthenticationActivity)));
                        Finish();
                    }
                }
            }
            return true;
        }

        public async void SteamAuth(ulong userid)
        {
            SteamUserId = userid;
            firebase_database = new DatabaseHandler();
            if (await firebase_database.AccountExistsAsync(SteamUserId.ToString()))
            {
                // If the Steam user exists, sign them in with the Steam user ID appended to '@clanbutton.com'.
                auth.SignInWithEmailAndPassword($"{SteamUserId.ToString()}@clanbutton.com", "nopass");
				StartActivity(new Android.Content.Intent(this, typeof(MainActivity)));
				Finish();
			}
            else
            {
                // Otherwise, create the user.
                auth.CreateUserWithEmailAndPassword($"{SteamUserId.ToString()}@clanbutton.com", "nopass").AddOnCompleteListener(this);
                // This then calls the OnCompleteListener method OnComplete().
            }
        }

        public async void OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                auth = FirebaseAuth.Instance;
                user = auth.CurrentUser;

                // Create an account in the Firebase realtime database.
                await firebase_database.CreateAccount(user.Uid.ToString(), SteamUserId, user.Email);
                var account = await firebase_database.GetAccountAsync(user.Uid.ToString());
                account.Update();
				// Start the SearchActivity for the new user that was created.
				StartActivity(new Android.Content.Intent(this, typeof(MainActivity)));
				Finish();

			}
            else
            {
                Toast.MakeText(this, $"{task.Exception.Message}.", ToastLength.Long).Show();
                return;
            }
        }
    }

    internal class ExtendedWebViewClient : WebViewClient
    {
        public AuthenticationActivity steamAuthentication;
        
        public override async void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            // Get the Steam URL and convert it to a URI.
            Uri Url = new Uri(url);

            if (Url.Authority.Equals("clanbutton"))
            {
                steamAuthentication.webView.Visibility = ViewStates.Gone;
                // If the end of the URI is 'clanbutton', create a Firebase user (i.e authenticate).
                Uri userAccountUrl = new Uri(HttpUtility.ParseQueryString(Url.Query).Get("openid.identity"));
                ulong SteamUserId = ulong.Parse(userAccountUrl.Segments[userAccountUrl.Segments.Length - 1]);
                // Call SteamAuth method providing the new SteamUserId obtained from the URL when the user logs in to Steam.
                steamAuthentication.SteamAuth(SteamUserId);
                // Stop loading the WebView.
                view.StopLoading();
            };
        }
    }
}